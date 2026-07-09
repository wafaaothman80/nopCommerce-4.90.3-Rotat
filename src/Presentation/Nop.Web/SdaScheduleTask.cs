using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace Nop.Web
{
    public class SdaScheduleTask : IScheduleTask
    {
        private readonly IOrderService _orderService;
        private readonly INopDataProvider _dataProvider;
        private readonly Nop.Services.Logging.ILogger _logger;

        private static readonly NetSuiteApiConfig _config;
        private static readonly RestClient _restClient;

        private const string KEY_PREF = "ShippingPreference";
        private const string KEY_DATE = "RequestedShippingDate";
        private const string ASAP = "Ship as soon as possible";
        private const string CHOOSE = "Choose a shipping date";

        public SdaScheduleTask(
            IOrderService orderService,
            INopDataProvider dataProvider,
            Nop.Services.Logging.ILogger logger)
        {
            _orderService = orderService;
            _dataProvider = dataProvider;
            _logger = logger;
        }

        static SdaScheduleTask()
        {
            _config = new NetSuiteApiConfig();
            _restClient = CreateRestClient();
        }

        private static RestClient CreateRestClient()
        {
            var client = new RestClient();
            var oAuth1 = OAuth1Authenticator.ForAccessToken(
                _config.ClientId, _config.ClientSecret,
                _config.TokenId, _config.TokenSecret,
                OAuthSignatureMethod.HmacSha256);
            oAuth1.Realm = _config.AccountId;
            client.Authenticator = oAuth1;
            return client;
        }

        
        public async Task ExecuteAsync()
        {
            try
            {
                _logger.Information("SdaScheduleTask: Starting.");
                var now = DateTime.UtcNow;

               
                await ConsolidateCustomerGroupsAsync(now);

              
                await LockGroupsReachingWindowAsync(now);

              
                var orders = await _orderService.SearchOrdersAsync(
                    psIds: new List<int> { (int)PaymentStatus.Paid },
                    osIds: new List<int> { (int)OrderStatus.Processing });

                _logger.Information(
                    $"SdaScheduleTask: {orders.TotalCount} paid/processing orders.");

                foreach (var order in orders)
                {
                    try
                    {
                        if (await SdaExistsForOrderAsync(order.Id))
                            continue;

                        var cv = DeserializeCustomValues(order.CustomValuesXml);
                        var pref = cv.TryGetValue(KEY_PREF, out var p) ? p?.ToString() : null;
                        if (string.IsNullOrEmpty(pref))
                            continue;

                        DateTime? shipDate = null;
                        if (pref.Equals(CHOOSE, StringComparison.OrdinalIgnoreCase))
                        {
                            var raw = cv.TryGetValue(KEY_DATE, out var d) ? d?.ToString() : null;
                            if (TryParseDate(raw, out var parsed))
                                shipDate = parsed;
                        }

                        await ProcessOrderAsync(order, shipDate, now);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"SdaScheduleTask: Order {order.Id}: {ex.Message}", ex);
                    }
                }

               
                await ProcessLockedGroupsAsync();

               
                await ProcessSdaCreatedGroupsWithPendingOrdersAsync();

                _logger.Information("SdaScheduleTask: Complete.");
            }
            catch (Exception ex)
            {
                _logger.Error($"SdaScheduleTask: Fatal: {ex.Message}", ex);
            }
        }

        private async Task ConsolidateCustomerGroupsAsync(DateTime now)
        {
            var cutoff = now.Date.AddDays(30);

            var duplicates = await _dataProvider.QueryAsync<CustomerGroupCount>(@"
                SELECT   [CustomerId], COUNT(*) AS GroupCount
                FROM     [dbo].[NS_ShipmentGroup]
                WHERE    [RequestedShipDate] <= @Cut
                GROUP BY [CustomerId]
                HAVING   COUNT(*) > 1",
                new DataParameter("@Cut", cutoff));

            foreach (var dup in duplicates)
            {
                try
                {
                    // Get groups — prefer one with SDA, then earliest
                    var groups = (await _dataProvider.QueryAsync<ShipmentGroupRow>(@"
                        SELECT [Id]               AS GroupId,
                               [CustomerId],
                               [RequestedShipDate],
                               [Status],
                               [SdaNumber],
                               [IsLocked]
                        FROM   [dbo].[NS_ShipmentGroup]
                        WHERE  [CustomerId]        = @Cust
                        AND    [RequestedShipDate] <= @Cut
                        ORDER BY
                               CASE WHEN [SdaNumber] IS NOT NULL THEN 0 ELSE 1 END,
                               [CreatedOnUtc] ASC",
                        new DataParameter("@Cust", dup.CustomerId),
                        new DataParameter("@Cut", cutoff))).ToList();

                    if (groups.Count <= 1)
                        continue;

                    var primary = groups.First();
                    var others = groups.Skip(1).ToList();

                    _logger.Information(
                        $"SdaScheduleTask: Consolidating {others.Count} groups into " +
                        $"group {primary.GroupId} for customer {dup.CustomerId}.");

                    foreach (var other in others)
                    {
                        
                        await _dataProvider.ExecuteNonQueryAsync(@"
                            UPDATE [dbo].[NS_ShipmentGroupOrder]
                            SET    [ShipmentGroupId] = @Primary
                            WHERE  [ShipmentGroupId] = @Other
                            AND    [OrderId] NOT IN (
                                SELECT [OrderId] FROM [dbo].[NS_ShipmentGroupOrder]
                                WHERE  [ShipmentGroupId] = @Primary
                            )",
                            new DataParameter("@Primary", primary.GroupId),
                            new DataParameter("@Other", other.GroupId));

                        
                        await _dataProvider.ExecuteNonQueryAsync(@"
                            DELETE FROM [dbo].[NS_ShipmentGroupOrder]
                            WHERE [ShipmentGroupId] = @Other",
                            new DataParameter("@Other", other.GroupId));

                        await _dataProvider.ExecuteNonQueryAsync(@"
                            DELETE FROM [dbo].[NS_ShipmentGroup] WHERE [Id] = @Id",
                            new DataParameter("@Id", other.GroupId));

                        _logger.Information(
                            $"SdaScheduleTask: Merged group {other.GroupId} → {primary.GroupId}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        $"SdaScheduleTask: Consolidate customer {dup.CustomerId}: {ex.Message}", ex);
                }
            }
        }

       
        private async Task ProcessOrderAsync(Order order, DateTime? shipDate, DateTime now)
        {
            var erpId = await GetErpOrderIdAsync(order.Id);
            if (erpId <= 0)
            {
                _logger.Warning($"SdaScheduleTask: Order {order.Id} — no ERP ID. Skipping.");
                return;
            }

            var effectiveDate = shipDate ?? now.Date;
            var daysAway = (effectiveDate.Date - now.Date).TotalDays;

          
            var activeSda = await GetCustomerActiveSdaGroupAsync(order.CustomerId, now);
            if (activeSda != null)
            {
               
                _logger.Information(
                    $"SdaScheduleTask: Order {order.Id} — active SDA group {activeSda.GroupId} " +
                    $"(SDA {activeSda.SdaNumber}). Adding to group for append.");
                await AddOrderToGroupAsync(activeSda.GroupId, order.Id, erpId);
                return;
            }

           
            var existingGroupId = await GetCustomerAnyGroupAsync(order.CustomerId, now);
            int groupId;

            if (existingGroupId > 0)
            {
                groupId = existingGroupId;
                _logger.Information(
                    $"SdaScheduleTask: Order {order.Id} added to existing group {groupId}.");
            }
            else
            {
                groupId = await CreateShipmentGroupAsync(order.CustomerId, effectiveDate);
                _logger.Information(
                    $"SdaScheduleTask: Order {order.Id} → new group {groupId} " +
                    $"(ship {effectiveDate:dd/MM/yyyy}).");
            }

            await AddOrderToGroupAsync(groupId, order.Id, erpId);

            if (daysAway <= 7)
            {
                await LockGroupAsync(groupId);
                _logger.Information($"SdaScheduleTask: Group {groupId} locked (≤7 days).");
            }
        }

       
        private async Task ProcessLockedGroupsAsync()
        {
            var groups = (await _dataProvider.QueryAsync<ShipmentGroupRow>(@"
                SELECT [Id]               AS GroupId,
                       [CustomerId],
                       [RequestedShipDate],
                       [Status],
                       [SdaNumber],
                       [IsLocked]
                FROM   [dbo].[NS_ShipmentGroup]
                WHERE  [IsLocked] = 1
                AND    [Status]   = 'Locked'",
                Array.Empty<DataParameter>())).ToList();

            foreach (var grp in groups)
            {
                try
                {
                    var pending = await GetPendingOrdersForGroupAsync(grp.GroupId);
                    if (!pending.Any())
                        continue;

                    _logger.Information(
                        $"SdaScheduleTask: Group {grp.GroupId} LOCKED — " +
                        $"CREATE SDA for {pending.Count} orders: " +
                        $"[{string.Join(", ", pending.Select(x => x.orderId))}]");

                    var sda = await CreateNewSdaAsync(
                        pending,
                        grp.RequestedShipDate,
                        $"Requested ship date: {grp.RequestedShipDate:dd/MM/yyyy} — nopCommerce");

                    if (!string.IsNullOrEmpty(sda))
                        await SaveGroupSdaNumberAsync(grp.GroupId, sda);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        $"SdaScheduleTask: ProcessLocked group {grp.GroupId}: {ex.Message}", ex);
                }
            }
        }

       
        private async Task ProcessSdaCreatedGroupsWithPendingOrdersAsync()
        {
            var groups = (await _dataProvider.QueryAsync<ShipmentGroupRow>(@"
                SELECT g.[Id]               AS GroupId,
                       g.[CustomerId],
                       g.[RequestedShipDate],
                       g.[Status],
                       g.[SdaNumber],
                       g.[IsLocked]
                FROM   [dbo].[NS_ShipmentGroup] g
                WHERE  g.[Status]    = 'SdaCreated'
                AND    g.[SdaNumber] IS NOT NULL
                AND    EXISTS (
                    -- Has at least one order without a successful SDA transaction
                    SELECT 1 FROM [dbo].[NS_ShipmentGroupOrder] go
                    WHERE  go.[ShipmentGroupId] = g.[Id]
                    AND    NOT EXISTS (
                        SELECT 1 FROM [dbo].[NS_SdaTransaction] t
                        WHERE  t.[OrderId] = go.[OrderId] AND t.[HasSda] = 1
                    )
                )",
                Array.Empty<DataParameter>())).ToList();

            foreach (var grp in groups)
            {
                try
                {
                    var pending = await GetPendingOrdersForGroupAsync(grp.GroupId);
                    if (!pending.Any())
                        continue;

                    _logger.Information(
                        $"SdaScheduleTask: Group {grp.GroupId} SdaCreated — " +
                        $"APPEND {pending.Count} orders to SDA {grp.SdaNumber}: " +
                        $"[{string.Join(", ", pending.Select(x => x.orderId))}]");

                    await AppendToExistingSdaAsync(grp.SdaNumber, pending);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        $"SdaScheduleTask: ProcessSdaCreated group {grp.GroupId}: {ex.Message}", ex);
                }
            }
        }

       
        private async Task<string> CreateNewSdaAsync(
            List<(int orderId, long erpId)> orders,
            DateTime? shipDate, string notes)
        {
            try
            {
                var url = _config.ApiRoot + "&objType=91&objId=0";
                var payload = new
                {
                    objType = 91,
                    objId = 0,
                    salesOrders = orders.Select(o => o.erpId).ToArray(),
                    notes = notes
                };

                var (ok, content) = await CallApiAsync(url, payload);
                if (!ok)
                    return null;

                dynamic result = JsonConvert.DeserializeObject(content);
                if (result.success != true || result.data == null || result.data.Count == 0)
                {
                    _logger.Warning($"SdaScheduleTask: CreateSda — no data: {content}");
                    return null;
                }

                string retSda = null;
                foreach (var dataItem in result.data)
                {
                    string sdaNum = dataItem.sdaNumber;
                    string status = dataItem.status;
                    bool hasSda = dataItem.hasSda == true;
                    string message = dataItem.message != null
                        ? dataItem.message.ToString() : null;
                    DateTime? sdaDate = dataItem.sdaDate != null
                        ? DateTime.Parse(dataItem.sdaDate.ToString()) : (DateTime?)null;
                    string errors = dataItem.errors != null
                        ? JsonConvert.SerializeObject(dataItem.errors) : null;

                   
                    var responseMsg = CombineMessage(message, errors);

                 
                    foreach (var (oId, eId) in orders)
                    {
                        await InsertSdaTransactionAsync(
                            oId, eId, sdaNum, sdaDate, status, hasSda, responseMsg);
                    }

                    if (hasSda && !string.IsNullOrEmpty(sdaNum))
                        retSda = sdaNum;

                    _logger.Information(
                        $"SdaScheduleTask: CreateSda result — SDA={sdaNum} " +
                        $"status={status} hasSda={hasSda} msg={message}");
                }

                return retSda;
            }
            catch (Exception ex)
            {
                _logger.Error($"SdaScheduleTask: CreateNewSdaAsync: {ex.Message}", ex);
                return null;
            }
        }

      
        private async Task AppendToExistingSdaAsync(
            string sdaNumber, List<(int orderId, long erpId)> orders)
        {
            try
            {
                var url = _config.ApiRoot + "&objType=92&objId=0";
                var payload = new
                {
                    objType = 92,
                    objId = 0,
                    salesOrders = new[]
                    {
                        new
                        {
                            SDANumber   = sdaNumber,
                            erpOrderIds = orders.Select(o => o.erpId).ToArray()
                        }
                    },
                    notes = "RequestAppend SDA from nopCommerce integration"
                };

                var (ok, content) = await CallApiAsync(url, payload);
                if (!ok)
                    return;

                dynamic result = JsonConvert.DeserializeObject(content);

              
                if (result.data == null || result.data.Count == 0)
                {
                    _logger.Warning(
                        $"SdaScheduleTask: AppendSda {sdaNumber} — no data: {content}");
                    return;
                }

                foreach (var dataItem in result.data)
                {
                    string retSda = dataItem.sdaNumber ?? sdaNumber;
                    string status = dataItem.status;
                    bool hasSda = dataItem.hasSda == true ||
                                     (status != null &&
                                      status.Equals("Appended",
                                          StringComparison.OrdinalIgnoreCase));
                    string message = dataItem.message != null
                        ? dataItem.message.ToString() : null;
                    DateTime? sdaDate = dataItem.sdaDate != null
                        ? DateTime.Parse(dataItem.sdaDate.ToString()) : (DateTime?)null;
                    string errors = dataItem.errors != null
                        ? JsonConvert.SerializeObject(dataItem.errors) : null;

                    var responseMsg = CombineMessage(message, errors);

                    if (!hasSda)
                    {
                       
                        _logger.Warning(
                            $"SdaScheduleTask: AppendSda {sdaNumber} REJECTED by ERP: {message}");
                    }
                    else
                    {
                        _logger.Information(
                            $"SdaScheduleTask: AppendSda {sdaNumber} SUCCESS. " +
                            $"Orders [{string.Join(", ", orders.Select(o => o.orderId))}]");
                    }

                    foreach (var (oId, eId) in orders)
                        await InsertSdaTransactionAsync(
                            oId, eId, retSda, sdaDate, status, hasSda, responseMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"SdaScheduleTask: AppendToExistingSdaAsync: {ex.Message}", ex);
            }
        }

        
        private async Task<(bool ok, string content)> CallApiAsync(string url, object payload)
        {
            var req = new RestRequest(url, Method.POST);
            req.AddHeader("Content-Type", "application/json");
           // req.AddStringBody(JsonConvert.SerializeObject(payload), DataFormat.Json);
            req.AddJsonBody(payload);
            var resp = await _restClient.ExecuteAsync(req);
            _logger.Information(
                $"SdaScheduleTask: API → {resp.StatusCode}: {resp.Content}");
            if (!resp.IsSuccessful || string.IsNullOrEmpty(resp.Content))
            {
                _logger.Error($"SdaScheduleTask: API failed {resp.StatusCode}: {resp.Content}");
                return (false, resp.Content);
            }
            return (true, resp.Content);
        }

       
        private async Task<List<(int orderId, long erpId)>> GetPendingOrdersForGroupAsync(
            int groupId)
        {
            var rows = await _dataProvider.QueryAsync<GroupOrderRow>(@"
                SELECT go.[OrderId], go.[ErpOrderId]
                FROM   [dbo].[NS_ShipmentGroupOrder] go
                WHERE  go.[ShipmentGroupId] = @Grp
                AND    NOT EXISTS (
                    SELECT 1 FROM [dbo].[NS_SdaTransaction] t
                    WHERE  t.[OrderId] = go.[OrderId] AND t.[HasSda] = 1
                )",
                new DataParameter("@Grp", groupId));

            return rows.Select(r => (r.OrderId, r.ErpOrderId)).ToList();
        }

        private async Task InsertSdaTransactionAsync(
            int orderId, long erpId, string sdaNum, DateTime? sdaDate,
            string status, bool hasSda, string responseMsg)
        {
            await _dataProvider.ExecuteNonQueryAsync(@"
                INSERT INTO [dbo].[NS_SdaTransaction]
                    ([OrderId],[ErpOrderId],[SdaNumber],[SdaDate],[Status],
                     [HasSda],[ResponseMessage],[CreatedOnUtc])
                VALUES (@O,@E,@S,@D,@St,@H,@M,@C)",
                new DataParameter("@O", orderId),
                new DataParameter("@E", erpId),
                new DataParameter("@S", (object)sdaNum ?? DBNull.Value),
                new DataParameter("@D", (object)sdaDate ?? DBNull.Value),
                new DataParameter("@St", (object)status ?? DBNull.Value),
                new DataParameter("@H", hasSda),
                new DataParameter("@M", (object)responseMsg ?? DBNull.Value),
                new DataParameter("@C", DateTime.UtcNow));
        }

        private async Task LockGroupsReachingWindowAsync(DateTime now)
        {
            await _dataProvider.ExecuteNonQueryAsync(@"
                UPDATE [dbo].[NS_ShipmentGroup]
                SET    [IsLocked]=1,[Status]='Locked',[UpdatedOnUtc]=GETUTCDATE()
                WHERE  [Status]='Open'
                AND    [RequestedShipDate] <= @Cut",
                new DataParameter("@Cut", now.Date.AddDays(7)));
        }

        private async Task<ShipmentGroupRow> GetCustomerActiveSdaGroupAsync(
            int customerId, DateTime now)
        {
            var rows = await _dataProvider.QueryAsync<ShipmentGroupRow>(@"
                SELECT TOP 1
                       [Id]               AS GroupId,
                       [CustomerId],
                       [RequestedShipDate],
                       [Status],
                       [SdaNumber],
                       [IsLocked]
                FROM   [dbo].[NS_ShipmentGroup]
                WHERE  [CustomerId] = @Cust
                AND    [SdaNumber]  IS NOT NULL
                AND    [Status]     = 'SdaCreated'
                AND    (
                    [RequestedShipDate] <= @Cut7
                    OR [SdaCreatedOnUtc] >= @Cut7Ago
                )
                ORDER BY [SdaCreatedOnUtc] DESC",
                new DataParameter("@Cust", customerId),
                new DataParameter("@Cut7", now.Date.AddDays(7)),
                new DataParameter("@Cut7Ago", now.AddDays(-7)));
            return rows.FirstOrDefault();
        }

        private async Task<int> GetCustomerAnyGroupAsync(int customerId, DateTime now)
        {
            var r = await _dataProvider.QueryAsync<int>(@"
                SELECT TOP 1 [Id]
                FROM   [dbo].[NS_ShipmentGroup]
                WHERE  [CustomerId] = @Cust
                AND    [SdaNumber]  IS NULL
                AND    [Status]     IN ('Open','Locked')
                AND    [RequestedShipDate] <= @Cut
                ORDER BY [CreatedOnUtc] DESC",
                new DataParameter("@Cust", customerId),
                new DataParameter("@Cut", now.Date.AddDays(30)));
            return r.FirstOrDefault();
        }

        private async Task<int> CreateShipmentGroupAsync(int customerId, DateTime shipDate)
        {
            var r = await _dataProvider.QueryAsync<int>(@"
                INSERT INTO [dbo].[NS_ShipmentGroup]
                    ([CustomerId],[RequestedShipDate],[Status],[IsLocked],
                     [CreatedOnUtc],[UpdatedOnUtc])
                VALUES (@C,@D,'Open',0,GETUTCDATE(),GETUTCDATE());
                SELECT SCOPE_IDENTITY();",
                new DataParameter("@C", customerId),
                new DataParameter("@D", shipDate.Date));
            return r.FirstOrDefault();
        }

        private async Task AddOrderToGroupAsync(int groupId, int orderId, long erpId)
        {
            await _dataProvider.ExecuteNonQueryAsync(@"
                IF NOT EXISTS (
                    SELECT 1 FROM [dbo].[NS_ShipmentGroupOrder]
                    WHERE [ShipmentGroupId]=@G AND [OrderId]=@O
                )
                INSERT INTO [dbo].[NS_ShipmentGroupOrder]
                    ([ShipmentGroupId],[OrderId],[ErpOrderId],[AddedOnUtc])
                VALUES (@G,@O,@E,GETUTCDATE())",
                new DataParameter("@G", groupId),
                new DataParameter("@O", orderId),
                new DataParameter("@E", erpId));
        }

        private async Task LockGroupAsync(int groupId)
        {
            await _dataProvider.ExecuteNonQueryAsync(@"
                UPDATE [dbo].[NS_ShipmentGroup]
                SET    [IsLocked]=1,[Status]='Locked',[UpdatedOnUtc]=GETUTCDATE()
                WHERE  [Id]=@Id AND [IsLocked]=0",
                new DataParameter("@Id", groupId));
        }

        private async Task SaveGroupSdaNumberAsync(int groupId, string sdaNumber)
        {
            await _dataProvider.ExecuteNonQueryAsync(@"
                UPDATE [dbo].[NS_ShipmentGroup]
                SET    [SdaNumber]=@S,[Status]='SdaCreated',
                       [SdaCreatedOnUtc]=GETUTCDATE(),[UpdatedOnUtc]=GETUTCDATE()
                WHERE  [Id]=@Id",
                new DataParameter("@S", sdaNumber),
                new DataParameter("@Id", groupId));
        }

        private async Task<bool> SdaExistsForOrderAsync(int orderId)
        {
            var r = await _dataProvider.QueryAsync<int>(
                "SELECT COUNT(1) FROM [dbo].[NS_SdaTransaction] " +
                "WHERE [OrderId]=@Id AND [HasSda]=1",
                new DataParameter("@Id", orderId));
            return r.FirstOrDefault() > 0;
        }

        private async Task<long> GetErpOrderIdAsync(int orderId)
        {
            var r = await _dataProvider.QueryAsync<long>(
                "SELECT ErpOrderId FROM [dbo].[Order] WHERE Id=@Id",
                new DataParameter("@Id", orderId));
            return r.FirstOrDefault();
        }

        private static string CombineMessage(string message, string errors)
        {
            if (string.IsNullOrEmpty(errors) || errors == "null" || errors == "[]")
                return message;
            if (string.IsNullOrEmpty(message))
                return errors;
            return $"{message} | {errors}";
        }

        private static bool TryParseDate(string raw, out DateTime date)
        {
            date = default;
            if (string.IsNullOrEmpty(raw))
                return false;
            foreach (var fmt in new[] { "dd/MM/yyyy", "dd/mm/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" })
                if (DateTime.TryParseExact(raw, fmt,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out date))
                    return true;
            return DateTime.TryParse(raw, out date);
        }

        private static Dictionary<string, object> DeserializeCustomValues(string xml)
        {
            var r = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(xml))
                return r;
            try
            {
                foreach (var item in XDocument.Parse(xml).Descendants("item"))
                {
                    var k = item.Element("key")?.Value;
                    var v = item.Element("value")?.Value;
                    if (!string.IsNullOrEmpty(k))
                        r[k] = v;
                }
            }
            catch { }
            return r;
        }

      
        private class ShipmentGroupRow
        {
            public int GroupId { get; set; }
            public int CustomerId { get; set; }
            public DateTime RequestedShipDate { get; set; }
            public string Status { get; set; }
            public string SdaNumber { get; set; }
            public bool IsLocked { get; set; }
        }

        private class GroupOrderRow
        {
            public int OrderId { get; set; }
            public long ErpOrderId { get; set; }
        }

        private class CustomerGroupCount
        {
            public int CustomerId { get; set; }
            public int GroupCount { get; set; }
        }
    }








}
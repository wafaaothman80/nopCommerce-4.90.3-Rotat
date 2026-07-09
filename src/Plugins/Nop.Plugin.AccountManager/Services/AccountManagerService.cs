using System.Xml;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using LinqToDB.Data;
using Nop.Data;
using Nop.Plugin.AccountManager.Domain;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Plugin.AccountManager.Models;
using System.Collections.Generic;
//using static SkiaSharp.HarfBuzz.SKShaper;

namespace Nop.Plugin.AccountManager.Services;

/// <summary>
/// Customer service
/// </summary>
public partial class AccountManagerService : IAccountManagerService
{
  

    protected readonly CustomerSettings _customerSettings;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly INopDataProvider _dataProvider;
    protected readonly IRepository<Address> _customerAddressRepository;
    protected readonly IRepository<BlogComment> _blogCommentRepository;
    protected readonly IRepository<Account_Manager> _account_ManagerRepository;
    protected readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;
    protected readonly IRepository<AccountManagerRigionMapping> _AccountManagerRigionMappingRepository;
    protected readonly IRepository<CustomerPassword> _customerPasswordRepository;
    protected readonly IRepository<Nop.Plugin.AccountManager.Domain.Rigion> _rigionRepository;
    protected readonly IRepository<ForumPost> _forumPostRepository;
    protected readonly IRepository<ForumTopic> _forumTopicRepository;
    protected readonly IRepository<GenericAttribute> _gaRepository;
    protected readonly IRepository<NewsComment> _newsCommentRepository;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IRepository<ProductReview> _productReviewRepository;
    protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
    protected readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;
    protected readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
    protected readonly IShortTermCacheManager _shortTermCacheManager;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;
    protected readonly ShoppingCartSettings _shoppingCartSettings;
    protected readonly TaxSettings _taxSettings;



    public AccountManagerService(CustomerSettings customerSettings,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        INopDataProvider dataProvider,
        IRepository<Address> customerAddressRepository,
        IRepository<BlogComment> blogCommentRepository,
        IRepository<Account_Manager> account_ManagerRepository,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<AccountManagerRigionMapping> AccountManagerRigionMappingRepository,
        IRepository<CustomerPassword> customerPasswordRepository,
        IRepository<Nop.Plugin.AccountManager.Domain.Rigion> rigionRepository,
        IRepository<ForumPost> forumPostRepository,
        IRepository<ForumTopic> forumTopicRepository,
        IRepository<GenericAttribute> gaRepository,
        IRepository<NewsComment> newsCommentRepository,
        IRepository<Order> orderRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<PollVotingRecord> pollVotingRecordRepository,
        IRepository<ShoppingCartItem> shoppingCartRepository,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ShoppingCartSettings shoppingCartSettings,
        TaxSettings taxSettings)
    {
        _customerSettings = customerSettings;
        _eventPublisher = eventPublisher;
        _genericAttributeService = genericAttributeService;
        _dataProvider = dataProvider;
        _customerAddressRepository = customerAddressRepository;
        _blogCommentRepository = blogCommentRepository;
        _account_ManagerRepository = account_ManagerRepository;
        _customerAddressMappingRepository = customerAddressMappingRepository;
        _AccountManagerRigionMappingRepository = AccountManagerRigionMappingRepository;
        _customerPasswordRepository = customerPasswordRepository;
        _rigionRepository = rigionRepository;
        _forumPostRepository = forumPostRepository;
        _forumTopicRepository = forumTopicRepository;
        _gaRepository = gaRepository;
        _newsCommentRepository = newsCommentRepository;
        _orderRepository = orderRepository;
        _productReviewRepository = productReviewRepository;
        _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
        _pollVotingRecordRepository = pollVotingRecordRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _shortTermCacheManager = shortTermCacheManager;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _shoppingCartSettings = shoppingCartSettings;
        _taxSettings = taxSettings;
    }



    /// <summary>
    /// Gets a dictionary of all customer roles mapped by ID.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation and contains a dictionary of all customer roles mapped by ID.
    /// </returns>
    protected virtual async Task<IDictionary<int, Nop.Plugin.AccountManager.Domain.Rigion>> GetAllAccountManagerRigionDictionaryAsync()
    {
        return await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<Nop.Plugin.AccountManager.Domain.Rigion>.AllCacheKey),
            async () => await _rigionRepository.Table.ToDictionaryAsync(cr => cr.Id));
    }



    


    public virtual async Task PrepareModelAccountManagerRiginsAsync<TModel>(TModel model) where TModel : ISupportedModel
    {
        ArgumentNullException.ThrowIfNull(model);

        //prepare available customer roles
        var availableRoles =  GetAllRigionsAsync(showHidden: true);
        model.AvailableAccountManagerRigions = availableRoles.Select(rigin => new SelectListItem
        {
            Text =rigin.RigionName,
            Value = rigin.Id.ToString(),
            Selected = model.SelectedRigionIds.Contains(rigin.Id)
        }).ToList();
    }

  


    public virtual async Task<IPagedList<Account_Manager>> GetAllAccountManagersAsync(int[] rigionIds = null,
        string email = null, string AccountManagerName = null, string phone = null,

        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
    {
        var AccountManagers = await _account_ManagerRepository.GetAllPagedAsync(query =>
        {
           

            query = query.Where(c => !c.Deleted);

            if (rigionIds != null && rigionIds.Length > 0)
            {
                query = query.Join(_AccountManagerRigionMappingRepository.Table, x => x.Id, y => y.AccountManagerId,
                        (x, y) => new { AccountManager = x, Mapping = y })
                    .Where(z => rigionIds.Contains(z.Mapping.RigionId))
                    .Select(z => z.AccountManager)
                    .Distinct();
            }

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(AccountManagerName))
                query = query.Where(c => c.AccountManagerName.Contains(AccountManagerName));
            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(c => c.Phone.Contains(phone));
           

           

            

            query = query.OrderByDescending(c => c.Id);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return AccountManagers;
    }




    public virtual async Task DeleteAccountManagerAsync(Account_Manager account_Manager)
    {
        ArgumentNullException.ThrowIfNull(account_Manager);



        account_Manager.Deleted = true;

       

        await _account_ManagerRepository.UpdateAsync(account_Manager, false);
        await _account_ManagerRepository.DeleteAsync(account_Manager);
    }

   
    public virtual async Task<Account_Manager> GetAccountManagerByIdAsync(int account_ManagerId)
    {
        return await _account_ManagerRepository.GetByIdAsync(account_ManagerId, cache => default, useShortTermCache: true);
    }

    
    public virtual async Task<IList<Account_Manager>> GetAccountManagersByIdsAsync(int[] accountManagerIds)
    {
        return await _account_ManagerRepository.GetByIdsAsync(accountManagerIds, includeDeleted: false);
    }

    public virtual async Task InsertAccountManagerAsync(Account_Manager account_Manager)
    {
        await _account_ManagerRepository.InsertAsync(account_Manager);
    }

    public virtual async Task UpdateAccountManagerAsync(Account_Manager account_Manager)
    {
        await _account_ManagerRepository.UpdateAsync(account_Manager);
    }

   
 

    #region manager region

    public async Task AddrigionMappingAsync(AccountManagerRigionMapping roleMapping)
    {
        await _AccountManagerRigionMappingRepository.InsertAsync(roleMapping);
    }

   
    public async Task RemoverigionMappingAsync(Account_Manager accountManager, Nop.Plugin.AccountManager.Domain.Rigion rigion)
    {
        ArgumentNullException.ThrowIfNull(accountManager);

        ArgumentNullException.ThrowIfNull(rigion);

        var mapping = await _AccountManagerRigionMappingRepository.Table
            .SingleOrDefaultAsync(ccrm => ccrm.AccountManagerId == accountManager.Id && ccrm.RigionId == rigion.Id);

        if (mapping != null)
            await _AccountManagerRigionMappingRepository.DeleteAsync(mapping);
    }
  



    public virtual async Task <List<int>> GetRigionIdsAsync(Account_Manager account_Manager, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(account_Manager);

        return (await GetRigionsAsync(account_Manager, showHidden: showHidden))
            .Select(cr => cr.Id).ToList();
            //.ToArray();
    }

    public virtual async Task<IList<Nop.Plugin.AccountManager.Domain.Rigion>> GetRigionsAsync(Account_Manager account_Manager, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(account_Manager);

        var allRolesById = await GetAllRigionsDictionaryAsync();

        var mappings =  await _AccountManagerRigionMappingRepository.GetAllAsync(query => query.Where(crm => crm.AccountManagerId == account_Manager.Id));

        return mappings.Select(mapping => allRolesById.TryGetValue(mapping.RigionId, out var rigion) ? rigion : null)
            .Where(cr => cr != null && (showHidden || cr.Active))
            .ToList();
    }

    public virtual async Task<int[]> GetAccountManagerRigionsIdsAsync(Account_Manager account_Manager, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(account_Manager);

        return (await GetAccountManagerRigionsAsync(account_Manager, showHidden: showHidden))
            .Select(cr => cr.Id)
            .ToArray();
    }


    /// </returns>
    public virtual async Task<IList<Nop.Plugin.AccountManager.Domain.Rigion>> GetAccountManagerRigionsAsync(Account_Manager account_Manager, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(account_Manager);

        var allRigionsById = await GetAllRigionsDictionaryAsync();

        var mappings =  await _AccountManagerRigionMappingRepository.GetAllAsync(query => query.Where(crm => crm.AccountManagerId == account_Manager.Id));

        return mappings.Select(mapping => allRigionsById.TryGetValue(mapping.RigionId, out var rigion) ? rigion : null)
            .Where(cr => cr != null && (showHidden || cr.Active))
            .ToList();
    }

    protected virtual async Task<IDictionary<int, Nop.Plugin.AccountManager.Domain.Rigion>> GetAllRigionsDictionaryAsync()
    {
        return await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<Nop.Plugin.AccountManager.Domain.Rigion>.AllCacheKey),
            async () => await _rigionRepository.Table.ToDictionaryAsync(cr => cr.Id));
    }



    //public async Task RemoverigionMappingAsync(Account_Manager accountManager, Rigion rigion)
    //{
    //    ArgumentNullException.ThrowIfNull(accountManager);

    //    ArgumentNullException.ThrowIfNull(rigion);

    //    var mapping = await _AccountManagerRigionMappingRepository.Table
    //        .SingleOrDefaultAsync(ccrm => ccrm.AccountManagerId == accountManager.Id && ccrm.RigionId == rigion.Id);

    //    if (mapping != null)
    //        await _AccountManagerRigionMappingRepository.DeleteAsync(mapping);
    //}
    public  Task RemoveRigionMappingAsync(Account_Manager accountManager, Nop.Plugin.AccountManager.Domain.Rigion rigion)
    {
        ArgumentNullException.ThrowIfNull(accountManager);

        ArgumentNullException.ThrowIfNull(rigion);

        var mapping =  _AccountManagerRigionMappingRepository.Table
            .SingleOrDefaultAsync(ccrm => ccrm.AccountManagerId == accountManager.Id && ccrm.RigionId == rigion.Id);

        if (mapping != null)
             _AccountManagerRigionMappingRepository.DeleteAsync(mapping.Result);
        return Task.CompletedTask;
    }



    public Task DeleteRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion) => _rigionRepository.DeleteAsync(rigion);


    //public virtual async Task DeleterigionAsync(Rigion rigion)
    //{
    //    ArgumentNullException.ThrowIfNull(rigion);



    //    await _rigionRepository.DeleteAsync(rigion);
    //}

    //public virtual async Task<Rigion> GetrigionByIdAsync(int rigionId)
    //{
    //    var allRolesById = await GetAllRigionsDictionaryAsync();

    //    return allRolesById.TryGetValue(rigionId, out var rigion) ? rigion : null;
    //}
    public async Task<Nop.Plugin.AccountManager.Domain.Rigion> GetRigionByIdAsync(int rigionId)
    {
        var allRolesById =  GetAllRigionsDictionaryAsync();

        // return (Task<Domain.Rigion>)allRolesById.Result.Where(x=>x.Key== rigionId);

        Nop.Plugin.AccountManager.Domain.Rigion rigion =   allRolesById.Result.FirstOrDefault(x=>x.Key== rigionId).Value;
        return rigion;

       




    }

    // public virtual async Task<IList<Rigion>> GetAllrigionsAsync(bool showHidden = false)
    //{
    //    var allRigionsById = await GetAllRigionsDictionaryAsync();

    //    return allRigionsById.Values
    //        .Where(cr => showHidden || cr.Active)
    //        .ToList();
    //}


    public IList<Nop.Plugin.AccountManager.Domain.Rigion> GetAllRigionsAsync(bool showHidden = false)
    {
        IList<Nop.Plugin.AccountManager.Domain.Rigion> allRigionsById = GetAllRigionsDictionaryAsync().Result.Values.Where(x => x.Active == true).ToList();

        return allRigionsById;
    }
    public Task InsertRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion) => _rigionRepository.InsertAsync(rigion);





    public Task UpdateRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion) => _rigionRepository.UpdateAsync(rigion);



    public virtual async Task<Account_Manager> GetAccountManagerByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var query = from c in _account_ManagerRepository.Table
                    orderby c.Id
                    where c.Email == email
                    select c;
        var _accountManager = await query.FirstOrDefaultAsync();

        return _accountManager;
    }

    public virtual async Task UpsertAccountManagerCustomerMappingByERPCustomerIdAsync(int erpCustomerId, int accountManagerPluginId)
    {
        if (erpCustomerId <= 0 || accountManagerPluginId <= 0)
            return;

        // Find the nopCommerce customer by ERPCustomerId,
        // then insert into AccountManager_CustomerMapping using Account_Manager.Id as AccountManagerId
        await _dataProvider.ExecuteNonQueryAsync(@"
            DECLARE @CustomerId INT = (
                SELECT TOP 1 Id FROM Customer
                WHERE ERPCustomerId = @ERPCustomerId AND Deleted = 0
            );
            IF @CustomerId IS NOT NULL
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM AccountManager_CustomerMapping
                    WHERE Customer_Id = @CustomerId AND AccountManagerId = @AccountManagerId
                )
                BEGIN
                    INSERT INTO AccountManager_CustomerMapping (Customer_Id, AccountManagerId, IsPrimary)
                    VALUES (@CustomerId, @AccountManagerId, 0);
                END
            END",
            new DataParameter("@ERPCustomerId", erpCustomerId),
            new DataParameter("@AccountManagerId", accountManagerPluginId));
    }




    #endregion






}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Nop.Data;
using NopStation.Plugin.Misc.AlgoliaSearch.Domains;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Services
{
    public class SubstitutesService : ISubstitutesService
    {
        private readonly INopDataProvider _dataProvider;
    

        public SubstitutesService(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
          
        }

        public async Task<int> GetTotalCountAsync()
        {
            var sql = @"SELECT COUNT(*) AS Total
                        FROM [rotat].[rotat-usr].[substitutesNonProduct]";

            var rows = await _dataProvider.QueryAsync<dynamic>(sql);
            foreach (var r in rows)
                return Convert.ToInt32(r.Total);

            return 0;
        }

        public async Task<IList<SubstituteRow>> GetSubstitutesBatchAsync(int pageIndex, int pageSize)
        {
            var sql = @"
SELECT ProductId, substitutesId, stock_code, substitute_code, substitute_type, description
FROM [rotat].[rotat-usr].[substitutesNonProduct]
ORDER BY substitutesId
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

            var rows = await _dataProvider.QueryAsync<dynamic>(
                sql,
                new DataParameter("@offset", pageIndex * pageSize),
                new DataParameter("@pageSize", pageSize)
            );

            var list = new List<SubstituteRow>();
            foreach (var r in rows)
            {
                list.Add(new SubstituteRow
                {
                    ProductId = Convert.ToInt32(r.ProductId),
                    SubstitutesId = Convert.ToInt32(r.substitutesId),
                    StockCode = r.stock_code?.ToString(),
                    SubstituteCode = r.substitute_code?.ToString(),
                    SubstituteType = r.substitute_type?.ToString(),
                    Description = r.description?.ToString()
                });
            }

            return list;
        }

        public async Task<IList<SubstituteRow>> SearchSubstitutesAsync(string term, int pageIndex, int pageSize)
        {
            term ??= "";

            var sql = @"
SELECT ProductId, substitutesId, stock_code, substitute_code, substitute_type, description
FROM [rotat].[rotat-usr].[substitutesNonProduct]
WHERE stock_code LIKE @p
   OR substitute_code LIKE @p
   OR description LIKE @p
ORDER BY substitutesId
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

            var rows = await _dataProvider.QueryAsync<dynamic>(
                sql,
                new DataParameter("@p", $"%{term}%"),
                new DataParameter("@offset", pageIndex * pageSize),
                new DataParameter("@pageSize", pageSize)
            );

            var list = new List<SubstituteRow>();
            foreach (var r in rows)
            {
                list.Add(new SubstituteRow
                {
                    ProductId = Convert.ToInt32(r.ProductId),
                    SubstitutesId = Convert.ToInt32(r.substitutesId),
                    StockCode = r.stock_code?.ToString(),
                    SubstituteCode = r.substitute_code?.ToString(),
                    SubstituteType = r.substitute_type?.ToString(),
                    Description = r.description?.ToString()
                });
            }

            return list;
        }

       
        public async Task<IList<SubstituteRow>> GetSubstitutesByIdsAsync(IList<int> substituteIds)
        {
            if (substituteIds == null || substituteIds.Count == 0)
                return new List<SubstituteRow>();

           
            var parameters = new List<DataParameter>();
            var inParts = new List<string>();

            for (var i = 0; i < substituteIds.Count; i++)
            {
                var name = "@id" + i;
                inParts.Add(name);
                parameters.Add(new DataParameter(name, substituteIds[i]));
            }

            var sql = $@"
SELECT ProductId, substitutesId, stock_code, substitute_code, substitute_type, description
FROM [rotat].[rotat-usr].[substitutesNonProduct]
WHERE substitutesId IN ({string.Join(",", inParts)});";

            var rows = await _dataProvider.QueryAsync<dynamic>(sql, parameters.ToArray());

            var list = new List<SubstituteRow>();
            foreach (var r in rows)
            {
                list.Add(new SubstituteRow
                {
                    ProductId = Convert.ToInt32(r.ProductId),
                    SubstitutesId = Convert.ToInt32(r.substitutesId),
                    StockCode = r.stock_code?.ToString(),
                    SubstituteCode = r.substitute_code?.ToString(),
                    SubstituteType = r.substitute_type?.ToString(),
                    Description = r.description?.ToString()
                });
            }

            return list;
        }


      
    }






}


using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Services
{
    public interface ISubstitutesService
    {
        Task<IList<SubstituteRow>> SearchSubstitutesAsync(string term, int pageIndex, int pageSize);
        Task<IList<SubstituteRow>> GetSubstitutesBatchAsync(int pageIndex, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<IList<SubstituteRow>> GetSubstitutesByIdsAsync(IList<int> substituteIds);
      

       

    }

    public class SubstituteRow
    {
        public int ProductId { get; set; }
        public int SubstitutesId { get; set; }
        public string StockCode { get; set; }
        public string SubstituteCode { get; set; }
        public string Description { get; set; }
        public string SubstituteType { get; set; }
    }
}

using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Recommendations.SimilarProducts.Models.Admin
{
    public class SimilarityCreateModel
    {
        public int ProductId { get; set; }
        public int[] SimilarProductIds { get; set; } = Array.Empty<int>();
        public int SimilarityPercent { get; set; } // 0..100 في الواجهة
    }
}

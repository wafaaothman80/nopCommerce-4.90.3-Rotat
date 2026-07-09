using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Recommendations.SimilarProducts.Models.Admin
{
    public class SimilarityListItemModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // للعرض
        public int SimilarProductId { get; set; }
        public string SimilarProductName { get; set; } // للعرض
        public int SimilarityPercent { get; set; } // (Similarity * 100) rounded
        public DateTime CreatedOnUtc { get; set; }
    }
}

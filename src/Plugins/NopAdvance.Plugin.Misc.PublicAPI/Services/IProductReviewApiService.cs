using Nop.Core.Domain.Catalog;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public interface IProductReviewApiService
{
    Task<bool> CanAddReviewAsync(int productId, int customerId, int storeId);
    Task<ProductReview?> GetProductReviewByIdAsync(int productReviewId);

    Task InsertProductReviewAsync(ProductReview productReview);

    Task SetProductReviewHelpfulnessAsync(ProductReview productReview, int customerId, bool wasHelpful);
    Task UpdateProductReviewHelpfulnessTotalsAsync(ProductReview productReview);

    Task UpdateProductReviewTotalsAsync(Product product);
}

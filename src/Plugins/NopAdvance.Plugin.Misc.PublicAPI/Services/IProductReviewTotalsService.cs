using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public interface IProductReviewTotalsService
{
    Task UpdateProductReviewTotalsAsync(Product product);
    Task UpdateProductReviewHelpfulnessTotalsAsync(ProductReview productReview);
}

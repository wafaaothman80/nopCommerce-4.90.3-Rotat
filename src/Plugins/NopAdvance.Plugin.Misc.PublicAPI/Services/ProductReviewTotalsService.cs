using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public class ProductReviewTotalsService : IProductReviewTotalsService
{
    private readonly IRepository<ProductReview> _productReviewRepository;
    private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
    private readonly IProductService _productService;

    public ProductReviewTotalsService(
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IProductService productService)
    {
        _productReviewRepository = productReviewRepository;
        _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
        _productService = productService;
    }

    public virtual async Task UpdateProductReviewTotalsAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        
        var approved = await _productReviewRepository.Table
            .Where(r => r.ProductId == product.Id && r.IsApproved)
            .ToListAsync();

        var notApproved = await _productReviewRepository.Table
            .Where(r => r.ProductId == product.Id && !r.IsApproved)
            .ToListAsync();

        product.ApprovedTotalReviews = approved.Count;
        product.NotApprovedTotalReviews = notApproved.Count;

        product.ApprovedRatingSum = approved.Sum(x => x.Rating);
        product.NotApprovedRatingSum = notApproved.Sum(x => x.Rating);

        await _productService.UpdateProductAsync(product);
    }

    public virtual async Task UpdateProductReviewHelpfulnessTotalsAsync(ProductReview productReview)
    {
        ArgumentNullException.ThrowIfNull(productReview);

      
        var helpfulYes = await _productReviewHelpfulnessRepository.Table
            .Where(h => h.ProductReviewId == productReview.Id && h.WasHelpful)
            .CountAsync();

        var helpfulNo = await _productReviewHelpfulnessRepository.Table
            .Where(h => h.ProductReviewId == productReview.Id && !h.WasHelpful)
            .CountAsync();

        productReview.HelpfulYesTotal = helpfulYes;
        productReview.HelpfulNoTotal = helpfulNo;

        await _productReviewRepository.UpdateAsync(productReview);
    }
}

using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;

namespace NopAdvance.Plugin.Misc.PublicAPI.Services;

public class ProductReviewApiService : IProductReviewApiService
{
    private readonly IRepository<ProductReview> _productReviewRepository;
    private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
    private readonly IProductService _productService;

    public ProductReviewApiService(
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IProductService productService)
    {
        _productReviewRepository = productReviewRepository;
        _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
        _productService = productService;
    }

    public async Task<bool> CanAddReviewAsync(int productId, int customerId, int storeId)
    {
        
        var alreadyReviewed = await _productReviewRepository.Table
            .Where(r => r.ProductId == productId && r.CustomerId == customerId && r.StoreId == storeId)
            .AnyAsync();

        return !alreadyReviewed;
    }

    public async Task<ProductReview?> GetProductReviewByIdAsync(int productReviewId)
    {
        return await _productReviewRepository.GetByIdAsync(productReviewId);
    }

    public async Task InsertProductReviewAsync(ProductReview productReview)
    {
        ArgumentNullException.ThrowIfNull(productReview);
        await _productReviewRepository.InsertAsync(productReview);
    }

    public async Task SetProductReviewHelpfulnessAsync(ProductReview productReview, int customerId, bool wasHelpful)
    {
        ArgumentNullException.ThrowIfNull(productReview);

        // one vote per customer per review: insert or update
        var existing = await _productReviewHelpfulnessRepository.Table
            .FirstOrDefaultAsync(x => x.ProductReviewId == productReview.Id && x.CustomerId == customerId);

        if (existing == null)
        {
            existing = new ProductReviewHelpfulness
            {
                ProductReviewId = productReview.Id,
                CustomerId = customerId,
                WasHelpful = wasHelpful
            };
            await _productReviewHelpfulnessRepository.InsertAsync(existing);
        }
        else
        {
            existing.WasHelpful = wasHelpful;
            await _productReviewHelpfulnessRepository.UpdateAsync(existing);
        }
    }

    public async Task UpdateProductReviewHelpfulnessTotalsAsync(ProductReview productReview)
    {
        ArgumentNullException.ThrowIfNull(productReview);

        var yes = await _productReviewHelpfulnessRepository.Table
            .Where(x => x.ProductReviewId == productReview.Id && x.WasHelpful)
            .CountAsync();

        var no = await _productReviewHelpfulnessRepository.Table
            .Where(x => x.ProductReviewId == productReview.Id && !x.WasHelpful)
            .CountAsync();

        productReview.HelpfulYesTotal = yes;
        productReview.HelpfulNoTotal = no;

        await _productReviewRepository.UpdateAsync(productReview);
    }

    public async Task UpdateProductReviewTotalsAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var approvedRatingSum = 0;
        var notApprovedRatingSum = 0;
        var approvedTotalReviews = 0;
        var notApprovedTotalReviews = 0;

        var reviews = _productReviewRepository.Table
            .Where(r => r.ProductId == product.Id)
            .ToAsyncEnumerable();

        await foreach (var pr in reviews)
        {
            if (pr.IsApproved)
            {
                approvedRatingSum += pr.Rating;
                approvedTotalReviews++;
            }
            else
            {
                notApprovedRatingSum += pr.Rating;
                notApprovedTotalReviews++;
            }
        }

        product.ApprovedRatingSum = approvedRatingSum;
        product.NotApprovedRatingSum = notApprovedRatingSum;
        product.ApprovedTotalReviews = approvedTotalReviews;
        product.NotApprovedTotalReviews = notApprovedTotalReviews;

        // This method exists in IProductService in basically all versions
        await _productService.UpdateProductAsync(product);
    }
}

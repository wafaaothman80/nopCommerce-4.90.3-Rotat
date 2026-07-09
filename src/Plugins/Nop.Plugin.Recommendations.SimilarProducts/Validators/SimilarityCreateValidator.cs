using FluentValidation;
using Nop.Plugin.Recommendations.SimilarProducts.Models.Admin;

namespace Nop.Plugin.Recommendations.SimilarProducts.Validators
{
    public class SimilarityCreateValidator : AbstractValidator<SimilarityCreateModel>
    {
        public SimilarityCreateValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product is required");

            RuleFor(x => x.SimilarProductIds)
               .NotNull().WithMessage("Similar products are required")
               .Must(list => list != null && list.Any()).WithMessage("Select at least one similar product");
            ;

            RuleFor(x => x.SimilarityPercent)
                .InclusiveBetween(0, 100).WithMessage("Similarity must be between 0 and 100");
        }
    }
}

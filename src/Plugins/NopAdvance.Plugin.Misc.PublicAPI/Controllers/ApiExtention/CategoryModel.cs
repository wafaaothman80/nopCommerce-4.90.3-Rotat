using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers;

public partial record CategoryApiModel : BaseNopEntityModel
{
    public CategoryApiModel()
    {
        PictureModel = new PictureModel();
        FeaturedProducts = new List<ProductApiOverviewModel>();
        SubCategories = new List<SubCategoryModel>();
        CategoryBreadcrumb = new List<CategoryApiModel>();
        CatalogProductsModel = new CatalogProductsModel();
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public string MetaKeywords { get; set; }
    public string MetaDescription { get; set; }
    public string MetaTitle { get; set; }
    public string SeName { get; set; }

    public PictureModel PictureModel { get; set; }

    public bool DisplayCategoryBreadcrumb { get; set; }
    public IList<CategoryApiModel> CategoryBreadcrumb { get; set; }

    public IList<SubCategoryModel> SubCategories { get; set; }

    public IList<ProductApiOverviewModel> FeaturedProducts { get; set; }

    public CatalogProductsModel CatalogProductsModel { get; set; }

    public string JsonLd { get; set; }

    #region Nested Classes

    public partial record SubCategoryModel : BaseNopEntityModel
    {
        public SubCategoryModel()
        {
            PictureModel = new PictureModel();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public string Description { get; set; }

        public PictureModel PictureModel { get; set; }
    }

    #endregion
}
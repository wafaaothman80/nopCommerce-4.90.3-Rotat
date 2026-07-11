using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;

namespace Nop.Web.Areas.Admin.Controllers;

/// <summary>
/// Serves the Rotat customization documentation (markdown files stored in App_Data/Documentation)
/// to administrators via the admin panel (Help > Rotat Customizations)
/// </summary>
public partial class RotatDocsController : BaseAdminController
{
    protected readonly INopFileProvider _fileProvider;

    public RotatDocsController(INopFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public virtual IActionResult Index(string file = "README.md")
    {
        var docsPath = _fileProvider.MapPath("~/App_Data/Documentation");

        if (!_fileProvider.DirectoryExists(docsPath))
            return Content("Documentation folder not found: App_Data/Documentation");

        var files = _fileProvider.GetFiles(docsPath, "*.md")
            .Select(f => _fileProvider.GetFileName(f))
            .OrderBy(f => f)
            .ToList();

        //only allow files that actually exist in the documentation folder (no path traversal)
        var selected = files.FirstOrDefault(f => f.Equals(file, StringComparison.OrdinalIgnoreCase))
                       ?? files.FirstOrDefault(f => f.Equals("README.md", StringComparison.OrdinalIgnoreCase))
                       ?? files.FirstOrDefault();

        ViewBag.Files = files;
        ViewBag.SelectedFile = selected;
        ViewBag.Content = selected != null
            ? _fileProvider.ReadAllText(_fileProvider.Combine(docsPath, selected), System.Text.Encoding.UTF8)
            : string.Empty;

        return View();
    }
}

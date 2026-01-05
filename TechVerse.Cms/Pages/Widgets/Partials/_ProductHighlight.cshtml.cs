using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.ContentManagement;

namespace TechVerse.Cms.Pages.Widgets.Partials
{
    public class _ProductHighlightModel : PageModel
    {
        public ContentItem ContentItem { get; set; }
    }
}

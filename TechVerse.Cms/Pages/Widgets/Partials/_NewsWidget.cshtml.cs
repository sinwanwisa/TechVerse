using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.ContentManagement;

namespace TechVerse.Cms.Pages.Widgets.Partials
{
    public class _NewsWidgetModel : PageModel
    {
        public ContentItem ContentItem { get; set; }

    }
}

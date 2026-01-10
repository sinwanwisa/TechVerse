using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.Search.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechVerse.Cms.Pages.Search
{
    public class IndexModel : PageModel
    {
        private readonly ISearchService _searchService;
        private readonly IOrchardHelper _orchard;

        public IndexModel(ISearchService searchService, IOrchardHelper orchard)
        {
            _searchService = searchService;
            _orchard = orchard;
        }

        // ใช้ BindProperty เพื่อให้ค่าจาก Form ผูกกับตัวแปรนี้อัตโนมัติ
        [BindProperty(SupportsGet = true)]
        public string Terms { get; set; }

        public IEnumerable<ContentItem> Results { get; set; } = Enumerable.Empty<ContentItem>();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Terms))
            {
                // "Search" คือชื่อ Index ที่สร้างในหน้า Admin (Lucene/Elastic)
                var searchResult = await _searchService.SearchAsync(" SearchAllIndex", Terms, start: 0, size: 20);

                if (searchResult.Success)
                {
                    // แปลง IDs ที่ได้จาก Search Index เป็น ContentItem จริงๆ
                    Results = await _orchard.GetContentItemsByIdAsync(searchResult.ContentItemIds);
                }
            }
        }
    }
}
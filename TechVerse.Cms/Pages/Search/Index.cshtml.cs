using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechVerse.Cms.Pages.Search
{
    public class IndexModel : PageModel
    {
        private readonly ISearchService _searchService;
        private readonly IOrchardHelper _orchard;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public IndexModel(
            ISearchService searchService,
            IOrchardHelper orchard,
            IContentDefinitionManager contentDefinitionManager,
            LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _searchService = searchService;
            _orchard = orchard;
            _contentDefinitionManager = contentDefinitionManager;
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Terms { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedTypes { get; set; } = new();

        public IEnumerable<ContentItem> Results { get; set; } = Enumerable.Empty<ContentItem>();
        public List<ContentTypeDisplay> AvailableTypes { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1. โหลด Content Types ที่ถูกเลือกไว้ใน Lucene Index
            var settings = await _luceneIndexSettingsService.GetSettingsAsync();
            var indexSettings = settings.FirstOrDefault(x => x.IndexName == "SearchAllIndex");

            if (indexSettings?.IndexedContentTypes != null)
            {
                var defs = await _contentDefinitionManager.ListTypeDefinitionsAsync();
                AvailableTypes = defs
                    .Where(t => indexSettings.IndexedContentTypes.Contains(t.Name))
                    .Select(t => new ContentTypeDisplay
                    {
                        Name = t.Name,
                        DisplayName = t.DisplayName
                    })
                    .OrderBy(t => t.DisplayName)
                    .ToList();
            }

            // 2. ถ้าไม่มีคำค้นและไม่ได้เลือก Filter ให้แสดงหน้าว่าง หรือคุณอาจจะใส่ logic ให้ดึงเนื้อหาล่าสุดมาโชว์ก็ได้
            if (string.IsNullOrWhiteSpace(Terms) && !SelectedTypes.Any())
                return;

            // 3. เตรียมคำค้น (ถ้า Terms ว่างให้ใช้ "*" เพื่อค้นหาทั้งหมดภายใต้ filter)
            var searchQuery = string.IsNullOrWhiteSpace(Terms) ? "*" : Terms;

            var result = await _searchService.SearchAsync(
                "SearchAllIndex",
                searchQuery,
                start: 0,
                size: 50
            );

            if (result.Success)
            {
                // ดึง ContentItems แบบตัวเต็ม (Full Content) เพื่อให้ได้ข้อมูล AliasPart และ MarkdownBodyPart
                var items = await _orchard.GetContentItemsByIdAsync(result.ContentItemIds);

                // Filter ตามประเภทที่ผู้ใช้เลือกใน Sidebar
                Results = SelectedTypes.Any()
                    ? items.Where(x => SelectedTypes.Contains(x.ContentType))
                    : items;
            }
        }
    }

    public class ContentTypeDisplay
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }
}
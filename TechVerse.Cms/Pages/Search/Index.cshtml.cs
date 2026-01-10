using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene;
using Microsoft.Extensions.DependencyInjection;

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
              LuceneIndexSettingsService luceneIndexSettingsService) // ฉีดเพิ่ม
        {
            _searchService = searchService;
            _orchard = orchard;
            _contentDefinitionManager = contentDefinitionManager;
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        [BindProperty(SupportsGet = true)]
        public string Terms { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedTypes { get; set; } = new List<string>();

        public IEnumerable<ContentItem> Results { get; set; } = Enumerable.Empty<ContentItem>();
        public List<ContentTypeDisplay> AvailableTypes { get; set; } = new List<ContentTypeDisplay>();

        public async Task OnGetAsync()
        {
            // 1. ดึงการตั้งค่าของ Index ทั้งหมด
            var allSettings = await _luceneIndexSettingsService.GetSettingsAsync();

            // ค้นหา Index ที่ชื่อ "SearchAllIndex"
            var indexSettings = allSettings.FirstOrDefault(x => x.IndexName == "SearchAllIndex");

            if (indexSettings != null && indexSettings.IndexedContentTypes != null)
            {
                var indexedTypes = indexSettings.IndexedContentTypes;
                var allDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();

                AvailableTypes = allDefinitions
                    .Where(t => indexedTypes.Contains(t.Name))
                    .Select(t => new ContentTypeDisplay
                    {
                        Name = t.Name,
                        DisplayName = t.DisplayName
                    })
                    .OrderBy(t => t.DisplayName)
                    .ToList();
            }


            // 2. ค้นหาข้อมูล
            if (!string.IsNullOrWhiteSpace(Terms))
            {
                // ตรวจสอบชื่อ Index ใน Admin Dashboard (Search -> Indexes)
                var searchResult = await _searchService.SearchAsync("SearchAllIndex", Terms, start: 0, size: 50);

                if (searchResult.Success)
                {
                    var items = await _orchard.GetContentItemsByIdAsync(searchResult.ContentItemIds);

                    // 3. กรองตามประเภทที่เลือก (Client-side filtering)
                    if (SelectedTypes != null && SelectedTypes.Any())
                    {
                        Results = items.Where(x => SelectedTypes.Contains(x.ContentType));
                    }
                    else
                    {
                        Results = items;
                    }
                }
            }
        }
    }

    public class ContentTypeDisplay
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

}

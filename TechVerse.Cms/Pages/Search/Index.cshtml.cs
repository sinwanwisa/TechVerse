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

            if (string.IsNullOrWhiteSpace(Terms) && !SelectedTypes.Any())
                return;

            var searchQuery = string.IsNullOrWhiteSpace(Terms) ? "*" : Terms;

            var result = await _searchService.SearchAsync(
                "SearchAllIndex",
                searchQuery,
                start: 0,
                size: 50
            );

            if (result.Success)
            {
                var items = await _orchard.GetContentItemsByIdAsync(result.ContentItemIds);

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
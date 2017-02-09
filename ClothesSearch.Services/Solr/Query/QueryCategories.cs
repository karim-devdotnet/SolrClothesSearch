using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClothesSearch.Services.Solr.Documents;
using SolrNet;
using ClothesSearch.Services.DTOs;
using SolrNet.Commands.Parameters;

namespace ClothesSearch.Services.Solr.Query
{
    public class QueryCategories: QueryBase<ClothesDocument>
    {
        private readonly ISolrOperations<ClothesDocument> _solrOperations;

        public QueryCategories(ISolrOperations<ClothesDocument> solrOperations)
        {
            _solrOperations = solrOperations;
        }

        public IEnumerable<AutoSuggestItemDto> Execute(Int32 maxCount, String queryString, Func<string, string> encode, params string[] highlightBraces)
        {
            if (maxCount == 0 || string.IsNullOrEmpty(queryString)) return new List<AutoSuggestItemDto>();

            var queryOptions = new QueryOptions()
            {
                Rows = 0,
                Start = 0,
                Facet = new FacetParameters() 
                {
                    MinCount = 1,
                    Limit = -1,
                    Sort = true
                }
            };

            queryOptions.Facet.Queries = new List<ISolrFacetQuery>() { new SolrFacetFieldQuery("Kategorie") };

            var resultCategory = _solrOperations.Query(string.Format("KategorieSuche:({0})",queryString), queryOptions);

            var result = new List<AutoSuggestItemDto>();
            foreach (var category in resultCategory.FacetFields["Kategorie"])
            {
                if (result.Count == maxCount) break;

                result.Add(new AutoSuggestItemDto()
                {
                    FilterQuery =EncodeAndHighlightKeyWords(category.Key,queryString,encode,highlightBraces),
                    DisplayName = category.Key,
                    RowCount = category.Value,
                    Type = AutoSuggestItemTypes.Category
                });
            }

            return result;
        }
    }
}

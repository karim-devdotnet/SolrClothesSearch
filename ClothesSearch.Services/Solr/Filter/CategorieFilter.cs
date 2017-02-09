using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClothesSearch.Services.Solr.Documents;
using SolrNet;

namespace ClothesSearch.Services.Solr.Filter
{
    public class CategorieFilter:FilterBase<ClothesDocument>
    {
        public List<FilterFacet> FilterFacets{get;set;}
        public String ActiveFilterQuery { get; set; }

        public CategorieFilter()
        {
            FilterFacets = new List<FilterFacet>();
        }

        public override void SetQueryOptions(SolrNet.Commands.Parameters.QueryOptions queryOptions)
        {
            queryOptions.AddFacets(new SolrFacetFieldQuery("Kategorie") 
            {
                MinCount = 1,
                Sort = false
            });

            if (!string.IsNullOrEmpty(ActiveFilterQuery))
            {
                queryOptions.AddFilterQueries(new SolrQuery(string.Format("Kategorie:\"{0}\"",QuoteString(ActiveFilterQuery))));
            }
        }

        public override void GetResult(SolrQueryResults<ClothesDocument> solrResult)
        {
            foreach (var facet in solrResult.FacetFields["Kategorie"])
            {
                if (string.IsNullOrEmpty(ActiveFilterQuery) || ActiveFilterQuery == facet.Key)
                {
                    FilterFacets.Add(new FilterFacet() { 
                    Count = facet.Value,
                    DisplayName = facet.Key,
                    FilterQuery = facet.Key,
                    IsActive = (ActiveFilterQuery == facet.Key)
                    });
                }
            }
        }
    }
}

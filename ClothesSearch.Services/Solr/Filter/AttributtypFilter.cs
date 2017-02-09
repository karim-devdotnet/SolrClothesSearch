using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClothesSearch.Services.Solr.Documents;
using SolrNet;
using System.Text.RegularExpressions;

namespace ClothesSearch.Services.Solr.Filter
{
    public class AttributtypFilter:FilterBase<ClothesDocument>
    {
        public List<FilterFacet> FilterFacets { get; set; }
        public String ActiveFilterQuery { get; set; }

        public AttributtypFilter()
        {
            FilterFacets = new List<FilterFacet>();
        }

        public override void SetQueryOptions(SolrNet.Commands.Parameters.QueryOptions queryOptions)
        {
            queryOptions.AddFacets(new SolrFacetFieldQuery("Attributtyp")
            {
                MinCount = 1,
                Limit = -1,
                Sort = false
            });
        }

        public override void GetResult(SolrQueryResults<ClothesDocument> solrResult)
        {
            try
            {
                foreach (var facet in solrResult.FacetFields["Attributtyp"])
                {
                    FilterFacets.Add(new FilterFacet()
                        {
                            DisplayName = GetDisplayName(facet.Key),
                            FilterQuery = facet.Key,
                            Count = facet.Value,
                        });
                }
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">1¦Farbe</param>
        /// <returns></returns>
        private string GetDisplayName(string input)
        {
            var regex = new Regex(@"(\d*)¦(.*)$",RegexOptions.Compiled);
            var match = regex.Match(input);
            if (!match.Success) return "Fehler!!";
            return match.Groups[2].Value;
        } 
    }
}

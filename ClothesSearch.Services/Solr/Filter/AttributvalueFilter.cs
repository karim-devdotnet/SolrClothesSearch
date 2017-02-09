using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClothesSearch.Services.Solr.Documents;
using SolrNet;
using System.Text.RegularExpressions;

namespace ClothesSearch.Services.Solr.Filter
{
    public class AttributvalueFilter: FilterBase<ClothesDocument>
    {
        public List<FilterFacet> FilterFacets{get;set;}
        public String ActiveFilterQuery { get; set; }
        private const string delimiter = ",";
        private readonly Regex _regexAttributwert = new Regex(@"(\d*)¦(.*)$");

        public AttributvalueFilter()
        {
            FilterFacets = new List<FilterFacet>();
        }

        public override void SetQueryOptions(SolrNet.Commands.Parameters.QueryOptions queryOptions)
        {            
            //Attributwert
            queryOptions.AddFacets(new SolrFacetFieldQuery("Attributwert")
            {
                MinCount = 1,
                Limit = -1,
                Sort = false
            });


            if (!string.IsNullOrEmpty(ActiveFilterQuery))
            {
                var wert = ActiveFilterQuery.TrimStart('(', '(', '"', ',', ':', ':');

                ActiveFilterQuery = BuildAttributFilter(wert);
                queryOptions.AddFilterQueries(new SolrQuery(string.Format("Attributwert:{0}", ActiveFilterQuery))); //.Replace("¦","%A6")
            }
        }

        public override void GetResult(SolrQueryResults<ClothesDocument> solrResult)
        {
            foreach (var facet in solrResult.FacetFields["Attributwert"])
            {
                if (string.IsNullOrEmpty(facet.Key)) continue;
                FilterFacets.Add(new FilterFacet()
                {
                    Count = facet.Value,
                    DisplayName = GetDisplayName(facet.Key),
                    FilterQuery = facet.Key,
                    IsActive = !string.IsNullOrEmpty(ActiveFilterQuery) ? ActiveFilterQuery.Contains(facet.Key) : false
                });
            }
        }

        private string BuildAttributFilter(string input)
        {
            var str = input.Split(new String[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

            string queryString = "";

            foreach (var att in str.GroupBy(x => GetAttributId(x)).ToList())
            {
                queryString = queryString + string.Format("({0}) AND ", string.Join(" OR ", att.Select(x => string.Format("\"{0}\"", x))));
                //queryString = string.Join(", ", att);
            }

            char[] trim = { 'A', 'N', 'D', ' ' };
            queryString = "(" + queryString.TrimEnd(trim) + ")";

            return queryString;
        }

        /// <summary>
        /// liefert die Attribut-Id aus einer zeichenkette zurück.
        /// </summary>
        /// <param name="input">z.B.: 2¦M</param>
        /// <returns>2061</returns>
        private String GetAttributId(string input)
        {
           var match = _regexAttributwert.Match(input);
           if (!match.Success) return null; 
           return match.Groups[1].Value;

        }

        private String GetDisplayName(String input)
        {
            //  2¦M
            string displayName = null;
            var match = _regexAttributwert.Match(input);
            if (match.Success) return match.Groups[2].Value;

            return displayName;
        }
    }
}

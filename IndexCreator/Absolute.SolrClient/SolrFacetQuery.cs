using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Absolute.SolrClient
{
    public class SolrFacetQuery
    {
        public SolrFacetQuery(string fieldName, string prefix, FacetSort sort = FacetSort.Count, int limit = 100, int minCount = 1)
        {
            FieldName = fieldName;
            MinCount = minCount;
            Prefix = prefix;
            Limit = limit;
            Sort = sort;
        }

        public string FieldName { get; private set; }
        public int MinCount { get; private set; }
        public int Limit { get; private set; }
        public string Prefix { get; private set; }
        public FacetSort Sort { get; private set; }

        internal IDictionary<string, string> CreateParams()
        {
            var result = new Dictionary<string, string>();
            result.Add("facet.field", FieldName);
            result.Add(string.Format("f.{0}.facet.mincount", FieldName), MinCount.ToString());
            result.Add(string.Format("f.{0}.facet.limit", FieldName), Limit.ToString());
            result.Add(string.Format("f.{0}.facet.sort", FieldName), Sort.ToString().ToLower());
            if (!string.IsNullOrEmpty(Prefix))
            {
                result.Add(string.Format("f.{0}.facet.prefix", FieldName), Prefix);
            }
            return result;
        }
    }

    public enum FacetSort
    {
        Count,
        Index
    }
}

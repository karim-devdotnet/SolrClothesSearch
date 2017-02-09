using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Absolute.SolrClient
{
    public class SolrFacetResult
    {
        internal SolrFacetResult(SolrFacetQuery facetQuery)
        {
            FacetQuery = facetQuery;
            Counts = new Dictionary<string, int>();
        }

        public SolrFacetQuery FacetQuery { get; private set; }
        public IDictionary<string, int> Counts { get; private set; }
        public string FieldName
        {
            get { return FacetQuery.FieldName; }
        }
    }
}

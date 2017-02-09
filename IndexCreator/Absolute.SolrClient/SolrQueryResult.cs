using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Absolute.SolrClient
{
    public class SolrQueryResult<TDoc> where TDoc : ISolrDocument
    {
        public SolrQueryResult(SolrQueryDescription<TDoc> queryDesc)
        {
            QueryDescription = queryDesc;
            Documents = new LinkedList<TDoc>();
            Facets = new LinkedList<SolrFacetResult>();
            Highlights = new Dictionary<string, IDictionary<string, ICollection<string>>>();
        }

        public SolrQueryDescription<TDoc> QueryDescription { get; private set; }
        public int NumFound { get; internal set; }
        public int StartIndex { get; set; }
        public int QueryTime { get; internal set; }
        public ICollection<TDoc> Documents { get; private set; }
        public ICollection<SolrFacetResult> Facets { get; private set; }
        public IDictionary<string, IDictionary<string, ICollection<string>>> Highlights { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClothesSearch.Services.Solr.Documents;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace ClothesSearch.Services.Solr.Query
{
    public class QueryPhrase:QueryBase<ClothesDocument>
    {
        public string Phrase { get; set; }
        public SolrQueryResults<ClothesDocument> ExecuteQuery(ISolrOperations<ClothesDocument> solrOperations, QueryOptions queryOptions)
        {
            try
            {
                var queryPhrase = string.IsNullOrEmpty(Phrase) ? "*:*" : string.Format("\"{0}\"",QuoteString(Phrase));

                var result = solrOperations.Query(queryPhrase, queryOptions);

                return result;
            }
            catch
            { return null;}
        }
    }
}

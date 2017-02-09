using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet.Commands.Parameters;
using SolrNet;
using System.Text.RegularExpressions;

namespace ClothesSearch.Services.Solr.Filter
{
    public abstract class FilterBase<TDocument>
    {
        public abstract void SetQueryOptions(QueryOptions queryOptions);
        public abstract void GetResult(SolrQueryResults<TDocument> solrResult);

        protected String QuoteString(string value)
        {
            string r = Regex.Replace(value, "(\\+|\\-|\\&\\&|\\|\\||\\!|\\{|\\}|\\[|\\]|\\^|\\(|\\)|\\\"|\\~|\\:|\\;|\\\\)", "\\$1");
            if (r.IndexOf(' ') != -1)
                r = string.Format("{0}", r);
            return r;
        }

    }
}

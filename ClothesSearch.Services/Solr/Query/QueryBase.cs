using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using SolrNet.Attributes;

namespace ClothesSearch.Services.Solr.Query
{
    public abstract class QueryBase<TDocument>
    {
        protected String QuoteString(string value)
        {
            string r = Regex.Replace(value, "(\\+|\\-|\\&\\&|\\|\\||\\!|\\{|\\}|\\[|\\]|\\^|\\(|\\)|\\\"|\\~|\\:|\\;)", "\\$1"); //  |\\\\
            return r;
        }

        protected String GetSolrFileName<TField>(Expression<Func<TField,object>> field)
        {
            MemberExpression expression = field.Body as MemberExpression;

            var attrs = expression.Member.GetCustomAttributes(typeof(SolrFieldAttribute), false);

            SolrFieldAttribute att= (SolrFieldAttribute) attrs[0];

            return att.FieldName;
        }

        /// <summary>
        /// Wraps matched strings in HTML span elements styled with a background-color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="keywords">Whitespace-separated list of strings to be highlighted</param>
        /// <param name="cssClass">The Css color to apply</param>
        /// <param name="fullMatch">false for returning all matches, true for whole word matches only</param>
        /// <returns>string</returns>
        public string EncodeAndHighlightKeyWords(string text, string keywords, Func<string, string> encode, params string[] highlightBraces)
        {
            //Encode
            text = encode(text);

            //Highlight
            if (text == String.Empty || keywords == String.Empty || highlightBraces == null || highlightBraces.Count() != 2)
                return text;
            var words = keywords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return
                    words.Select(word => word.Trim()).Aggregate(text,
                             (current, pattern) =>
                             Regex.Replace(current,
                                             Regex.Escape(pattern),
                                               string.Format("§{0}?",
                                               "$0"),
                                               RegexOptions.IgnoreCase))
                                               .Replace("§", highlightBraces[0])
                                               .Replace("?", highlightBraces[1]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;

namespace Absolute.SolrClient
{
    public class SolrQueryBuilder<TDoc> where TDoc : ISolrDocument
    {
        private readonly SolrIndex<TDoc> _index;

        public SolrQueryBuilder(SolrIndex<TDoc> index)
        {
            _index = index;
            _parts = new LinkedList<KeyValuePair<string, object>>();
        }

        private LinkedList<KeyValuePair<string, object>> _parts;


        /// <summary>
        /// Angabe des Suchfelds mit dem darin zu suchenden Wert.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="dynamicKey"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        public SolrQueryBuilder<TDoc> Expression(Expression<Func<TDoc, object>> field, object value, string dynamicKey, decimal boost)
        {
            if (field == null || value == null || Stringify(value).Trim().Length == 0)
            {
                if (_parts.Count > 0 && _parts.Last.Value.Key == "OPERATOR")
                {
                    _parts.RemoveLast();
                }
                return this;
            }
            var part = string.Format("{0}:({1})", _index.GetFieldName(field, dynamicKey), Stringify(value));
            if (boost > 0)
            {
                part = string.Format("{0}^{1}", part, Stringify(boost));
            }
            _parts.AddLast(new KeyValuePair<string, object>("EXPRESSION", part));
            return this;
        }

        public SolrQueryBuilder<TDoc> Expression(Expression<Func<TDoc, object>> field, object value, string dynamicKey)
        {
            return Expression(field, value, dynamicKey, 0);
        }

        public SolrQueryBuilder<TDoc> Expression(Expression<Func<TDoc, object>> field, object value, decimal boost)
        {
            return Expression(field, value, null, boost);
        }

        public SolrQueryBuilder<TDoc> Expression(Expression<Func<TDoc, object>> field, object value)
        {
            return Expression(field, value, null, 0);
        }

        public SolrQueryBuilder<TDoc> AllExpression()
        {
            _parts.AddLast(new KeyValuePair<string, object>("EXPRESSION", "*:*"));
            return this;
        }

        public SolrQueryBuilder<TDoc> NegativeExpression(Expression<Func<TDoc, object>> field, object value, string dynamicKey)
        {
            if (field == null || value == null || Stringify(value).Trim().Length == 0)
            {
                if (_parts.Count > 0 && _parts.Last.Value.Key == "OPERATOR")
                {
                    _parts.RemoveLast();
                }
                return this;
            }
            var part = string.Format("-{0}:({1})", _index.GetFieldName(field, dynamicKey), Stringify(value));
            _parts.AddLast(new KeyValuePair<string, object>("EXPRESSION", part));
            return this;
        }

        public SolrQueryBuilder<TDoc> NegativeExpression(Expression<Func<TDoc, object>> field, object value)
        {
            return NegativeExpression(field, value, null);
        }


        /// <summary>
        /// Suche im Standardsuchfeld (text).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SolrQueryBuilder<TDoc> DefaultExpression(object value)
        {
            if (value == null || Stringify(value).Trim().Length == 0)
            {
                if (_parts.Count > 0 && _parts.Last.Value.Key == "OPERATOR")
                {
                    _parts.RemoveLast();
                }
                return this;
            }
            _parts.AddLast(new KeyValuePair<string, object>("EXPRESSION", string.Format("({0})", Stringify(value))));
            return this;
        }

        /// <summary>
        /// Suche im Standardsuchfeld (text).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SolrQueryBuilder<TDoc> DefaultExpressionOhneStringify(object value)
        {
            if (value == null || Stringify(value).Trim().Length == 0)
            {
                if (_parts.Count > 0 && _parts.Last.Value.Key == "OPERATOR")
                {
                    _parts.RemoveLast();
                }
                return this;
            }
            _parts.AddLast(new KeyValuePair<string, object>("EXPRESSION", string.Format("({0})", value)));
            return this;
        }

        public SolrQueryBuilder<TDoc> Join(Expression<Func<TDoc, object>> field, SolrQueryOperator queryOperator, IEnumerable values, string dynamicKey = null, decimal boost = 0)
        {
            if (!values.GetType().IsGenericType)
            {
                return this;
            }
            foreach (var value in values)
            {
                Expression(field, value, dynamicKey, boost);
                Operator(queryOperator);
            }
            if (_parts.Count > 0 && _parts.Last.Value.Key == "OPERATOR")
            {
                _parts.RemoveLast();
            }
            return this;
        }

        /// <summary>
        /// Der Operator zwischen dem zuletzt angegeben und dem nächsten Ausdruck.
        /// </summary>
        /// <param name="queryOperator"></param>
        /// <returns></returns>
        public SolrQueryBuilder<TDoc> Operator(SolrQueryOperator queryOperator)
        {
            if (_parts.Count() > 0)
            {
                _parts.AddLast(new KeyValuePair<string, object>("OPERATOR", queryOperator.ToString().ToUpper()));
            }
            return this;
        }

        /// <summary>
        /// Eine in Klammern stehende Unterabfrage.
        /// </summary>
        /// <param name="subQuery"></param>
        /// <returns></returns>
        public SolrQueryBuilder<TDoc> SubQuery(SolrQueryBuilder<TDoc> subQuery)
        {
            _parts.AddLast(new KeyValuePair<string, object>("SUBQUERY", subQuery));
            return this;
        }

        public string Create()
        {
            if (_parts.Count == 0)
            {
                return null;
            }
            var result = string.Empty;
            var parts = _parts;
            if (parts.Last.Value.Key == "OPERATOR")
            {
                parts.RemoveLast();
            }

            foreach (var part in parts)
            {
                if (part.Key == "SUBQUERY")
                {
                    var subquery = (SolrQueryBuilder<TDoc>)part.Value;
                    result += string.Format("({0})", subquery.Create());
                    continue;
                }
                result += part.Value.ToString() + " ";
            }
            return result.Trim();
        }

        private string Stringify(object value)
        {
            var culture = new CultureInfo("en-US");
            if (value is decimal)
            {
                return ((decimal)value).ToString("0.###", culture);
            }
            else if (value is double)
            {
                return ((double)value).ToString("0.###", culture);
            }
            else if (value is DateTime)
            {
                return ((DateTime)value).ToString("u");
            }

            var result = Regex.Replace(value.ToString(), @"([+\-!\(\)\{\}\[\]\^~\?\:\\]{1})", "\\$1");
            return result;// UrlEncode(result);

        }

        private string UrlEncode(string value)
        {
            return value
                .Replace("Ä", "%C4")
                .Replace("Ö", "%D6")
                .Replace("Ü", "%DC")
                .Replace("ä", "%E4")
                .Replace("ö", "%F6")
                .Replace("ü", "%FC")
                .Replace("ß", "%DF");
        }
    }

    public enum SolrQueryOperator
    {
        And,
        Or
    }
}

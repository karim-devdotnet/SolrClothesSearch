using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Runtime.Serialization;
using System.Collections;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Absolute.SolrClient
{
    public class SolrIndex<TDoc> : ISolrIndex<TDoc>, ISolrIndex where TDoc : ISolrDocument
    {
        public SolrIndex(string solrBaseUri) : this(solrBaseUri, null)
        {
        }

        public SolrIndex(string solrBaseUri, string name)
        {
            SolrBaseUri = new Uri(solrBaseUri.TrimEnd('/') + "/");
            Name = name;
            DocFields = DetermineDocFields();
            if (DocFields.Count == 0)
            {
                throw new Exception("Konnte keine Felder für den SolrIndex ermitteln.");
            }
            _serializer = new SolrSerializer<TDoc>(DocFields);
            DetermineCoreInfos();
        }

        public readonly Uri SolrBaseUri;
        internal readonly IDictionary<string, PropertyInfo> DocFields = new Dictionary<string, PropertyInfo>();
        private readonly SolrSerializer<TDoc> _serializer;
        public ICollection<SolrCoreInfo> CoreInfos { get; private set; }
        public string Name { get; private set; }

        public SolrQueryDescription<TDoc> NewQueryDescription()
        {
            return new SolrQueryDescription<TDoc>(this);
        }

        public SolrQueryBuilder<TDoc> NewQueryBuilder()
        {
            return new SolrQueryBuilder<TDoc>(this);
        }
        
        public SolrQueryResult<TDoc> Query(SolrQueryDescription<TDoc> queryDesc)
        {
            if (string.IsNullOrEmpty(queryDesc.Query))
            {
                throw new Exception("Ohne Query kann keine Abfrage ausgeführt werden.");
            }
            var conn = new SolrConnection(CoreInfos.First().Uri);
            var paramList = new Dictionary<string, string>();
            if (CoreInfos.Count > 1)
            {
                paramList.Add("shards", string.Join(",", CoreInfos.Select(x => x.Uri.ToString())));
            }
            var queryString = queryDesc.Query;
            if (string.IsNullOrEmpty(queryString))
            {
                queryString = "*:*";
            }
            paramList.Add("q", queryString);
            if (queryDesc.StartIndex.HasValue)
            {
                paramList.Add("start", queryDesc.StartIndex.ToString());
            }
            if (queryDesc.RowCount.HasValue)
            {
                paramList.Add("rows", queryDesc.RowCount.ToString());
            }

            // Filter
            if (!string.IsNullOrEmpty(queryDesc.FilterQuery))
            {
                paramList.Add("fq", queryDesc.FilterQuery);
            }

            // Anzeigefelder
            var fl = string.Join(",", queryDesc.FieldNames);
            paramList.Add("fl", fl);

            // Facets
            if (queryDesc.FacetQueries.Count > 0)
            {
                paramList.Add("facet", "on");
                foreach (var facet in queryDesc.FacetQueries)
                {
                    foreach (var kvp in facet.CreateParams())
                    {
                        paramList.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            // Highlighting
            if (queryDesc.Highlighting.Fields.Count > 0)
            {
                paramList.Add("hl", "on");
                if (queryDesc.Highlighting.RequireFieldMatch)
                {
                    paramList.Add("hl.requireFieldMatch", "true");
                }
                paramList.Add("hl.fl", string.Join(",", queryDesc.Highlighting.Fields));
            }

            // Sortierung
            if (queryDesc.SortFields.Count > 0)
            {
                paramList.Add("sort", string.Join(",", queryDesc.SortFields));
            }

            var stream = conn.Post("select", paramList);
            return _serializer.Deserialize(stream, queryDesc);
        }

        public string GetFieldName(Expression<Func<TDoc, object>> field, string dynamicKey)
        {
            var expression = field.Body as MemberExpression;
            if (expression == null)
            {
                if (field.Body is UnaryExpression)
                {
                    expression = ((UnaryExpression)field.Body).Operand as MemberExpression;
                }
                else
                {
                    return null;
                }
            }
            var name = expression.Member.Name;
            if (DocFields.Any(x => x.Value.Name == name))
            {
                name = DocFields.Single(x => x.Value.Name == name).Key;
                if (!string.IsNullOrEmpty(dynamicKey))
                {
                    return name.Replace("*", dynamicKey);
                }
            }
            return name;
        }

        public string GetFieldName(Expression<Func<TDoc, object>> field)
        {
            return GetFieldName(field, null);
        }

        internal bool IsStoredField(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttributes(typeof(SolrFieldAttribute), true).FirstOrDefault() as SolrFieldAttribute;
            if (attr == null || !attr.Stored)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Ermitteln der Felder des angegebenen DocTypes
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, PropertyInfo> DetermineDocFields()
        {
            var docFields = new Dictionary<string, PropertyInfo>();
            var props = typeof(TDoc).GetProperties();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(SolrFieldAttribute), true).FirstOrDefault() as SolrFieldAttribute;
                if (attr != null)
                {
                    docFields.Add(attr.Name, prop);
                }
            }
            return docFields;
        }

        /// <summary>
        /// Ermitteln der zum Index zugehörigen Cores. Bei mehreren Cores wird automatisch Sharding verwendet.
        /// </summary>
        /// <returns></returns>
        private bool DetermineCoreInfos()
        {
            CoreInfos = new List<SolrCoreInfo>();
            var xdoc = new XmlDocument();
            var webRequest = WebRequest.Create(SolrConnection.CombineUri(SolrBaseUri, new Uri("admin/cores?action=STATUS", UriKind.Relative), null));

            var response = webRequest.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(responseStream))
                {
                    xdoc.LoadXml(sr.ReadToEnd());
                }
            }
            foreach (XmlNode node in xdoc.SelectNodes("/response/lst[@name='status']/lst"))
            {
                var name = node.SelectSingleNode("@name").Value;
                var coreInfo = new SolrCoreInfo(new Uri(SolrBaseUri, name + "/"), _serializer, this);
                if (string.IsNullOrEmpty(name))
                {
                    coreInfo = new SolrCoreInfo(SolrBaseUri, _serializer, this);
                    name = "Main";
                }
                coreInfo.Name = name;
                coreInfo.InstanceDir = node.SelectSingleNode("str[@name='instanceDir']").InnerText;
                coreInfo.DataDir = node.SelectSingleNode("str[@name='dataDir']").InnerText;
                coreInfo.NumDocs = int.Parse(node.SelectSingleNode("lst[@name='index']/int[@name='numDocs']").InnerText);
                CoreInfos.Add(coreInfo);
            }

            return CoreInfos.Count > 0;

        }

    }
}

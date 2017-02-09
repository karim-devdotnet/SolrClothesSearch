using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.XPath;
using System.IO;

namespace Absolute.SolrClient
{
    public class SolrSerializer<TDoc> : ISolrSerializer where TDoc : ISolrDocument
    {
        private readonly IDictionary<string, PropertyInfo> _documentFields;

        internal SolrSerializer(IDictionary<string, PropertyInfo> documentFields)
        {
            _documentFields = documentFields;
        }

        public XmlElement Serialize(ISolrDocument document, XmlDocument xdoc)
        {
            if (!(document is TDoc))
            {
                throw new Exception(string.Format("Das übergebene Dokument ist nicht vom Typ '{0}'.", typeof(TDoc)));
            }
            var docElement = xdoc.CreateElement("doc");
            foreach (var field in _documentFields)
            {
                var value = field.Value.GetValue(document, null);
                if (value == null)
                {
                    continue;
                }
                AppendSerializedField(docElement, field.Key, field.Value.PropertyType, value);
            }
            return docElement;
        }

        public SolrQueryResult<TDoc> Deserialize(Stream stream, SolrQueryDescription<TDoc> queryDesc)
        {


            var docType = typeof(TDoc);
            var result = new SolrQueryResult<TDoc>(queryDesc);

            var xPathDoc = new XPathDocument(stream);
            var xMainNav = xPathDoc.CreateNavigator();
            xMainNav.MoveToFirstChild(); // response
            if (xMainNav.Name != "response")
            {
                return result;
            }

            // Infos
            result.QueryTime = xMainNav.SelectSingleNode("lst[@name='responseHeader']/int[@name='QTime']").ValueAsInt;
            xMainNav = xMainNav.SelectSingleNode("result");
            result.NumFound = xMainNav.SelectSingleNode("@numFound").ValueAsInt;
            result.StartIndex = xMainNav.SelectSingleNode("@start").ValueAsInt;

            // Dokumente
            var xSolrDoc = xMainNav.Select("doc");
            while (xSolrDoc.MoveNext())
            {
                var doc = Activator.CreateInstance(docType);
                var xField = xSolrDoc.Current.SelectChildren(XPathNodeType.Element);
                while (xField.MoveNext())
                {
                    var fieldNode = xField.Current;
                    var fieldNodeName = fieldNode.SelectSingleNode("@name").Value;
                    PropertyInfo field = null;
                    if (queryDesc.DynamicFieldKeys.Any(x => x.Value == fieldNodeName))
                    {
                        fieldNodeName = queryDesc.DynamicFieldKeys.First(x => x.Value == fieldNodeName).Key;
                    }
                    if (_documentFields.ContainsKey(fieldNodeName))
                    {
                        field = _documentFields[fieldNodeName];
                    }
                    else
                    {
                        continue;
                    }
                    if (field.PropertyType.IsGenericType && field.PropertyType.GetInterface("IEnumerable") != null)
                    {
                        var genericArguments = field.PropertyType.GetGenericArguments();
                        var argumentType = genericArguments[0];
                        var listType = typeof(LinkedList<>);
                        var list = field.GetValue(doc, null);
                        if (list == null)
                        {
                            list = Activator.CreateInstance(listType.MakeGenericType(argumentType));
                            field.SetValue(doc, list, null);
                        }
                        if (fieldNode.Name == "arr")
                        {
                            var xArr = fieldNode.SelectChildren(XPathNodeType.Element);
                            while (xArr.MoveNext())
                            {
                                var arrNode = xArr.Current;
                                var value = GetNodeValue(arrNode);
                                field.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, null, list, new object[] { Convert.ChangeType(value, argumentType) });
                            }
                        }
                        else
                        {
                            var value = GetNodeValue(fieldNode);
                            field.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, null, list, new object[] { Convert.ChangeType(value, argumentType) });
                        }
                    }
                    else
                    {
                        var type = field.PropertyType;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                        var value = GetNodeValue(fieldNode);
                        if (!string.IsNullOrEmpty(value))
                        {
                            field.SetValue(doc, Convert.ChangeType(value, type), null);
                        }
                    }

                }
                result.Documents.Add((TDoc)doc);
            }
            xMainNav.MoveToNext();


            // Facets
            if (xMainNav.SelectSingleNode("@name").Value == "facet_counts")
            {
                var xFacetNodes = xMainNav.Select("lst[@name='facet_fields']/lst");
                while (xFacetNodes.MoveNext())
                {
                    var facetNode = xFacetNodes.Current;
                    var fieldName = facetNode.SelectSingleNode("@name").Value;
                    var facetQuery = queryDesc.FacetQueries.SingleOrDefault(x => x.FieldName == fieldName);
                    if (facetQuery == null)
                    {
                        continue;
                    }
                    var facetResult = new SolrFacetResult(facetQuery);
                    var xCountNodes = facetNode.SelectChildren(XPathNodeType.Element);
                    while (xCountNodes.MoveNext())
                    {
                        var countNode = xCountNodes.Current;
                        facetResult.Counts.Add(countNode.SelectSingleNode("@name").Value, countNode.ValueAsInt);
                    }
                    result.Facets.Add(facetResult);
                }
                xMainNav.MoveToNext();
            }

            // Highlights
            if (xMainNav.SelectSingleNode("@name").Value == "highlighting")
            {
                var xHighlighNodes = xMainNav.Select("lst");
                while (xHighlighNodes.MoveNext())
                    {
                    var idNode = xHighlighNodes.Current;
                    var id = idNode.SelectSingleNode("@name").Value;
                    var fieldList = new Dictionary<string, ICollection<string>>();
                    result.Highlights.Add(id, fieldList);
                    var xFieldNodes = idNode.Select("arr");
                    while (xFieldNodes.MoveNext())
                    {
                        var fieldNode = xFieldNodes.Current;
                        var fieldName = fieldNode.SelectSingleNode("@name").Value;
                        var valueList = new LinkedList<string>();
                        fieldList.Add(fieldName, valueList);
                        var xValueNodes = fieldNode.Select("str");
                        while (xValueNodes.MoveNext())
                        {
                            var valueNode = xValueNodes.Current;
                            var value = valueNode.Value;
                            valueList.AddLast(value);
                        }
                    }
                }
            }

            return result;
        }

        public SolrQueryResult<TDoc> Deserialize(XmlDocument xdoc, SolrQueryDescription<TDoc> queryDesc)
        {
            var docType = typeof(TDoc);
            var result = new SolrQueryResult<TDoc>(queryDesc);

            // Infos
            result.QueryTime = int.Parse(xdoc.SelectSingleNode("/response/lst[@name='responseHeader']/int[@name='QTime']").InnerText);
            result.NumFound = int.Parse(xdoc.SelectSingleNode("/response/result/@numFound").Value);
            result.StartIndex = int.Parse(xdoc.SelectSingleNode("/response/result/@start").Value);


            // Dokumente
            var docNodes = xdoc.SelectNodes("/response/result/doc");
            foreach (XmlNode docNode in docNodes)
            {
                var doc = Activator.CreateInstance(docType);
                foreach (XmlNode fieldNode in docNode.ChildNodes)
                {
                    var fieldNodeName = fieldNode.SelectSingleNode("@name").Value;
                    var field = (PropertyInfo)null;
                    foreach (var kvp in _documentFields)
                    {
                        if (kvp.Key == fieldNodeName || Regex.IsMatch(fieldNodeName, "^" + kvp.Key.Replace("*", ".*") + "$"))
                        {
                            field = kvp.Value;
                            break;
                        }
                    }
                    if (field == null)
                    {
                        continue;
                    }
                    if (field.PropertyType.IsGenericType && field.PropertyType.GetInterface("IEnumerable") != null)
                    {
                        var genericArguments = field.PropertyType.GetGenericArguments();
                        var argumentType = genericArguments[0];
                        var listType = typeof(LinkedList<>);
                        var list = field.GetValue(doc, null);
                        if (list == null)
                        {
                            list = Activator.CreateInstance(listType.MakeGenericType(argumentType));
                            field.SetValue(doc, list, null);
                        }
                        if (fieldNode.Name == "arr")
                        {
                            foreach (XmlNode arrNode in fieldNode.ChildNodes)
                            {
                                var value = GetNodeValue(arrNode);
                                field.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, null, list, new object[] { Convert.ChangeType(value, argumentType) });
                            }
                        }
                    }
                    else
                    {
                        var type = field.PropertyType;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                        var value = GetNodeValue(fieldNode);
                        field.SetValue(doc, Convert.ChangeType(value, type), null);
                    }
                }
                result.Documents.Add((TDoc)doc);
            }

            // Facets
            var facetNodes = xdoc.SelectNodes("/response/lst[@name='facet_counts']/lst[@name='facet_fields']/lst");
            if (facetNodes.Count > 0)
            {
                foreach (XmlNode facetNode in facetNodes)
                {
                    var fieldName = facetNode.SelectSingleNode("@name").Value;
                    var facetQuery = queryDesc.FacetQueries.SingleOrDefault(x => x.FieldName == fieldName);
                    if (facetQuery == null)
                    {
                        continue;
                    }
                    var facetResult = new SolrFacetResult(facetQuery);
                    foreach (XmlNode countNode in facetNode.ChildNodes)
                    {
                        facetResult.Counts.Add(countNode.SelectSingleNode("@name").Value, int.Parse(countNode.InnerText));
                    }
                    result.Facets.Add(facetResult);
                }
            }

            // Highlights
            var highlightNodes = xdoc.SelectNodes("/response/lst[@name='highlighting']/lst");
            if (highlightNodes.Count > 0)
            {
                foreach (XmlNode idNode in highlightNodes)
                {
                    var id = idNode.SelectSingleNode("@name").Value;
                    var fieldList = new Dictionary<string, ICollection<string>>();
                    result.Highlights.Add(id, fieldList);
                    foreach (XmlNode fieldNode in idNode.SelectNodes("arr"))
                    {
                        var fieldName = fieldNode.SelectSingleNode("@name").Value;
                        var valueList = new LinkedList<string>();
                        fieldList.Add(fieldName, valueList);
                        foreach (XmlNode valueNode in fieldNode.SelectNodes("str"))
                        {
                            var value = valueNode.InnerText;
                            valueList.AddLast(value);
                        }
                    }
                }
            }
            return result;
        }

        private string GetNodeValue(XmlNode node)
        {
            var value = node.InnerText;
            if (node.Name == "double")
            {
                value = value.Replace(".", ",");
            }
            return value;
        }

        private string GetNodeValue(XPathNavigator node)
        {
            var value = node.Value;
            if (node.Name == "double")
            {
                value = value.Replace(".", ",");
            }
            return value;
        }

        private void AppendSerializedField(XmlElement xmlElement, string fieldName, Type dataType, object dataObject)
        {
            if (dataType.IsGenericType && dataType.GetInterface("IEnumerable") != null)
            {
                foreach (var val in (IEnumerable)dataObject)
                {
                    if (val == null)
                    {
                        continue;
                    }
                    var attributeName = fieldName;
                    var genVal = val;
                    var genType = val.GetType();
                    // Dictionary
                    if (genType.Name == "KeyValuePair`2")
                    {
                        attributeName = attributeName.Replace("*", val.GetType().GetProperty("Key").GetValue(val, null).ToString());
                        genVal = val.GetType().GetProperty("Value").GetValue(val, null);
                        genType = genVal.GetType();
                    }

                    AppendSerializedField(xmlElement, attributeName, genType, genVal);
                }
            }
            else if (dataType == typeof(decimal) || dataType == typeof(Nullable<decimal>))
            {
                var text = ((decimal)dataObject).ToString("0.###",new CultureInfo("en-US"));
                xmlElement.AppendChild(CreateFieldElement(xmlElement.OwnerDocument, fieldName, text));
            }
            else if (dataType == typeof(DateTime) || dataType == typeof(Nullable<DateTime>))
            {
                var text = ((DateTime)dataObject).ToUniversalTime().ToString("u", DateTimeFormatInfo.InvariantInfo).Replace(" ", "T");
                xmlElement.AppendChild(CreateFieldElement(xmlElement.OwnerDocument, fieldName, text));
            }
            else
            {
                xmlElement.AppendChild(CreateFieldElement(xmlElement.OwnerDocument, fieldName, dataObject.ToString()));
            }

        }

        private XmlElement CreateFieldElement(XmlDocument doc, string attributeName, string value)
        {
            var fieldElement = doc.CreateElement("field");
            var fieldNameAttr = doc.CreateAttribute("name");
            fieldNameAttr.Value = attributeName;
            fieldElement.Attributes.Append(fieldNameAttr);
            fieldElement.InnerText = value;
            return fieldElement;
        }
    }
}

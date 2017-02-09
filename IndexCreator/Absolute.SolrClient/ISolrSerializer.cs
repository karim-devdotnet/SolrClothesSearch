using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Absolute.SolrClient
{
    public interface ISolrSerializer
    {
        XmlElement Serialize(ISolrDocument document, XmlDocument xdoc);
    }
}

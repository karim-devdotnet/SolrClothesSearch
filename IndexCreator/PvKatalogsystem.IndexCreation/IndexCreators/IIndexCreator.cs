using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Absolute.SolrClient;

namespace PvKatalogsystem.IndexCreation.IndexCreators
{
    public interface IIndexCreator
    {
        string Name { get; }
        ISolrIndex SolrIndex { get; }
        void Init(string connectionString, Uri solrUri);
        ICollection<int> DetermineObjectIds();
        ICollection<ISolrDocument> CreateDocuments(IEnumerable<int> objectIds);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Absolute.SolrClient;

namespace PvKatalogsystem.IndexCreation
{
    internal class ChunkSendEventArgs : EventArgs
    {
        public ChunkSendEventArgs(ICollection<ISolrDocument> documents)
        {
            Documents = documents;
        }

        public ICollection<ISolrDocument> Documents { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PvKatalogsystem.IndexCreation.IndexCreators;
using Absolute.SolrClient;
using System.Threading;
using PvKatalogsystem.IndexCreation.Configuration;

namespace PvKatalogsystem.IndexCreation
{
    internal class ChunkSendDispatcher : IDisposable
    {
        public readonly ISolrIndex Index;
        private readonly List<ChunkSendController> _controllerList;
        public int DispatchCount = 0;
        private object _locker = new object();

        public ChunkSendDispatcher(ISolrIndex index, int chunkSize)
        {
            Index = index;
            _controllerList = new List<ChunkSendController>();
            foreach (var core in Index.CoreInfos)
            {
                var controller = new ChunkSendController(core, chunkSize);
                _controllerList.Add(controller);
                controller.ChunksSent += OnChunksSent;
            }
        }

        public void AddDocuments(ICollection<ISolrDocument> chunk)
        {
            foreach (var doc in chunk)
            {
                var index = Math.Abs(doc.Id.GetHashCode()) % _controllerList.Count;
                _controllerList[index].AddDocument(doc);
                lock (_locker)
                {
                    DispatchCount++;
                }
            }
        }

        public void FlushAll()
        {
            foreach (var controller in _controllerList)
            {
                controller.Flush(true);
            }
        }

        private void OnChunksSent(object sender, ChunkSendEventArgs e)
        {
            if (ChunksSent != null)
            {
                ChunksSent(this, new ChunkSendEventArgs(e.Documents));
            }
        }

        public event EventHandler<ChunkSendEventArgs> ChunksSent;

        public void Dispose()
        {
            foreach (var controller in _controllerList)
            {
                controller.Dispose();
            }
        }
    }
}

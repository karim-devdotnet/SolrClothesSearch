using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Absolute.SolrClient;
using System.Threading;
using System.Diagnostics;
using PvKatalogsystem.IndexCreation.Helper;
using PvKatalogsystem.IndexCreation.IndexCreators;

namespace PvKatalogsystem.IndexCreation
{
    internal class ChunkSendController : IDisposable
    {
        private readonly int _chunkSize;
        private readonly string _indexName;
        private readonly SolrCoreInfo _core;
        private readonly Thread _thread;
        private readonly ManualResetEvent _queueFilledEvent = new ManualResetEvent(false);
        private LinkedList<ISolrDocument> _sendQueue = new LinkedList<ISolrDocument>();
        private int _sendCount = 0;

        public ChunkSendController(SolrCoreInfo core, int chunkSize)
        {
            _core = core;
            _chunkSize = chunkSize;
            _indexName = core.Index.Name;
            _thread = new Thread(new ThreadStart(SendChunks));
            _thread.Name = string.Format("ChunkSend Core '{0}'", _core.Name);
            _thread.Start();
        }

        private void SendChunks()
        {
            while (true)
            {
                _queueFilledEvent.WaitOne();
                _queueFilledEvent.Reset();
                Flush();
            }
        }

        public void AddDocument(ISolrDocument document)
        {
            lock (_queueLock)
            {
                _sendQueue.AddLast(document);
                if (_sendQueue.Count > 0 && _sendQueue.Count % _chunkSize == 0)
                {
                    _queueFilledEvent.Set();
                }
            }
        }


        public void Flush(bool commit = false)
        {
            lock (_queueLock)
            {
                for (var j = 0; j < _sendQueue.Count; j += _chunkSize)
                {
                    // Queue auslesen
                    var chunk = new LinkedList<ISolrDocument>();
                    var count = _sendQueue.Count;
                    for (var i = 0; i < _chunkSize && i < count; i++)
                    {
                        var item = _sendQueue.First;
                        _sendQueue.RemoveFirst();
                        chunk.AddLast(item);
                    }
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        _core.AddRange(chunk, false);
                        LogManager.Info(string.Format("Core '{0}': Dokumente indiziert. ({1} ms)", _core.Name, sw.ElapsedMilliseconds), _indexName);
                        sw.Stop();
                        _sendCount += chunk.Count;
                        if (commit || _sendCount >= _chunkSize * 20)
                        {
                            _core.Commands.Commit();
                            _sendCount = 0;
                        }

                        if (ChunksSent != null)
                        {
                            ChunksSent(this, new ChunkSendEventArgs(chunk));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error(ex, _indexName);
                        throw;
                    }
                }
            }
        }

        public void Dispose()
        {
            _thread.Abort();
        }

        private readonly object _queueLock = new object();
        public event EventHandler<ChunkSendEventArgs> ChunksSent;
    }
}

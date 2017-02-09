using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PvKatalogsystem.IndexCreation.Configuration;
using PvKatalogsystem.IndexCreation.Helper;
using PvKatalogsystem.IndexCreation.IndexCreators;
using Absolute.SolrClient;
using System.IO;
using System.Net;

namespace PvKatalogsystem.IndexCreation
{
    public class IndexCreationController
    {
        private IEnumerable<IIndexCreator> _creators;
        private readonly object _threadLocker = new object();
        private readonly IDictionary<int, Thread> _threads = new Dictionary<int, Thread>();
        private readonly ManualResetEvent _threadReady = new ManualResetEvent(true);
        internal const string ManagementTablePrefix = "Solr_";
        private string _managementConnectionString;
        private ICollection<int> _objectIds;
        private LinkedList<int> _objectIdsReady;
        private LongStopwatch _creatorSw;


        public IndexCreationController()
        {
            LogManager.Start("logs", true);

            Init();

            //var provider = new ManagementProvider(_managementConnectionString);

            foreach (var creator in _creators)
            {
                var chunkSize = IndexCreationConfig.Instance.Indices.GetItemByKey(creator.Name).ChunkSize;

                LogManager.Info("Ermitteln der ObjectIds...", creator.Name);
                _objectIds = creator.DetermineObjectIds();

                ////TEST
                //_objectIds = new LinkedList<int>();
                //_objectIds.Add(165122399);

                _objectIdsReady = new LinkedList<int>();
                LogManager.Info(string.Format("{0:0,0} gefunden.", _objectIds.Count()), creator.Name);

                //LogManager.Info("Speichern der ObjectIds...", creator);
                //provider.InsertDeterminedIds(ids, creator.Name);

                LogManager.Info("Index wird geleert...", creator.Name);
                foreach (var core in creator.SolrIndex.CoreInfos)
                {
                    core.Commands.Truncate();
                    core.Commands.Commit();
                    core.Commands.Optimize();
                }

                LogManager.Info("Starte Erstellung der Solr-Dokumente...", creator.Name);
                _creatorSw = LongStopwatch.StartNew();

                using (var dispatcher = new ChunkSendDispatcher(creator.SolrIndex, chunkSize))
                {
                    dispatcher.ChunksSent += OnChunksSent;

                    for (var i = 0; i < _objectIds.Count; i += chunkSize)
                    {
                        // Thread starten
                        while (!oneThreadReady())
                        {
                            _threadReady.Reset();
                            _threadReady.WaitOne(1000);
                        }
                        var threadIndex = _threads.First(x => x.Value == null || x.Value.ThreadState == System.Threading.ThreadState.Stopped).Key;
                        _threads[threadIndex] = new Thread(new ParameterizedThreadStart(CreateChunkThread));
                        _threads[threadIndex].Start(new ChunkInfos(creator, _objectIds.Skip(i).Take(chunkSize), dispatcher));
                        Thread.Sleep(500);

                        if (i + chunkSize >= _objectIds.Count)
                        {
                            // Warten bis alle Threads fertig sind
                            while (!allThreadsReady())
                            {
                                Thread.Sleep(1000);
                            }

                            // Die restlichen Dokumente an Solr senden
                            dispatcher.FlushAll();

                            // Warten bis alle verarbeitet wurden
                            var sw = Stopwatch.StartNew();
                            while (_objectIdsReady.Count < _objectIds.Count && sw.ElapsedMilliseconds < 60000)
                            {
                                Thread.Sleep(1000);
                            }
                            sw.Stop();

                            // Prüfen ob alle Objekte gesendet wurden
                            if (_objectIdsReady.Count < _objectIds.Count)
                            {
                                lock (_threadLocker)
                                {
                                    var retryList = _objectIds.Except(_objectIdsReady);
                                    _objectIds = new LinkedList<int>();
                                    foreach (var id in retryList)
                                    {
                                        _objectIds.Add(id);
                                    }
                                    _objectIdsReady = new LinkedList<int>();
                                }
                                chunkSize /= 2;
                                if (chunkSize < 1)
                                {
                                    LogManager.Error(string.Format("Die Chunkgröße hat den minimalen Wert (1) erreicht. {0} Dokumente konnten leider nicht übermittelt werden. Die fehlenden Ids werden in error.log geschrieben.", _objectIds.Count()));
                                    using (var streamWriter = new StreamWriter("logs/error.log", false))
                                    {
                                        foreach (var id in _objectIds)
                                        {
                                            streamWriter.WriteLine(id);
                                        }
                                        streamWriter.Flush();
                                        streamWriter.Close();
                                    }
                                    _objectIds = new LinkedList<int>();
                                }
                                else
                                {
                                    i = 0;
                                    _creatorSw.Restart();
                                    LogManager.Info(string.Format("Es konnten nicht alle Dokumente übermittelt werden. {0} Dokumente werden erneut verarbeitet. Die Chunkgröße wird auf {1} gesetzt.", _objectIds.Count, chunkSize));
                                }
                            }
                        }
                    }

                    // Index optimieren
                    for (var j = 1; j <= 2; j++)
                    {
                        foreach (var core in creator.SolrIndex.CoreInfos)
                        {
                            LogManager.Info(string.Format("{0}. Indexoptimierung für Core '{1}' wird durchgeführt.", j, core.Name), creator.Name);
                            var sw = Stopwatch.StartNew();
                            core.Commands.Optimize();
                            LogManager.Info(string.Format("Indexoptimierung erfolgreich. ({0} ms)", sw.ElapsedMilliseconds), creator.Name);
                            sw.Stop();
                        }
                    }

                    // Replikation anwerfen
                    Replicate(creator);
                }
                _creatorSw.Stop();
            }

            LogManager.Info("Indexerstellung beendet.");
            //Console.ReadKey(false);
            Environment.Exit(0);
        }

        private bool allThreadsReady()
        {
            return !(_threads.Count(x => x.Value == null || x.Value.ThreadState == System.Threading.ThreadState.Stopped) < IndexCreationConfig.Instance.Indices.Threads);
        }

        private bool oneThreadReady()
        {
            return !(_threads.Count(x => x.Value == null || x.Value.ThreadState == System.Threading.ThreadState.Stopped) == 0);
        }

        private void OnChunksSent(object sender, ChunkSendEventArgs e)
        {
            var dispatcher = (ChunkSendDispatcher)sender;
            lock (_threadLocker)
            {
                foreach (var id in e.Documents.Select(x => x.Id))
                {
                    _objectIdsReady.AddLast(int.Parse(id));
                }
            }
            var _documentTime = (double)_creatorSw.ElapsedMilliseconds / _objectIdsReady.Count;
            LogManager.Info(string.Format("{0:0,0} von {1:0,0} Dokumenten insgesamt.", _objectIdsReady.Count, _objectIds.Count()), dispatcher.Index.Name);
            LogManager.Info(string.Format("{0:F2} ms/Dokument, Restdauer: {1:hh}:{1:mm}:{1:ss}, Gesamtdauer: {2:hh}:{2:mm}:{2:ss}", _documentTime, TimeSpan.FromMilliseconds(_documentTime * (_objectIds.Count - _objectIdsReady.Count)), TimeSpan.FromMilliseconds(_documentTime * _objectIds.Count)), dispatcher.Index.Name);
        }

        private void CreateChunkThread(object infos)
        {
            _threadReady.Reset();

            var chunkInfos = (ChunkInfos)infos;
            var trycount = 0;
            var error = false;

            do
            {
                try
                {
                    error = false;
                    // Chunk erstellen
                    var sw = Stopwatch.StartNew();
                    var chunk = chunkInfos.Creator.CreateDocuments(chunkInfos.ObjectIds);
                    LogManager.Info(string.Format("Chunks erstellt. ({0} ms)", sw.ElapsedMilliseconds), chunkInfos.Creator.Name);
                    chunkInfos.Dispatcher.AddDocuments(chunk);
                }
                catch (Exception ex)
                {
                    LogManager.Error(ex);
                    trycount++;
                    error = true;
                    if (ex.Message.Contains("Timeout"))
                    {
                        using (var sw = new StreamWriter("timeout_" + Guid.NewGuid().ToString(), false))
                        {
                            foreach (var id in chunkInfos.ObjectIds)
                            {
                                sw.WriteLine(id);
                            }
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }
            while (error && trycount < 3);

            _threadReady.Set();
        }

        private void SendChunkDispatcher()
        {
            while (true)
            {

            }
        }

        private void Init()
        {
            LogManager.Info("Starte Initialisierung...");
            //_managementConnectionString = IndexCreationConfig.Instance.Database.ManagementConnectionString;
            LoadIndexCreators();
            //foreach (var creator in _creators)
            //{
            //    var provider = new ManagementProvider(_managementConnectionString);
            //    provider.CreateIdsTypeIfNotExists();
            //    provider.CreateTableIfNotExists(creator.Name);
            //}
            for (var i = 0; i < IndexCreationConfig.Instance.Indices.Threads; i++)
            {
                _threads.Add(i, null);
            }
        }

        private void LoadIndexCreators()
        {
            var creatorList = new List<IIndexCreator>();
            var baseSolrUri = new Uri(IndexCreationConfig.Instance.MasterServer.Uri);
            var replicationSlaveUris = new List<string>();
            foreach (ReplicationServerConfig slave in IndexCreationConfig.Instance.ReplicationServer)
            {
                replicationSlaveUris.Add(slave.Uri);
            }
            foreach (var type in GetType().Assembly.GetExportedTypes().Where(x => typeof(IIndexCreator).IsAssignableFrom(x) && !x.IsInterface && x.Namespace == IndexCreationConfig.Instance.Indices.Namespace))
            {
                var creator = Activator.CreateInstance(type) as IIndexCreator;
                if (creator == null)
                {
                    continue;
                }

                var conf = IndexCreationConfig.Instance.Indices.GetItemByKey(creator.Name);
                if (conf == null)
                {
                    LogManager.Info(string.Format("Keinen Konfigurationsabschnitt für Index [{0}] gefunden.", creator.Name));
                    continue;
                }
                var relativeUri = conf.RelativeUri;
                if (string.IsNullOrEmpty(relativeUri))
                {
                    throw new Exception(string.Format("Keine RelativeUri im Konfigurationsabschnit des Index [{0}] gefunden.", conf.Name));
                }
                var solrUri = new Uri(baseSolrUri, relativeUri.TrimEnd('/') + "/");
                creator.Init(IndexCreationConfig.Instance.Database.SourceConnectionString, solrUri);
                creatorList.Add(creator);
            }
            _creators = creatorList;
        }

        private void Replicate(IIndexCreator creator)
        {
            if (!IndexCreationConfig.Instance.Indices.GetItemByKey(creator.Name).Replication)
            {
                return;
            }

            if (IndexCreationConfig.Instance.ReplicationServer == null || IndexCreationConfig.Instance.ReplicationServer.Count == 0)
            {
                LogManager.Info("Wenn eine Replikation gewünscht ist muss in der Config mindestens ein ReplicationSlave eingetragen sein.", creator.Name);
                return;
            }
            foreach (ReplicationServerConfig slave in IndexCreationConfig.Instance.ReplicationServer)
            {
                try
                {   var slaveUri = new Uri(slave.Uri);
                    var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(slaveUri, creator.SolrIndex.CoreInfos.First().Uri.AbsolutePath.TrimEnd('/') + "/replication?command=fetchindex"));
                    webRequest.Method = "GET";
                    var webResponse = webRequest.GetResponse();
                    LogManager.Info(string.Format("Replikation auf {0} erfolgreich gestartet.", slave.Uri), creator.Name);
                }
                catch (Exception ex)
                {
                    LogManager.Error(string.Format("Replikation auf {0} fehlerhaft.", slave.Uri), ex, creator.Name);
                }
            }
        }


        private struct ChunkInfos
        {
            public ChunkInfos(IIndexCreator creator, IEnumerable<int> objectIds, ChunkSendDispatcher dispatcher)
            {
                Creator = creator;
                ObjectIds = objectIds;
                Dispatcher = dispatcher;
            }
            public IIndexCreator Creator;
            public IEnumerable<int> ObjectIds;
            public ChunkSendDispatcher Dispatcher;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Absolute.SolrClient
{
    public class SolrCommands
    {
        private readonly SolrConnection _connection;

        public SolrCommands(SolrConnection connection)
        {
            _connection = connection;
        }

        public bool Update(SolrCoreInfo core, XmlDocument document)
        {
            var result = false;
            using (var ms = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(ms))
                {
                    document.WriteTo(xw);
                    xw.Close();
                }
                ms.Position = 0;
                result = _connection.PostStream("update/", "text/xml", ms);
                ms.Close();
            }
            return result;
        }

        public void Commit()
        {
            try
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("stream.body", "<commit waitFlush=\"false\" waitSearcher=\"false\" />");
                _connection.Get("update/", 100, paramList);
            }
            catch { }
        }

        public bool Optimize()
        {
            var paramList = new Dictionary<string, string>();
            paramList.Add("optimize", "true");
            return _connection.Get("update/", -1, paramList);
        }

        public bool Truncate()
        {
            var paramList = new Dictionary<string, string>();
            paramList.Add("stream.body", "<delete><query>*:*</query></delete>");
            return _connection.Get("update/", 300000, paramList);
        }

        public bool Replicate(Uri slaveUri)
        {
            throw new NotImplementedException();
        }

        public bool Backup(string path)
        {
            throw new NotImplementedException();
        }
    }
}

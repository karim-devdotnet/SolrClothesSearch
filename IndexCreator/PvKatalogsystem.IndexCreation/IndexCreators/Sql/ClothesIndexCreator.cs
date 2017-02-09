using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PvKatalogsystem.IndexCreation.Documents;
using Absolute.SolrClient;
using System.Data.SqlClient;
using System.Data;
using PvKatalogsystem.IndexCreation.Helper;
using Microsoft.SqlServer.Server;
using PvKatalogsystem.IndexCreation.Mapper.Sql;

namespace PvKatalogsystem.IndexCreation.IndexCreators.Sql
{
    public class ClothesIndexCreator : IndexCreatorBase, IIndexCreator
    {
        public SolrIndex<ClothesDocument> _solrIndex;

        public override void Init(string connectionString, Uri solrUri)
        {
            base.Init(connectionString, solrUri);

            _solrIndex = new SolrIndex<ClothesDocument>(solrUri.ToString(), Name);
        }

        public string Name
        {
            get { return "Clothes"; }
        }

        public ISolrIndex SolrIndex
        {
            get { return _solrIndex; }
        }

        public ICollection<int> DetermineObjectIds()
        {
            var objectIds = new LinkedList<int>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new SqlCommand("_SolrDetermineClothes", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt32("Id");
                            if (id.HasValue)
                                objectIds.AddLast(id.Value);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return objectIds;
        }

        public ICollection<ISolrDocument> CreateDocuments(IEnumerable<int> objectIds)
        {
            IEnumerable<ISolrDocument> docList = null;
            var idList = new LinkedList<SqlDataRecord>();
            
            foreach (var id in objectIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, id);
                idList.AddLast(record);
                
            }
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new SqlCommand("_SolrGetClothes", conn))
                {
                    comm.CommandTimeout = 500;
                    comm.CommandType = CommandType.StoredProcedure;
                    var param = new SqlParameter("@objectIds", SqlDbType.Structured);
                    param.Direction = ParameterDirection.Input;
                    param.TypeName = "IdsType";
                    param.Value = idList;
                    comm.Parameters.Add(param);
                    using (var reader = comm.ExecuteReader())
                    {
                        docList = ClothesDocumentMapper.Map(reader);
                        reader.Close();
                    }
                }
                conn.Close();
            }

            return docList.ToList();
        }

    }
}

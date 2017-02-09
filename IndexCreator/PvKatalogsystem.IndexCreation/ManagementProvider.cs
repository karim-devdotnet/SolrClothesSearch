using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PvKatalogsystem.IndexCreation.Helper;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlClient;

namespace PvKatalogsystem.IndexCreation
{
    public class ManagementProvider
    {
        private readonly string _connectionString;

        internal ManagementProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal bool CreateTableIfNotExists(string indexName)
        {
            var result = false;
            var tableName = IndexCreationController.ManagementTablePrefix + indexName;
            if (!SqlHelper.ExistsObject(tableName, _connectionString))
            {
                var query = SqlHelper.GetSqlScript("CreateManagementTable.sql");
                query = query.Replace("{{TableName}}", tableName);
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var comm = new SqlCommand(query, conn))
                    {
                        result = comm.ExecuteNonQuery() > 0;
                    }
                    conn.Close();
                }
            }
            return result;
        }

        internal bool CreateIdsTypeIfNotExists()
        {
            var result = false;
            if (!SqlHelper.ExistsType("IdsType", _connectionString))
            {
                var query = SqlHelper.GetSqlScript("CreateIdsType.sql");
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var comm = new SqlCommand(query, conn))
                    {
                        result = comm.ExecuteNonQuery() > 0;
                    }
                    conn.Close();
                }
            }
            return result;
        }

        internal bool InsertDeterminedIds(IEnumerable<int> objectIds, string indexName)
        {
            var result = false;
            var insertList = new LinkedList<SqlDataRecord>();
            foreach (var id in objectIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, id);
                insertList.AddLast(record);
            }
            var query = SqlHelper.GetSqlScript("InsertDetermineIds.sql");
            query = query.Replace("{{TableName}}", IndexCreationController.ManagementTablePrefix + indexName);
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var comm = new SqlCommand(query, conn))
                {
                    var param = new SqlParameter("@ids", SqlDbType.Structured);
                    param.Direction = ParameterDirection.Input;
                    param.TypeName = "IdsType";
                    param.Value = insertList;
                    comm.Parameters.Add(param);

                    result = comm.ExecuteNonQuery() > 0;
                }
                conn.Close();
            }
            return result;
        }
    }
}

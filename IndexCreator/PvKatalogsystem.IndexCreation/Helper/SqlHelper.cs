using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.IO;

namespace PvKatalogsystem.IndexCreation.Helper
{
    internal static class SqlHelper
    {
        internal static string GetSqlScript(string scriptName)
        {
            string script = null;
            foreach (var name in Assembly.GetCallingAssembly().GetManifestResourceNames())
            {
                if (name.Contains(scriptName))
                {
                    script = new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(name)).ReadToEnd();

                }
            }
            return script;
        }

        internal static bool ExistsType(string typeName, string connectionString)
        {
            var result = false;
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var comm = new SqlCommand(string.Format("SELECT TYPE_ID('{0}')", typeName), conn))
                {
                    using (var reader = comm.ExecuteReader())
                    {
                        reader.Read();
                        result = !(reader[0] is DBNull);
                    }

                }
                conn.Close();
            }
            return result;
        }


        internal static bool ExistsObject(string objectName, string connectionString)
        {
            var result = false;
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var comm = new SqlCommand(string.Format("SELECT OBJECT_ID('{0}')", objectName), conn))
                {
                    using (var reader = comm.ExecuteReader())
                    {
                        reader.Read();
                        result = !(reader[0] is DBNull);
                    }

                }
                conn.Close();
            }
            return result;
        }

    }
}

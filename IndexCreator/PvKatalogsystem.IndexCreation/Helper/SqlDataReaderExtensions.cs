using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace PvKatalogsystem.IndexCreation.Helper
{
    public static class SqlDataReaderExtensions
    {
        public static bool? GetBoolean(this SqlDataReader reader, string fieldName)
        {
            var ord = reader.GetOrdinal(fieldName);
            return reader.IsDBNull(ord) ? null : (bool?)reader.GetBoolean(ord);
        }

        public static int? GetInt32(this SqlDataReader reader, string fieldName)
        {
            var ord = reader.GetOrdinal(fieldName);
            return reader.IsDBNull(ord) ? null : (int?)reader.GetInt32(ord);
        }

        public static long? GetInt64(this SqlDataReader reader, string fieldName)
        {
            var ord = reader.GetOrdinal(fieldName);
            return reader.IsDBNull(ord) ? null : (long?)reader.GetInt64(ord);
        }

        public static decimal? GetDecimal(this SqlDataReader reader, string fieldName)
        {
            var ord = reader.GetOrdinal(fieldName);
            return reader.IsDBNull(ord) ? null : (decimal?)reader.GetDecimal(ord);
        }

        public static DateTime? GetDateTime(this SqlDataReader reader, string fieldName)
        {
            var ord = reader.GetOrdinal(fieldName);
            return reader.IsDBNull(ord) ? null : (DateTime?)reader.GetDateTime(ord);
        }

        public static string GetString(this SqlDataReader reader, string fieldName)
        {
            var ord = reader.GetOrdinal(fieldName);
            return reader.IsDBNull(ord) ? null : reader.GetString(ord);
        }

    }
}

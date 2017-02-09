using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Data.SqlClient;

namespace PvKatalogsystem.IndexCreation.IndexCreators
{
    public abstract class IndexCreatorBase
    {
        protected string ConnectionString;
        protected Uri SolrUri;

        public virtual void Init(string connectionString, Uri solrUri)
        {
            ConnectionString = connectionString;
            SolrUri = solrUri;
        }


    }
}

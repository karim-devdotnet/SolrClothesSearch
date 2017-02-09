using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Absolute.SolrClient
{
    public class SolrFieldAttribute : Attribute
    {
        public SolrFieldAttribute()
        {
        }

        public string Name { get; set; }
        public bool Stored { get; set; }
        public bool Indexed { get; set; }
    }
}

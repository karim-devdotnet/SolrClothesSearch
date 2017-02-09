using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesSearch.Services.Solr.Filter
{
    public class FilterFacet
    {
        public Boolean IsActive
        {
            get;
            set;
        }

        public String FilterQuery
        {
            get;
            set;
        }

        public String DisplayName
        {
            get;
            set;
        }

        public Int32 Count
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesSearch.Services.DTOs
{
    [Serializable]
    public class FacetFilterDto
    {
        public String FilterQuery { get; set; }

        public String DisplayName { get; set; }

        public Int32 RowCount { get; set; }

        public Boolean IsActive { get; set; }
    }
}

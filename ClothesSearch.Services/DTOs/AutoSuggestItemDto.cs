using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesSearch.Services.DTOs
{
    [Serializable]
    public class AutoSuggestItemDto
    {
        public String FilterQuery { get; set; }

        public String DisplayName { get; set; }

        public Int32 RowCount { get; set; }

        public AutoSuggestItemTypes Type { get; set; }
    }
}

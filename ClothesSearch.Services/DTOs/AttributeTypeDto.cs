using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesSearch.Services.DTOs
{
    public class AttributeTypeDto
    {
        public AttributeTypeDto()
        {
            AttributeValues = new List<FacetFilterDto>();
        }

        public string DisplayName { get; set; }
        public int RowCount { get; set; }
        public List<FacetFilterDto> AttributeValues { get; set; }
    }
}

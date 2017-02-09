using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesSearch.Services.DTOs
{
    [Serializable]
    public class FilterDto
    {
        public FilterDto()
        {
            CategorieFilters = new List<FacetFilterDto>();
            MarkeFilters = new List<FacetFilterDto>();
            Attributes = new List<AttributeTypeDto>();
        }

        public List<FacetFilterDto> CategorieFilters { get; set; }
        public List<FacetFilterDto> MarkeFilters { get; set; }
        public List<AttributeTypeDto> Attributes { get; set; }
        public String AttributValueFilter { get; set; }
    }
}

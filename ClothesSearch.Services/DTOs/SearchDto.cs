using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ClothesSearch.Services.DTOs
{
    [Serializable]
    public class SearchDto
    {
        public SearchDto()
        {
            Filters = new FilterDto();
            Articles = new List<ArticleDto>();
        }

        public String Query { get; set; }

        public FilterDto Filters { get; set; }

        public List<ArticleDto> Articles { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesSearch.Services.DTOs
{
    public class ArticleDto
    {
        public ArticleDto()
        {
            Attribut = new LinkedList<string>();
        }
        public string Id { get; set; }

        public string Artikelnummer { get; set; }

        public string Preiseinheit { get; set; }

        public string Geschlecht { get; set; }

        public decimal? Verkaufspreis { get; set; }

        public string Kategorie { get; set; }

        public string Marke { get; set; }

        public ICollection<string> Attribut { get; set; }
    }
}

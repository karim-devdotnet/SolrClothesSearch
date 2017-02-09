using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet.Attributes;

namespace ClothesSearch.Services.Solr.Documents
{
    public class ClothesDocument
    {
        public ClothesDocument()
        {
            Attribut = new LinkedList<string>();
            Attributtyp = new LinkedList<string>();
            Attributwert = new LinkedList<string>();

            KategorieSuche = new LinkedList<string>();
            MarkeSuche = new LinkedList<string>();

            Suchtext = new LinkedList<string>();
        }

        [SolrField("Id")]
        public string Id { get; set; }

        [SolrField("Artikelnummer")]
        public string Artikelnummer { get; set; }

        [SolrField("Preiseinheit")]
        public string Preiseinheit { get; set; }

        [SolrField("Geschlecht")]
        public string Geschlecht { get; set; }

        [SolrField("Verkaufspreis")]
        public decimal? Verkaufspreis { get; set; }

        [SolrField("Kategorie")]
        public string Kategorie { get; set; }

        [SolrField("KategorieSuche")]
        public ICollection<string> KategorieSuche { get; set; }

        [SolrField("Marke")]
        public string Marke { get; set; }

        [SolrField("MarkeSuche")]
        public ICollection<string> MarkeSuche { get; set; }

        [SolrField("Attribut")]
        public ICollection<string> Attribut { get; set; }

        [SolrField("Attributtyp")]
        public ICollection<string> Attributtyp { get; set; }

        [SolrField("Attributwert")]
        public ICollection<string> Attributwert { get; set; }

        [SolrField("Suchtext")]
        public ICollection<string> Suchtext { get; set; }
    }
}

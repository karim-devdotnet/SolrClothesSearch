using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Absolute.SolrClient;

namespace PvKatalogsystem.IndexCreation.Documents
{
    public class ClothesDocument : ISolrDocument
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

        [SolrField(Name = "Id")]
        public string Id { get; set; }

        [SolrField(Name = "Artikelnummer")]
        public string Artikelnummer { get; set; }

        [SolrField(Name = "Preiseinheit")]
        public string Preiseinheit { get; set; }

        [SolrField(Name = "Geschlecht")]
        public string Geschlecht { get; set; }

        [SolrField(Name = "Verkaufspreis")]
        public decimal? Verkaufspreis { get; set; }

        [SolrField(Name = "Kategorie")]
        public string Kategorie { get; set; }

        [SolrField(Name = "KategorieSuche")]
        public ICollection<string> KategorieSuche { get; set; }

        [SolrField(Name = "Marke")]
        public string Marke { get; set; }

        [SolrField(Name = "MarkeSuche")]
        public ICollection<string> MarkeSuche { get; set; }

        [SolrField(Name = "Attribut")]
        public ICollection<string> Attribut { get; set; }

        [SolrField(Name = "Attributtyp")]
        public ICollection<string> Attributtyp { get; set; }

        [SolrField(Name = "Attributwert")]
        public ICollection<string> Attributwert { get; set; }

        [SolrField(Name = "Suchtext")]
        public ICollection<string> Suchtext { get; set; }

    }
}

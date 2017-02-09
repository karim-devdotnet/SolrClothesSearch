using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PvKatalogsystem.IndexCreation.Documents;
using System.Data.SqlClient;
using PvKatalogsystem.IndexCreation.Helper;

namespace PvKatalogsystem.IndexCreation.Mapper.Sql
{
    public class ClothesDocumentMapper
    {
        public static IEnumerable<ClothesDocument> Map(SqlDataReader reader)
        {
            var docList = new Dictionary<string, ClothesDocument>();
            while (reader.Read())
            {
                var doc = new ClothesDocument();
                doc.Id = (reader.GetInt32("ArtikelId") ?? 0).ToString();

                // Doppelte Einträge checken
                if (docList.ContainsKey(doc.Id))
                {
                    LogManager.Error(string.Format("Doppeltes Objekt gefunden. ObjectId: {0}", doc.Id));
                    continue;
                }

                doc.Artikelnummer = reader.GetString("Artikelnummer");
                if (!doc.Suchtext.Contains(doc.Artikelnummer)) doc.Suchtext.Add(doc.Artikelnummer);

                doc.Geschlecht = reader.GetString("Geschlecht");

                doc.Verkaufspreis = reader.GetDecimal("Verkaufspreis") ?? 0;

                doc.Preiseinheit = reader.GetString("Preiseinheit");

                doc.Kategorie = reader.GetString("Kategorie");
                doc.KategorieSuche.Add(doc.Kategorie);
                if (!doc.Suchtext.Contains(doc.Kategorie)) doc.Suchtext.Add(doc.Kategorie);

                doc.Marke = reader.GetString("Marke");
                doc.MarkeSuche.Add(doc.Marke);
                if (!doc.Suchtext.Contains(doc.Marke)) doc.Suchtext.Add(doc.Marke);

                docList.Add(doc.Id, doc);
            }

            // Attribute
            if (reader.NextResult())
            {
                MapAttribute(reader, docList);
            }           

            var result = docList.Select(x => x.Value);

            return result;
        }


        private static void MapAttribute(SqlDataReader reader, IDictionary<string, ClothesDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var ids = new List<string>();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                ids.Add(artikelId);

                var attrId = reader.GetInt32("AttributId") ?? 0;
                var attrTyp = reader.GetString("Attributetyp");
                var attrWert = reader.GetString("Attributwert");
                docList[artikelId].Attribut.Add(string.Format("{0}¦{1}¦{2}", attrId.ToString(), attrTyp, attrWert));

                if (!docList[artikelId].Suchtext.Contains(attrTyp)) docList[artikelId].Suchtext.Add(attrTyp);
                if (!docList[artikelId].Suchtext.Contains(attrWert)) docList[artikelId].Suchtext.Add(attrWert);

                //Attributtyp
                if (!string.IsNullOrEmpty(attrTyp))
                {
                    if (!docList[artikelId].Attributtyp.Contains(attrTyp)) docList[artikelId].Attributtyp.Add(string.Format("{0}¦{1}",attrId.ToString(),attrTyp));
                }
                //Attributwert
                if (!string.IsNullOrEmpty(attrWert))
                {
                    if (!docList[artikelId].Attributwert.Contains(attrTyp)) docList[artikelId].Attributwert.Add(string.Format("{0}¦{1}", attrId.ToString(), attrWert));
                }

            }
        }

        

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PvKatalogsystem.IndexCreation.Documents;
using System.Data.SqlClient;
using PvKatalogsystem.IndexCreation.Helper;
using Absolute.SolrClient;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;

namespace PvKatalogsystem.IndexCreation.Mapper
{
    internal class ArtikelDocumentMapper
    {
        public static IEnumerable<ArtikelDocument> Map(SqlDataReader reader)
        {
            var docList = new Dictionary<string, ArtikelDocument>();
            while (reader.Read())
            {
                var doc = new ArtikelDocument();
                doc.Id = (reader.GetInt32("ArtikelId") ?? 0).ToString();

                // Doppelte Einträge checken
                if (docList.ContainsKey(doc.Id))
                {
                    LogManager.Error(string.Format("Doppeltes Objekt gefunden. ObjectId: {0}", doc.Id));
                    continue;
                }

                // Grunddaten
                doc.ArtikelnummerSuche = reader.GetString("Artikelnummer");
                doc.Artikelnummer = reader.GetString("Artikelnummer");
                doc.Katalognummer = reader.GetString("Katalognummer");
                doc.Vorschaubild = reader.GetString("Vorschaubild");
                doc.IsLieferbar = reader.GetBoolean("IsLieferbar") ?? false;
                doc.IsPseudoartikel = reader.GetBoolean("IsPseudoartikel") ?? false;
                doc.Bezeichnung = reader.GetString("Bezeichnung");
                doc.Marketingbezeichnung = reader.GetString("Marketingbezeichnung");
                doc.Warengruppe = reader.GetString("Warengruppe");
                doc.PitstopWarengruppe = reader.GetString("PitstopWarengruppe");
                doc.HasMaterialCertification = reader.GetBoolean("HasMaterialCertification") ?? false;
                doc.IsRemanufacturedPart = reader.GetBoolean("IsRemanufacturedPart") ?? false;
                doc.IsSelfServicePackage = reader.GetBoolean("IsSelfServicePackage") ?? false;

                // Hersteller
                doc.Herstellername = reader.GetString("HerstellerName");
                doc.Herstellernummer = reader.GetInt64("HerstellerNummer") ?? long.MaxValue;
                doc.Hersteller = string.Format("{0}{{{1}}}¦{2}", doc.Herstellername, reader.GetInt32("HerstellerId"), reader.GetString("HerstellerLogo"));

                // Verpackungseinheit
                doc.VerpackungseinheitBezeichnung = reader.GetString("Mengeneinheitsbezeichnung");
                doc.IsVerpackungseinheitGanzzahl = reader.GetBoolean("IsGanzzahlMengeneingabe") ?? false;

                // Preiseinheit
                doc.PreiseinheitMenge = reader.GetDecimal("Preiseinheitsmenge") ?? 0;
                doc.PreiseinheitBezeichnung = reader.GetString("PreisEinheitBezeichnung");

                doc.PreisEinheitUmrechnungsFaktor = reader.GetDecimal("PreisEinheitUmrechnungsFaktor") ?? 0;
                doc.TechnischeMengeneinheitBezeichnung = reader.GetString("TechnischeMengeneinheitBezeichnung");
                doc.MengenEinheitsFaktor = reader.GetDecimal("MengenEinheitsFaktor") ?? 0;
               
                var verkaufspreis = reader.GetDecimal("Verkaufspreis") ?? 0;
                if (verkaufspreis > 0)
                {
                    doc.Verkaufspreis.Add(0, verkaufspreis);
                }

                // Topprodukt
                var tpId = reader.GetInt32("TopproduktId");
                if (tpId.HasValue)
                {
                    var tpTyp = reader.GetString("TopproduktTyp");
                    var tpGueltigVon = reader.GetDateTime("TopproduktGueltigVon");
                    var tpGueltigBis = reader.GetDateTime("TopproduktGueltigBis");
                    var tpNotiz = reader.GetString("TopproduktNotiz");
                    var sameKTypeOnly = reader.GetBoolean("TopproduktSameKTypeOnly") ?? false;
                    var isTp = reader.GetBoolean("IsTopprodukt") ?? false;

                    doc.Topprodukt = string.Format("{0}¦{1}¦{2}¦{3}¦{4}¦{5}", 
                            tpTyp, 
                            tpGueltigVon.HasValue ? tpGueltigVon.Value.ToUniversalTime().ToString("u", DateTimeFormatInfo.InvariantInfo).Replace(" ", "T") : null,
                            tpGueltigBis.HasValue ? tpGueltigBis.Value.ToUniversalTime().ToString("u", DateTimeFormatInfo.InvariantInfo).Replace(" ", "T") : null,
                            tpNotiz,
                            sameKTypeOnly ? 1 : 0,
                            tpId
                        );
                    doc.IsTopprodukt = isTp;
                }


                // AutoSuggest Hersteller
                doc.SuggestHersteller.Add(doc.Herstellername);

                // Suchtext
                doc.Suchtext.Add(doc.Herstellername);
                doc.Suchtext.Add(doc.Marketingbezeichnung);
                foreach (var syn in doc.Synonym)
                {
                    doc.Suchtext.Add(syn);
                }


                docList.Add(doc.Id, doc);

            }

            if (reader.NextResult())
            {
                MapGenericArticle(reader, docList);
            }

            if (reader.NextResult())
            {
                MapZusatzbezeichnung(reader, docList);
            }

            if (reader.NextResult())
            {
                MapBreadcrumbs(reader, docList);
            }

            if (reader.NextResult())
            {
                MapFahrzeuge(reader, docList);
            }

            if (reader.NextResult())
            {
                MapCsoInfos(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAttribute(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAttributbloecke(reader, docList);
            }

            if (reader.NextResult())
            {
                MapInformationen(reader, docList);
            }

            if (reader.NextResult())
            {
                MapGebrauchsnummern(reader, docList);
            }

            if (reader.NextResult())
            {
                MapReferenznummern(reader, docList);
            }

            if (reader.NextResult())
            {
                MapEaNummern(reader, docList);
            }

            if (reader.NextResult())
            {
                MapVergleichsnummern(reader, docList);
            }

            if (reader.NextResult())
            {
                MapErsatzartikelnummern(reader, docList);
            }

            if (reader.NextResult())
            {
                MapB2COPreise(reader, docList);
            }

            if (reader.NextResult())
            {
                MapArtikelsortierung(reader, docList);
            }

            if (reader.NextResult())
            {
                MapHerstellersortierung(reader, docList);
            }

            if (reader.NextResult())
            {
                MapDokumente(reader, docList);
            }

            if (reader.NextResult())
            {
                MapLinks(reader, docList);
            }

            if (reader.NextResult())
            {
                MapStuecklisten(reader, docList);
            }

            if (reader.NextResult())
            {
                MapVerwandteArtikel(reader, docList);
            }

            if (reader.NextResult())
            {
                MapTauschartikel(reader, docList);
            }

            if (reader.NextResult())
            {
                MapGefahrgutinfos(reader, docList);
            }

            if (reader.NextResult())
            {
                MapLogistikinfos(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAktionsinfos(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAktionsnummerKundengruppe(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAktionsnummerAbsatzgruppe(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAktionsnummerPreisgruppe(reader, docList);
            }

            if (reader.NextResult())
            {
                MapAktionsTeaser(reader, docList);
            }

            var result = docList.Select(x => x.Value);

            return result;
        }

        private static void MapAktionsTeaser(SqlDataReader reader, Dictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var i = 0;
            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                    i++;
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var teaserTyp = reader.GetString("Teasertyp");
                var aktNr = reader.GetInt64("Aktionsnummer");
                var sortNr = reader.GetInt64("Sortiernummer") ?? long.MaxValue;
                var isZufall = reader.GetBoolean("IsZufallsanzeige") ?? false;

                doc.TeaserAktion.Add(string.Format("{0}¦{1}¦{2}¦{3}", teaserTyp, sortNr, isZufall ? 1 : 0, aktNr));

            }
        }

        private static void MapErsatzartikelnummern(SqlDataReader reader, Dictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var i = 0;
            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                    i++;
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var artNr = reader.GetString("Artikelnummer");
                var typ = reader.GetString("Typ");
                doc.Ersatzartikelnummer.Add(string.Format("{0}¦{1}", typ, artNr));
            }
        }

        private static void MapVergleichsnummern(SqlDataReader reader, Dictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var i = 0;
            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                    i++;
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var artNr = reader.GetString("Artikelnummer");
                doc.Vergleichsnummer.Add(artNr);
            }
        }

        private static void MapEaNummern(SqlDataReader reader, Dictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var i = 0;
            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                    i++;
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var eaNr = reader.GetString("EaNummer");
                doc.EaNummer.Add(eaNr);
            }
        }

        private static void MapReferenznummern(SqlDataReader reader, Dictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var i = 0;
            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                    i++;
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var artNr = reader.GetString("Artikelnummer");
                var hersteller = reader.GetString("Hersteller");
                doc.Referenznummer.Add(string.Format("{0}¦{1}", hersteller, artNr));
                doc.ReferenznummerSuche.Add(artNr);
            }
        }

        private static void MapGenericArticle(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            var i = 0;
            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                    i++;
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var id = reader.GetInt32("GenArtId") ?? 0;
                var bezeichnung = reader.GetString("GenArtBezeichnung");
                if (!doc.GenArtId.Contains(id))
                {
                    doc.GenArtId.Add(id);
                    doc.GenerischerArtikel.Add(string.Format("{0}{{{1}}}", bezeichnung, id));
                    doc.Suchtext.Add(bezeichnung);
                }
            }
        }

        private static void MapZusatzbezeichnung(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var bezeichnung = reader.GetString("Zusatzbezeichnung");
                if (!doc.Zusatzbezeichnung.Contains(bezeichnung))
                {
                    doc.Zusatzbezeichnung.Add(bezeichnung);
                    doc.Suchtext.Add(bezeichnung);
                }
            }
        }

        private static void MapBreadcrumbs(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var suchbaumtyp = reader.GetString("Suchbaumtyp");
                if (suchbaumtyp == null)
                {
                    suchbaumtyp = "NONE";
                }
                if (!doc.Suchbaumtyp.Contains(suchbaumtyp))
                {
                    doc.Suchbaumtyp.Add(suchbaumtyp);
                }

                var breadcrumb = "";
                var index = 0;
                for (var i = 5; i >= 0; i--) 
                {
                    var node = reader.GetString("Node" + i);
                    var nodeId = reader.GetInt32("NodeId" + i);
                    if (nodeId == null)
                    {
                        continue;
                    }
                    breadcrumb += string.Format("{0}{{{1}}}¦", node, nodeId);
                    var key = string.Format("{0}_{1}", suchbaumtyp, index);
                    if (!doc.Suchbaumknoten.ContainsKey(key))
                    {
                        doc.Suchbaumknoten.Add(key, new LinkedList<string>());
                    }
                    if (i != 0)
                    {
                        doc.Suchbaumknoten[key].Add(breadcrumb.TrimEnd('¦'));
                    }
                    else
                    {
                        doc.Suchbaumknoten[key].Add(breadcrumb + "1");
                    }
                    index++;
                    if (node.StartsWith("Knoten"))
                    {
                        continue;
                    }
                    if (!doc.SuchbaumknotenSuche.Contains(node))
                    {
                        doc.SuchbaumknotenSuche.Add(node);
                    }
                }

                // AutoSuggest Kategorien
                var genArt = reader.GetString("Node0");
                if (genArt != null && !doc.SuggestKategorie.Contains(genArt))
                {
                    doc.SuggestKategorie.Add(genArt);
                }

                // Wenn Knotenbezeichnungen fehlen, sollen diese nicht indiziert werden.
                if (Regex.IsMatch(breadcrumb, @"Knoten\040[0-9]*"))
                {
                    continue;
                }

                var isSuggest = reader.GetBoolean("IsSuggest") ?? false;
                var isTopSuggest = reader.GetBoolean("IsTopSuggest") ?? false;
                if (!doc.Breadcrumb.ContainsKey(suchbaumtyp))
                {
                    doc.Breadcrumb.Add(suchbaumtyp, new LinkedList<string>());
                }
                doc.Breadcrumb[suchbaumtyp].Add(string.Format("{0}¦{1}¦{2}", isSuggest ? 1 : 0, isTopSuggest ? 1 : 0, breadcrumb + "1"));

            }
        }

        private static void MapFahrzeuge(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var fz = reader.GetString("Fahrzeug");
                var regAll = new Regex(@"^(.*)¦(.*)\{(\d*)\}¦(.*)\{(\d*)\}\{([\d\.]*)\}\{([\d\.]*)\}¦(.*)\{(\d*)\}\{([\d\.]*)\}\{([\d\.]*)\}");
                var match = regAll.Match(fz);
                if (!match.Success)
                {
                    continue;
                }

                var fzTyp = match.Groups[1].Value;
                var hstName = match.Groups[2].Value;
                var hstId = match.Groups[3].Value;
                var mdlBez = match.Groups[4].Value;
                var mdlId = match.Groups[5].Value;
                var mdlBjMin = match.Groups[6].Value;
                var mdlBjMax = match.Groups[7].Value;
                var fzBez = match.Groups[8].Value;
                var fzId = match.Groups[9].Value;
                var fzBjMin = match.Groups[10].Value;
                var fzBjMax = match.Groups[11].Value;
                var fzTypId = fzTyp + fzId;

                if (!doc.Fahrzeugtyp.Contains(fzTyp))
                {
                    doc.Fahrzeugtyp.Add(fzTyp);
                }
                if (!doc.FahrzeugId.Contains(fzTypId))
                {
                    doc.FahrzeugId.Add(fzTypId);
                }
                if (!doc.FahrzeugherstellerSuche.Contains(hstName))
                {
                    doc.FahrzeugherstellerSuche.Add(hstName);
                }
                if (!doc.FahrzeugmodellSuche.Contains(mdlBez))
                {
                    doc.FahrzeugmodellSuche.Add(mdlBez);
                }
                if (!doc.FahrzeugbezeichnungSuche.Contains(fzBez))
                {
                    doc.FahrzeugbezeichnungSuche.Add(fzBez);
                }

                var item = string.Format("{0}¦{1}{{{2}}}", fzTyp, hstName, hstId);
                if (!doc.Fahrzeughersteller.Contains(item))
                {
                    doc.Fahrzeughersteller.Add(item);
                }
                item = string.Format("{0}¦{1}¦{2}{{{3}}}{{{4}}}{{{5}}}", fzTyp, hstId, mdlBez, mdlId, mdlBjMin, mdlBjMax);
                if (!doc.Fahrzeugmodell.Contains(item))
                {
                    doc.Fahrzeugmodell.Add(item);
                }
                item = string.Format("{0}¦{1}¦{2}¦{3}{{{4}}}{{{5}}}{{{6}}}", fzTyp, hstId, mdlId, fzBez, fzId, fzBjMin, fzBjMax);
                if (!doc.Fahrzeugbezeichnung.Contains(item))
                {
                    doc.Fahrzeugbezeichnung.Add(item);
                }
            }
        }

        private static void MapCsoInfos(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var csoCode = reader.GetString("CsoCode");
                if (!doc.HlkArtikelnummer.ContainsKey(csoCode))
                {
                    doc.HlkArtikelnummer.Add(csoCode, string.Format("{0}¦{1}", reader.GetString("Hlka").Trim(), reader.GetString("ArticleNumber").Trim()));
                }
                if (!doc.CsoCode.Contains(csoCode))
                {
                    doc.CsoCode.Add(csoCode);
                }
            }
        }

        private static void MapAttribute(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var attribut = CreateAttribut(reader, doc, null, null);

                doc.Attribut.Add(attribut);

            }
        }

        private static void MapAttributbloecke(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var genArtId = reader.GetInt32("GenArtId") ?? 0;
                //var blockId = reader.GetInt64("BlockId") ?? 0;
                var blockSortNumber = reader.GetInt64("BlockSortNumber") ?? long.MaxValue;

                var fahrzeugtyp = reader.GetString("Fahrzeugtyp");
                var fahrzeugId = reader.GetInt32("FahrzeugId");

                var attribut = CreateAttribut(reader, doc, fahrzeugtyp, fahrzeugId);
                var fahrzeugIdent = string.Format("{0}{1}", fahrzeugtyp, fahrzeugId);

                if (!doc.FahrzeugId.Contains(fahrzeugIdent))
                {
                    continue;
                }

                if (!doc.Attributblock.ContainsKey(fahrzeugIdent))
                {
                    doc.Attributblock.Add(fahrzeugIdent, new List<string>());
                }
                var attrBloecke = (List<string>)doc.Attributblock[fahrzeugIdent];
                attrBloecke.Add(string.Format("{0}¦{1}¦{2}", genArtId, blockSortNumber, attribut));

            }
            
        }

        private static void MapInformationen(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var fzgTyp = reader.GetString("Fahrzeugtyp");
                var fzgId = reader.GetInt32("FahrzeugId");
                var name = reader.GetString("Name");
                var wert = reader.GetString("Wert");
                var displayImmediate = reader.GetBoolean("DisplayImmediate") ?? false;
                var sortiernummer = reader.GetInt64("Sortiernummer") ?? long.MaxValue;

                if (string.IsNullOrEmpty(wert))
                {
                    continue;
                }

                var entry = string.Format("{0}¦{1}¦{2}¦{3}", sortiernummer, displayImmediate ? 1 : 0, name, wert);

                if (fzgId.HasValue)
                {
                    var fzgTypId = string.Format("{0}{1}", fzgTyp, fzgId);
                    if (!doc.InformationByFahrzeug.ContainsKey(fzgTypId))
                    {
                        doc.InformationByFahrzeug.Add(fzgTypId, new LinkedList<string>());
                    }
                    doc.InformationByFahrzeug[fzgTypId].Add(entry);
                }
                else
                {
                    doc.Information.Add(entry);
                }
            }
        }

        private static void MapGebrauchsnummern(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;
                var nr = reader.GetString("Gebrauchsnummer");
                var dspImmediate = reader.GetBoolean("DisplayImmediate") ?? false;
                doc.Gebrauchsnummer.Add(string.Format("{0}¦{1}", dspImmediate ? 1 : 0, nr));
                doc.GebrauchsnummerSuche.Add(nr);
            }
        }

        private static void MapB2COPreise(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var bestand = reader.GetDecimal("Bestand");
                if (bestand.HasValue && bestand.Value == 0)
                {
                    doc.IsLieferbar = false;
                }

                for (var i = 1; i <= 5; i++)
                {
                    var preis = reader.GetDecimal("Kundenpreis0" + i) ?? 0;
                    if (preis > 0)
                    {
                        doc.Verkaufspreis.Add(i, preis);
                        doc.VerkaufspreisSort.Add(i, 1 / doc.PreiseinheitMenge * preis);
                    }
                }

            }
        }

        private static void MapArtikelsortierung(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var genart = reader.GetInt32("GenArtId");
                var sortnummer = reader.GetInt64("Sortiernummer");
                var fzgId = string.Format("{0}{1}", reader.GetString("Fahrzeugtyp"), reader.GetInt32("FahrzeugId"));
                if (!doc.FahrzeugId.Contains(fzgId))
                {
                    continue;
                }
                if (sortnummer.HasValue && !doc.Artikelsortiernummer.ContainsKey(fzgId))
                {
                    doc.Artikelsortiernummer.Add(fzgId, string.Format("{0}¦{1}", genart, sortnummer));
                }
            }
        }

        private static void MapHerstellersortierung(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var genart = reader.GetInt32("GenArtId");
                var sortnummer = reader.GetInt64("Sortiernummer");
                if (genart.HasValue && sortnummer.HasValue)
                {
                    doc.Herstellersortiernummer.Add(string.Format("{0}¦{1}", genart, sortnummer));
                }
            }
        }

        private static void MapDokumente(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var bezeichnung = reader.GetString("Bezeichnung");
                var dateityp = reader.GetString("Dateityp");
                var typ = reader.GetString("Typ");
                var dateiname = reader.GetString("Dateiname");
                var sortiernummer = reader.GetInt64("Sortiernummer") ?? long.MaxValue;

                var value = string.Format("{0}¦{1}¦{2}¦{3}¦{4}", sortiernummer, typ, bezeichnung, dateityp, dateiname);
                doc.Dokument.Add(value);
            }
        }

        private static void MapLinks(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var bezeichnung = reader.GetString("Bezeichnung");
                var beschreibung = reader.GetString("Beschreibung");
                var uri = reader.GetString("Uri");
                var sortiernummer = reader.GetInt64("Sortiernummer") ?? long.MaxValue;

                var value = string.Format("{0}¦{1}¦{2}¦{3}", sortiernummer, bezeichnung, uri, beschreibung);
                doc.Link.Add(value);
            }
        }

        private static void MapStuecklisten(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var sortiernummer = reader.GetInt64("Sortiernummer") ?? long.MaxValue;
                var typ = reader.GetString("Typ");
                var menge = reader.GetDecimal("Menge") ?? 1;
                var unterartikelId = reader.GetInt32("UnterartikelId");

                if (!unterartikelId.HasValue)
                {
                    continue;
                }

                var value = string.Format("{0}¦{1}¦{2}¦{3}", typ, sortiernummer, menge.ToString("0.###", culture), unterartikelId);
                
                doc.Stueckliste.Add(value);

            }
        }

        private static void MapVerwandteArtikel(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var grad = reader.GetInt32("Grad") ?? int.MaxValue;
                var typ = reader.GetString("Typ");
                var sameKTypeOnly = reader.GetBoolean("SameKTypeOnly") ?? false;
                var unterartikelId = reader.GetInt32("UnterartikelId");

                if (!unterartikelId.HasValue)
                {
                    continue;
                }

                var value = string.Format("{0}¦{1}¦{2}¦{3}", typ, grad, sameKTypeOnly ? 1 : 0, unterartikelId);

                doc.VerwandteArtikel.Add(value);
            }
        }

        private static void MapTauschartikel(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var csoCode = reader.GetString("CsoCode");
                var typ = reader.GetString("Typ");
                var unterartikelId = reader.GetInt32("UnterartikelId");
                var abDatum = reader.GetDateTime("AbDatum") ?? DateTime.Today;

                if (!unterartikelId.HasValue)
                {
                    continue;
                }

                var value = string.Format("{0}¦{1}¦{2}", typ, abDatum.ToString("u"), unterartikelId);

                if (!doc.Tauschartikel.ContainsKey(csoCode))
                {
                    doc.Tauschartikel.Add(csoCode, new List<string>());
                }
                doc.Tauschartikel[csoCode].Add(value);
            }
        }

        private static void MapGefahrgutinfos(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                for (var i = 1; i < reader.FieldCount; i++)
                {
                    var key = reader.GetName(i);
                    var type = reader.GetFieldType(i);
                    if (type == typeof(bool))
                    {
                        var val = reader.GetBoolean(key) ?? false;
                        if (val)
                        {
                            doc.Gefahrgutinformation.Add(key);
                        }
                    }
                    else if (type == typeof(decimal))
                    {
                        var val = reader.GetDecimal(key);
                        if (val.HasValue)
                        {
                            doc.Gefahrgutinformation.Add(string.Format("{0}¦{1}", key, val.Value.ToString("0.###", culture)));
                        }
                    }
                    else if (type == typeof(string))
                    {
                        var val = reader.GetString(key);
                        if (!string.IsNullOrEmpty(val))
                        {
                            doc.Gefahrgutinformation.Add(string.Format("{0}¦{1}", key, val));
                        }
                    }
                }
            }

            if (reader.NextResult())
            {
                docListEnum = docList.GetEnumerator();
                docsAvailable = docListEnum.MoveNext();
                while (reader.Read())
                {
                    var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                    while (docsAvailable && docListEnum.Current.Key != artikelId)
                    {
                        docsAvailable = docListEnum.MoveNext();
                    }
                    if (!docsAvailable)
                    {
                        break;
                    }

                    var doc = docListEnum.Current.Value;

                    var ausnahme = reader.GetString("Ausnahme");
                    if (!string.IsNullOrEmpty(ausnahme))
                    {
                        var ausnahmen = doc.Gefahrgutinformation.SingleOrDefault(x => x.StartsWith("Ausnahmen¦"));
                        if (!string.IsNullOrEmpty(ausnahmen))
                        {
                            doc.Gefahrgutinformation.Remove(ausnahmen);
                            ausnahmen += ", " + ausnahme;
                        }
                        else
                        {
                            ausnahmen = "Ausnahmen¦" + ausnahme;
                        }
                        doc.Gefahrgutinformation.Add(ausnahmen);
                    }
                }
            }
        }

        private static void MapLogistikinfos(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var fsa = reader.GetString("FirmaStandortAbteilung") ?? "000.000.000";
                if (!doc.Logistikinfo.ContainsKey(fsa))
                {
                    doc.Logistikinfo.Add(fsa, new List<string>());
                }

                var menge = reader.GetDecimal("MindestabnahmeMenge") ?? 0;
                var key = "Mindestabnahmemenge";
                if (menge == 0)
                {
                    menge = reader.GetDecimal("VerpackungsMenge") ?? 1;
                    key = "Verpackungsmenge";
                }

                var value = string.Format("{0}¦{1}", key, menge);

                doc.Logistikinfo[fsa].Add(value);
            }
        }

        private static void MapAktionsinfos(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var typ = reader.GetString("Typ");
                var aktNummer = reader.GetInt64("Aktionsnummer");
                if (!aktNummer.HasValue)
                {
                    continue;
                }

                var aktBez = reader.GetString("Aktionsbezeichnung");
                var aktName = reader.GetString("Aktionsanzeigename");
                var aktClaim = reader.GetString("Aktionsclaim");
                var aktLogo = reader.GetString("Aktionslogo");
                var aktNote = reader.GetString("Aktionsnotiz");
                var artName = reader.GetString("Artikelanzeigename");
                var artDesc = reader.GetString("Artikelbeschreibung");
                var artBild = reader.GetString("Artikelvorschaubild");
                var dateMin = reader.GetDateTime("AnzeigedatumMin");
                var dateMax = reader.GetDateTime("AnzeigedatumMax");
                var mengeMin = reader.GetDecimal("MengeMin");
                var mengeMax = reader.GetDecimal("MengeMax");
                var showVerf = reader.GetBoolean("ShowVerfuegbarkeit") ?? false;
                var showStrPr = reader.GetBoolean("ShowStreichpreis") ?? false;
                var bonusfaktor = reader.GetDecimal("Bonusfaktor");
                var bonuspunkte = reader.GetInt32("Bonuspunkte");
                var verkAktPreis = reader.GetDecimal("Verkaufsaktionspreis");

                var entry = string.Format(
                        culture, 
                        "{0}¦{1}¦{2}¦{3}¦{4}¦{5}¦{6}¦{7}¦{8}¦{9}¦{10}¦{11}¦{12:0.###}¦{13:0.###}¦{14}¦{15}¦{16:0.###}¦{17}¦{18:0.00}",
                        aktNummer, typ, aktBez, aktName, aktClaim, aktLogo, aktNote, artName, artDesc, artBild, 
                        dateMin.HasValue ? dateMin.Value.ToUniversalTime().ToString("u", DateTimeFormatInfo.InvariantInfo).Replace(" ", "T") : null,
                        dateMax.HasValue ? dateMax.Value.ToUniversalTime().ToString("u", DateTimeFormatInfo.InvariantInfo).Replace(" ", "T") : null,
                        mengeMin.HasValue ? (Math.Truncate(mengeMin.Value * 1000) / 1000) : (decimal?)null,
                        mengeMax.HasValue ? (Math.Truncate(mengeMax.Value * 1000) / 1000) : (decimal?)null,
                        showVerf ? 1 : 0,
                        showStrPr ? 1 : 0,
                        bonusfaktor,
                        bonuspunkte,
                        verkAktPreis
                    );

                doc.Aktionsinfo.Add(entry);
                
            }
        }

        private static void MapAktionsnummerAbsatzgruppe(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var typ = reader.GetString("Typ");
                var aktNummer = reader.GetInt64("Aktionsnummer");
                var absatzgruppe = reader.GetInt32("Absatzgruppe");
                if (!absatzgruppe.HasValue || !aktNummer.HasValue)
                {
                    continue;
                }

                if (!doc.AktionsnummerAbsatzgruppe.ContainsKey(absatzgruppe.Value))
                {
                    doc.AktionsnummerAbsatzgruppe.Add(absatzgruppe.Value, new LinkedList<long>());
                }
                doc.AktionsnummerAbsatzgruppe[absatzgruppe.Value].Add(aktNummer.Value);
            }
        }

        private static void MapAktionsnummerKundengruppe(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var typ = reader.GetString("Typ");
                var aktNummer = reader.GetInt64("Aktionsnummer");
                var kundengruppe = reader.GetInt32("Kundengruppe");
                if (!kundengruppe.HasValue || !aktNummer.HasValue)
                {
                    continue;
                }

                if (!doc.AktionsnummerKundengruppe.ContainsKey(kundengruppe.Value))
                {
                    doc.AktionsnummerKundengruppe.Add(kundengruppe.Value, new LinkedList<long>());
                }
                doc.AktionsnummerKundengruppe[kundengruppe.Value].Add(aktNummer.Value);
            }
        }

        private static void MapAktionsnummerPreisgruppe(SqlDataReader reader, IDictionary<string, ArtikelDocument> docList)
        {
            var culture = new CultureInfo("en-US");
            var docListEnum = docList.GetEnumerator();
            var docsAvailable = docListEnum.MoveNext();

            while (reader.Read())
            {
                var artikelId = (reader.GetInt32("ArtikelId") ?? 0).ToString();
                while (docsAvailable && docListEnum.Current.Key != artikelId)
                {
                    docsAvailable = docListEnum.MoveNext();
                }
                if (!docsAvailable)
                {
                    break;
                }

                var doc = docListEnum.Current.Value;

                var typ = reader.GetString("Typ");
                var aktNummer = reader.GetInt64("Aktionsnummer");
                for (var i = 1; i <= 5; i++)
                {
                
                }
                if (!aktNummer.HasValue)
                {
                    continue;
                }

                for (var i = 1; i <= 5; i++)
                {
                    if ((reader.GetInt64("Aktionsnummer0" + i) ?? 0) > 0)
                    {
                        if (!doc.AktionsnummerPreisgruppe.ContainsKey(i))
                        {
                            doc.AktionsnummerPreisgruppe.Add(i, new LinkedList<long>());
                        }
                        doc.AktionsnummerPreisgruppe[i].Add(aktNummer.Value);
                    }
                }
            }
        }

        #region Helper

        private static string CreateAttribut(SqlDataReader reader, ArtikelDocument doc, string fahrzeugtyp, int? fahrzeugId)
        {
            var culture = new CultureInfo("en-US");
            var hideName = reader.GetBoolean("HideName") ?? false;
            var displayImmediate = reader.GetBoolean("DisplayImmediate") ?? false;
            var sortNumber = reader.GetInt64("SortNumber") ?? long.MaxValue;
            var attribut = "";

            var alphaNumericTypeId = reader.GetInt32("AlphaNumericTypeId");
            var numericTypeId = reader.GetInt32("NumericTypeId");
            var dateTypeId = reader.GetInt32("DateTypeId");
            var keyTypeId = reader.GetInt32("KeyTypeId");
            var noValueTypeId = reader.GetInt32("NoValueTypeId");

            var attrTyp = (string)null;
            var attrWert = (string)null;

            if (alphaNumericTypeId.HasValue)
            {
                var type = reader.GetString("AlphaNumericType");
                var typeShort = reader.GetString("AlphaNumericTypeShort");

                var valueFrom = reader.GetString("AlphaNumericValue");
                var valueTo = reader.GetString("AlphaNumericValueTo");

                var value = string.IsNullOrEmpty(valueFrom) ? "..." : valueFrom;
                value += string.IsNullOrEmpty(valueTo) ? "" : " - " + valueTo;

                attribut = string.Format("{0}¦{1}¦{2}¦{3}¦{4}¦{5}¦", sortNumber, displayImmediate ? 1 : 0, hideName ? 1 : 0, type, typeShort, value);

                attrTyp = string.Format("{0}{1}¦{2}¦{3}", fahrzeugtyp, fahrzeugId, alphaNumericTypeId, typeShort);
                attrWert = string.Format("{0}{1}¦{2}¦{3}¦", fahrzeugtyp, fahrzeugId, alphaNumericTypeId, value);

                AddToCollection(doc.Suchtext, type, typeShort, valueFrom, valueTo);
                AddToCollection(doc.AttributSuche, type, typeShort, valueFrom, valueTo);
            }
            else if (numericTypeId.HasValue)
            {
                var type = reader.GetString("NumericType");
                var typeShort = reader.GetString("NumericTypeShort");

                var valueFrom = reader.GetDecimal("NumericValue");
                var valueTo = reader.GetDecimal("NumericValueTo");
                var unit = reader.GetString("NumericUnit");

                var value = valueFrom.HasValue ? string.Format("{0}", valueFrom.Value.ToString("0.###", culture), unit) : "...";
                value += valueTo.HasValue ? " - " + string.Format("{0}", valueTo.Value.ToString("0.###", culture), unit) : "";

                attribut = string.Format("{0}¦{1}¦{2}¦{3}¦{4}¦{5}¦{6}", sortNumber, displayImmediate ? 1 : 0, hideName ? 1 : 0, type, typeShort, value, unit);

                attrTyp = string.Format("{0}{1}¦{2}¦{3}", fahrzeugtyp, fahrzeugId, numericTypeId, typeShort);
                var numericValue = valueFrom.HasValue ? valueFrom.Value.ToString("0.###", culture) : valueTo.HasValue ? valueTo.Value.ToString("0.###", culture) : null;
                attrWert = string.Format("{0}{1}¦{2}¦{3}¦{4}", fahrzeugtyp, fahrzeugId, numericTypeId, string.Format("{0} {1}", value, unit).TrimEnd(), numericValue);

                AddToCollection(doc.Suchtext, type, typeShort, valueFrom.ToString(), valueTo.ToString(), unit);
                AddToCollection(doc.AttributSuche, type, typeShort, valueFrom.ToString(), valueTo.ToString(), unit);
            }
            else if (dateTypeId.HasValue)
            {
                var type = reader.GetString("DateType");
                var typeShort = reader.GetString("DateTypeShort");

                var valueFrom = reader.GetDateTime("DateValue");
                var valueTo = reader.GetDateTime("DateValueTo");

                var value = valueFrom.HasValue ? valueFrom.Value.ToString("MM.yyyy") : "...";
                value += valueTo.HasValue ? " - " + valueTo.Value.ToString("MM.yyyy") : "";

                attribut = string.Format("{0}¦{1}¦{2}¦{3}¦{4}¦{5}¦", sortNumber, displayImmediate ? 1 : 0, hideName ? 1 : 0, type, typeShort, value);

                attrTyp = string.Format("{0}{1}¦{2}¦{3}", fahrzeugtyp, fahrzeugId, dateTypeId, typeShort);
                attrWert = string.Format("{0}{1}¦{2}¦{3}¦", fahrzeugtyp, fahrzeugId, dateTypeId, value);

                AddToCollection(doc.Suchtext, type, typeShort);
                AddToCollection(doc.AttributSuche, type, typeShort);
                if (valueFrom.HasValue)
                {
                    AddToCollection(doc.Suchtext, valueFrom.Value.ToString("MM.yyyy"));
                    AddToCollection(doc.AttributSuche, valueFrom.Value.ToString("MM.yyyy"));
                }
                if (valueTo.HasValue)
                {
                    AddToCollection(doc.Suchtext, valueTo.Value.ToString("MM.yyyy"));
                    AddToCollection(doc.AttributSuche, valueTo.Value.ToString("MM.yyyy"));
                }
            }
            else if (keyTypeId.HasValue)
            {
                var type = reader.GetString("KeyType");
                var typeShort = reader.GetString("KeyTypeShort");

                var value = reader.GetString("KeyValue");

                attribut = string.Format("{0}¦{1}¦{2}¦{3}¦{4}¦{5}¦", sortNumber, displayImmediate ? 1 : 0, hideName ? 1 : 0, type, typeShort, value);

                attrTyp = string.Format("{0}{1}¦{2}¦{3}", fahrzeugtyp, fahrzeugId, keyTypeId, typeShort);
                attrWert = string.Format("{0}{1}¦{2}¦{3}¦", fahrzeugtyp, fahrzeugId, keyTypeId, value);

                AddToCollection(doc.Suchtext, type, typeShort, value);
                AddToCollection(doc.AttributSuche, type, typeShort, value);
            }
            else if (noValueTypeId.HasValue && !hideName)
            {
                var type = reader.GetString("NoValueType");
                var typeShort = reader.GetString("NoValueTypeShort");

                attribut = string.Format("{0}¦{1}¦{2}¦{3}¦{4}¦¦", sortNumber, displayImmediate ? 1 : 0, hideName ? 1 : 0, type, typeShort);

                AddToCollection(doc.Suchtext, type, typeShort);
                AddToCollection(doc.AttributSuche, type, typeShort);
            }

            // Attributtypen und -werte eintragen
            if (attrTyp != null && attrWert != null)
            {
                if (string.IsNullOrEmpty(fahrzeugtyp))
                {
                    attrTyp = attrTyp.TrimStart('¦');
                    attrWert = attrWert.TrimStart('¦');
                    if (!doc.Attributtyp.Contains(attrTyp))
                    {
                        doc.Attributtyp.Add(attrTyp);
                    }
                    doc.Attributwert.Add(attrWert);
                }
                else
                {
                    if (!doc.AttributtypFahrzeug.Contains(attrTyp))
                    {
                        doc.AttributtypFahrzeug.Add(attrTyp);
                    }
                    doc.AttributwertFahrzeug.Add(attrWert);
                }
            }

            return attribut;

        }

        private static void AddToCollection<T>(ICollection<T> collection, params T[] values)
        {
            foreach (var val in values)
            {
                if (val == null || string.IsNullOrEmpty(val as string))
                {
                    continue;
                }
                if (!collection.Contains(val))
                {
                    collection.Add(val);
                }
            }
        }

        #endregion

    }
}

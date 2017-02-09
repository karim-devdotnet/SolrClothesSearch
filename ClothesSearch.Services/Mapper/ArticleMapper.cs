using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClothesSearch.Services.DTOs;
using ClothesSearch.Services.Solr.Documents;
using System.Text.RegularExpressions;

namespace ClothesSearch.Services.Mapper
{
    public class ArticleMapper
    {
        private readonly static Regex _regex = new Regex(@"(\d*)¦(.*)¦(.*)$",RegexOptions.Compiled);

        public static ArticleDto Map(ClothesDocument doc)
        {
            var article = new ArticleDto();

            article.Id = doc.Id;
            article.Artikelnummer = doc.Artikelnummer;
            article.Geschlecht = doc.Geschlecht;
            article.Kategorie = doc.Kategorie;
            article.Marke = doc.Marke;
            article.Verkaufspreis = doc.Verkaufspreis;
            article.Preiseinheit = doc.Preiseinheit;
            article.Attribut = MapAttribut(doc.Attribut);

            return article;
        }

        public static ICollection<string> MapAttribut(ICollection<string> attrList)
        {
            var attribute = new List<string>();
            foreach (var att in attrList)
            {
                var match = _regex.Match(att);
                if (!match.Success) continue;

                attribute.Add(string.Format("{0}: {1}", match.Groups[2].Value, match.Groups[3].Value));
            }
            return attribute;
        }
    }
}

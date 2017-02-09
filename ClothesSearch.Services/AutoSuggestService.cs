using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet;
using ClothesSearch.Services.Solr.Documents;
using Microsoft.Practices.ServiceLocation;
using System.Configuration;
using ClothesSearch.Services.DTOs;
using ClothesSearch.Services.Solr.Query;

namespace ClothesSearch.Services
{
    public class AutoSuggestService
    {
        private readonly ISolrOperations<ClothesDocument> _solrOperations;

        public AutoSuggestService()
        {
            try 
            {
                _solrOperations =ServiceLocator.Current.GetInstance<ISolrOperations<ClothesDocument>>();
            }
            catch
            {
                Startup.Init<ClothesDocument>(ConfigurationManager.AppSettings["Solr_Clothes"]);
                _solrOperations = ServiceLocator.Current.GetInstance<ISolrOperations<ClothesDocument>>();
            }
        }

        public IEnumerable<AutoSuggestItemDto> ExecuteQuery(Int32 maxCount, String queryString, Func<string, string> encode, params string[] highlightBraces)
        {
            var result = new List<AutoSuggestItemDto>();

            //Categories
            var cat = new QueryCategories(_solrOperations);
            result.AddRange(cat.Execute(maxCount, queryString,encode,highlightBraces));

            //Brands
            var brand = new QueryBrands(_solrOperations);
            result.AddRange(brand.Execute(maxCount, queryString,encode, highlightBraces));

            return result;
        }
    }
}

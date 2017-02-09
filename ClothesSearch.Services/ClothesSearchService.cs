using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet;
using ClothesSearch.Services.Solr.Documents;
using Microsoft.Practices.ServiceLocation;
using System.Configuration;
using ClothesSearch.Services.DTOs;
using ClothesSearch.Services.Solr.Filter;
using ClothesSearch.Services.Solr.Query;
using SolrNet.Commands.Parameters;
using ClothesSearch.Services.Mapper;

namespace ClothesSearch.Services
{
    public class ClothesSearchService
    {
        public readonly ISolrOperations<ClothesDocument> _solrOperations;
        public ClothesSearchService()
        {
            try
            {
                _solrOperations = ServiceLocator.Current.GetInstance<ISolrOperations<ClothesDocument>>();
            }
            catch
            {
                Startup.Init<ClothesDocument>(ConfigurationManager.AppSettings["Solr_Clothes"]);
                _solrOperations = ServiceLocator.Current.GetInstance<ISolrOperations<ClothesDocument>>();
            }
        }

        public SearchDto ExecuteSearch(SearchDto parameterList)
        {
            //Kategorie
            var categorieFilter = new CategorieFilter();
            if (parameterList.Filters != null)
            {
                var filterQuery = parameterList.Filters.CategorieFilters.Where(x => x.IsActive).Select(x => x.FilterQuery).FirstOrDefault();
                if (filterQuery != null)
                {
                    categorieFilter.ActiveFilterQuery = filterQuery;
                }                    
            }

            //Marken
            var markeFilter = new BrandFilter();
            if (parameterList.Filters != null)
            {
                var filterQuery = parameterList.Filters.MarkeFilters.Where(x => x.IsActive).Select(x => x.FilterQuery).FirstOrDefault();
                if (filterQuery != null)
                {
                    markeFilter.ActiveFilterQuery = filterQuery;
                }
            }

            //Attributtyp
            var attributtypFilter = new AttributtypFilter();

            //Attributwert
            var attributvalueFilter = new AttributvalueFilter();
            if (parameterList.Filters != null)
            {
                var filterQuery = parameterList.Filters.AttributValueFilter;
                if (!string.IsNullOrEmpty(filterQuery))
                {
                    attributvalueFilter.ActiveFilterQuery = filterQuery;
                }
            }


            //Execute query
            var queryOptions = new QueryOptions() 
            {
                Start = 0,
                Rows = 100
            };

            var query = new QueryPhrase();
            query.Phrase = parameterList.Query;
            categorieFilter.SetQueryOptions(queryOptions);
            markeFilter.SetQueryOptions(queryOptions);
            attributtypFilter.SetQueryOptions(queryOptions);
            attributvalueFilter.SetQueryOptions(queryOptions);

            var solrResult = query.ExecuteQuery(_solrOperations,queryOptions);

            if (solrResult == null) return new SearchDto();

            //Result
            categorieFilter.GetResult(solrResult);
            markeFilter.GetResult(solrResult);
            attributtypFilter.GetResult(solrResult);
            attributvalueFilter.GetResult(solrResult);

            var searchResult=new SearchDto();

            //ArticleList
            foreach(var article in solrResult)
            {
                searchResult.Articles.Add(ArticleMapper.Map(article));
            
            }


            //Kategorie
            foreach (var facet in categorieFilter.FilterFacets)
            {
                searchResult.Filters.CategorieFilters.Add(new FacetFilterDto()
                    {
                        FilterQuery=facet.FilterQuery,
                        DisplayName=facet.DisplayName,
                        RowCount=facet.Count,
                        IsActive=facet.IsActive
                    });
            }

            //Marken
            foreach (var facet in markeFilter.FilterFacets)
            {
                searchResult.Filters.MarkeFilters.Add(new FacetFilterDto()
                {
                    FilterQuery = facet.FilterQuery,
                    DisplayName = facet.DisplayName,
                    RowCount = facet.Count,
                    IsActive = facet.IsActive
                });
            }

            //AttributFilter
            BuildAttributepairs(searchResult, attributtypFilter.FilterFacets, attributvalueFilter.FilterFacets);

            return searchResult;
        }

        /// <summary>
        /// Füllt den AttributePairs-Filter mit Werten ein.
        /// </summary>
        /// <param name="searchResult">Ergebnis-Objekt</param>
        /// <param name="typ">Attributnamen</param>
        /// <param name="value">Attributwerte</param>        
        private void BuildAttributepairs(SearchDto searchResult, ICollection<FilterFacet> typ, ICollection<FilterFacet> value)
        {
            AttributeTypeDto attType = null;

            List<FacetFilterDto> valueList = null;
            //FacetFilter_SortByDisplayName comparerByName = new FacetFilter_SortByDisplayName();

            foreach (var nameFacet in typ)
            {
                attType = new AttributeTypeDto();
                attType.DisplayName = nameFacet.DisplayName;
                attType.RowCount = nameFacet.Count;

                valueList = new List<FacetFilterDto>();

                foreach (var valueFacet in value.Select(x => x).Where(x => GetAttributId(x.FilterQuery) == GetAttributId(nameFacet.FilterQuery)).ToList())
                {

                    attType.AttributeValues.Add(new FacetFilterDto()
                    {
                        IsActive = valueFacet.IsActive,
                        DisplayName = valueFacet.DisplayName,
                        FilterQuery = valueFacet.FilterQuery,
                        RowCount = valueFacet.Count
                    });
                }

                //if (attType.AttributeValues.Count > 0) attType.AttributeValues.Sort(comparerByName);

                searchResult.Filters.Attributes.Add(attType);

            }

        }

        private string GetAttributId(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var str = input.Split('¦');
                return str[0];
            }
            else return null;
        }
    }
}

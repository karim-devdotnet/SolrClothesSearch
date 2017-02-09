using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothesSearch.Web.Models;
using ClothesSearch.Services;
using ClothesSearch.Services.DTOs;

namespace ClothesSearch.Web.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index(string query, string categoryFilter, string brandFilter, string attributValue)
        {
            //if (string.IsNullOrEmpty(query)) return View();

            var _service = new ClothesSearchService();
            
            var param = new SearchDto();
            param.Query = query;// string.IsNullOrEmpty(query) ? query : query.Replace(" & ", "+%26+");
            
            //Kategorie
            if(!string.IsNullOrEmpty(categoryFilter)) param.Filters.CategorieFilters.Add(new FacetFilterDto(){FilterQuery = categoryFilter , IsActive = true});
            //Marke
            if (!string.IsNullOrEmpty(brandFilter)) param.Filters.MarkeFilters.Add(new FacetFilterDto() { FilterQuery = brandFilter, IsActive = true });
            //Attributvalue
            if (!string.IsNullOrEmpty(attributValue)) param.Filters.AttributValueFilter = attributValue;


            var model = _service.ExecuteSearch(param);
            
            return View(model);
        }
        

        public ActionResult About()
        {
            return View();
        }
    }
}

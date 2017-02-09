using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothesSearch.Services;
using ClothesSearch.Web.Models;

namespace ClothesSearch.Web.Controllers
{
    public class AutoSuggestController : Controller
    {

        [HttpPost]
        public ActionResult Index(string query)
        {
            SuggestItemModel suggest = new SuggestItemModel();
            var model = suggest.Service.ExecuteQuery(10, query, HttpUtility.HtmlEncode, "<em>", "</em>");

            return PartialView(model);
        }

    }
}

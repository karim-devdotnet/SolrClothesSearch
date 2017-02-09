using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothesSearch.Services;

namespace ClothesSearch.Web.Models
{
    public class SuggestItemModel
    {
        public readonly AutoSuggestService Service;

        public SuggestItemModel()
        {
            Service = new AutoSuggestService();
        }
    }
}
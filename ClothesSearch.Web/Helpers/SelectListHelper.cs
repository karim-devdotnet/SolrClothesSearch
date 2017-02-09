using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesSearch.Web.Helpers
{
    public static class SelectListHelper
    {
        public static IEnumerable<SelectListItem> SelectList<T>(this HtmlHelper helper, IEnumerable<T> items, Func<T, string> textField, Func<T, string> valueField, string selectedValue, bool emptyItem = false)
        {
            if (items == null)
                return new List<SelectListItem>();

            var result = items.Select(x => new SelectListItem()
            {
                Text = textField(x),
                Value = valueField(x),
                Selected = selectedValue == valueField(x)
            }).ToList();

            if (emptyItem)
                result.Insert(0, new SelectListItem());

            return result;
        }
        public static IEnumerable<SelectListItem> SelectList<T>(this HtmlHelper helper, IEnumerable<T> items, Func<T, string> textField, Func<T, string> valueField, Func<T, bool> selectedField, bool emptyItem = false)
        {
            if (items == null)
                return new List<SelectListItem>();

            var result = items.Select(x => new SelectListItem()
            {
                Text = textField(x),
                Value = valueField(x),
                Selected = selectedField(x)
            }).ToList();

            if (emptyItem)
                result.Insert(0, new SelectListItem());

            return result;
        }
        public static IEnumerable<SelectListItem> SelectList<T>(this HtmlHelper helper, IEnumerable<T> items, Func<T, string> textField, Func<T, string> valueField, T selectedItem, bool emptyItem = false)
        {
            return SelectList(helper, items, textField, valueField, selectedItem == null ? null : valueField(selectedItem), emptyItem);
        }
        public static IEnumerable<SelectListItem> SelectList<T>(this HtmlHelper helper, IEnumerable<T> items, Func<T, string> textField, Func<T, string> valueField, bool emptyItem = false)
        {
            return SelectList(helper, items, textField, valueField, (string)null, emptyItem);
        }
    }
}
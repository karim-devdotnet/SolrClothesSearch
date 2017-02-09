/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/modernizr-1.7.js" />

//init
(function () {
    var api = function () {
//        var autoSuggestResult;

        var init = function () {
            //           $("#FreetextSearch input#Query").keyup(execAutoSuggest);
            $("html").click(function () {
                $(".AutoSuggest").slideUp(100);
            });
            $("#FilterList select").each(initFilterListAutoPostback);
//            $("#SortList select").each(initSortListAutoPostback);
            $(".VerticalMiddle").each(verticalMiddle);
        }

        var verticalMiddle = function () {
            var item = $(this);
            var container = item.parent();
            if (item[0].tagName == "IMG") {
                item.error(function () { item.replaceWith("<div style=\"text-align:center; background-color:#f2f2f2; line-height:" + container.innerHeight() + "px\">Not found</div>"); });
                item.load(function () { item.css("margin-top", parseInt((container.innerHeight() - item.outerHeight()) / 2)); });
                return;
            }
            item.css("margin-top", parseInt((container.innerHeight() - item.outerHeight()) / 2));
        }


        var initFilterListAutoPostback = function () {
            //            $(this).change(function () {
            //                $(this).closest("form").submit();
            //            });

            //CategoryFilter
            $('#categoryFilter').change(function () {
                $(this).closest("form").submit();
            });
            //BrandFilter
            $('#brandFilter').change(function () {
                $(this).closest("form").submit();
            });
           
        }

//        var initSortListAutoPostback = function () {
//            //Sort
//            $('#SortOrder').change(function () {
//                $(this).closest("form").submit();
//            });
//        }

        $(document).ready(init);
    };



    new api();

})()


//execAutoSuggest
function execAutoSuggest(url) 
{

    var form = $("#formSearch");
    var data = { query: form.find("#Query").val() };
    $.post(url, data, renderAutoSuggest,"html");
}


//renderAutoSuggest
var renderAutoSuggest = function (data) {
    var inputQuery = $("#formSearch input#Query");
    var autoSuggest = $(".AutoSuggest");

    if ($(data).children().length = 0) {
        autoSuggest.slideUp(100);
        return;
    }

    if (autoSuggest.length == 0) {
        $("body").append(data);
        autoSuggest = $(".AutoSuggest");
        autoSuggest.css({ "left": inputQuery.position().left, "top": inputQuery.position().top + inputQuery.outerHeight() + parseInt(inputQuery.css("margin-top")) });
    }
    else {
        autoSuggest.html($(data).html());
    }

    //event auf die items regesttrieren
    autoSuggest.find("ul > li").click(function (e) {

        e.stopPropagation();
        selectAutoSuggestItem(this)
    });

    //selectAutoSuggestItem
    var selectAutoSuggestItem = function (item) {
        var form = $("#formSearch");
        form.find("input#Query").val($(item).find(".DisplayName").text());
        $(".AutoSuggest").slideUp(100, function () {
            form[0].submit();
        });
    }

}


var selectedAttributs

function aFunction(elm) {
    var attributSel = elm.form.elements["attributValueFilter"]
    selectedAttributs = getSelections(attributSel)
    //alert(selectedAttributs)
    document.getElementById('attributValue').value = selectedAttributs;
    return false;
}

// returns references to the
// actual option elements (not values)
function getSelections(select) {
    var options = select.options,
sOptions = [],
opt, k = 0;
    while (opt = options[k++])
        if (opt.selected)
            sOptions[sOptions.length] = opt.value
        return sOptions
    }
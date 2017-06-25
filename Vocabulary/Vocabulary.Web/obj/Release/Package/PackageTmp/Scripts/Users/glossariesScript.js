$(document).ready(function () {
    var id = $("#language").html();
    var target = $("#list-glossary #" + id);
    target.addClass('disabled');

    var g = $("#glossary-language-menu li:first a:first");
    var t = $("#translation-language-menu li:first a:first");

    if (t.attr('id') == id) {
        t.attr('id', g.attr('id'));
        t = t.find('div');
        t.attr('class', '');
        t.addClass(g.get(0).children[0].className);
        t = t.parent();
    }
    if (g.attr('id') != id) {
        g.attr("id", id);
        g = g.find('div');
        g.attr('class', '');
        g.addClass(target.get(0).children[0].className);
    }
    $("#list-translation #" + id).addClass('disabled');
    $("#list-translation #" + t.attr('id')).addClass('disabled');
});

function onclickEvent(e) {
    var link = e.currentTarget;
    if (link.tagName != "A") return;
    var langId = $("#translation-language-menu li:first a:first").attr('id');
    window.location.href = "/User/Glossary/Glossary/" + link.getAttribute("id") + "?langId=" + langId;
}

$("#list-glossary a").click(function (e) {
    var target = e.currentTarget;
    if (target.classList.contains('disabled')) return;

    var gl = $("#glossary-language-menu li:first a:first"); // selected glossary language
    var tr = $("#translation-language-menu li:first a:first"); // selected translation language
    var id = target.getAttribute("id");
    if (tr.attr('id') == id) { //
        tr.attr('id', gl.attr('id'));
        tr = tr.find('div');
        tr.attr('class', '');
        tr.addClass(gl.get(0).children[0].className);
        tr = tr.parent();
    }
    if (gl.attr('id') != tr.attr('id')) {
        $('#list-translation #' + gl.attr('id')).removeClass('disabled');
    }

    gl.attr("id", id);
    gl = gl.find('div');
    gl.attr("class", "");
    gl.addClass(target.children[0].className);

    //delete disabled from previous language and add for current
    $('#list-glossary .disabled').removeClass("disabled");
    target.classList.add("disabled");

    //disable language for translation
    $("#list-translation #" + id).addClass("disabled");
    $("#list-translation #" + tr.attr('id')).addClass('disabled');

    changeGlossaryLanguage(id);
});

$("#list-translation a").click(function (e) {
    var t = e.currentTarget;
    if (t.classList.contains('disabled')) return;
    t.classList.add("disabled");

    var r = $("#translation-language-menu li:first a:first");
    r.attr("id", t.getAttribute("id"));

    //delete disabled from previous language
    $("#list-translation #" + r.attr('id')).removeClass("disabled");

    r = r.find('div');
    r.attr("class", "");
    r.addClass(t.children[0].className);

});

//unused. at least, now
function selectLanguageClass(id) {
    switch(id) {
        case '1': return "language-logo english-logo";
        case '2': return "language-logo ukrainian-logo";
        case '3': return "language-logo russian-logo";

        default: return "";
    }
}

$.ajaxSetup({
    beforeSend: function () {
        // show gif here, eg:
        $("#loading-container").show();
    },
    complete: function () {
        // hide gif here, eg:
        setTimeout(function () {
            $("#loading-container").hide();
        }, 1000);
    }
});

function changeGlossaryLanguage(id, page, count) {
    var url = "/User/Glossary/GlossariesData?langId=" + id;
    if (page != undefined) {
        url += "&page=" + page;
    }
    if (count != undefined) {
        url += "&count=" + page;
    }
    $.ajax({
        type: "GET",
        url: url,
        loadingElementId: "loading-container",
        success: function (data) {
            if (data == null) return;
            var cont = $("#glossaries-container");
            cont.html("");

            for (var i = 0; i < data.glossaries.length; i++) {
                var gl = data.glossaries[i];
                cont.append('<div class="glossary-container">' +
                    '<a id="' + gl.Id + '" href="javascript:void(0)"  onclick="onclickEvent(event)">' +
                    '<img src="/Glossary/GetGlossaryIcon/' + gl.Id + '" />' +
                    '<span>' + gl.Name + '</span>' +
                    '<span class="count">' + gl.Count + ' слов</span>' +
                    '<span class="language">' + data.list[data.language] + '</span></a></div>');
            }
        }
    });
}
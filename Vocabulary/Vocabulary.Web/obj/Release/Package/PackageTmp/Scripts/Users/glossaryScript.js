var $widget = $('.pop-up-widget');

function playAudio(id) {
    $("audio#" + id).get(0).play();
}

function getDataAjax(id, langId, index) {
    $.ajax({
        type: "GET",
        url: "/User/Glossary/GlossaryData/?glossaryId=" + id + "&langId=" + langId + "&index=" + index,
        success: function (data) {
            if (data == null) return;
            var p = $('.pop-up-widget');
            p.css('display', 'block');
            var t = p.find('tbody');
            t.html("");
            // example table
            for (var i = 0; i < data.examples.length; i++) {
                t.append('<tr>' +
                    '<td style="text-align: left">' + data.examples[i].Phrase + '</td>' +
                    '<td style="text-align: left">' + data.examples[i].Translation + '</td>' +
                    '<td><img src="/Content/soundIcon.png" style="width: 25px; height: 25px;" onclick="playAudio(' + data.examples[i].Id + ')" />' +
                    ' <audio id="' + data.examples[i].Id + '"> <source src="/User/Glossary/GetGlobalExampleAudio?id=' + data.examples[i].Id + '"/></audio></td>');
            }
            t = $(".widget-phrase-container");
            var cont = t.find('.phrase');
            cont.html("");
            cont.append(data.phrase.Phrase);

            cont = t.find('.transcription');
            cont.html("");
            cont.append(data.phrase.Transcription);

            if (data.translation != null) {
                cont = t.find('.translation');
                cont.html("");
                cont.append(data.translation.TranslationPhrase);
            }
            if (data.phrase.Frequency != null) {
                cont = t.find('.frequency');
                cont.html("");
                cont.append(data.phrase.Frequency);
            }

            cont = t.find('.audio');
            cont.find('audio').remove();
            cont.append("<audio  id='widget-audio'><source src='/User/Glossary/GetGlobalPhraseAudio?id=" + data.phrase.Id + "'/></audio>");

            //navigation buttons
            t = p.find('.left-button');
            if (data.nav.leftIndex == -1)
                t.addClass("disabled");
            else {
                t.removeClass("disabled");
                t.attr('data-id', id);
                t.attr('data-langId', langId);
                t.attr('data-index', data.nav.leftIndex);
            }

            t = p.find('.right-button');
            if (data.nav.rightIndex == -1)
                t.addClass("disabled");
            else {
                t.removeClass("disabled");
                t.attr('data-id', id);
                t.attr('data-langId', langId);
                t.attr('data-index', data.nav.rightIndex);
            }

            //add-buttons
            //TODO: add possibility to remove from vocabulary?
            t = p.find('.add-buttons');
            if (data.isAdded) {
                t.attr('data-status', 'added');
                t.find('a').addClass('disabled');
                cont = t.find('a:first');
                cont.text('Already added');

            } else {
                t.attr('data-status', 'not-added');
                t.attr('data-phrase-id', data.phrase.Id);
                t.find('a').removeClass('disabled');
                cont = t.find('a:first');
                cont.text('Add as learned');

            }

        }
    });
}

$(".pop-up-widget.background").click(function (e) {
    $('.pop-up-widget').css('display', 'none');
});

$(".left-button").add(".right-button").click(function (e) {
    var t = e.target;
    if (t.tagName != 'A') return;
    if (t.classList.contains('disabled')) return;
    getDataAjax(t.getAttribute('data-id'), t.getAttribute('data-langId'), t.getAttribute('data-index'));
});

$(".add-button").click(function (e) {
    var target = $(e.currentTarget).find('.not-added');
    if (!target) return;

    $.ajax({
        type: "POST",
        url: "/User/Glossary/AddPhraseToVocabulary/" + target.attr('data-phrase-id') + "?glossaryId=" + target.attr('data-glossary-id'),
        success: function(data) {
            if (!data.result) return;
            target.removeClass('not-added').addClass('added');
        }
    });

});

$('#select-all').click(function(e) {
    var elList = $('.add-button .not-added');
    var phraseIdList = [];
    for (var i = 0; i < elList.length; i++) {
        phraseIdList[i] = $(elList[i]).attr('data-phrase-id');
    }
    if (elList.length != 0) {
        $.ajax({
            type: "POST",
            url: "/User/Glossary/AddPhrasesToVocabulary?glossaryId=" + $(elList[0]).attr('data-glossary-id'),
            data: JSON.stringify({ "phraseIdList": phraseIdList }),
            contentType: "application/json",
            success: function (data) {
                if (!data.result) return;
                for (var i = 0; i < elList.length; i++) {
                    elList[i].removeClass('not-added').addClass('added');
                }
            }
        });
    }
});

$(".add-buttons a").click(function(e) {
    var t = $(e.currentTarget);
    var p = t.parent();
    if (p.attr('data-status') == 'added') return;
    var b = t.attr('data-status') == "learned";
    $.ajax({
        type: "POST",
        url: "/User/Glossary/AddPhraseToVocabulary/" + p.attr('data-phrase-id') + 
            "?glossaryId=" + p.attr('data-glossary-id') + "&status=" + b,
        success: function(data) {
            if (!data.result) return;
            p.attr('data-status', 'added');
            t.addClass('disabled');
            t.text('Already Added');
            $(".add-button .not-added[data-phrase-id='" + p.attr('data-phrase-id') + "']").removeClass('not-added').addClass('added');
        }
    });
});


var exAjax = null; //examples ajax
var dataAjax = null; //vocabulary data
var addDataAjax = null; //loading more vocabulary data async

var count = 300; //count of loaded items per request

function getVocabularyDataAsync() {
    var url = "/User/Vocabulary/VocabularyData?phraseLangId=" + plId +
        "&transLangId=" + trlId + "&phraseType=" + phraseType +
        "&glossary=" + glId +
        "&learnState=" + learnState + "&search=" + search;
    if (dataAjax && dataAjax.readyState != 4) {
        dataAjax.abort();
    }
    $contentContainer.html("");
    dataAjax = $.ajax({
        type: "POST",
        url: url,
        async: true,
        success: function (data) {
            if (data == null) return;
            var t = $contentContainer;
            $elCount.text(data.data.length);
            for (var i = 0; i < data.data.length; i++) {
                $(data.data[i]).appendTo(t).fadeIn('slow');
            }
        },
        beforeSend: function () {
            $loadContainer.show();
        },
        complete: function () {
            $loadContainer.hide();
        }
    });

}
function addVocabularyDataAsync() {
    var url = "/User/Vocabulary/VocabularyData?phraseLangId=" + plId +
        "&transLangId=" + trlId + "&phraseType=" + phraseType +
        "&glossary=" + glId + "&learnState=" + learnState +
        "&search=" + search + "&page=" + page + "&count=" + count;
    if (addDataAjax && addDataAjax.readyState != 4) {
        addDataAjax.abort();
    }

    addDataAjax = $.ajax({
        type: "POST",
        url: url,
        async: true,
        success: function (data) {
            if (data == null) return;
            if (data.data.length == 0) return;
            readyState = true;

            var t = $contentContainer;
            $elCount.text(data.data.length + parseInt($elCount.text()));
            for (var i = 0; i < data.data.length; i++) {
                $(data.data[i]).appendTo(t).fadeIn('slow');
            }
        },
        beforeSend: function () {
            $loadData.show();
        },
        complete: function () {
            $loadData.hide();
        }
    });

}

function getDataAsync(id, index) {
    var url = "/User/Vocabulary/VocabularyElementData/" + id + "?transLangId=" + trlId;
    if (exAjax && exAjax.readyState != 4) {
        exAjax.abort();
    }
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            if (data == null) return;
            exAjax = getExamplesAsync(data.phrase.Id, data.phrase.GlobalPhraseId);
            var p = $widget;
            p.css('display', 'block');

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
            //cont.append("<audio  id='widget-audio'><source src='/User/GetUsersPhraseAudio?id=" + data.phrase.Id + "'/></audio>");
            cont.append("<audio  id='widget-audio'><source src='" + data.audio + "'/></audio>");

            //auto-play
            if (autoPlay) {
                playAudio('widget-audio');
            }

            //navigation buttons
            var target = $('.content-container[data-phrase-id="' + id + '"]');

            t = p.find('.left-button');
            if (target.parent().prev()[0] == null)
                t.addClass("disabled");
            else {
                t.removeClass("disabled");
                t.attr('data-index', parseInt(index) - 1);
                cont = target.parent().prev().find('.content-container');
                t.attr('data-phrase-id', cont.attr('data-phrase-id'));
                t.attr('data-lang-id', cont.attr('data-lang-id'));
            }

            t = p.find('.right-button');
            if (target.parent().next()[0] == null)
                t.addClass("disabled");
            else {
                t.removeClass("disabled");
                t.attr('data-index', parseInt(index) + 1);
                cont = target.parent().next().find('.content-container');
                t.attr('data-phrase-id', cont.attr('data-phrase-id'));
                t.attr('data-lang-id', cont.attr('data-lang-id'));
            }

            //remove-button
            t = p.find('.remove-button');
            t.attr('data-phrase-id', data.phrase.Id);
        },
        beforeSend: function () {
            $loadPhrase.show();
        },
        complete: function () {
            $loadPhrase.hide();
        }
    });

}
function getExamplesAsync(id, glId) {
    var url = "/User/Vocabulary/VocabularyExamplesData/" + id + "?glId=" + glId + "&transLangId=" + trlId;
    var t = $widget.find('tbody');
    t.html("");
    $exCount.text("");
    return $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            if (data == null) return;
            // example table
            $exCount.text(data.examples.length);
            for (var i = 0; i < data.examples.length; i++) {
                $(data.examples[i]).appendTo(t).fadeIn('slow');
            }
        },
        beforeSend: function () {
            $loadExamples.show();
        },
        complete: function () {
            $loadExamples.hide();
        }
    });
}

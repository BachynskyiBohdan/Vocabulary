//initialization
$('#phrase-language').val($('.content-container').attr('data-lang-id'));
$('#translation-language').val($('.content-container').attr('data-trans-lang-id'));

var plId = $('#phrase-language').val();
var trlId = $('#translation-language').val();
var phraseType = $('.phrase-type .selected').text();
var learnState = -1;
var search = $('#search').val();
var glId = "All";

var autoPlay = false;

var $widget = $('.pop-up-widget');
var $menu = $('#main-header-menu');
var $menuSelected = $('#selected-header-menu');
var $curElement = $('.element-indicator span:first');
var $elCount = $('.element-indicator span:last');

var count = 0;

{ //search
    //setup before functions
    var typingTimer; //timer identifier
    var doneTypingInterval = 500; //time in ms, 0.5 second 
    var $input = $('#search');
    //on keyup, start the countdown
    $input.on('keyup', function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(doneTyping, doneTypingInterval);
    });
    //on keydown, clear the countdown 
    $input.on('keydown', function () {
        clearTimeout(typingTimer);
    });

    //user is "finished typing," do something
    function doneTyping() {
        search = $('#search').val();
        getVocabularyDataAsync();
    }
}

function playAudio(id) {
    $('audio#' + id)[0].play();
}

$(".pop-up-widget.background").click(function (e) {
    $widget.css('display', 'none');
});

$(document).on("click",".content-container", function (e) {
    var target = $(e.currentTarget);
    $curElement.text(target.attr('data-index'));
    getDataAsync(target.attr('data-phrase-id'), target.attr('data-index'));
});
$(document).on("click", '.phrase-container .add-button', function (e) {
    var t = $(e.currentTarget);
    var ch = $(t.find('div'));
    if (ch.hasClass('added')) {
        count--;
        $('.summary span').text(count);
        if (count == 0) {
            //hide sub-menu
            $menu.css('display', 'block');
            $menuSelected.css('display', 'none');
            $(".add-button.summary div").removeClass('added').addClass('not-added');
            $('.summary span').text("");
        }

        ch.removeClass('added').addClass('not-added');
    } else {
        if (count == 0) {
            //display sub-menu
            $menu.css('display', 'none');
            $menuSelected.css('display', 'block');
        } 
        count++;
        $('.summary span').text(count);

        ch.removeClass('not-added').addClass('added');
    }
});
$(document).on("click", '.phrase-container .glossary-info', function (e) {
    var t = $(e.currentTarget);
    glId = t.attr('data-glossary-id');
    var cont = $('.content-container:first');
    var btn = e.which;

    if (btn == 1) {
        getVocabularyDataAsync();
    } else if (btn == 2) {
        var url = "/Glossary/Glossary/" + glId + "?langId=" + cont.attr('data-trans-lang-id');
        window.open(url, '_blank');
    }
});
$(document).on("click", '.phrase-container .remove-button', function (e) {
    var t = $(e.currentTarget);
    var stateList = [t.parent().parent().find('.content-container').attr('data-phrase-id')];
    deleteElementsAjax(stateList);
});

$(".left-button").add(".right-button").click(function (e) {
    var t = e.currentTarget;
    if (t.classList.contains('disabled')) return;
    $curElement.text(t.getAttribute('data-index'));
    getDataAsync(t.getAttribute('data-phrase-id'), t.getAttribute('data-index'));
});
$('.pop-up-widget .remove-button').click(function (e) {
    var t = $(e.currentTarget);
    var stateList = [t.attr('data-phrase-id')];
    deleteElementsAjax(stateList);
    $widget.css('display', 'none');
});
$('.auto-play').click(function(e) {
    var t = $(e.currentTarget);
    t.toggleClass('disabled');
    autoPlay = !autoPlay;
});

//header-menu events
$('.add-button.summary').click(function(e) {
    var t = $(e.currentTarget);
    var ch = $(t.find('div'));
    var array = $('.phrase-container .add-button div');
    array.each(function(index) {
        var el = $(this);
        if (ch.hasClass('added')) {
            el.removeClass('added').addClass('not-added');
        } else {
            el.removeClass('not-added').addClass('added');
        }
    });
    if (ch.hasClass('added')) {
        $('.summary span').text("");
        count = 0;
        //hide sub-menu
        $menu.css('display', 'block');
        $menuSelected.css('display', 'none');
        ch.removeClass('added').addClass('not-added');
    } else {
        //display sub-menu
        $menu.css('display', 'none');
        $menuSelected.css('display', 'block');
        count = array.length;
        $('.summary span').text(count);

        ch.removeClass('not-added').addClass('added');
    }
});

// main-header-menu events
$('#main-header-menu #phrase-language').change(function () {
    plId = $('#phrase-language').val();
    getVocabularyDataAsync();
});
$('#main-header-menu #translation-language').change(function () {
    trlId = $('#translation-language').val();
    getVocabularyDataAsync();
});
$('#main-header-menu .phrase-type div').click(function (e) {
    var t = $(e.currentTarget);
    $('.phrase-type .selected').removeClass('selected');
    t.addClass('selected');
    phraseType = t.text();
    getVocabularyDataAsync();
});
$('#main-header-menu .learning-state > div').click(function (e) {
    var t = $(e.currentTarget);
    $('.learning-state .selected').removeClass('selected');
    t.addClass('selected');
    var ch = t.find('div');
    if (ch[0] == null)
        learnState = -1;
    else {
        switch (ch[0].classList[0]) {
            case "unknown":
                learnState = 0;
                break;
            case "first":
                learnState = 2;
                break;
            case "known":
                learnState = 1;
                break;
        }
    }
    getVocabularyDataAsync();
});

// selected-header-menu events
$('#selected-header-menu .learning-state .btn').click(function(e) {
    var t = $(e.currentTarget).find('div');
    var state = 0;
    switch (t[0].classList[0]) {
        case 'unknown': state = 0; break;
        case 'first': state = 25; break;
        case 'second': state = 50; break;
        case 'third': state = 75; break;
        case 'known': state = 100; break;
    }
    var array = $('.phrase-container .add-button .added');
    var stateList = [];
    array.each(function(i) {
        stateList[i] = $(this).parent().attr('data-phrase-id');
    });
    changeStateAjax(state, stateList, t[0].classList[0]);
});
function changeStateAjax(st, list, name) {
    $.ajax({
        type: "POST",
        url: "/User/ChangeState?state=" + st,
        data: JSON.stringify({ 'phraseIdList': list }),
        contentType: "application/json",
        success: function(data) {
            for (var i = 0; i < list.length; i++) {
                var t = $('.content-container[data-phrase-id="' + list[i] + '"]').parent().find(' .learning-state div')[0];
                t.className = name;
            }
        }
    });
}

$('#selected-header-menu .remove-button').click(function(e) {
    var array = $('.phrase-container .add-button .added');
    var stateList = [];
    array.each(function(i) {
        stateList[i] = $(this).parent().attr('data-phrase-id');
    });
    deleteElementsAjax(stateList);
});
function deleteElementsAjax(list) {
    $.ajax({
        type: "POST",
        url: "/User/DeleteFromVocabulary",
        data: JSON.stringify({ 'phraseIdList': list }),
        contentType: "application/json",
        success: function (data) {
            for (var i = 0; i < list.length; i++) {
                $('.content-container[data-phrase-id="' + list[i] + '"]').parent().remove();
            }
            var t = $(".add-button.summary");
            t.find("span").text("");
            $menu.css('display', 'block');
            $menuSelected.css('display', 'none');
            t.find("div").removeClass('added').addClass('not-added');
        }
    });
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

//parsing html also (always) can be done on the server side, but I do that here for clarity 
function getVocabularyDataAsync() {
    var url = "/User/VocabularyData?phraseLangId=" + plId +
        "&transLangId=" + trlId + "&phraseType=" + phraseType +
        "&glossary=" + glId + 
        "&learnState=" + learnState + "&search=" + search;
    $.ajax({
        type: "POST",
        url: url,
        success: function (data) {
            if (data == null) return;
            var t = $('.container.container-fluid');
            t.html('');
            $elCount.text(data.phrases.length);
            for (var i = 0; i < data.phrases.length; i++) {
                var phrase = data.phrases[i];
                var translation = data.translations[i];
                var str = '<div class="phrase-container"><div class="add-button" data-phrase-id="' + phrase.id + '"><div class="not-added"></div></div>' +
                    '<div class="audio"><img src="/Content/soundIcon.png" style="width: 25px; height: 25px;" onclick="playAudio(\'audio-' + phrase.Id + '\')" />' +
                    '<audio id="audio-' + phrase.Id + '"><source src="/User/GetUsersPhraseAudio/' + phrase.Id + '"/></audio></div>' +
                    '<div class="content-container" data-phrase-id="' + phrase.Id + '" data-lang-id="' + phrase.LanguageId + '" data-index="' + (i+1) + '"' +
                    ' data-trans-lang-id="' + translation.LanguageId + '">' + '<div class="phrase">' + phrase.Phrase + '</div>';
                if (phrase.Transcription != null)
                    str += '<div class="transcription">' + phrase.Transcription + '</div>';
                str += '—<div class="translation">' + translation.TranslationPhrase + '</div></div><div class="information">';
                if (phrase.GlossaryId != null)
                    str += '<div class="glossary-info" data-glossary-id="'+phrase.GlossaryId+'"><a href="javascript:void(0)" title="' + phrase.GlossaryName + '">'
                        + phrase.GlossaryName + '</a> </div>';

                str += '<div class="remove-button" data-phrase-id="' + phrase.Id + '" title="Remove phrase from vocabulary"></div>' +
                    '<div class="learning-state" title="learning state: ' + (phrase.LearningState * 100) + '%">';
                switch ((phrase.LearningState * 100)) {
                    case 0:
                        str += '<div class="unknown"></div>';
                        break;
                    case 25:
                        str += '<div class="first"></div>';
                        break;
                    case 50:
                        str += '<div class="second"></div>';
                        break;
                    case 75:
                        str += '<div class="third"></div>';
                        break;
                    case 100:
                        str += '<div class="known"></div>';
                        break;
                }
                str += '</div>';
                if (phrase.Frequency != null)
                    str += '<div class="frequency" title="word rank">' + phrase.Frequency + '</div>';
                str += '</div></div>';

                t.append(str);
            }
        }
    });

}
function getDataAsync(id, index) {
    var url = "/User/VocabularyElementData/" + id + "?langId=" + plId + "&transLangId=" + trlId;
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            if (data == null) return;
            var p = $widget;
            p.css('display', 'block');
            var t = p.find('tbody');
            t.html("");
            // example table
            for (var i = 0; i < data.examples.length; i++) {
                var str = '<tr>' +
                    '<td style="text-align: left">' + data.examples[i].Phrase + '</td>' +
                    '<td style="text-align: left">' + data.examples[i].Translation + '</td>' +
                    '<td><img src="/Content/soundIcon.png" style="width: 25px; height: 25px;" onclick="playAudio(' + data.examples[i].Id + ')" />' +
                    ' <audio id="' + data.examples[i].Id;
                if (data.examples[i].IsUsersExample) {
                    str += '"> <source src="/User/GetUsersExampleAudio?id=' + data.examples[i].Id + '"/></audio></td>';
                } else {
                    str += '"> <source src="/User/GetGlobalExampleAudio?id=' + data.examples[i].Id + '"/></audio></td>';
                }
                t.append(str);
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
            cont.append("<audio  id='widget-audio'><source src='/User/GetUsersPhraseAudio?id=" + data.phrase.Id + "'/></audio>");

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
        }
    });
}


//keyboard events for navigation 
$(document).on('keypress', function (e) {
    if ($widget.css('display') == 'none') return;
    if (e.charCode == 32) {
        playAudio('widget-audio');
        e.preventDefault();
    } else {
        switch (e.keyCode) {
            case 37:
            {
                $('.left-button').click();
                break;
            }
            case 39:
            {
                $('.right-button').click();
                break;
            }
            case 13:
            {
                $('.resize-button').click();
                break;
            }
            case 38:
            case 40:
            {
                e.preventDefault();
            }
        }
    }
});

$('remove-button').click(function (e) {
    var t = $(e.currentTarget);
    var id = t.attr('data-phrase-id');
    if (id) {
        $.ajax({
            type: "POST",
            url: "/User/DeleteFromVocabulary/" + id,
            success: function (data) {
                if (data.result) {
                    $('.content-container[data-phrase-id="' + id + '"]').parent().remove();
                }
            }
        });
    }
});


// widget expand set-up
var wg = $widget.find('.widget-glossary-container');
var lb = $widget.find(".left-button");
var rb = $widget.find(".right-button");
var wp = $widget.find(".widget-phrase-container");
var we = $widget.find(".widget-examples-container");

//widget-glossary-container
var wg_top = "5%";
var wg_top_small = "20%";

var wg_width = "80%";
var wg_width_small = "35%";

var wg_height = "85%";
var wg_height_small = "60%";

//navigation-button
var nb_left = "5.5%";
var nb_left_small = "25%";

//widget-phrase-container
var wp_width = "40%";
var wp_width_small = "100%";

var wp_bottom = "40%";
var wp_bottom_small = "35%";

$('.resize-button').click(function (e) {
    var t = $(e.currentTarget);
    t.toggleClass('expand');
    if (t.hasClass('expand')) {
        wg.css('top', wg_top);
        wg.css('width', wg_width);
        wg.css('height', wg_height);

        lb.css('left', nb_left);
        rb.css('right', nb_left);

        wp.css('width', wp_width);
        wp.css('bottom', wp_bottom);

        we.css('display', 'block');
    } else {
        we.css('display', 'none');

        wg.css('top', wg_top_small);
        wg.css('width', wg_width_small);
        wg.css('height', wg_height_small);

        lb.css('left', nb_left_small);
        rb.css('right', nb_left_small);

        wp.css('width', wp_width_small);
        wp.css('bottom', wp_bottom_small);
    }
});
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
var $contentContainer = $(".container.container-fluid");
var $exCount = $('.widget-examples-container span:first');

var $curElement = $('.element-indicator span:first');
var $elCount = $('.element-indicator span:last');

var $loadPhrase = $('.loading-logo#loading-phrase');
var $loadExamples = $('.loading-logo#loading-examples');
var $loadContainer = $('.loading-logo#loading-data');
var $loadData = $('.loading-logo#loading-more-data');

var selectedCount = 0;

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
        selectedCount--;
        $('.summary span').text(selectedCount);
        if (selectedCount == 0) {
            //hide sub-menu
            $menu.css('display', 'block');
            $menuSelected.css('display', 'none');
            $(".add-button.summary div").removeClass('added').addClass('not-added');
            $('.summary span').text("");
        }

        ch.removeClass('added').addClass('not-added');
    } else {
        if (selectedCount == 0) {
            //display sub-menu
            $menu.css('display', 'none');
            $menuSelected.css('display', 'block');
        } 
        selectedCount++;
        $('.summary span').text(selectedCount);

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
        selectedCount = 0;
        //hide sub-menu
        $menu.css('display', 'block');
        $menuSelected.css('display', 'none');
        ch.removeClass('added').addClass('not-added');
    } else {
        //display sub-menu
        $menu.css('display', 'none');
        $menuSelected.css('display', 'block');
        selectedCount = array.length;
        $('.summary span').text(selectedCount);

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
    phraseType = t.attr('data-type');
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
        url: "/User/Vocabulary/ChangeState?state=" + st,
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
        url: "/User/Vocabulary/DeleteFromVocabulary",
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

$('remove-button').click(function (e) {
    var t = $(e.currentTarget);
    var id = t.attr('data-phrase-id');
    if (id) {
        $.ajax({
            type: "POST",
            url: "/User/Vocabulary/DeleteFromVocabulary/" + id,
            success: function (data) {
                if (data.result) {
                    $('.content-container[data-phrase-id="' + id + '"]').parent().remove();
                }
            }
        });
    }
});
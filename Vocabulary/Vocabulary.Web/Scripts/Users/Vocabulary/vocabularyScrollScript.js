var readyState = true;
var page = 1;
var count = 300;

var $scrollNav = $('.scroll-navigation-container');
var $scrollNavIcon = $(".scroll-nav-logo");
var lastPos = 0;
var clicked = false;

$(window).scroll(function () {

    var scrollTop = document.documentElement.scrollTop;
    var height = $('.container.container-fluid').height() - $(window).height();

    if (scrollTop > 225) {
        $scrollNavIcon.addClass('top');
        $scrollNav.css('display', 'block');
        clicked = false;
    } else {
        $scrollNavIcon.removeClass('top');
        if (!clicked)
            $scrollNav.css('display', 'none');
    }

    //pre-loading vocabulary elements
    if (readyState & scrollTop > height) {
        //loading new portion of elements
        readyState = false;
        page++;
        addVocabularyDataAsync();
    }
});

$scrollNav.click(function (e) {
    var top = document.documentElement.scrollTop;
    if (top > 225) {
        document.documentElement.scrollTop = 0;
        lastPos = top;
        $scrollNavIcon.removeClass('top');
        clicked = true;
    } else {
        document.documentElement.scrollTop = lastPos;
        $scrollNavIcon.addClass('top');
    }
});
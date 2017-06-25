//keyboard events for navigation 
$(document).on('keypress', function (e) {
    if ($widget.css('display') == 'none') return;
    if (e.charCode == 32 | e.charCode == 48) {
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
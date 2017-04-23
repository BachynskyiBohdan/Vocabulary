$(document).ready(function () {
    //sortTable();
    setTimeout(function () {
        $('.alert').css("display", 'none');
    }, 2500);
});

function playAudio(str) {
    $('audio#' + str).get(0).play();
}

var lastIndex = 0;
$('th').click(function () {
    var col = $(this).index();
    if (this.innerHTML == "Audio" || this.innerHTML == "") return;
    var b = col != lastIndex;
    var table, rows, switching, i, x, y, shouldSwitch;
    table = document.querySelector('tbody');
    switching = true;
    /*Make a loop that will continue until
    no switching has been done:*/
    while (switching) {
        //start by saying: no switching is done:
        switching = false;
        rows = table.getElementsByTagName("TR");
        /*Loop through all table rows (except the
        first, which contains table headers):*/
        for (i = 0; i < (rows.length - 1) ; i++) {
            //start by saying there should be no switching:
            shouldSwitch = false;
            /*Get the two elements you want to compare,
            one from current row and one from the next:*/
            x = rows[i].getElementsByTagName("TD")[col];
            y = rows[i + 1].getElementsByTagName("TD")[col];
            //check if the two rows should switch place:
            if (b) {
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
                    //if so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            } else {
                if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {
                    //if so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            }

        }
        if (shouldSwitch) {
            /*If a switch has been marked, make the switch
            and mark that a switch has been done:*/
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
        }
    }
    if (!b) {
        lastIndex = -1;
    } else {
        lastIndex = col;
    }
});
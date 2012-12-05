$(
function () {

    var begin = new Date().getTime();

    window.setInterval(displayTime, 10);

    $('#clicker').click( recordTime );

    function displayTime() {
        var now = new Date().getTime();
        now = now - begin;
        $('#timer').html(now.toString());
    }

    function recordTime() {
        $('#click-times').html($('#click-times').html() + '<br />' + $('#timer').text());
    }
}
);
//move inside jQuery block after debuging
var words = [];
var images = [];
var consequences = [];
var spoken = [];
var startTime;
var clickGo;
var clickID1s = [];
var clickID2s = [];
var clickOptionTimes = [];
var optionsClicked = [];
var indicesClicked = [];
var trialIndex;

$(
function () {
    $('#gobutton').click(start);
    $('.trial').hide();

    function start() {
        console.log('Starting.');
        clickGo = markTime();
        $('#instructions').hide();
        $('#gobutton').off('click');
        trialIndex = 0;
        stepOne();
    }

    function stepOne() {
        console.log('Step one.');
        $('#trial-' + trialIndex).show();
        $('#id1-' + trialIndex).show();
        $('#id2-' + trialIndex).hide();
        $('.option').hide();
        $('#id1-' + trialIndex).click(stepTwo);
    }

    function stepTwo() {
        console.log('Step two.');
        clickID1s.push(markTime());
        $('#id1-' + trialIndex).hide();
        $('#id1-' + trialIndex).off('click');
        $('#id2-' + trialIndex).show();
        $('#id2-' + trialIndex).click( stepThree );
    }

    function stepThree() {
        console.log('Step three.');
        clickID2s.push(markTime());
        $('#id2-' + trialIndex).hide();
        $('#id2-' + trialIndex).off('click');
        $('#options-' + trialIndex).show();
        $('#option-' + trialIndex + '-1').show();
        $('#option-' + trialIndex + '-2').show();
        $('#option-' + trialIndex + '-3').show();
        $('#option-' + trialIndex + '-1').click( stepFour );
        $('#option-' + trialIndex + '-2').click( stepFour );
        $('#option-' + trialIndex + '-3').click( stepFour );
    }

    function stepFour() {
        console.log('Step four.');
        clickOptionTimes.push(markTime());
        optionsClicked.push($(this).data('wordID'));
        indicesClicked.push($(this).data('index'));
        if ($(this).data('correct') ) {
            alert('Correct!');
        }
        else {
            alert('Wrong!');
        }
        $('.option').off('click');
        $('.option').hide();
        trialIndex++;
        if (trialIndex < 3) stepOne();
        else end();
    }

    function end() {
        console.log('End trial block.');
        alert('Upload data to server here.');
    }

    function markTime() {
        console.log('Marking time.');
        return (new Date()).getTime() - startTime;
    }
    //$("#instructions").hide();
    
    //var correct = Math.floor(Math.random() * 3) + 1;
    //$('.option').click(mark);
    /*
    var start = (new Date()).getTime();

    function mark() {
        if ($(this).html() == correct) {
            $(this).css({ "background-color": "green" });
        }
        else {
            $(this).css({ "background-color": "red" });
        }
        alert(correct);
        var end = (new Date()).getTime();
        $('#timer').html(end - start);
        $('.option').off('click');
    }
    */
    //$.getJSON('Ajax/getWords/', 'listkey=1', function (data) { prep(data); } );

    var startTime = (new Date()).getTime();
});
$(
function () {
    var words = [];
    var images = [];
    var consequences = [];
    var spoken = [];
    var startTime;
    var clickGo;
    var showID1s = [];
    var showID2s = [];
    var clickID1s = [];
    var clickID2s = [];
    var optionIDs = [];
    var showOptionTimes = [];
    var clickOptionTimes = [];
    var optionsClicked = [];
    var trialIndex;
    var trialSize = $('#trialSize').val();
    var study = $('#sid').val();
    var trialType = $('#trialTypeKey').val();
    var playSounds = $('#playSounds').val();
    var results;

    $('#feedback').hide();
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
        words.push($('#trial-' + trialIndex + '-word').val());
        $('#trial-' + trialIndex).show();
        $('#id1-' + trialIndex).show();
        showID1s.push(markTime());
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
        showID2s.push(markTime());
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
        showOptionTimes.push(markTime());
        var options = [];
        options.push($('#option-' + trialIndex + '-1').data('wordid'));
        options.push($('#option-' + trialIndex + '-2').data('wordid'));
        options.push($('#option-' + trialIndex + '-3').data('wordid'));
        optionIDs.push(options);
        $('#option-' + trialIndex + '-1').click( stepFour );
        $('#option-' + trialIndex + '-2').click( stepFour );
        $('#option-' + trialIndex + '-3').click( stepFour );
    }

    function stepFour() {
        console.log('Step four.');
        clickOptionTimes.push(markTime());
        console.log('Option ID clicked: ' + $(this).data('wordid'));
        optionsClicked.push($(this).data('wordid'));
        $('.option').off('click');
        $('.option').hide();
        stepFive($(this).data('correct'));
    }

    function stepFive(correct) {
        if (correct) {
            if (playSounds) { document.getElementById('sound-' + trialIndex).play(); displayMessage('Correct.', 3000, stepSix); }
            else { displayMessage('Correct.', 3000, stepSix); }
        }
        else displayMessage(' ', 3000, stepSix);
    }

    function stepSix() {
        trialIndex++;
        if (trialIndex < trialSize) stepOne();
        else end();
    }

    function displayMessage(message, duration, callback) {
        $('#feedback').html(message);
        $('#feedback').show().delay(duration).fadeOut(callback);
    }

    function end() {

        console.log('End trial block.');

        var block = {
            timeStarted: JSON.stringify(startTime),
            userID: $('#userID').val(),
            studyID: study,
            typeID: trialType,
            goTime: clickGo,
            wordOrder: JSON.stringify(words),
            ID1shown: JSON.stringify(showID1s),
            ID1times: JSON.stringify(clickID1s),
            ID2shown: JSON.stringify(showID2s),
            ID2times: JSON.stringify(clickID2s),
            optionsPresented: JSON.stringify(showOptionTimes),
            choiceIDs: JSON.stringify(optionIDs),
            guessTimes: JSON.stringify(clickOptionTimes),
            guessesMade: JSON.stringify(optionsClicked)
        };

        console.log(block);

        results = $.ajax({
            type: "POST",
            url: "/Ajax/saveTrialBlock/",
            dataType: "json",
            data: block,
            async: false        
        }).responseText;

        displayMessage(results, 300000);
    }

    function markTime() {
        console.log('Marking time.');
        return (new Date()).getTime() - startTime.getTime();
    }

    var startTime = new Date();
});
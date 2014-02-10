$(
function () {
    $('#user_controls').hide();
    $('#group_controls').hide();
    $('#settings_controls').hide();
    $('#words_controls').hide();
    $('#resultsTab').addClass('selectedTab');
    $('.disable').prop('disabled', true);

    $('#usersTab').click(function () {
        $('#results_controls').hide();
        $('#group_controls').hide();
        $('#settings_controls').hide();
        $('#words_controls').hide();
        $('#user_controls').show();

        $('#usersTab').addClass('selectedTab');
        $('#settingsTab').removeClass('selectedTab');
        $('#groupsTab').removeClass('selectedTab');
        $('#wordsTab').removeClass('selectedTab');
        $('#resultsTab').removeClass('selectedTab');
    });
    $('#groupsTab').click(function () {
        $('#results_controls').hide();
        $('#group_controls').show();
        $('#settings_controls').hide();
        $('#words_controls').hide();
        $('#user_controls').hide();

        $('#usersTab').removeClass('selectedTab');
        $('#settingsTab').removeClass('selectedTab');
        $('#wordsTab').removeClass('selectedTab');
        $('#resultsTab').removeClass('selectedTab');
        $('#groupsTab').addClass('selectedTab');
    });
    $('#settingsTab').click(function () {
        $('#results_controls').hide();
        $('#group_controls').hide();
        $('#settings_controls').show();
        $('#words_controls').hide();
        $('#user_controls').hide();

        $('#usersTab').removeClass('selectedTab');
        $('#settingsTab').addClass('selectedTab');
        $('#wordsTab').removeClass('selectedTab');
        $('#resultsTab').removeClass('selectedTab');
        $('#groupsTab').removeClass('selectedTab');
    });
    $('#wordsTab').click(function () {
        $('#results_controls').hide();
        $('#group_controls').hide();
        $('#settings_controls').hide();
        $('#words_controls').show();
        $('#user_controls').hide();

        $('#usersTab').removeClass('selectedTab');
        $('#settingsTab').removeClass('selectedTab');
        $('#wordsTab').addClass('selectedTab');
        $('#resultsTab').removeClass('selectedTab');
        $('#groupsTab').removeClass('selectedTab');
    });
    $('#resultsTab').click(function () {
        $('#results_controls').show();
        $('#group_controls').hide();
        $('#settings_controls').hide();
        $('#words_controls').hide();
        $('#user_controls').hide();

        $('#usersTab').removeClass('selectedTab');
        $('#settingsTab').removeClass('selectedTab');
        $('#wordsTab').removeClass('selectedTab');
        $('#resultsTab').addClass('selectedTab');
        $('#groupsTab').removeClass('selectedTab');
    });
    $('#studyResultsPanel').change(updateControlPanel);
    $('#userResultsPanel').change(updateControlPanel);

    $('.userUpdateButton').click(function () {
        var key = this.id.replace("userChangeButton", "");
        var block = {
            userID: key,
            userActive: $('#activeForUser' + key).val(),
            userPassword: $('#passBox' + key).val(),
            studyID: $('#studyForUser' + key).val(),
            studyUserGroupID: $('#groupForUser' + key).val()
        };

        $.ajax({
            type: "POST",
            url: "/Ajax/userUpdate/",
            dataType: "html",
            data: block,
            success: function (msg) {
                $('.userPasswords').val('');
                alert(msg);
            }
        })
    });

    $('.userCreateButton').click(function () {
        var block = {
            userName: $('#newuser_name').val(),
            userActive: $('#newuser_active').val(),
            userPassword: $('#newuser_password').val(),
            studyID: $('#newuser_study').val(),
            studyUserGroupID: $('#newuser_group').val()
        };

        alert ($.ajax({
            type: "POST",
            url: "/Ajax/userCreate/",
            dataType: "html",
            data: block,
            async: false
        }).responseText);

        location.reload();

    });

    $('.studyCreateButton').click(function () {
        var block = {
            studyName: $('#newstudy_name').val(),
            hearIn: $('#newstudy_hear').val(),
            seeIn: $('#newstudy_see').val(),
            hours: $('#newstudy_hours').val(),
            minutes: $('#newstudy_minutes').val(),
            seconds: $('#newstudy_seconds').val(),
            trials: $('#newstudy_trials').val(),
            target: $('#newstudy_target').val(),
            adminID: $('#userID').val()
        };

        alert($.ajax({
            type: "POST",
            url: "/Ajax/studyCreate/",
            dataType: "html",
            data: block,
            async: false
        }).responseText);

        location.reload();

    });
    

    $('.studyUpdateButton').click(function () {
        var key = this.id.replace("studyChangeButton", "");
        var block = {
            studyID: key,
            hearIn: $('#hearI' + key).val(),
            seeIn: $('#seeI' + key).val(),
            hours: $('#hours' + key).val(),
            minutes: $('#minutes' + key).val(),
            seconds: $('#seconds' + key).val(),
            trials: $('#trials' + key).val(),
            fluency: $('#fluency' + key).val()
        };
        
        $.ajax({
            type: "POST",
            url: "/Ajax/studyUpdate/",
            dataType: "html",
            data: block,
            success: function (msg) {
                alert(msg);
            }
        })
    });
});

    function updateControlPanel() {
        var block = {
            studyID: $('#studyResultsPanel').val(),
            userID: $('#userResultsPanel').val()
        };
        if (block.studyID == "n/a" || block.userID == "n/a") {
            $('#dataView').replaceWith('<div class="subpanel" id="dataView">Select a study and user.</div>');
        }
        else {
            $.ajax({
                type: "POST",
                url: "/Ajax/adminPanel/",
                dataType: "html",
                data: block,
                success: function (result) {
                    $('#dataView').replaceWith(result);
                }
            });
        }
    }
$(document).ready(function () {

    /*
     * Show modal dialog for confirmation of deletion of tweet
     */
    $('#deleteTweetModal').on('show.bs.modal', function (event) {
        // Button that triggered the modal
        var button = $(event.relatedTarget);
        // Extract tweet's id from the button's 'data-tweet' attribute
        var tweetId = button.data('tweet');

        // Add 'action' attribute to the form with url to the delete action
        var deleteForm = $('#deleteTweetForm');
        // Provide 'page-handler' OnPostDeleteAsync and the tweet's id in the url
        deleteForm.attr('action', `/Index?handler=delete&id=${tweetId}`);

        // AJAX request to get tweet from database
        $.ajax({
            type: 'GET',
            url: `/Index?handler=Tweet&id=${tweetId}`,
            contentType: 'application/json',
            dataType: 'json',
            success: function (response) {
                var tweetInfoBody = $('#tweetInfoBody');
                tweetInfoBody.empty();

                // List for the userName and user's slug
                var $ul = $('<ul class="list-inline">');
                // Username as link to the profile
                var $userName = $('<li class="list-inline-item">')
                    .append(`<a href="/${response.authorSlug}">${response.authorName}</a>`);
                // User's slug as unclickable text
                var $userSlug = $('<li class="list-inline-item">')
                    .append(`<span class="text-muted">@${response.authorSlug}</span>`);
                // Appent two <li> tags to the <ul> and the whole list to the <div>
                $ul.append($userName, $userSlug).appendTo(tweetInfoBody);
                // Tweet's text. Appens to the <div>
                var $tweetText = $('<p>').append(response.text).appendTo(tweetInfoBody);
            },
            failure: function (response) {
                alert(response);
            }
        });
    });

    /*
     * After modal dialog is closed, remove content (tweet info) from the body
     */
    $('#deleteTweetModal').on('hide.bs.modal', function (event) {
        // Clean form's 'action' attribute if the form was just closed
        // and not submitted
        var deleteForm = $('#deleteTweetForm');
        deleteForm.removeAttr('action');

        // Clean div from tweet info
        var tweetInfoBody = $('#tweetInfoBody');
        tweetInfoBody.empty();
    });

    /*
     * Like button functionality
     */
    $('.btn-like').click(function (e) {
        var btnClicked = $(this);
        var tweetId = btnClicked.data('id');

        $.ajax({
            type: 'GET',
            url: `/Index?handler=UpdateLikes&id=${tweetId}`,
            contentType: 'application/json',
            dataType: 'json',
            success: function (response) {
                var tweetLikeCount = btnClicked.find('span');
                tweetLikeCount.text(`${response}`);
            },
            failure: function (response) {
                console.log(response);
            }
        });
    });

    /*
     * Send Tweet button disabled and hidden by default
     */
    $('#sendTweetForm input[type=submit]').prop('disabled', true);
    $('#sendTweetForm input[type=submit]').hide();

    /*
     * Show Send Tweet button when any element inside form is focused
     */
    $('#sendTweetForm').focusin(function () {
        $('#sendTweetForm input[type=submit]').show();
    });

    /*
     * Hide Send Tweet button when any element inside form loses
     * focus or text box doesn't have any value
     */
    $('#sendTweetForm').focusout(function () {
        if (!$(this).find('input[type = text]').val().trim())
            $('#sendTweetForm input[type=submit]').hide();
    });

    /*
     * Enable Send Tweet button only if text box has value
     */
    $('#sendTweetForm input[type=text]').keyup(function () {
        var sendTweetBtn = $('#sendTweetForm input[type=submit]');

        if ($(this).val().trim())
            sendTweetBtn.prop('disabled', false);
        else
            sendTweetBtn.prop('disabled', true);
    });

    /*
     * Click event for the main Follow button (profile page)
     */
    $('.btn-follow-main').click(function (e) {
        e.preventDefault();

        var pressedBtn = $(this);
        var userName = pressedBtn.find('span').data('username');

        $.ajax({
            type: 'POST',
            url: `/Index/?handler=Follow&userName=${userName}`,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: 'application/json',
            dataType: 'json',
            success: function (response) {
                pressedBtn.addClass('d-none');
                $('.btn-following-main').removeClass('d-none');
                $('#followersCount span').text(response);
            }
        });
    });

    /*
     * Show Unfollow button when mouse enters Following button
     */
    $('.btn-following-main').mouseenter(function () {
        $(this).addClass('d-none');
        $('.btn-unfollow-main').removeClass('d-none');
    });

    /*
     * Show Following button when mouse leaves Unfollow button
     */
    $('.btn-unfollow-main').mouseleave(function () {
        $(this).addClass('d-none');
        $('.btn-following-main').removeClass('d-none');
    });

});

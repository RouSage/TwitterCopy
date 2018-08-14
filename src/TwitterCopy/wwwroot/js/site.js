$(document).ready(function () {

    /*
     * Show modal dialog for confirmation of deletion of tweet
     */
    $('#deleteTweetModal').on('show.bs.modal', function (event) {
        // Button that triggered the modal
        var button = $(event.relatedTarget);
        // Extract tweet's id from the button's 'data-tweet' attribute
        var tweetId = button.data('tweet-id');

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
                AlertMessage.showAlertMessage(response);
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
    $(document.body).on('click', '.btn-like', function (e) {
        e.preventDefault();

        var btnClicked = $(this);
        var tweetId = btnClicked.data('tweet-id');

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
                AlertMessage.showAlertMessage(response);
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
     * Show Following button when mouse leaves Unfollow button
     */
    function unfollowMouseleaveHandler(unfollowBtn) {
        // hide Unfollow button when mouseleave
        unfollowBtn.addClass('d-none');
        // show Following button
        unfollowBtn.prev('.btn-following-main').removeClass('d-none');
    }

    /*
     * Enable mouseleave event for the Unfollow button by default
     */
    $(document.body).on('mouseleave', '.btn-unfollow-main', function (e) {
        var pressedBtn = $(e.currentTarget);
        if (!pressedBtn.hasClass('d-none'))
            unfollowMouseleaveHandler(pressedBtn);
    });
    /*
     * Show Unfollow button when mouse enters Following button
     */
    $(document.body).on('mouseenter', '.btn-following-main', function () {
        // hide Following button when hover
        $(this).addClass('d-none');
        // and show Unfollow button
        $(this).next('.btn-unfollow-main').removeClass('d-none');
    });

    /*
     * Click event for the main Follow button (profile page)
     */
    $(document.body).on('click', '.btn-follow-main', function (e) {
        e.preventDefault();

        var pressedBtn = $(this);
        var profileUserSlug = $('#profileInfoSlug').data('slug');
        var userToFollow = pressedBtn.find('span').data('userslug');
        var followingBtn = pressedBtn.next('.btn-following-main');
        var unfollowBtn = followingBtn.next('.btn-unfollow-main');

        $.ajax({
            type: 'POST',
            url: `/Index/?handler=Follow&userSlug=${userToFollow}`,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: 'application/json',
            dataType: 'json',
            success: function (response) {
                // hide Follow button
                pressedBtn.addClass('d-none');
                // show Following button
                followingBtn.removeClass('d-none');
                // update Followers count if you're on a followed user's page
                if (profileUserSlug === response.slug) {
                    $('#followersCount').text(response.count);
                }
                else if (profileUserSlug === response.currentUserSlug) {
                    $('#followingCount').text(response.count);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(JSON.parse(JSON.stringify(jqXHR.responseJSON)).message);
            }
        });
    });

    /*
     * Click event for the main unfollow button
     */
    $(document.body).on('click', '.btn-unfollow-main', function (e) {
        e.preventDefault();

        var pressedBtn = $(this);
        var profileUserSlug = $('#profileInfoSlug').data('slug');
        var userSlug = pressedBtn.find('span').data('userslug');
        var followingBtn = pressedBtn.prev('.btn-following-main');
        var followBtn = followingBtn.prev('.btn-follow-main');

        $.ajax({
            type: 'POST',
            url: `/Index?handler=Unfollow&userSlug=${userSlug}`,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: 'application/json',
            dataType: 'json',
            success: function (response) {
                // hide Unfollow button
                pressedBtn.addClass('d-none');
                // show Follow button
                followBtn.removeClass('d-none');
                // update Followers count if you're on the user's page only
                if (profileUserSlug === response.slug) {
                    $('#followersCount').text(response.count);
                }
                else if (profileUserSlug === response.currentUserSlug) {
                    $('#followingCount').text(response.count);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(JSON.parse(JSON.stringify(jqXHR.responseJSON)).message);
            }
        });
    });

    /*
     * Click event for the Retweet button
     */
    $(document.body).on('click', '.btn-retweet', function (e) {
        e.preventDefault();

        var clickedBtn = $(this);
        var tweetId = clickedBtn.data('tweet-id');

        $.ajax({
            type: 'POST',
            url: `/Index?handler=Retweet&id=${tweetId}`,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: 'application/json',
            dataType: 'json',
            success: function (response) {
                // find a span on the clicked button
                // which has a retweet count
                var tweetRetweetCount = clickedBtn.find('span');
                // set a new count of retweets
                tweetRetweetCount.text(`${response}`);
            },
            failure: function (response) {
                AlertMessage.showAlertMessage(response);
            }
        });
    });

    var ProfileEditor = {
        updateProfileInfo: function (newData) {
            $('#profileInfoUserName').text(newData.username);
            $('#profileInfoBio').text(newData.bio);
            $('#profileInfoLocation').text(newData.location);
            $('#profileInfoWebsite').text(newData.website);
        },
        saveChangesButtonClick: function () {
            $('#profileEdit').addClass('d-none');
            $('#profileInfo').removeClass('d-none');
            $('#cancelEditBtn').addClass('d-none');
            $('#saveEditBtn').addClass('d-none');
            $('#editProfileBtn').removeClass('d-none');
        },
        cancelButtonClick: function () {
            $('#cancelEditBtn').addClass('d-none');
            $('#saveEditBtn').addClass('d-none');
            $('#editProfileBtn').removeClass('d-none');
            $('#profileEdit').addClass('d-none');
            $('#profileInfo').removeClass('d-none');
        },
        editButtonClick: function () {
            $('#editProfileBtn').addClass('d-none');
            $('#cancelEditBtn').removeClass('d-none');
            $('#saveEditBtn').removeClass('d-none');
            $('#profileInfo').addClass('d-none');
            $('#profileEdit').removeClass('d-none');
        }
    };

    /*
     * Click event for the Edit profile button
     */
    $('#editProfileBtn').click(function (e) {
        e.preventDefault();
        // call function to hide Edit profile button
        // and show a Cancel, Save changes buttons
        // and a form for profile editing
        ProfileEditor.editButtonClick();
    });

    /*
     * Click event for the Cancel button
     * which cancels profile editing
     */
    $('#cancelEditBtn').click(function (e) {
        e.preventDefault();
        ProfileEditor.cancelButtonClick();
    });

    /*
     * Click for the Save changes button
     */
    $('#saveEditBtn').click(function (e) {
        // Get all values from the form for profile editing
        var token = $('input:hidden[name="__RequestVerificationToken"]').val();
        var userName = $('input[name="userName"]').val();
        var bio = $('input[name="bio"]').val();
        var location = $('input[name="location"]').val();
        var website = $('input[name="website"]').val();

        // Create new object which will be posted to the server
        // with new profile info values
        var postedValues = {};
        postedValues['__RequestVerificationToken'] = token;
        postedValues.UserName = userName;
        postedValues.Bio = bio;
        postedValues.Location = location;
        postedValues.Website = website;

        var postUrl = '/Profiles/Index?handler=EditUser';

        // Post values to the server
        $.post(postUrl, postedValues, function (response) {
            // update profile info
            ProfileEditor.updateProfileInfo(response);
            ProfileEditor.saveChangesButtonClick();
        }).fail(function (jqXHR, textStatus, errorThrown) {
            AlertMessage.showAlertMessage(JSON.parse(JSON.stringify(jqXHR.responseJSON)).message);
        });
    });

    $('.tweet-actionable').click(function (e) {
        var tweet = $(this);
        var tweetId = tweet.data('tweet-id');
        var userSlug = tweet.data('user-slug');

        if (e.target.localName !== 'button'
            && e.target.localName !== 'span'
            && e.target.localName !== 'a'
            && e.target.localName !== 'strong'
            && e.target.localName !== 'b') {
            $.ajax({
                type: 'GET',
                url: `/Profiles/Index/${userSlug}`,
                data: {
                    handler: 'Status',
                    tweetId: tweetId
                },
                contentType: 'application/json; charset=utf-8',
                success: function (response) {
                    var tweetModal = $('#tweetModal');
                    var tweetContainer = tweetModal.find('.modal-body');
                    tweetContainer.html(response);
                    tweetModal.modal('show');
                },
                failure: function (response) {
                    AlertMessage.showAlertMessage(response);
                }
            });

        }
    });

    var AlertMessage = {
        showAlertMessage: function (message) {
            var alert = $('#alertMessage');
            alert.removeClass('d-none');
            var messageText = alert.find('.message-text');
            messageText.text(message);
        },
        hideAlertMessage: function () {
            $('#alertMessage').addClass('d-none');
        }
    };

    $('#alertMessage .close').on('click', AlertMessage.hideAlertMessage);

});

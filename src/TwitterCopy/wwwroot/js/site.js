﻿$(document).ready(function () {

    /*
     * Show modal dialog for confirmation of deletion of tweet
     */
    $(document.body).on('click', '.btn-delete-tweet', function (event) {
        var button = $(this);
        // Extract tweet's id from the button's 'data-tweet-id' attribute
        var tweetId = button.data('tweet-id');

        // AJAX request to get tweet from database
        $.ajax({
            type: 'GET',
            url: '/Tweets/GetTweet',
            data: {
                id: tweetId
            },
            success: function (response) {
                var deleteTweetModal = $('#deleteTweetModal');
                var tweetInfoBody = deleteTweetModal.find('#tweetInfoBody');
                var confirmDeleteBtn = deleteTweetModal.find('#confirmDeleteBtn');

                // Empty modal's body <div> and fill it with response HTML
                tweetInfoBody.empty();
                tweetInfoBody.html(response);
                // Add data-* attribute to the modal's Delete button
                // so you can send tweet id to the server
                confirmDeleteBtn.attr('data-tweet-id', tweetId);
                // Show modal
                deleteTweetModal.modal('show');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
            }
        });
    });

    $(document.body).on('click', '#confirmDeleteBtn', function (event) {
        var button = $(this);
        var tweetId = button.data('tweet-id');
        var deleteTweetModal = $('#deleteTweetModal');

        $.ajax({
            type: 'POST',
            url: '/Tweets/Delete',
            data: {
                id: tweetId
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            success: function (response) {
                // Hide modal if it's still open
                if (deleteTweetModal.hasClass('.show')
                    || deleteTweetModal.css('display') !== 'none') {
                    deleteTweetModal.modal('hide');
                }
                // Empty modal's body <div>
                var tweetInfoBody = deleteTweetModal.find('#tweetInfoBody');
                tweetInfoBody.empty();

                AlertMessage.showAlertMessage(response.message);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (deleteTweetModal.hasClass('.show')
                    || deleteTweetModal.css('display') !== 'none') {
                    deleteTweetModal.modal('hide');
                }
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
            }
        });
    });

    /*
     * After modal dialog is closed, remove content (tweet info) from the body
     */
    $('#deleteTweetModal').on('hide.bs.modal', function (event) {
        // Clean div from tweet info
        var tweetInfoBody = $('#deleteTweetModal').find('#tweetInfoBody');
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
            url: '/Tweets/UpdateLikes',
            data: {
                id: tweetId
            },
            dataType: 'json',
            success: function (response) {
                var tweetLikeCount = btnClicked.find('span');
                tweetLikeCount.text(`${response}`);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
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

        $.ajax({
            type: 'POST',
            url: '/Profile/Follow',
            data: {
                userSlug: userToFollow
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
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
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
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
            url: '/Profile/Unfollow',
            data: {
                userSlug: userSlug
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
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
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
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
            url: '/Tweets/Retweet',
            data: {
                id: tweetId
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            dataType: 'json',
            success: function (response) {
                // find a span on the clicked button
                // which has a retweet count
                var tweetRetweetCount = clickedBtn.find('span');
                // set a new count of retweets
                tweetRetweetCount.text(`${response}`);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
            }
        });
    });

    var ProfileEditor = {
        updateProfileInfo: function (newData) {
            $('#profileInfoUserName').text(newData.username);
            $('#profileInfoBio').text(newData.bio);
            $('#profileInfoLocation').text(newData.location);
            $('#profileInfoWebsite').text(newData.website);
            $('#profileInfoAvatar').attr('src', newData.avatar);
            $('#profileInfoBanner').attr('src', newData.banner);
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
        },
        updateProfileAvatar: function (newAvatar) {
            $('#profileInfoAvatar').attr('src', newAvatar);
        },
        updateProfileBanner: function (newBanner) {
            $('#profileInfoBanner').attr('src', newBanner);
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
        var userName = $('input[name="userName"]').val();
        var slug = $('#profileInfoSlug');
        var bio = $('input[name="bio"]').val();
        var location = $('input[name="location"]').val();
        var website = $('input[name="website"]').val();
        var avatar = $('input[name="Avatar"]')[0].files[0];
        var banner = $('input[name="Banner"]')[0].files[0];

        // Create new FormData object which will be posted to the server
        // with new profile info values
        var postedValues = new FormData();
        postedValues.append('userName', userName);
        postedValues.append('slug', slug);
        postedValues.append('bio', bio);
        postedValues.append('location', location);
        postedValues.append('website', website);
        postedValues.append('avatar', avatar);
        postedValues.append('banner', banner);

        // Post values to the server
        $.ajax({
            type: 'POST',
            url: '/Profile/EditUser',
            data: postedValues,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: false,
            dataType: 'json',
            processData: false,
            success: function (response) {
                // update profile info
                ProfileEditor.updateProfileInfo(response);
                ProfileEditor.saveChangesButtonClick();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
            }
        });
    });

    $('#removeAvatarBtn').click(function (e) {

        var avatar = $('#profileInfoAvatar').data('avatar');
        $.ajax({
            type: 'POST',
            url: '/Profile/RemoveAvatar',
            data: {
                avatar: avatar
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            dataType: 'json',
            success: function (response) {
                AlertMessage.showAlertMessage(response.message);
                ProfileEditor.updateProfileAvatar(response.avatar);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
            }
        });
    });

    $('#removeBannerBtn').click(function (e) {

        var banner = $('#profileInfoBanner').data('banner');
        $.ajax({
            type: 'POST',
            url: '/Profile/RemoveBanner',
            data: {
                banner: banner
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            dataType: 'json',
            success: function (response) {
                AlertMessage.showAlertMessage(response.message);
                ProfileEditor.updateProfileBanner(response.banner);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
            }
        });
    });

    $(document.body).on('click', '.btn-reply', function (e) {
        $(this).closest('.tweet-actionable').click();
    });

    $(document.body).on('click', '.tweet-actionable', function (e) {
        var tweet = $(this);
        var tweetId = tweet.data('tweet-id');
        var userSlug = tweet.data('user-slug');

        var tweetModal = $('#tweetModal');
        if (tweetModal.css('display') !== 'none') {
            tweetModal.modal('hide');
        }

        if (e.target.localName !== 'button'
            && e.target.localName !== 'span'
            && e.target.localName !== 'a'
            && e.target.localName !== 'strong'
            && e.target.localName !== 'b') {
            $.ajax({
                type: 'GET',
                url: '/Tweets/GetStatus',
                data: {
                    slug: userSlug,
                    tweetId: tweetId
                },
                success: function (response) {
                    var tweetContainer = tweetModal.find('.modal-body');
                    tweetContainer.html(response);
                    tweetModal.modal('show');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
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

    $(document.body).on('show.bs.modal', '#tweetModal', function() {

        var modal = $('#tweetModal');
        var sendReplyForm = modal.find('#sendReplyForm');
        var sendReplyBtn = sendReplyForm.find('#sendReplyBtn');
        var input = sendReplyForm.find('input[type=text]');

        modal.find('.btn-reply').on('click', function (e) {
            if (!input.is(':focus'))
                input.focus();
        });

        /*
         * Show Send Reply button when any element inside form is focused
         */
        input.on('focus', function () {
            sendReplyBtn.show();
        });

        /*
         * Hide Send Reply button when any element inside form loses
         * focus or text box doesn't have any value
         */
        input.on('focusout', function () {
            if (!input.val().trim())
                sendReplyBtn.hide();
        });

        /*
         * Enable Send Reply button only if text box has value
         */
        input.on('keyup', function () {
            if (input.val().trim())
                sendReplyBtn.prop('disabled', false);
            else
                sendReplyBtn.prop('disabled', true);
        });

        sendReplyBtn.on('click', function (e) {
            var replyText = input.val();
            var tweetId = $(this).data('tweet-id');

            $.ajax({
                type: 'POST',
                url: '/Tweets/Reply',
                data: {
                    replyText: replyText,
                    tweetId: tweetId
                },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",
                        $('input:hidden[name="__RequestVerificationToken"]').val());
                },
                success: function (response) {
                    var repliesFromBlock = modal.find('.tweet-replies-from');
                    repliesFromBlock.prepend(response);
                    input.val("");
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    AlertMessage.showAlertMessage(jqXHR.responseJSON.message);
                }
            });
        });
    });

    $('#selectLanguageForm button[type="button"]').on('click', function (e) {
        var form = $('#selectLanguageForm');
        var returnUrl = form.data('return-url');
        var culture = form.find('select option:selected').val();

        $.ajax({
            type: 'POST',
            url: '/Profile/SetLanguage',
            data: {
                culture: culture,
                returnUrl: returnUrl
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            }
        });
    });
});

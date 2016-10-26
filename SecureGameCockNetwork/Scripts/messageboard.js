
//****//
//****//
//****//
//****//
//****//
//****//
//****//


var post = function (id, message, username, date) {
    this.id = id;
    this.message = message;
    this.username = username;
    this.date = date;
    this.comments = ko.observableArray([]);

    this.addComment = function (context) {
        var comment = $('input[name="comment"]', context).val();
        if (comment.length > 0) {
            $.connection.boardHub.server.addComment(this.id, comment, vm.username())
            .done(function () {
                $('input[name="comment"]', context).val('');
            });
        }
    };
}

var comment = function (id, message, username, date) {
    this.id = id;
    this.message = message;
    this.username = username;
    this.date = date;
}

var vm = {
    posts: ko.observableArray([]),
    notifications: ko.observableArray([]),
    username: ko.observable(),
    password: ko.observable(),
    signedIn: ko.observable(false),
    errors: ko.observableArray(),
    isRegistering: ko.observable(false),
    confirmPassword: ko.observable(),

    signIn: function () {
        vm.isRegistering(false);
        vm.username($('#username').val());
        vm.password($('#password').val());
        vm.signedIn(true);
        vm.errors.removeAll();
        loginUser({
            grant_type: "password",
            username: vm.username(),
            password: vm.password()
        }).done(function (data) {
            vm.signedIn(false);

            if (data.userName && data.access_token) {
                //app.navigateToLoggedIn(data.userName, data.access_token, self.rememberMe());
            } else {
                vm.errors.push("An unexpected error occurred.");
            }
        }).failJSON(function (data) {
            vm.signedIn(false);

            if (data && data.error_description) {
                vm.errors.push(data.error_description);
            } else {
                vm.errors.push("An unexpected error occurred.");
            }
        });

    },
    writePost: function () {
        $.connection.boardHub.server.writePost(vm.username(), $('#message').val()).done(function () {
            $('#message').val('');
        });
    },
}
self.loginUser = function (data) {
    return $.ajax("/Token", {
        type: "POST",
        data: data
    });
};

showRegister = function () {
    vm.isRegistering(true);
}
showLogin = function () {
    vm.errors.removeAll();
    vm.isRegistering(false);
    vm.signedIn(false);
}


//app.addViewModel({
//    name: "Register",
//    bindingMemberName: "register",
//    factory: RegisterViewModel
//});

self.register = function () {
    registerUser({
        userName: vm.username(),
        password: vm.password(),
        confirmPassword: vm.confirmPassword()
    }).done(function (data) {
        login({
            grant_type: "password",
            username: vm.username(),
            password: vm.password()
        }).done(function (data) {
            vm.isRegistering(false);
            if (data.username && data.access_token) {
                //app.navigateToLoggedIn(data.userName, data.access_token, false /* persistent */);
            } else {
                vm.errors.push("An unexpected error occurred.");
            }
        });

    });

};

self.registerUser = function (data) {
    return $.ajax("/api/Account/Register", {
        type: "POST",
        data: data
    });
};
ko.applyBindings(vm);

function loadPosts() {
    $.get('/api/posts', function (data) {
        var postsArray = [];
        $.each(data, function (i, p) {
            var newPost = new post(p.id, p.message, p.username, p.dateposted);
            $.each(p.comments, function (j, c) {
                var newComment = new comment(c.id, c.message, c.username, c.dateposted);
                newPost.comments.push(newComment);
            });

            vm.posts.push(newPost);
        });
    });
}

$(function () {
    var hub = $.connection.boardHub;
    $.connection.hub.start().done(function () {
        loadPosts(); // Load posts when connected to hub
    });

    // Hub calls this after a new post has been added
    hub.client.receivedNewPost = function (id, username, message, date) {
        var newPost = new post(id, message, username, date);
        vm.posts.unshift(newPost);

        // If another user added a new post, add it to the activity summary
        if (username !== vm.username()) {
            vm.notifications.unshift(username + ' has added a new post.');
        }
    };

    // Hub calls this after a new comment has been added
    hub.client.receivedNewComment = function (parentPostId, commentId, message, username, date) {
        // Find the post object in the observable array of posts
        var postFilter = $.grep(vm.posts(), function (p) {
            return p.id === parentPostId;
        });
        var thisPost = postFilter[0]; //$.grep returns an array, we just want the first object

        var thisComment = new comment(commentId, message, username, date);
        thisPost.comments.push(thisComment);

        if (thisPost.username === vm.username() && thisComment.username !== vm.username()) {
            vm.notifications.unshift(username + ' has commented on your post.');
        }
    };
});

//aJAX PREFILTER
$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    jqXHR.failJSON = function (callback) {
        jqXHR.fail(function (jqXHR, textStatus, error) {
            var data;

            try {
                data = $.parseJSON(jqXHR.responseText);
            }
            catch (e) {
                data = null;
            }

            callback(data, textStatus, jqXHR);
        });
    };
});

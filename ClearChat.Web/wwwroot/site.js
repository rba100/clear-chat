/* global variable to hold page state */

var model = {
    selectedChannel: "",
    channels: [""],
    channelContentCache: {
    },
    userIdToColour: {}
};

$(function () {
    var channelList = $('#nav-section-channels');
    var outputContainer = $('#message-section');
    var messageContainer = $('#message-container');
    var converter = new showdown.Converter();

    var lastAuthor = "";
    var inputHistoryIndex = 0;
    var inputHistory = [];

    // Global key handler - focus text input if typing
    $(document).on('keydown', function(event) {
        if (event.target.id === 'text-input'
            || event.ctrlKey) return;
        $('#text-input').focus();
    });

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.onclose(function (error) {
        console.log("DEBUG: connection closed.");
        if (error) console.log("DEBUG: " + error);
    });

    connection.on("newMessage",
        function (chatItemRaw) {
            var cacheEntry = model.channelContentCache[chatItemRaw.channelName];
            if (cacheEntry) cacheEntry.messages.push(chatItemRaw);
            if (model.selectedChannel === chatItemRaw.channelName || chatItemRaw.channelName === "system") {
                appendSingleMessage(chatItemRaw);
            } else {
                var channelLinkIndex = model.channels.indexOf(chatItemRaw.channelName);
                if (channelLinkIndex < 0) return;
                var channelLink = channelList.children(":eq(" + channelLinkIndex + ")");
                channelLink.addClass('nav-section-channel-link-unread');
            }
        });

    connection.on('deleteMessage',
        function(messageId) {
            for (var channel in model.channelContentCache) {
                var cache = model.channelContentCache[channel];
                var index = cache.messages.findIndex(function(item) { return item.id === messageId; });
                if (index === -1) continue;
                cache.messages.splice(index, 1);
                if (channel === model.selectedChannel) dataRefresh(
                    messageContainer,
                    cache.messages.map(toMessageControlDataBinding));
            }
        });

    connection.on("channelMembership",
        function (names) {
            var shouldChangeChannel = names.indexOf(model.selectedChannel) === -1;
            if (shouldChangeChannel) model.selectedChannel = names[0];
            model.channels = names;
            channelList.html("");
            for (var i = 0; i < names.length; i++) {
                var channelName = names[i];
                var channelLink = instantiate('tmpt-nav-section-link', { channelName: channelName });
                var handler = changeChannelHandler(channelName);
                if (channelName === model.selectedChannel) {
                    channelLink.addClass('nav-section-channel-link-selected');
                }
                channelLink.click(handler);
                channelList.append(channelLink);
                if (typeof (model.channelContentCache[channelName]) === "undefined") {
                    model.channelContentCache[channelName] = { messages: [], lastAuthor: "" };
                    connection.send("getHistory", channelName).catch(function (error) {
                        console.log(error);
                    });
                }
            }
            if (shouldChangeChannel) changeChannelLocal(names[0]);
        });

    connection.on("userDetails",
        function(users) {
            for (var index in users) {
                var user = users[index];
                model.userIdToColour[user.userId] = user.hexColour;
            }
        });

    connection.on("channelHistory",
        function (channelName, historyItems) {
            var cacheEntry = model.channelContentCache[channelName];
            if (!cacheEntry) return;
            model.channelContentCache[channelName].lastAuthor = "";
            model.channelContentCache[channelName].messages = historyItems;
            if (model.selectedChannel === channelName) dataRefresh(messageContainer, historyItems.map(toMessageControlDataBinding));
        });

    connection.start().then(function () {
        $('#text-input').keypress(function (e) {
            if (e.which === 13) { // ENTER KEY
                send();
            }
        });
        $('#text-input').keydown(function (e) {
            if (e.which === 38) { // UP ARROW
                if (inputHistoryIndex < inputHistory.length) {
                    $('#text-input').val(inputHistory[inputHistory.length - 1 - inputHistoryIndex++]);
                }
            } else if(e.which === 40) { // DOWN ARROW
                if (inputHistoryIndex > 0) {
                    $('#text-input').val(inputHistory[inputHistory.length - 1 - --inputHistoryIndex]);
                }
            }
        });
        $('#text-input').focus();
        connection.send('GetChannels');
    });

    // See message-template in index.html
    function toMessageControlDataBinding(chatItem) {
        return {
            id: chatItem.id,
            userId: chatItem.userId,
            channelName: chatItem.channelName,
            timeStampUtc: new Date(chatItem.timeStampUtc).format("h:MM TT"),
            message: converter.makeHtml(emojione.shortnameToImage(chatItem.message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"))),
            userIdcss: { color: '#' + toColour(chatItem.userId) }
        };
    }

    function toColour(userId) {
        var knownColour = model.userIdToColour[userId];
        if (typeof (knownColour) === "undefined") {
            connection.send('getUserDetails', userId);
            return "000000";
        }
        return knownColour;
    }

    function send() {
        var message = $('#text-input').val();
        if (message === "") return;
        inputHistory.push(message);
        inputHistoryIndex = 0;
        var eventData = { Channel: model.selectedChannel, Body: message };
        connection.send("send", eventData).catch(function (error) {
            console.log(error);
        });
        $('#text-input').val("");
    }

    function appendSingleMessage(chatItem) {
        var sameAuthor = lastAuthor === chatItem.userId;
        var messageElement = instantiate('message-template', toMessageControlDataBinding(chatItem));
        if (sameAuthor) {
            messageElement.find("b").first().hide();
        }
        messageContainer.append(messageElement);
        lastAuthor = chatItem.userId;
        scrollToBottom();
    }

    function changeChannelHandler(channelName) {
        return function() {
            var link = $(this);
            channelList.children().removeClass('nav-section-channel-link-selected');
            link.addClass('nav-section-channel-link-selected');
            link.removeClass('nav-section-channel-link-unread');
            changeChannelLocal(channelName);
        };
    }

    function changeChannelLocal(channelName) {
        if (model.selectedChannel !== channelName) lastAuthor = "";
        model.selectedChannel = channelName;
        var cacheEntry = model.channelContentCache[channelName];
        if (model.selectedChannel === channelName) {
            dataRefresh(
                messageContainer,
                cacheEntry.messages.map(toMessageControlDataBinding));
            if (cacheEntry.messages.length)
                lastAuthor = cacheEntry.messages[cacheEntry.messages.length - 1].userId;
        }
    }

    function scrollToBottom() {
        outputContainer.scrollTop(2000000000);
    }
});
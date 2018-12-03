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
    var messageContainer = $('#message-container');
    var showNewMessageScrollWarning = $('#new-message-scroll-warning');
    var converter = new showdown.Converter();

    var lastAuthor = "";
    var inputHistoryIndex = 0;
    var inputHistory = [];

    // Global key handler
    $(document).on('keydown', function (e) {
        if (e.which === 38) { // UP ARROW
            if (inputHistoryIndex < inputHistory.length) {
                $('#text-input').val(inputHistory[inputHistory.length - 1 - inputHistoryIndex++]);
            }
        } else if (e.which === 40) { // DOWN ARROW
            if (inputHistoryIndex > 0) {
                $('#text-input').val(inputHistory[inputHistory.length - 1 - --inputHistoryIndex]);
            }
        }
        if (e.target.id === 'text-input' || e.ctrlKey) return;
        $('#text-input').focus();
    });

    showNewMessageScrollWarning.click(function() {
        showNewMessageScrollWarning.hide();
        messageContainer.children().last()[0].scrollIntoView();
    });
    function isScrolledToBottom() {
        return messageContainer[0].scrollHeight - messageContainer.scrollTop()
            === messageContainer.outerHeight();
    }

    messageContainer.scroll(function() {
        if (isScrolledToBottom()) showNewMessageScrollWarning.hide();
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
                var scrolled = isScrolledToBottom();
                var newMessage = appendSingleMessage(chatItemRaw);
                if (scrolled) newMessage.scrollIntoView();
                else showNewMessageScrollWarning.show();
            } else {
                var channelLinkIndex = model.channels.indexOf(chatItemRaw.channelName);
                if (channelLinkIndex < 0) return;
                var channelLink = channelList.children(":eq(" + channelLinkIndex + ")");
                channelLink.addClass('nav-section-channel-link-unread');
            }
        });

    connection.on('deleteMessage',
        function (messageId) {
            for (var channel in model.channelContentCache) {
                var cache = model.channelContentCache[channel];
                var index = cache.messages.findIndex(function (item) { return item.id === messageId; });
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
        function (users) {
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
            if (model.selectedChannel === channelName) {
                dataRefresh(messageContainer, historyItems.map(toMessageControlDataBinding));
                var last = messageContainer.children().last();
                if(last.length) last[0].scrollIntoView();
            }
        });

    connection.start().then(function () {
        $('#text-input').keypress(function (e) {
            if (e.which === 13) { // ENTER KEY
                send();
            }
        });
        $('#text-input').focus();
        connection.send('GetChannels');
    });

    // See message-template in index.html
    function toMessageControlDataBinding(chatItem) {
        var binding =  {
            userId: chatItem.userId,
            channelName: chatItem.channelName,
            timeStampUtc: new Date(chatItem.timeStampUtc).format("h:MM TT"),
            message: converter.makeHtml(emojione.shortnameToImage(chatItem.message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"))),
            userIdCss: { color: '#' + toColour(chatItem.userId) },
            headerAttributes: { title: chatItem.id }
        };

        if (chatItem.userId === 'ClearBot') {
            binding.headerAttributes.class = "clear-bot-style";
        } else if(chatItem.userId === 'System') {
            binding.headerAttributes.class = "system-bot-style";
        }

        return binding;
    }

    function toColour(userId) {
        if (typeof (model.userIdToColour[userId]) === "undefined") {
            model.userIdToColour[userId] = "000000";
            connection.send('getUserDetails', userId);
        }
        return model.userIdToColour[userId];
    }

    function send() {
        var message = $('#text-input').val();
        if (message === "") return;
        inputHistory.push(message);
        if (inputHistory.length > 40) inputHistory.shift();
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
        return messageElement[0];
    }

    function changeChannelHandler(channelName) {
        return function () {
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
        dataRefresh(
            messageContainer,
            cacheEntry.messages.map(toMessageControlDataBinding));
        if (cacheEntry.messages.length) {
            lastAuthor = cacheEntry.messages[cacheEntry.messages.length - 1].userId;
            messageContainer.children().last()[0].scrollIntoView();
        }
        showNewMessageScrollWarning.hide();
    }
});
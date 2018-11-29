/* global variable to hold page state */

var model = {
    selectedChannel: "",
    channels: [""],
    channelContentCache: {
    }
};

$(function () {
    var channelList = $('#nav-section-channels');
    var outputContainer = $('#output-container');
    var messageContainer = $('#message-container');
    var converter = new showdown.Converter();

    var lastAuthor = "";
    var lastMessageSent = "";

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

    connection.on("channelMembership",
        function (names) {
            if (names.indexOf(model.selectedChannel) === -1) {
                model.selectedChannel = names[0];
            }
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
                    model.channelContentCache[channelName] = { items: [], lastAuthor: "" };
                    connection.send("getHistory", channelName).catch(function (error) {
                        console.log(error);
                    });
                }
            }
        });

    connection.on("initHistory",
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
                $('#text-input').val(lastMessageSent);
            }
        });
        $('#text-input').focus();
        connection.send('GetChannels');
    });

    // See message-template in index.html
    function toMessageControlDataBinding(chatItem) {
        return {
            userId: chatItem.userId,
            channelName: chatItem.channelName,
            timeStampUtc: new Date(chatItem.timeStampUtc).format("h:MM TT"),
            message: converter.makeHtml(emojione.shortnameToImage(chatItem.message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"))),
            userIdcss: { color: '#' + chatItem.userIdColour }
        };
    }

    function send() {
        var message = $('#text-input').val();
        if (message === "") return;
        lastMessageSent = message;
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
            model.selectedChannel = channelName;
            var cacheEntry = model.channelContentCache[channelName];
            model.channelContentCache[channelName].messages;
            if (model.selectedChannel === channelName)
                dataRefresh(
                    messageContainer,
                    cacheEntry.messages.map(toMessageControlDataBinding));
        };
    }

    function scrollToBottom() {
        outputContainer.scrollTop(2000000000);
    }
});

// Instantiate an element from a template
// template   - the ID of a <template> element.
// parameters - an object like { labelText: 'field value' }
//              template must have an element with 'data-from' attribute
//              e.g. <p data-from="labelText"></p>
function instantiate(template, parameters) {
    if (typeof (template) === "string") template = $('#' + template);
    var newElement = $(template.html());
    if (typeof (parameters) !== "undefined")
        dataRefresh(newElement, parameters);
    return newElement;
}

function dataRefresh(element, parameters) {
    if (!parameters) return;
    if (typeof (parameters) === "object") {
        if (Array.isArray(parameters)) {
            var dataTemplate = element.attr("data-template");
            var container = element;
            if (!dataTemplate) {
                container = element.find("[data-template]");
                dataTemplate = container.attr('data-template');
            }
            if (!dataTemplate) return;
            container.html("");
            for (var value in parameters) {
                var itemElement = instantiate(dataTemplate, parameters[value]);
                container.append(itemElement);
            }
            return;
        } else for (var key in parameters) {
            var dataValue = parameters[key];
            var target = element.find("[data-from='" + key + "']")
                .addBack("[data-from='" + key + "']");
            if (target.length) {
                if (typeof (dataValue) === "string")
                    target.html(dataValue);
                else if (typeof (dataValue) === "object") {
                    dataRefresh(target, dataValue);
                }
            }
            var dataFieldTarget = element.find("[data-field='" + key + "']")
                .addBack("[data-field='" + key + "']");
            if (dataFieldTarget.length) {
                dataFieldTarget.data(key, dataValue);
            }
            var styleTarget = element.find("[data-css='" + key + "']");
            if (styleTarget.length) {
                for (var cssKey in dataValue) {
                    styleTarget.css(cssKey, dataValue[cssKey]);
                }
            }
        }
    }
    else if (typeof (parameters) === "string") {
        var targetElement = element.find('[data-from]').addBack('[data-from]');
        if (!targetElement.length) return;
        var textFrom = targetElement.attr('data-from');
        if (textFrom === "") targetElement.html(parameters);
    }
}
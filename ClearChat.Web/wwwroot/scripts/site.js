/* global variable to hold page state */

var model = {
    selectedChannel: "#default",
    channels: ["#default"],
    channelContentCache: {
        "#default": { items: [], lastAuthor: "" }
    }
};

$(function () {
    var workspace = $('#workspace');
    var navSection = $('#nav-section');
    var maxHistory = 400;
    var chatHistory = $('#history');
    var outputContainer = $('#output-container');
    var channelNameLabel = $('#channel-name');
    var converter = new showdown.Converter();

    var lastAuthor = "";
    var lastMessageSent = "";

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    var unreadMessages = 0;
    var focused = true;
    var channelName = "default";

    window.onfocus = function () {
        focused = true;
        unreadMessages = 0;
        setWindowTitle();
    };
    window.onblur = function () {
        focused = false;
    };

    function setWindowTitle() {
        if (focused || unreadMessages === 0) window.document.title = "Clear Chat - " + channelName;
        else window.document.title = "Clear Chat - " + channelName + " (" + unreadMessages + ")";
    }

    connection.onclose(function (error) {
        console.log("DEBUG: connection closed.");
        if (error) console.log("DEBUG: " + error);
    });

    connection.on("newMessage",
        function (chatItem) {
            insertChatItem(chatItem);
            if (!focused) {
                unreadMessages++;
                setWindowTitle();
            }
        });

    connection.on("updateChannelName",
        function (name) {
            var safeName = name.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            channelName = safeName;
            channelNameLabel.html(safeName);
            setWindowTitle();
        });

    connection.on("initHistory",
        function (historyItems) {
            chatHistory.empty();
            lastAuthor = "";
            for (var i = 0; i < historyItems.length; i++) {
                insertChatItem(historyItems[i]);
            }
        });

    connection.start().then(function () {
        $('#send-button').click(send);
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
        loadHistory();
    });

    function send() {
        var message = $('#text-input').val();
        if (message === "") return;
        lastMessageSent = message;
        connection.send("send", message).catch(function (error) {
            console.log(error);
        });
        $('#text-input').val("");
    }

    function loadHistory() {
        connection.send("getHistory");
    }

    function insertChatItem(chatItem) {
        var safeMessage = chatItem.message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var safeMessageWithEmoji = emojione.shortnameToImage(safeMessage);
        var processedMessage = converter.makeHtml(safeMessageWithEmoji);
        var row = $('<div class="chat-item">' +
            '<p>' +
            processedMessage +
            '</p>' +
            '</div>');
        if (lastAuthor !== chatItem.userId) {
            var nameElement = $('<span title="' +
                chatItem.timeStampUtc +
                '">' +
                '<b>' +
                chatItem.userId +
                '</b></span><br class="hide-for-wide" />' +
                '<span class="hide-for-small"> : </span>');
            nameElement.find('b').css("color", '#' + chatItem.userIdColour);
            row.prepend(nameElement);
        }
        lastAuthor = chatItem.userId;
        chatHistory.append(row);
        scrollToBottom();
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
function instantiateTemplate(template, parameters) {
    if (typeof (template) === "string") template = $('#' + template);
    var newElement = $(template.html());

    if (parameters && typeof (parameters) === "object") {
        for (var property in parameters) {
            var value = parameters[property];
            newElement.find("[data-from='" + property + "']").text(value);
        }
    }
    return newElement;
}
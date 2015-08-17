var messageUi = {};

messageUi.show = function(message, autoHide, scrollTop, delay) {
    var messageBox = $("<div class='messageUiBox'></div>");
    $("body").append(messageBox);
    messageBox.append("<div>" + message + "</div>");
    if (autoHide != null && autoHide) {
        setTimeout(function(e) {
            messageBox.remove();
        }, delay != null ? delay : 3000);
    } else {
        var modalShadow = $("<div class='bodyShadow'></div>");
        modalShadow.width($(document).width());
        modalShadow.height($(document).height());
        $("body").append(modalShadow);
        var okButton = $("<div class='messageUiOkButton'>Ok</div>");
        messageBox.append(okButton);
        okButton.click(function (e) {
            messageBox.remove();
            modalShadow.remove();
        });
    }
    if (scrollTop == null) scrollTop = 0;
    var top = scrollTop + 200;
    messageBox.css("top", top + "px");
};

messageUi.confirm = function (message, okCallback, cancelCallback, scrollTop) {
    var messageBox = $("<div class='messageUiBox'></div>");
    $("body").append(messageBox);
    messageBox.append("<div>" + message + "</div>");
    var modalShadow = $("<div class='bodyShadow'></div>");
    modalShadow.width($(document).width());
    modalShadow.height($(document).height());
    $("body").append(modalShadow);
    var okButton = $("<div class='messageUiOkButton'>Да</div>");
    messageBox.append(okButton);
    okButton.click(function (e) {
        if (okCallback != null) {
            okCallback();
        }
        messageBox.remove();
        modalShadow.remove();
    });
    var cancelButton = $("<div class='messageUiOkButton' style='background-color:red;'>Отмена</div>");
    messageBox.append(cancelButton);
    cancelButton.click(function (e) {
        if (cancelCallback != null) {
            cancelCallback();
        }
        messageBox.remove();
        modalShadow.remove();
    });
    if (scrollTop == null) scrollTop = 0;
    var top = scrollTop + 200;
    messageBox.css("top", top + "px");
};

messageUi.initProgressImage = function() {
    var url = "/Content/progress.gif";
    var progressImage = $("<img src='" + url + "' class='progressImg' />");
    $("body").append(progressImage);
    progressImage.hide();
};

messageUi.progressShow = function(modal, scrollTop) {
    var imgClone = $(".progressImg").clone();
    var wrapper = $("<div class='progressWrapper'></div>");
    wrapper.append(imgClone);
    $("body").append(wrapper);
    wrapper.show();
    imgClone.show();
    if (scrollTop == null) scrollTop = 0;
    var top = scrollTop + 200;
    wrapper.css("top", top + "px");
    this.progressElement = wrapper;
    if (modal != null && modal) {
        var modalShadow = $("<div class='bodyShadow'></div>");
        modalShadow.width($(document).width());
        modalShadow.height($(document).height());
        $("body").append(modalShadow);
        this.shadowElement = modalShadow;
    }
};

messageUi.progressHide = function() {
    if (this.progressElement != null) {
        this.progressElement.remove();
        this.progressElement = null;
    }
    if (this.shadowElement != null) {
        this.shadowElement.remove();
        this.shadowElement = null;
    }
};
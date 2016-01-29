$(document).on('submit', 'form', function () {
    var button = $(this).find("[type='submit']");
    setTimeout(function () {
        button.prop('disabled', true);
        showSpinnerAppend(button);
    }, 0);
});

function showSpinner(obj, offset, offsetTop, offsetLeft) {
    var of = "";
    var stOf = "";
    if (offset) {
        of = "on-element";
        stOf = "top:" + offsetTop + "px;left:" + offsetLeft + "px";
    }
    var loading = "<div class='spinner active " + of + "' style='" + stOf + "'><i class='fa fa-spin fa-spinner'></i></div>";
    $(obj).before(loading);
}

function showSpinnerAfter(obj, offset, offsetTop, offsetLeft) {
    var of = "";
    var stOf = "";
    if (offset) {
        of = "on-element";
        if (offsetTop == undefined || offsetTop == null || offsetTop == '') offsetTop = 5;
        stOf = "top:" + offsetTop + "px;left:" + offsetLeft + "px";
    }
    var loading = "<div class='spinner active " + of + "' style='" + stOf + "'><i class='fa fa-spin fa-spinner'></i></div>";
    $(obj).after(loading);
}

function showSpinnerAppend(obj, offset, offsetTop, offsetLeft) {
    var of = "";
    var stOf = "";
    if (offset) {
        of = "on-element";
        stOf = "top:" + offsetTop + "px;left:" + offsetLeft + "px";
    }
    var loading = "<div class='spinner active " + of + "' style='" + stOf + "'><i class='fa fa-spin fa-spinner'></i></div>";
    $(obj).prepend(loading);
}

function showSpinnerPrepend(obj, offset, offsetTop, offsetLeft) {
    var of = "";
    var stOf = "";
    if (offset) {
        of = "on-element";
        stOf = "top:" + offsetTop + "px;left:" + offsetLeft + "px";
    }
    var loading = "<div class='spinner active " + of + "' style='" + stOf + "'><i class='fa fa-spin fa-spinner'></i></div>";
    $(obj).prepend(loading);
}

function hideSpinner(obj) {
    if (obj) {
        $(obj).parent().find(".spinner.active").remove();
    } else {
        $(".spinner").remove();
    };
}

$(function () {
    $('.pull-down').each(function () {
        $(this).css('margin-top', $(this).parent().height() - $(this).height())
    });
});

function showSpinnerAppendAndDisable(obj, offset, offsetTop, offsetLeft) {
    $(obj).prop('disabled', true);
    var of = "";
    var stOf = "";
    if (offset) {
        of = "on-element";
        stOf = "top:" + offsetTop + "px;left:" + offsetLeft + "px";
    }
    var loading = "<div class='spinner active " + of + "' style='" + stOf + "'><i class='fa fa-spin fa-spinner'></i></div>";
    $(obj).prepend(loading);
}

function hideSpinnerAndEnabled(obj) {
    if (obj) {
        $(obj).parent().find(".spinner.active").remove();
        $(obj).prop('disabled', false);
    } else {
        $(".spinner").parent('.btn').prop('disabled', false);
        $(".spinner").remove();
    };
    
}
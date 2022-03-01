function copyToClipboard(text) {
    var location = document.getElementById('location').innerHTML;
    var dummy = document.createElement('textarea');
    document.body.appendChild(dummy);
    dummy.value = location+text;
    dummy.select();
    document.execCommand('copy');
    document.body.removeChild(dummy);
}
$(function () {
    $('[data-toggle="popover"]').popover({
        content: "copied"
    }).on('shown.bs.popover', function () {
        setTimeout(function (a) {
            a.popover('hide');
        }, 700, $(this));
    });
    $('.popover-dismiss').popover({
        trigger: 'focus',
        content: "copied"
    });
})
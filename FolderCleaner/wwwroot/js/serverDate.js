currentTime = new Date(document.getElementById('date').innerHTML);
function clockTick() {
    currentTime.setSeconds(currentTime.getSeconds() + 1),
        month = currentTime.getMonth() + 1,
        day = currentTime.getDate(),
        year = currentTime.getFullYear(),
        hours = currentTime.getHours(),
        minutes = currentTime.getMinutes(),
        seconds = currentTime.getSeconds(),
        text = (month + "/" + day + "/" + year + ' ' + hours + ':' + minutes + ':' + seconds);
    document.getElementById('date').innerHTML = text;
}
setInterval(clockTick, 1000);
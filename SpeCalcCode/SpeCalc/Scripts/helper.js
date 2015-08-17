function getDateString(date) {
    if (date == "Invalid Date") {
        date = new Date();
    }
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    var dateString = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
    return dateString;
}
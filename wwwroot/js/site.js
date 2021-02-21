// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
function display_c() {
    var refresh = 1000; // Refresh rate in milli seconds
    mytime = setTimeout('display_ct()', refresh)
}

function display_ct() {
    var x = new Date()
    let options = {
        hour: "2-digit", minute: "2-digit",
        second: "2-digit", weekday: "long", year: "numeric", month: "short",
        day: "numeric"
    };  
    document.getElementById('ct').innerHTML = x.toLocaleTimeString("en-us", options);
    display_c();
}
// Write your JavaScript code.
$('expandbutton').trigger('mouseenter') 
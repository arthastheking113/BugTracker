
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

$('#contact').summernote({
    height: 300,
    placeholder: "Tell me, I'm listening...",
    tabDisable: true,
    dialogsInBody: true
});
$('#Content').summernote({
    height: 120,
    placeholder: "Tell me, I'm listening...",
    tabDisable: true,
    dialogsInBody: true
});
$(function () {

    //Initialize Select2 Elements
    $('.select2').select2()
    //Initialize Select2 Elements
    $('.select2bs4').select2({
        theme: 'bootstrap4'
    })


    //Bootstrap Duallistbox
    $('.duallistbox').bootstrapDualListbox()

  
})


// Write your JavaScript code.
$('expandbutton').trigger('mouseenter') 
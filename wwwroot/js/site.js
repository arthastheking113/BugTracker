function display_c() {
    var refresh = 1000; // Refresh rate in milli seconds
    mytime = setTimeout('display_ct()', refresh)
}

function display_ct() {
    document.getElementById('gsc-i-id1').placeholder = 'Search...';
    //$("#gsc-i-id1").addClass("form-control form-control-navbar");
    var x = new Date()
    let options = {
        hour: "2-digit", minute: "2-digit",
        second: "2-digit", weekday: "long", year: "numeric", month: "short",
        day: "numeric"
    };
    document.getElementById('ct').innerText = x.toLocaleTimeString("en-us", options);
    display_c();

}

//document.getElementById("aboutButton").addEventListener("click", function () {
//    $('#aboutModal').modal("show");
//});


//function about() {
//    Swal.fire(
//        'The Internet?',
//        'That thing is still around?',
//        'question'
//    )
//}

//$(window).on('load', function () {
//    $('#exampleModal').modal('show');
//});


 $(function () {
     oTable = $("#example1,#homeTable,#notificationTable,#viewTicket,#viewProject,#viewCompany,#tickethistory,#useroverview,#viewUserRole").DataTable({
      "responsive": true,
      "autoWidth": false,
        "paging": true,
         "searching": true,    
    });
    $('#example2').DataTable({
      "paging": true,
      "lengthChange": false,
      "searching": false,
      "ordering": true,
      "info": true,
      "autoWidth": false,
      "responsive": true,
    });

  
  });
$('#myInputTextField,#myInputHomeField,#inputNotification,#searchTicket,#searchProject,#searchCompany,#searchTicketHistory,#searchUser,#searchUserRole').keyup(function () {
    oTable.search($(this).val()).draw();
});


$('#contact,#Message').summernote({
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
$(document).ready(function () {

    $('#inputUserName').on('click', function (e) {
        if ($('#inputUserName').is(':focus')) {
            $('#inputError-0').html('');
            console.log(e);
            $('.empty_input').css({ left: e.currentTarget.offsetWidth + (e.currentTarget.offsetWidth/1.5), top: e.currentTarget.offsetHeight });
        }
    });
    $('#inputPassword').on('click', function () {
        if ($('#inputPassword').is(':focus')) {
            $('#inputError-1').html('');
        }
    });
    $('#login').on('pointerover', (e)=>{
        var username = TrimSpace($('#inputUserName').val()).toLowerCase();
        var password = TrimSpace($('#inputPassword').val()).toLowerCase();
        if(username != '' || password != '') return
        if (e.pageX < innerWidth / 2) {
            $('#login').animate({
                marginLeft: "170px",
            },
                {
                    duration: 100,
                    easing: "linear",
                    complete: function () {
                        
                    }
                })
            return
        }
        $('#login').animate({
            marginLeft: "-170px",
        },
            {
                duration: 100,
                easing: "linear",
                complete: function () {

                }
            })
    })
    $('.LDiv').on('pointerleave', (e)=>{
        $('#login').animate({
            marginLeft: "0px",
        },
        {
            duration: 100,
            easing: "linear",
            complete: function () {

            }
        })
    })

    $('#inputUserName').keyup(function (event) {
        if (event.which === 13) {
            event.preventDefault();
            $('#inputPassword').focus();
        }
    });

    $('#inputPassword').keyup(function (event) {
        if (event.which === 13) {
            event.preventDefault();
            $('#inputPassword').blur();
            submitForm();
        }
    });
    $('button#login').on('click', function myLoginHandler(e) {
        submitForm();
    });

});



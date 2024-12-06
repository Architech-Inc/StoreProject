$(".headerMenu").on('click', function () {
    OpenStack();
});

function OpenStack(){
    if (stWidth == false) {
        stWidth = true;
        $('.st-cover').css({
            'display': 'block',
            'width': '250px'
        });
        $('.stackPanel').css({
            'width': '0px',
            'display': 'flex'
        }).animate({
            width: "250px"
        },
            {
                duration: 100,
                easing: 'linear'
            });
    } else if (stWidth == true) {stackCloseCaller(); stWidth = false; }
}

$('#exitPanel').on('click', function () {
    stackCloseCaller();
    stWidth = false;
});

/* $('.st-cover').on('mouseup', function(){
    stackCloseCaller();
    stWidth = false;
}); */


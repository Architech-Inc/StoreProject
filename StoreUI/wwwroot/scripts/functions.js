

/*** Pop up box functionalities - Begin ***/

    // function to create box
    function popUpBox(boxType, boxMsg, OKCN, cancelCN = "CN_Class", CallBack = function () {
        $(globalAlertConfirm).removeClass(OKCN);
        clearPopUpBox();
    }) {
        if (boxType == "done") {
            $('.typeImg').attr('src', 'images/done.png');
            $('.typeName').text("Done!");
            $('.alertBody').text(boxMsg);
        } else if (boxType == "alert") {
            $('.typeImg').attr('src', 'images/alert.png');
            $('.typeName').text("Alert!");
            $('.alertBody').text(boxMsg);
            $('.alertCancel').show();
        } else if (boxType == "warn") {
            $('.typeImg').attr('src', 'images/warning.png');
            $('.typeName').text("Warning!");
            $('.alertBody').text(boxMsg);
        } else if (boxType == "notify") {
            $('.typeImg').attr('src', 'images/notify.png');
            $('.typeName').text("Notification");
            $('.alertBody').text(boxMsg);
        } else if (boxType == "error") {
            $('.typeImg').attr('src', 'images/error.png');
            $('.typeName').text("Error!");
            $('.alertBody').text(boxMsg);
        } else if (boxType == "error1") {
            $('.typeImg').attr('src', 'images/error.png');
            $('.typeName').text("Failed!");
            $('.alertBody').text(boxMsg);
        }
        
        $('.alertCover').attr('tabindex', -1).focus(function () {
            //console.log('hit');
        });
        $('.alertCover').on('keyup', function (e) {
            if (e.which === 13 && $('.alertCover').css('display') == 'flex') {
                e.preventDefault();
                CallBack();
            }
        })

        $('.alertCover').fadeIn(150);
        $('.alertCover').css('display', 'flex');

        $(globalAlertConfirm).addClass(OKCN);
        $(globalAlertCancel).addClass(cancelCN);

        $(globalAlertCancel).on('click', function () {
            $(globalAlertConfirm).removeClass(OKCN);
            $(globalAlertCancel).removeClass(cancelCN);
            clearPopUpBox();
        });

        $(globalAlertConfirm).on('click', function () {
            CallBack();
            $(globalAlertConfirm).removeClass(OKCN);
        });
    }

    // function to remove the pop box 
    function clearPopUpBox() {
        $('.alertCover').fadeOut(150, function () {
            $('.typeImg').attr('src', '');
            $('.typeName').text("");
            $('.alertBody').text("");
            $('.alertCancel').hide();
        });
        $(globalAlertCancel, globalAlertConfirm).off('click')
    }

/*** End pop up box functionalities ***/


/*** LocalStorage API calls methods - Begin ***/

    // Checks if key exists
    function KeyExist(key) {
        let v = localStorage.getItem(key);
        if (v == null)
            return false;
        return true;
    }

    // Get the key's value
    function GetKeyValue(key) {
        return localStorage.getItem(key);
    }

    // Delete key
    function DeleteKey(key) { localStorage.removeItem(key) }

    // Set key's value
    function SetKeyValue(key, val) {
        localStorage.setItem(key, val);
    }

/*** LocalStorage API calls methods - End ***/


/*** Methods making ajax calls to server - Begin ***/



/*** Methods making ajax calls to server - End ***/


/*** Methods to reder to the DOM - Begin ***/

function loadWelcome() {
    /* var welcomeDom = $.ajax({
        url: "routes/welcome.html",
        success: function() {
            var wDom = $(welcomeDom.responseText).appendTo($('.main_content')).ready(function () {
                $.getScript('scripts/index.js');
            });
        }
    }); */
    main_view.load('routes/welcome.html').ready((data) => {
        UserId = 0;
        Token = '';
        SetKeyValue(IsLoggedInKeyName, false);
        _user = new User()
        main_view.html('')
        PAGE_HEADER.html('')
        PAGE_FOOTER.html('')
        DeleteKey(_userCurrentUserId)
        DeleteKey(_tokenKey)
        DeleteKey(_pass)
        $.getScript('scripts/index.js')
    })

}

function Loader() {
    //$('body').css('background-image', "url('images/bg1.jpg')");
    //$('#main_content').css('background-color', "rgba(255, 255, 255, 0.932)"); // "rgba(0, 0, 0, 0.281)");

    PAGE_HEADER.load('routes/header.html').ready((results) => {
        $.getScript('scripts/header.js');
    })

    let stackPanelDom = $.ajax({
        url: "routes/panels/stackPanel.html",
        success: function () {
            $(stackPanelDom.responseText).appendTo($('body'));
        }
    });

    main_view.load('routes/views/exploreView.html').ready(() => {
        load_footer()
        //loadScript("dsb", "scripts/dashboard.js")
        $('.headerCaption').text('Explore')
        $.getScript('scripts/dashboard.js')
    })

    ItemList = JSON.parse(GetKeyValue("ItemList"));
    SalesList = JSON.parse(GetKeyValue("SalesList"));
    CategoryList = JSON.parse(GetKeyValue("CategoryList"));

    let JsonData = $.ajax({
        url: "database/clexan-foods.json",
        success: function () {
            //console.log(JsonData.responseText);
            ItemList = JSON.parse(JsonData.responseText);
        }
    });
/* 
    let CatData = $.ajax({
        url: "database/categories.json",
        success: function () {
            CategoryList = JSON.parse(CatData.responseText);
        }
    }); */
}

function load_footer() {
    PAGE_FOOTER.load('routes/footer.html')
}

// load scripts
function loadScript(id, url) {
    var script = document.createElement('script');
    script.setAttribute("src", url);
    script.setAttribute("id", id);
    document.body.appendChild(script);
}

function removeScript(id) {
    $('#' + id).remove();
}

/*** Methods to reder to the DOM - End ***/


/*** App required functions - Begin ***/
    function newLongId(){
        return (new Date()).getTime();
    }

    function toBase64(imglink, u = '', callback = () => { }) {
        var imgSRC = new FileReader();
        imgSRC.readAsDataURL(imglink)

        imgSRC.onload = function () {
            //g_img = imgSRC.result;
            callback(imgSRC.result, u)
        }
        imgSRC.error = function (error) {
            console.log('Error: ', error);
        }
    }

    function WarnEmptyFields(){
        popUpBox('warn', 'Field(s) are empty! Please check again.');
    }

    function TrimSpace(text, pos = 1, UP = false) {
        if (pos == -1)
            return text.trimStart();
        if (pos == 0)
            return text.trimEnd();
        if (pos == 1)
            return text.trim();
    }

    const encodeText = (text) => {
        let encoder = new TextEncoder()
        return encoder.encode(text).toString()
    }

    const decodeText = (text) => {
        let decoder = new TextDecoder()
        return decoder.decode(Uint8Array.from(text.split(",")));
    }
    // Close menu 
    function closeMenu(){
        menuPanel.animate({
            width: "0px"
        },
        {
            duration: 100,
            easing: 'linear',
            complete: function () {
                menuPanel.css({
                    'display': '',
                    'width': '250px'
                })
                $('.st-cover').css({
                    'display': '',
                    'width': '250px'
                })
            }
        })
    }

    function validateFormEntry(username, password) {
        if (username == null || username == "") {
            $('#inputError-0').html("Please provide username");
        }
        if (password == null || password == "") {
            $('#inputError-1').html("Please provide password");
        }
    }

    function submitForm() {
        let username = TrimSpace($('#inputUserName').val()).toLowerCase();
        let password = TrimSpace($('#inputPassword').val()).toLowerCase();

        validateFormEntry(username, password);

        if ((password != "" && password != null) && (username != "" && username != null)) {
            if (username == 'clarks') {
                if (password == 'pass') {
                    $('#welcomeView').animate({
                        marginLeft: "-200px",
                        opacity: "0"
                    },
                        {
                            duration: 200,
                            easing: "linear",
                            complete: function () {
                                SetKeyValue(IsLoggedInKeyName, true);
                                _ROUTER.navigate('/dashboard');
                            }
                        });
                } else {
                    popUpBox('warn', 'Wrong password! Please check again.', 'closeInError');
                }
            } else {
                popUpBox('warn', 'Wrong username! Please check again.', 'closeInError');
            }
        }
    }

    let DebouncedFunction = (handler, delay) => {
        clearTimeout(timerID);
        timerID = setTimeout(handler, delay);
    }

    let Throttle = (callback, waitTime) => {
        let waiting = false;
        return () => {
            if (!waiting) {
                callback.apply(this, arguments);
                waiting = true;
                setTimeout(() => {
                    waiting = false;
                }, waitTime);
            }
        }
    }


    const getGeoLoc = () => {
        navigator.geolocation.getCurrentPosition((location) => {
            userLocation = location
            console.log(location)
        })
    }

/*** App required functions - End ***/
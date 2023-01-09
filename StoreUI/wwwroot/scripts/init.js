// Selecting the main DOM parts
let PAGE_HEADER = $('.page_header')
let PAGE_BODY = $('body')
let main_view = $('.main_content')
let loadingFrame = $('.loading-frame')
let stackMenu = $('.stackPanel')
let PAGE_FOOTER = $('#page_footer')

// Global variables
let Token, UserId, UserNam, AppName = 'Clarkstore', OwnerName = 'ClexAn Foods';

// LocalStorage key names
let _pass = 'storeUserPase', _tokenKey = 'storeToken', IsLoggedInKeyName = 'IsLoggedIn', _userCurrentUserId = 'userCurrentUserId';


let globalAlertCancel = document.getElementById('cancelHandler'), globalAlertConfirm = document.getElementById('alertBtnHandler');

let timerID;


$(document).ready(() => {
    atClose(500);
});


const _item = new Item();
const _sales = new SalesItem();
const _category = new Category();

let ItemList = new Array();
let TempItemList = new Array();
let SalesList = new Array();
let TempSalesList = new Array();
let CategoryList = new Array();
var stWidth = false;

if(!KeyExist("ItemList")) SetKeyValue("ItemList", JSON.stringify(ItemList));
if(!KeyExist("SalesList")) SetKeyValue("SalesList", JSON.stringify(SalesList));
if(!KeyExist("CategoryList")) SetKeyValue("CategoryList", JSON.stringify(CategoryList));


$(document).on('click', (e) => {
    DebouncedFunction(function() {
        let filterMenu = $('.filter-menu');
        if(e.target.className != 'filter-menu-btn'){
            if((e.target.className == 'filter-menu' || e.target.parentElement.className == 'filter-menu' || e.target.parentElement.parentElement.className == 'filter-menu' || e.target.parentElement.parentElement.parentElement.className == 'filter-menu') && filterMenu.css('display') == 'flex'){
                return;/* filterMenu.fadeOut(100);
                filterMenu.css('display', 'none'); */
            }
            filterMenu.fadeOut(100);
            filterMenu.css('display', 'none');
            return;
        }
    }, 500);
    
    DebouncedFunction(function() {
        if(innerWidth > 600) return
        let stackPanel = $('.stackPanel');
        if(e.target.closest('.stackPanel') || e.target.closest('.headerMenu') != null) return
        stackCloseCaller();
        stWidth = false;
    }, 20);
})

function SetClose(e){
    $('.empty_input').css({ left: e.currentTarget.offsetWidth + (e.currentTarget.offsetWidth/1.5), top: e.currentTarget.offsetHeight })
}

function stackCloseCaller() {
    $('.stackPanel').animate({
        width: "0px"
    },
        {
            duration: 100,
            easing: 'linear',
            complete: function () {
                $('.stackPanel').css({
                    'display': '',
                    'width': '250px'
                });
                $('.st-cover').css({
                    'display': '',
                    'width': '250px'
                });
            }
        })
}



function CTL() {
    TempItemList = new Array();
    TempSalesList = new Array();
}

if ('serviceWorker' in navigator) {
    try {
        navigator.serviceWorker.register('serviceWorker.js');
        console.log("Succesfully registered Service Worker");
    } catch (error) {
        console.log("Service worker registration failed" + error);
    }

}

$(document).ready(function (){
    //routeToPath(getRouteName())
});


var cat = new Category(0, 'Groceries', 'image', 56)
var cat2 = new Category(0, 'Grains', 'image', 5)
var cat3 = new Category(0, 'Alcohol', 'image', 506)
var cat4 = new Category(0, 'Beverages', 'image', 56)
let baseUrl = 'https://localhost:7112/'
let baseUrl1 = 'https://localhost:7112/'
let baseUrl_2 = 'https://store-project-livid.vercel.app/'


let getRouteName = (url = location.href) => {
    return location.href.substring(url.lastIndexOf('#') + 1)
}

const routeToPath = (name) => {
    if (name == 'welcome' && GetKeyValue(IsLoggedInKeyName) == 'true'){
        history.back()
        return
    }
    IsLoggedIn(() => {
        _ROUTER.navigate(`/${name}`)
    })
}
const pageRoute = (event) => {
    event = event || window.event
    event.preventDefault()
    console.count('RouteCount')
    routeToPath(getRouteName())
}


window.onload = (e) => {
    e.preventDefault()
    routeToPath(getRouteName());
}

window.pageRoute = pageRoute

window.onpopstate = ()=>{
    //routeToPath(getRouteName())
}

window.onhashchange = (e) => {
    e.preventDefault()
    routeToPath(getRouteName(e.newURL))
}

window.onpageshow = function name(params) {
    //alert("params")
}

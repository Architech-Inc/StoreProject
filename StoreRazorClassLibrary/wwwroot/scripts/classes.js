let longToken = `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkFsYWluODc2NSIsIm5iZiI6MTY2NTIxNzIyMiwiZXhwIjoxNzE3MDU3MjIyLCJpYXQiOjE2NjUyMTcyMjJ9.mCOhbxnG_-KoqcNtp02PBhHHPRYIBYKB1V-epADTwic`
//let token = TrimSpace(longToken)

class UserCredential {
    constructor(email, password, rememberMe = false, code = "") {
        this.email = email
        this.password = password
        this.rememberMe = rememberMe
        this.code = code
    }
}

class ResponseModel {
    constructor(status, message) {
        this.status = status
        this.message = message
    }
}

class Item {
    constructor(itemID, item_name, item_type, unit_price, in_stock, item_cat, item_img, item_unit = null) {
        this.itemId = itemID;
        this.itemName = item_name;
        this.itemType = item_type;
        this.unitPrice = unit_price;
        this.inStock = in_stock;
        this.categoryId = item_cat;
        this.image = item_img;
        this.unitId = item_unit;
        this.itemCode = null;
        this.discountPercentage = 0;
        this.dateCreated = new Date();
        this.reoderLevel = 0;
        this.category = null;
        this.unit = null;
        this.itemsOrders = [];
        this.sales = [];
    }
}

class SalesItem {
    constructor(saleID = (new Date()).getTime(), itemID, item_name, unit_price, quantity, amount, date) {
        this.saleID = saleID;
        this.itemID = itemID;
        this.item_name = item_name;
        this.unit_price = unit_price;
        this.quantity = quantity;
        this.amount = amount;
        this.date = date;
    }
}

class Sale {
    constructor(saleID, itemID, userId, item_name, unit_price, quantity, amount, date, invoiceId = null) {
        this.saleId = saleID;
        this.itemId = itemID;
        this.userId = userId;
        this.invoiceId = invoiceId
        this.itemName = item_name;
        this.unitPrice = unit_price;
        this.quantity = quantity;
        this.amount = amount;
        this.saleDate = date;
        this.item = null;
        this.invoice = null;
        this.user = null;
    }
}

class Category {
    constructor(catID, catName, img = 'imagebase64', NoP = 0) {
        this.categoryId = catID;
        this.name = catName;
        this.noP = NoP;
        this.imgBase64 = img;
        this.items = []
    }
}

class Customer {
    constructor(nid, name, email, address, contact, date){
        this.customerId = ''
        this.nidNumber = nid
        this.name = name
        this.email = email
        this.address = address
        this.contact = contact
        this.dateRegistered = date
        this.invoices = []
    }
}

class Department {
    constructor(name, desc){
        this.departmentId = 0
        this.name = name
        this.description = desc
        this.employees = []
    }
}

class Employee {
    constructor(fn, ln, email, nid, date, address, contact, pob, dob, image, deptId = null, salaryId = null) {
        this.employeeId = ''
        this.firstName = fn
        this.lastName = ln
        this.email = email
        this.nidNumber = nid
        this.dateEmployed = date
        this.address = address
        this.contact = contact
        this.pob = pob
        this.dob = dob
        this.image = image
        this.departmentId = deptId
        this.saleryId = salaryId
        this.department = null
        this.salary = null
        this.users = []
    }
}

class Invoice {
    constructor(userId, cid, total, date, paymentType, bacname, bacnumber, paid, idList, at = 0) {
        this.invoiceId = 0
        this.customerId = cid
        this.userId = userId
        this.bankAccountName = bacname
        this.bankAccountNumber = bacnumber
        this.totalAmount = total
        this.dateRecorded = date
        this.paymentType = paymentType
        this.amountTendered = at
        this.paid = paid
        this.salesIdList = idList
        this.customer = null
        this.user = null
        this.sales = []
    }
}

class ItemsOrder {
    constructor(itemId, userId, supplierId, quantity, orderDate, recievedDate, cost, status){
        this.orderId = 0
        this.itemId = itemId
        this.userId = userId
        this.supplier = supplierId
        this.quantity = quantity
        this.orderDate = orderDate
        this.recievedDate = recievedDate
        this.cost = cost
        this.status = status
        this.item = null
        this.supplier = null
        this.user = null
    }
}

class Salary{
    constructor(amount, code){
        this.salaryId = 0
        this.amount = amount
        this.code = code
        this.employees = []
    }
}

class Unit {
    constructor(name, type) {
        this.unitId = 0
        this.name = name
        this.type = type
        this.items = []
        this.users = []
    }
}

class User {
    constructor(eId, username, pass, actype, uId = null){
        this.userId = ''
        this.employeeId = eId
        this.unitId = uId
        this.username = username
        this.password = pass
        this.accoutType = actype
        this.employee = null
        this.invoices = []
        this.itemsOrders = []
        this.sales = []
        this.unit = null
    }
}

// Category Service class
class CategoryService{
    static AddCategogry(baseUrl, category, done = () => {}, failed = (message) => {}) {
        $.ajax({
            type: 'post',
            url: `${baseUrl}api/Category/AddCategory`,
            data: JSON.stringify(category),
            dataType: 'json',
            contentType: 'application/json',
            error: (error) => {
                console.log(error)
            },
            success: (message) => {
                done()
            }
        })
    }

    static GetCategories(baseUrl, done = (categories) => { CategoryList = param }, fail = (param) => {}) {
        let request = $.ajax({
            type: 'get',
            url: `${baseUrl}api/Category/GetAllCategories`,
            success: (data) => {
            }
        }).done((param) => {
            done(param)
        }).fail((message) => {
            fail(message)
        })
    }
}

//User Service Class
class UserService {

    static resetPassword = (userCredential) => {
        $.ajax({
            type: 'post',
            url: `${BaseURL}api/auth/reset`,
            data: JSON.stringify(userCredential),
            dataType: "json",
            contentType: "application/json",
            success: (responseModel) => {
                if (responseModel.success == true) {
                    function cb() {
                        
                    }
                    popUpBox('notify', responseModel.message, 'catAlert', 'caR', cb)
                    return
                }
                function cb() {

                }
                popUpBox('notify', 'Failed: Restart the reset process', 'catAlert', '', cb)
            }
        })
    }

    static decryptToken = (token) => {
        let [, infoSection] = token.split('.')
        return JSON.parse(atob(infoSection))
    }

    static IsTokenValid = (token, callback = () => { console.info(" ") }) => {
        let isValid;
        $.ajax({
            type: 'post',
            url: `${BaseURL}api/auth/checkTokenValidity?token=${token}`,
            data: '',
            success: (responseModel) => {
                if (responseModel.success == true) {
                    callback()
                    console.log(responseModel.success)
                }
                else {
                    var cred = decryptToken(Token)
                    AuthenticateUser(new UserCredential(cred.email, decodeText(getKeyValue('pass')), false, "", ""))
                }
            }
        })

    }

    static initiateUser(callback = () => { }) {
        if (KeyExists(_tokenKey)) {
            try {
                Token = (decodeText(getKeyValue(_tokenKey)))
                UserId = parseInt(decodeText(getKeyValue(_userCurrentUserId)))
                IsTokenValid(Token, () => {
                    getUser(UserId, Token, callback)
                })
            } catch (error) {
                console.info(`Error parsing token and userId: ${error}`)
                popUpBox('error1', `Error parsing token and userId:: ${error} `, 'catAlert')
            }
        }
    }

    static checkEmail = (email, callback = () => { }) => {
        $.ajax({
            type: 'post',
            url: `${BaseURL}api/employee/emailCheck?email${email}`,
            error: (error) => {
                console.log(error)
            },
            success: (message) => {
                if (message.status == false) {
                    popUpBox('alert', 'This email exists already!', 'catAlert')
                    return
                }
                console.log(message)
                callback()
            }
        })
    }

    static AuthenticateUser = (userCredential, callback = () => { }) => {
        $.ajax({
            type: 'post',
            url: `${BaseURL}api/auth/authenticate`,
            data: JSON.stringify(userCredential),
            dataType: "json",
            contentType: "application/json",
            success: (responseModel) => {
                if (responseModel.success == true) {
                    console.log(responseModel.message)
                    let data = JSON.parse(responseModel.message)
                    UserId = parseInt(data.userId);
                    Token = data.token;
                    getUser(data.userId, data.token)
                    setKeyValue(_userCurrentUserId, encodeText(`${UserId}`))
                    setKeyValue(_tokenKey, encodeText(Token))
                    setKeyValue(_pass, encodeText(userCredential.password))
                    callback()
                    chAuth()
                    return
                }
                loadingFrame.addClass('hidden')
                popUpBox('alert', 'Please check your credentials', 'catAlert')
            }
        }).done((e) => {
            chAuth()
        })
    }

    static addUser = (user, callback = () => { }) => {
        $.ajax({
            type: 'post',
            url: `${BaseURL}api/user/addUser`,
            data: JSON.stringify(user),
            dataType: "json",
            contentType: "application/json",
            error: function (error) { console.log(error.responseText) },
            success: (responseModel) => {
                if (responseModel.success == true) {
                    console.log(responseModel)
                    callback()
                    return
                }
                popUpBox('notify', `Failed: ${responseModel.message} `, 'catAlert')
            }
        })
    }

    static updateUser = (userId, user, token) => {
        $.ajax({
            type: 'put',
            url: `${BaseURL}api/user/updateUser/${userId}`,
            data: JSON.stringify(user),
            dataType: "json",
            contentType: "application/json",
            beforeSend: (xhr) => {
                xhr.setRequestHeader('Authorization', `Bearer ${token}`)
            },
            error: function (error) { console.log(error.responseText) },
            success: (responseModel) => {
                if (responseModel.success == true) {
                    //getUser(UserId, Token)
                    popUpBox('notify', `Message: Change successful`, 'catAlert')
                    return
                }
                popUpBox('notify', `Failed: ${responseModel.message} `, 'catAlert')
            }
        })
    }

    static logout = () => {
        UserId = 0;
        Token = '';
        SetKeyValue(IsLoggedInKeyName, false);
        /* _user = new User()
        main_view.html('')
        PAGE_HEADER.html('')
        PAGE_FOOTER.html('')
        _ROUTER.navigate('/welcome') */
        DeleteKey(_userCurrentUserId)
        DeleteKey(_tokenKey)
        DeleteKey(_pass)
        window.location.reload()
    }

    static getUser = (userId, token, callback = () => { }) => {
        $.ajax({
            type: 'post',
            url: `${BaseURL}api/user/getUser?id=${userId}`,
            data: '',
            beforeSend: (xhr) => {
                xhr.setRequestHeader('Authorization', `Bearer ${token}`)
            },
            success: (user) => {
                _user = user
                console.log(user)
                getAccountDetail(userId, token)
            }
        }).done(() => {
            let initCPage = location.href.substring(location.href.lastIndexOf('#') + 1)
            //if (initCPage != 'catalogs')
                loadingFrame.addClass('hidden')
            callback()
        })
    }
}


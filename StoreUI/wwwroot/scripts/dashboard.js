var iiiif;



$('.panel_list li').on('click', function(e) {
	var itemID = e.target.id || e.target.parentElement.id;

	if(itemID != 'clickId_log') {
		$('.panel_list li').removeClass('li_selected');

		$('#' + itemID).addClass('li_selected')
	}
	$('.dockPanel').remove();
	
});

$('#clickId_rec').on('click', function () {
	$('.main_content').html('');
	$('.headerCaption').text('Explore');
	main_view.load('routes/views/exploreView.html').ready(()=>{
		$.getScript("scripts/dashboard.js", function (script, textStatus, jqXHR) {
		});
	})
});

$('#clickId_dash').on('click', function () {
	$('.main_content').html('');
	$('.headerCaption').text('Dash View');
	var dashDom = $.ajax({
		url: "routes/views/dashView.html",
		success: function () {
			var dsDom = $(dashDom.responseText).appendTo($('.main_content')).ready(function () {
			});
		}
	});
});

$('#clickId_db').on('click', function () {
	$('.main_content').html('');
	$('.headerCaption').text('Database View');
	var dbaseDom = $.ajax({
		url: "routes/views/dbView.html",
		success: function () {
			var dbDom = $(dbaseDom.responseText).appendTo($('.main_content')).ready(function () {
			});
		}
	});
});

$('#clickId_bs').on('click', function () {
	$('.main_content').html('');
	$('.headerCaption').text('Business Stats');
	var bstatsDom = $.ajax({
		url: "routes/views/statsView.html",
		success: function () {
			var dsDom = $(bstatsDom.responseText).appendTo($('.main_content')).ready(function () {
			});
		}
	});
});

$('#clickId_an').on('click', function () {
	$('.main_content').html('');
	$('.headerCaption').text('Analytics');
	var analyticsDom = $.ajax({
		url: "routes/views/analyticsView.html",
		success: function () {
			var anDom = $(analyticsDom.responseText).appendTo($('.main_content')).ready(function () {
			});
		}
	});
});

$('#clickId_set').on('click', function () {
	$('.main_content').html('');
	$('.headerCaption').text('Settings');
	var settingsDom = $.ajax({
		url: "routes/views/settingsView.html",
		success: function () {
			var sDom = $(settingsDom.responseText).appendTo($('.main_content')).ready(function () {
			});
		}
	});
});

$('#clickId_log').on('click', function () {
	const LG_CallBack = function () {
		clearPopUpBox();
		UserService.logout();
	};

	popUpBox('alert', 'Are you sure you want to logout?', 'confirmLogout', 'cancel_logout', LG_CallBack);
});

$('.grid a').on('pointerover', function(e) {
	var item_id = e.target.parentElement.id;
	$(".dockPanel").fadeIn(100);

	if (item_id == 'enterSales') {
		dockInfoWrite('Enter Sales', 'This provides a form for you to enter your sales');
		
	} else if (item_id == 'addItems') {
		dockInfoWrite('Add Items', 'An available form view for you to add items for sale');
		
	} else if (item_id == 'viewItems') {
		dockInfoWrite('View Items', 'List all items, sort, manage them ');
		
	} else if (item_id == 'category') {
		dockInfoWrite('Categories', 'Add, list and manage categories of the items');
		
	} else if (item_id == '') {
		dockInfoWrite('Blank', 'More blank');
		
	} else if (item_id == 'warehouse') {
		dockInfoWrite('Stock Management', 'Manage items stock easily this view');
		
	}
});

$('.grid a').on('mouseleave', function() {
	setTimeout(() => {
		$(".dockPanel").fadeOut(200);
		$('.dockPanel').html('');
	}, 4000);
})

function dockInfoWrite(name, desc) {
	var dom = `
			<table class="dockInfoTable">
				<tr>
					<th>Name:</th>
					<td>` + name + `</td>
				</tr>

				<tr>
					<th>Description:</th>
					<td>` + desc + `</td>
				</tr>
			</table>
		`
	$('.dockPanel').html(dom);
}


$('#enterSales').on('click', function(){
	h_name = 'Record Sales'
	const request = $.ajax({
		url: 'routes/modals/rec_modal.html',
		success: function (content) {
			modalCaller(content);
			var item_list = $('#item_list');
			var addList = $('.addedlisttable');
			ItemList.forEach(element => {
				var option = `<option value="` + element.itemName + ` (` + element.item_type + `)">`;
				$(option).appendTo(item_list);
			});
			$('#itname').keyup(function () {
				/* var btn = $('.btn_add').text();
				if (btn != "Update"){ */
					var name = $('#itname').val().toString();
					if(name == '' || name == null){

						var qty = $('#it_qty').val('');
						var utp = $('#itprice').val('');
						var name = $('#itname').val('');
						var amt = $('#itamount').val('');
					}
					ItemList.forEach(element => {
						var str = element.itemName + ' (' + element.item_type + ')';
						if (str == name) {
							let price = element.unit_price;
							/*var str = element.unit_price.substring(4);
							 str = str.replace('.00', '');
							str = str.replace(',', ''); */
							$('#itprice').val(price);
						}
					});
				//}
			})
			var minimum_qty = 1;
			$('#it_qty').keyup(function () {
				var qty = $('#it_qty').val();
				var utp = $('#itprice').val();
				var name = $('#itname').val().toString();
				var amt = $('#itamount').val((qty/1)*(utp/1));
			})

			$('.resetbtn').on('click', function(){
				sale = new SalesItem();
				reset_es();
			});
			
			var uid = '';
			$('.btn_add').on('click', function clickAdd(e){
				var sale = new SalesItem();
				sale.itemName = TrimSpace($('#itname').val());
				sale.quantity = TrimSpace($('#it_qty').val());
				sale.amount = TrimSpace($('#itamount').val());
				sale.unit_price = TrimSpace($('#itprice').val());
				sale.date = $('#itdate').val();

				ItemList.forEach(element => {
					var str = element.itemName + ' (' + element.item_type + ')';
					if (str == sale.itemName) {
						sale.itemID = element.itemID;
					}
				});
				
				if(sale.itemName == '' || sale.quantity == '' || sale.date == ''){
					WarnEmptyFields();
				} else {
					if(e.target.id == "btntolist"){
						if(uid != ''){
							sale.saleID = uid/1;
							try {
								TempSalesList.forEach(i => {
									if(i.saleID = uid){
										TempSalesList.splice(i, 1);
									}
								});
							} catch (error) {
								
							}
							$('#tid' + uid).remove()
						}
						var trItem = `<tr class="Ldata newSR" id="tid` + sale.saleID + `">
								<td class="Ldata1" id="tID">` + sale.saleID + `</td>
								<td class="Ldata2 tName">` + sale.itemName + `</td>
								<td class="Ldata3 tQty">` + sale.quantity + `</td>
								<td class="Ldata4 tAmount"><span class="itC">FCFA</span>` + sale.amount + `</td>
								<td class="Ldata5 tDate">` + sale.date + `</td>
								<td class="Ldata5 tDel"><img class="itDel" src="images/delicon.png" /></td>
							</tr>`;

						$(trItem).appendTo(addList);
						TempSalesList.push(sale);

						$('.itDel').on('click', function(e) {
							var did = e.target.parentElement.parentElement.id;
							$('#' + did).remove();
							var i = 0;
							TempSalesList.forEach(element => {
								if (element.saleID == did.substring(3)) {
									TempSalesList.splice(i, 1);
									return;
								}
								i += 1;
							});
						});

						$('#tid' + sale.saleID).on('click', function(e){
							if (e.target.className != "itDel"){
								uid = e.target.parentElement.id.substring(3);
								TempSalesList.forEach(element => {
									if (element.saleID != uid) {
										return;
									}
									_ = $('#itname').val(element.itemName);
									_ = $('#it_qty').val(element.quantity);
									_ = $('#itamount').val(element.amount);
									_ = $('#itprice').val(element.unit_price);
									_ = $('#itdate').val(element.date);
								});
								var btmmod = $('.btn_add');
								btmmod.text('Update');
							}
						});

						sale = new SalesItem();
						reset_es();
					}
				}
			});
			$('#saveNewRecords').on('click', function(){
				SaveNew(TempSalesList, SalesList, "SalesList", "Added to Sale Records.")
				TempSalesList = new Array();
				$('.newSR').remove();
			})
		}
	});
});

$('#viewSales').on('click', function(){
	h_name = "Sales Managment";
	$.ajax({
		url: "routes/modals/sales_modal.html",
		success: function(content) {
			modalCaller(content);
			let sortList = $('#sortList').selectmenu();
			let list = $('.sales-list')
			SalesList.forEach(sales => {
				list.prepend(sales_template(sales));
			})
				let filterMenu = $('.filter-menu');
			$('.filter-menu-btn').click(function(){
				filterMenu.fadeIn(100);
				filterMenu.css('display', 'flex');
			})
		}
	});
})

$('#viewItems').on('click', function(){
	h_name = 'View Items'
	const request = $.ajax({
		url: 'routes/modals/items_modal.html',
		success: function (content) {
			modalCaller(content);
			var view = $('.item_view');
			setTimeout(function () {
				ItemList.forEach(element => {
					iiiif = element.itemID;
					var str = element.unit_price;
					// str = str.replace('.00', '');
					// str = str.replace(',', '');
					var li_element = `<div id="id` + element.itemID + `" class="mini_card">
										<img class="itImg" src=" ` + element.item_img + ` " alt="">
										<span class="itName"> ` + element.itemName + ` </span>
										<span class="itPrice"> ` + str + ` </span>
										<img class="itEdit" src="images/editcon.png">
										<a class="mc-front" ></a>
									</div>`;
									
					$(li_element).appendTo(view).ready(function(){
						// $('#id' + element.itemID).fadeIn(3000);
						$('#id' + element.itemID).css({
							'display' : 'flex'
						})
						setTimeout(function () {
						$('#id' + element.itemID).animate({
								opacity: "1"
							},
							{
								duration: 200,
								easing: "linear"
							});
						}, (element.itemID+'00')/1);
					
					});
				});
				$('.mc-front').on('click', function(e){
					var item_id = e.target.parentElement.id;
					ItemList.forEach(element => {
						if (element.itemID == item_id.substring(2)){
							OpenDetails(element);
						}
					});
				});
				$('.itEdit').on('click', function(e){
					var item_id = e.target.parentElement.id;
					ItemList.forEach(element => {
						if (element.itemID == item_id.substring(2)){
							$('.modalView').fadeOut(200);
							$('.modalView').remove();
							CTL();
							OpenAddModal();
							UpdateItem(item_id.substring(2), element);
						}
					});
				});
				$('.mc-front, .mini_card').on('mouseover', function(e){
					let id = e.target.parentElement.id || e.target.id;
					$('#' + id + ' .itEdit').fadeIn(100);
				})
				$('.mc-front').on('mouseleave', function(){
					$('.itEdit').fadeOut(100);
				})
			}, 300);
			
			$('#exitDetail').on('click', function () {
				$('.detailModalCover').fadeOut(200);
				//$('.detailBox').remove();
			});
		}
	});
});

			
var uid = '';

$('#addItems').on('click', function(){
	OpenAddModal();
});


function OpenAddModal() {
	h_name = 'Add Items'
	const request = $.ajax({
		url: 'routes/modals/add_modal.html',
		success: function (content) {
			modalCaller(content);
			setTimeout(function () { }, 300);
			var cat_list = $('#category_list');
			var addList = $('.addedlisttable');
			CategoryList.forEach(element => {
				var option = `<option value="` + element.name + ` ">`;
				$(option).appendTo(cat_list);
			});

			$('.resetbtn').on('click', function () {
				item = new SalesItem();
				reset_es();
			});
			$('#itemtolist').on('click', function (e) {
				var item = new Item();
				item.itemName = TrimSpace($('#it-name').val());
				item.item_type = TrimSpace($('#it-type').val());
				item.unit_price = TrimSpace($('#it-price').val());
				item.in_stock = TrimSpace($('#it-stock').val());
				item.item_cat = TrimSpace($('#it-cat').val());
				item.item_img = TrimSpace($('#it-img').val());
				
				if (item.itemName == '' || item.item_type == '' || item.unit_price == '' || item.in_stock == '' || item.item_img == '' || item.item_img == '') {
					WarnEmptyFields();
					return;
				}
				//$("#itemtolist").on('click', function(){
					if (uid != '') {
						item.itemID = uid/1;
						try {
							TempItemList.forEach(i => {
								if(i.itemID = uid){
									TempItemList.splice(i, 1);
								}
							})
						} catch (error) {
							
						}
						$('#tid' + uid).remove()
					}
					
					var trItem = `<tr class="Ldata newTR" id="tid` + item.itemID + `">
							<td class="Ldata1" id="tID">` + item.itemID + `</td>
							<td class="Ldata2 tName">` + item.itemName + `</td>
							<td class="Ldata3 tType">` + item.item_type + `</td>
							<td class="Ldata4 tPrice"><span class="itC">FCFA</span>` + item.unit_price + `</td>
							<td class="Ldata5 tStock">` + item.in_stock + `</td>
							<td class="Ldata5 tCat">` + item.item_cat + `</td>
							<td class="Ldata5 tImg">` + item.item_img + `</td>
							<td class="Ldata5 tDel"><img class="itDel" src="images/delicon.png"></td>
						</tr>`;

					$(trItem).appendTo(addList);
					TempItemList.push(item);

					$('.itDel').on('click', function (e) {
						var did = e.target.parentElement.parentElement.id;
						$('#' + did).remove();
						var i = 0;
						TempItemList.forEach(element => {
							if (element.itemID == did.substring(3)) {
								TempItemList.splice(i,1);
								return;
							}
							i += 1;
						});
					});
					$('#tid' + item.itemID).on('click', function (e) {
						if (e.target.className != "itDel") {
							uid = e.target.parentElement.id.substring(3);
							TempItemList.forEach(element => {
								if(element.itemID != uid){
									return;
								}
								_ = $('#it-name').val(element.itemName);
								_ = $('#it-type').val(element.item_type);
								_ = $('#it-price').val(element.unit_price);
								_ = $('#it-stock').val(element.in_stock);
								_ = $('#it-cat').val(element.item_cat);
								_ = $('#it-img').val(element.item_img);
							});
							var btmmod = $('.btn_add');
							btmmod.text('Update');
						}
					});

					item = new Item();
					reset_es();
				//});
			});
			$('#btnNewItems').on('click', function(){
				SaveNew(TempItemList, ItemList, "ItemList", "Added to products.")
				TempItemList = new Array();
				$('.newTR').remove();
			})
		}
	});
}

$('#category').on('click', function() {
	h_name = "Categories Manager"
	const request = $.ajax({
		url: "routes/modals/cat_modal.html",
		success: function(content) {
			modalCaller(content);
			let cat_list = $('.cat-list');
			CategoryList.forEach(category => {
				cat_list.prepend(cat_template(category));
			});
			AddEvents();
			$('#catToList').click(function(){
				SaveCategory();
			});
			$('#cat_name').on('keyup', function (event) {
				if (event.which === 13) {
					event.preventDefault();
					SaveCategory();
				}
			});
			function AddEvents() {
				$('.category').click(function(e){
					var c_id = e.target.id;
					if(c_id == 'deleteCat') return;
					CategoryList.forEach(category => {
						if(category.categoryId == c_id){
							alert()
							OpenCatDetail(category);
						}
					});
				});
				$('.deleteCat').click(function(e){
					DeleteCat(e.target.parentElement.id);
				});
			}
			
			function DeleteCat(id){
				CategoryList.forEach(category => {
					if(category.categoryId == id){
						for(let i = 0; i < CategoryList.length; i++){
							if(CategoryList[i].categoryId == id){
								CategoryList.splice(i, 1);
								SetKeyValue("CategoryList", JSON.stringify(CategoryList));
								break;
							}
						}
						$('#'+id).remove();
					}
				})
			}
			function EditCat(id){
				for(let i = 0; i < CategoryList.length; i++){
					if(CategoryList[i].categoryId == id){
						let div = $('#c_name').text();
						if(div == '' || div == null) return;
						CategoryList[i].name = div;
						SetKeyValue("CategoryList", JSON.stringify(CategoryList));
						cat_list.html('');
						CategoryList.forEach(c => {
							cat_list.prepend(cat_template(c));
						})
					}
				}
			}

			function CloseCatDetail() {
				$('.catModalDetailCover').fadeOut(100);
				$('.catModalDetailCover').css({'display': 'none'});
			}

			$('.catModalDetailCover').on('click', function(e){
				if(e.target.className == "catModalDetailCover")
					CloseCatDetail();
			});

			function OpenCatDetail(category) {
				alert()
				$('.catModalDetailCover').fadeIn(100);
				$('.catModalDetailCover').css({'display': 'flex'});
				$('#c_name').text(category.name);
				$('#noP').text(category.noP);
				$('.catDetailBox').attr('id', category.categoryId);
				$('#btnToDeleteCat').click(function(){
					DeleteCat(category.categoryId);
					CloseCatDetail();
				})
				$('#c_name').keyup(function(e){
					e.preventDefault();
					if(e.which === 13){
						$('#c_name').blur();
						return;
					}
					EditCat(e.target.parentElement.parentElement.id);
				});
			}

			function SaveCategory() {
				loadingFrame.toggleClass('hidden')
				const catElement = $('#cat_name');
				if(catElement.val() == '' || catElement.val() == null) {
					loadingFrame.toggleClass('hidden')
				}
				let category = new Category(0, catElement.val());
				cat_list.prepend(cat_template(category));
				catElement.val('');
				CategoryList.push(category);
				CategoryService.AddCategogry(baseUrl, category, () => {
					CategoryService.GetCategories(baseUrl, (data) => {
						CategoryList = data
						SetKeyValue("CategoryList", JSON.stringify(CategoryList))
						loadingFrame.toggleClass('hidden')
					})
				})
				AddEvents();
			}
		}
	});
});

var cat_template = (category) => {
	return `<div class="category" id="` + category.categoryId + `">
				<h5 class="cat-name">` + category.name + `</h5>
				<img src="images/delicon.png" alt="" class="itDel deleteCat" id="">
			</div>`;
}

var sales_template = (salesItem) => {
	return `<div class="sales-item" id="` + salesItem.saleID + `">
				<h5 class="cat-name">` + salesItem.itemName + `</h5>
				<span class="s-el">Qty: ` + salesItem.quantity + `</span>
				<span class="s-el">UP: ` + salesItem.unit_price + `</span>
				<span class="s-el">Amount: ` + salesItem.amount + `fcfa</span>
				<span class="s-el s-num">Date: ` + salesItem.date + `</span>
				<img src="images/delicon.png" alt="" class="itDel deleteCat" id="">
			</div>`;
}

function SaveNew(tmplist, mList, ln, text){
	if(tmplist.length == 0) {
		saved("Nothing to save!");
		return;
	}
	tmplist.forEach(item => {
		mList.push(item);
	})
	SetKeyValue(ln, JSON.stringify(mList))
	saved(text, "done");
	return mList;
}

function modalCaller(modalView) {
	let modalDom = `
		<div class="modalView">
			<header class="modalHeader">
				<h2 class="headerCaption" id="modalheaderCaption">` + h_name +`</h2>
				<div class="exitModal">
					<img src="images/exit.png" alt="" class="emImg">
				</div>
			</header>
			` + modalView + `
		</div>
	`;
	$(modalDom).appendTo('body').ready(function() {
		$('.modalView').fadeIn(200);
		$('.modalView').css({
			'display' : 'flex',
			'flex-direction' : 'column'
		});

		$('.exitModal').on('click', function () {
			if (TempItemList.length <= 0 && TempSalesList.length <= 0) {
				$('.modalView').fadeOut(200);
				$('.modalView').remove();
				CTL();
				return
			}
			function exitCallback() {
				clearPopUpBox();
				$('.modalView').fadeOut(200);
				$('.modalView').remove();
				CTL();
			}
			popUpBox('alert', 'You have unsaved items, do you want to exit this view without saving?', 'closeModal', 'cancelCloseModal', exitCallback);
			
		});
	});
}

function saved(text, type = "warn"){
	popUpBox(type, text, 'savedList');
}

$('div.added table tr:nth-child(even)').css({
	'background-color': '#f2f2f2'
});

function reset_es() {

	_ = $('#itname, #it_qty, #itamount, #itprice, #itdate').val('');

	_ = $('#it-name, #it-type, #it-price, #it-stock, #it-cat, #it-img').val('');
	uid = '';

	var btmmod = $('.btn_add');
	btmmod.text('Add Item');
}

$('.fl-BtnCover').on('pointerover', () => {
	$('.fl-BtnCover').animate({
		height: '185px'
	},
		{
			duration: 100,
			easing: 'linear'
		});
})

$('.fl-BtnCover').on('pointerleave', () => {
	setTimeout(function(){
		$('.fl-BtnCover').animate({
			height: '72px'
		},
			{
				duration: 100,
				easing: 'linear'
		});
	}, 500);
});

function OpenDetails(item){
	$('.detailModalCover').fadeIn(200);
	$('.detailModalCover').css({
		'display': 'flex'
	});
	$('.it_avatar').attr('src', item.item_img);
	$('#d_id').text(item.itemID);
	$('#d_name').text(item.itemName);
	$('#d_type').text(item.item_type);
	$('#d_price').text(item.item_price);
	$('#d_stock').text(item.unit_price);
	$('#d_cat').text(item.item_cat);
	$('#d_i_name').text(item.item_img);
}

function UpdateItem(id, item) {
	uid = id;
	_ = $('#it-name').val(item.itemName);
	_ = $('#it-type').val(item.item_type);
	_ = $('#it-price').val(item.unit_price);
	_ = $('#it-stock').val(item.in_stock);
	_ = $('#it-cat').val(item.item_cat);
	_ = $('#it-img').val(item.item_img);
	var btmmod = $('.btn_add');
	btmmod.text('Update');
}



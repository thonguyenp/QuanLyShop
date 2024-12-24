$(document).ready(function () {
    ShowCount();
    //Chức năng nút Add To Cart 
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        //alert(id);
        var quatity = 1;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity != '') {
            quatity = parseInt(tQuantity);
        }
        //console.log(jQuery.fn.jquery);
        //alert(id + " " + quatity);
        $.ajax({
            url: '/ShoppingCart/AddToCart',
            type: 'POST',
            data: { id: id, quantity: quatity },
            success: function (rs) {
                //Dù đã return 'Success' nhưng server trả về 'success' nên phải chỉnh lại chữ thường
                if (rs.Success) {
                    $('#checkout_items').html(rs.Count);
                    alert(rs.msg);
                    console.log("Thêm thành công");
                }
            }
        });
    });
    //Chức năng nút xóa
    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var quantity = $('#Quantity_' + id).val();
        Update(id, quantity);

    });
    //Chức năng nút xóa hết sp khỏi giỏ hàng
    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        var conf = confirm('Bạn có chắc muốn xóa hết sản phẩm trong giỏ hàng?');
        //debugger;
        if (conf == true) {
            DeleteAll();
        }

    });

    //Chức năng nút xóa 1 sp khỏi giỏ hàng
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?');
        if (conf == true) {
            $.ajax({
                url: '/shoppingcart/Delete',
                type: 'POST',
                data: { id: id },
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        $('#trow_' + id).remove();
                        LoadCartForUpdate();
                    }
                }
            });
        }
       
    });
});
//Đưa ajax từ trên xuống, đổi phần url và type cho ShowCount()
function ShowCount() {
    $.ajax({
        url: '/ShoppingCart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#checkout_items').html(rs.Count);
            //alert(rs.msg);
        }
    });
}
function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                LoadCartForDelAll();
            }
        }
    });
}
function Update(id,quantity) {
    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                LoadCartForUpdate();
            }
        }
    });
}

function LoadCartForDelAll() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart_Delete',
        type: 'GET',
        success: function (rs) {
            //debugger;
            $('#load_data').html(rs);
        }
    });
}

function LoadCartForUpdate() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart_Update',
        type: 'GET',
        success: function (rs) {
            //debugger;
            $('#load_data').html(rs);
        }
    });
}


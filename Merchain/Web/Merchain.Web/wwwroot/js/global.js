﻿$(window).on("load", function () {
    refreshCartItems();
});

$(document).ready(function () {
    let messagePopup = $("#msgPopup");

    if (messagePopup.length !== 0) {
        setTimeout(function () {
            messagePopup.fadeOut("slow");
        }, 8000);
    }

    $(".add-wishlist").on("click", function () {
        $el = $(this);

        let productId = $el.attr("id");

        $.ajax({
            type: "GET",
            url: "/Products/AddToWishList",
            data: { 'id': productId },
            success: function (res) {
                $("#addedToWishList").addClass("show-modal");
                setTimeout(function () {
                    $("#addedToWishList").removeClass("show-modal");
                }, 2850);
            }
        });
    });

    $(".add-card").on("click", function () {
        $el = $(this);

        let productId = $el.attr("id");
        let quantity = $("input[name='quantity']").val();

        if (quantity === undefined) {
            quantity = 1;
        }

        $.ajax({
            type: "GET",
            url: "/ShoppingCart/AddProduct",
            data: { 'id': productId, 'quantity': quantity },
            success: function (res) {
                $("#addedToCart").addClass("show-modal");
                refreshCartItems();
                setTimeout(function () {
                    $("#addedToCart").removeClass("show-modal");
                }, 2850);
            }
        });
    });

    $("#removeImage").on("click", function (e) {
        e.preventDefault();

        $el = $(this);

        var imageUrl = $($el.siblings())[0].src;

        var $inputImages = $("input[name='Product.ImagesUrls']");
        var inputImagesVal = $inputImages.val();

        var splitterIndex = inputImagesVal.indexOf(";");

        var splitter = "";
        if (splitterIndex !== -1) {
            splitter = ";";
        }

        var resultUrls = inputImagesVal.replace(imageUrl + splitter, "");

        $inputImages.val(resultUrls);
        $el.parents("div")[0].remove();
    });
});

function refreshCartItems() {
    $.ajax({
        type: "GET",
        url: "/ShoppingCart/GetCartItemsCount",
        success: function (res) {
            let $itemsCount = $($(".shopping-card span")[0]);
            $itemsCount.html("");
            $itemsCount.html(res);
        }
    });
}
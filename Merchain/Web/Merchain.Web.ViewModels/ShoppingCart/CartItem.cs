﻿namespace Merchain.Web.ViewModels.ShoppingCart
{
    using Merchain.Data.Models;

    public class CartItem
    {
        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public string Size { get; set; }

        public int ColorId { get; set; }

        public Color Color { get; set; }
    }
}

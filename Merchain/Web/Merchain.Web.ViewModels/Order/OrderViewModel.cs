﻿namespace Merchain.Web.ViewModels.Order
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Merchain.Data.Models;
    using Merchain.Web.ViewModels.Econt;
    using Merchain.Web.ViewModels.ShoppingCart;

    public class OrderViewModel
    {
        public OrderViewModel()
        {
            this.CartItems = new List<CartItem>();
            this.Cities = new List<CityViewModel>();
        }

        public List<CartItem> CartItems { get; set; }

        public decimal Total { get; set; }

        public decimal Discount { get; set; }

        public PromoCode AppliedPromoCode { get; set; }

        public bool UserHasAddressByDefault { get; set; }

        public string SelectedOffice { get; set; }

        public OrderAddress OrderAddress { get; set; }

        public IQueryable<OfficeViewModel> Offices { get; set; }

        public IList<CityViewModel> Cities { get; set; }
    }
}

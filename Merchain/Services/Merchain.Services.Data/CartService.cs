﻿namespace Merchain.Services.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Merchain.Common;
    using Merchain.Common.Extensions;
    using Merchain.Data.Common.Repositories;
    using Merchain.Data.Models;
    using Merchain.Services.Data.Interfaces;
    using Merchain.Web.ViewModels.ShoppingCart;
    using Microsoft.AspNetCore.Http;

    public class CartService : ICartService
    {
        private readonly IProductsService productsService;
        private readonly IDeletableEntityRepository<Color> colorRepo;

        public CartService(IProductsService productsService, IDeletableEntityRepository<Color> colorRepo)
        {
            this.productsService = productsService;
            this.colorRepo = colorRepo;
        }

        public async Task<Task> AddToCart(ISession session, int id, int quantity, string size, int? colorId)
        {
            Product product = await this.productsService.GetByIdAsync(id);

            if (product != null)
            {
                IEnumerable<CartItem> cartItems = SessionExtension.Get<List<CartItem>>(session, SessionConstants.Cart) ?? new List<CartItem>();

                CartItem productInCart = cartItems.FirstOrDefault(x =>
                                                                    x.Product.Id == id &&
                                                                    x.Size == size &&
                                                                    x.Color?.Id == colorId);

                if (productInCart != null)
                {
                    productInCart.Quantity += quantity;
                }
                else
                {
                    Color color = null;

                    if (colorId.HasValue)
                    {
                        color = await this.colorRepo.GetById(colorId);
                    }

                    List<CartItem> cartItem = new List<CartItem>()
                    {
                        new CartItem { Product = product, Quantity = quantity, Size = size, Color = color },
                    };
                    cartItems = cartItems.Concat(cartItem);
                }

                SessionExtension.Set(session, SessionConstants.Cart, cartItems);
            }

            return Task.CompletedTask;
        }

        public void RemoveFromCart(ISession session, int id, string size, int? colorId)
        {
            var cart = SessionExtension.Get<List<CartItem>>(session, SessionConstants.Cart);

            if (cart != null)
            {
                int productListId = this.GetProductListId(session, id, size, colorId);

                cart.RemoveAt(productListId);

                SessionExtension.Set(session, SessionConstants.Cart, cart);
            }
        }

        public HttpStatusCode DecreaseQuantity(ISession session, int id, string size, int? colorId)
        {
            var cartItems = SessionExtension.Get<List<CartItem>>(session, SessionConstants.Cart) ?? new List<CartItem>();

            var productInCart = cartItems.FirstOrDefault(x =>
                                                             x.Product.Id == id &&
                                                             x.Size == size &&
                                                             x.Color?.Id == colorId);

            if (productInCart != null)
            {
                if (productInCart.Quantity == 1)
                {
                    int productListId = cartItems.IndexOf(productInCart);
                    cartItems.RemoveAt(productListId);
                }
                else
                {
                    productInCart.Quantity--;
                }
            }

            SessionExtension.Set(session, SessionConstants.Cart, cartItems);

            return HttpStatusCode.OK;
        }

        public void EmptyCart(ISession session)
        {
            SessionExtension.Set(session, SessionConstants.Cart, new List<CartItem>());
        }

        public int GetCartItemsCount(ISession session)
        {
            var cart = SessionExtension.Get<List<CartItem>>(session, SessionConstants.Cart);

            return cart != null ? cart.Count : 0;
        }

        private int GetProductListId(ISession session, int id, string size, int? colorId)
        {
            var cart = SessionExtension.Get<List<CartItem>>(session, SessionConstants.Cart);

            for (int i = 0; i < cart.Count; i++)
            {
                CartItem item = cart[i];
                if (item.Product.Id.Equals(id) && item.Size == size && item.Color?.Id == colorId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

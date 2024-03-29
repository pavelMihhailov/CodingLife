﻿namespace Merchain.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Merchain.Common;
    using Merchain.Common.Extensions;
    using Merchain.Data.Models;
    using Merchain.Services.Data.Interfaces;
    using Merchain.Services.Econt.Models.Response;
    using Merchain.Services.Interfaces;
    using Merchain.Services.Mapping;
    using Merchain.Web.ViewModels.Econt;
    using Merchain.Web.ViewModels.Order;
    using Merchain.Web.ViewModels.ShoppingCart;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class OrderController : Controller
    {
        private readonly IOrderService orderService;
        private readonly ICartService cartService;
        private readonly IPromoCodesService promoCodesService;
        private readonly IEcontService econtService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<OrderController> logger;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IPromoCodesService promoCodesService,
            IEcontService econtService,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderController> logger)
        {
            this.orderService = orderService;
            this.cartService = cartService;
            this.promoCodesService = promoCodesService;
            this.econtService = econtService;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var viewModel = await this.orderService.OrdersOfUser(this.User.Identity.Name);

            return this.View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> StatusCheck(string id)
        {
            IEnumerable<OrderInfoViewModel> allOrders = await this.orderService.AllOrders();

            if (!Guid.TryParse(id, out Guid wantedGuid))
            {
                return this.RedirectToAction("Index", "Products");
            }

            OrderInfoViewModel orderViewModel = allOrders.FirstOrDefault(x => x.Guid == wantedGuid);

            if (orderViewModel == null || id.Equals("00000000-0000-0000-0000-000000000000"))
            {
                return this.RedirectToAction("Index", "Products");
            }

            return this.View(orderViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id < 1 || this.User == null)
            {
                return this.RedirectToAction("Index", "Products");
            }

            IEnumerable<OrderInfoViewModel> allOrders = await this.orderService.AllOrders();
            OrderInfoViewModel orderViewModel = allOrders.FirstOrDefault(x => x.OrderId == id);

            ApplicationUser user = await this.userManager.GetUserAsync(this.User);
            bool isUserAdmin = await this.userManager.IsInRoleAsync(user, GlobalConstants.AdministratorRoleName);

            if (orderViewModel.UserId != user.Id && !isUserAdmin)
            {
                return this.RedirectToAction("Index", "Products");
            }

            return this.View(orderViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string promoCode)
        {
            var viewModel = new OrderViewModel();

            if (!string.IsNullOrWhiteSpace(promoCode) && !this.ApplyPromoCodeValid(promoCode, viewModel))
            {
                this.TempData[ViewDataConstants.ErrorMessage] = "Въведеният от вас промо код не е валиден.";

                return this.RedirectToAction("Index", "ShoppingCart");
            }

            var cartItems = SessionExtension.Get<List<CartItem>>(this.HttpContext.Session, SessionConstants.Cart);

            if (cartItems == null || !cartItems.Any())
            {
                this.TempData[ViewDataConstants.ErrorMessage] = "Нямате добавени продукти в количката.";

                return this.RedirectToAction("Index", "ShoppingCart");
            }

            ApplicationUser user = null;
            if (this.User.Identity.IsAuthenticated)
            {
                user = await this.userManager.FindByNameAsync(this.User.Identity.Name);
            }

            viewModel.CartItems = cartItems;
            viewModel.Total = cartItems.Sum(x => x.Product.Price * x.Quantity);
            viewModel.UserHasAddressByDefault = this.UserHasDefaultAddress(user);

            await this.AddEcontOffices(viewModel);

            return this.View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MakeOrder(MakeOrderInputModel inputModel)
        {
            var addressModel = inputModel.OrderAddress;

            bool validAddress = true;

            if (!addressModel.ShipToOffice)
            {
                await this.econtService.ValidateAddress(
                       addressModel.Country, addressModel.Address, addressModel.Address2);
            }

            if (!this.ModelState.IsValid || !validAddress)
            {
                this.TempData[ViewDataConstants.ErrorMessage] = "Попълнените данни не бяха правилни. Моля опитайте отново.";

                return this.RedirectToAction("Index", "ShoppingCart");
            }

            try
            {
                string username = this.User.Identity.Name;
                bool success = await this.orderService.PlaceOrder(inputModel.CartItems, username ?? string.Empty, addressModel);

                if (success)
                {
                    if (inputModel.PromoCodeId != null && username != null)
                    {
                        await this.promoCodesService.MarkAsUsed((int)inputModel.PromoCodeId);
                    }

                    this.cartService.EmptyCart(this.HttpContext.Session);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                this.TempData[ViewDataConstants.FromOrderPage] = true;

                return this.RedirectToAction("ThankYou");
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Could not complete the order!!!\n{ex.Message}");
                this.TempData[ViewDataConstants.ErrorMessage] = "Възникна проблем при обработването на вашата поръчка. Моля опитайте отново.";

                return this.RedirectToAction("Index");
            }
        }

        public IActionResult ThankYou()
        {
            object fromOrderPage = this.TempData[ViewDataConstants.FromOrderPage];

            if (string.IsNullOrWhiteSpace(fromOrderPage?.ToString()))
            {
                return this.RedirectToAction("Index", "Home");
            }

            return this.View();
        }

        private bool ApplyPromoCodeValid(string promoCode, OrderViewModel viewModel)
        {
            var promoCodeFromDb = this.promoCodesService.GetByCodeAsync(promoCode);
            if (promoCodeFromDb != null)
            {
                decimal discount = Math.Round(viewModel.Total * promoCodeFromDb.PercentageDiscount / 100, 2);
                viewModel.Discount = discount;
                viewModel.Total -= discount;
                viewModel.AppliedPromoCode = promoCodeFromDb;

                return true;
            }

            return false;
        }

        private bool UserHasDefaultAddress(ApplicationUser user)
        {
            if (user != null &&
                !string.IsNullOrWhiteSpace(user.Address) &&
                !string.IsNullOrWhiteSpace(user.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(user.Country))
            {
                return true;
            }

            return false;
        }

        private async Task AddEcontOffices(OrderViewModel viewModel)
        {
            IQueryable<Office> econtOffices = await this.econtService.GetOffices();

            var officesInBG = econtOffices
                .Where(x => x.CountryCode.Equals("BGR"))
                .To<OfficeViewModel>();

            foreach (var office in officesInBG)
            {
                if (!viewModel.Cities.Any(x => x.Id.Equals(office.CityId)))
                {
                    viewModel.Cities.Add(new CityViewModel { Id = office.CityId, Name = office.CityName });
                }
            }

            viewModel.Offices = officesInBG;
        }
    }
}

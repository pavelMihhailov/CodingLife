﻿namespace Merchain.Web.Areas.Administration.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Merchain.Data.Models;
    using Merchain.Services.Data.Interfaces;
    using Merchain.Web.ViewModels.Administration.Categories;
    using Merchain.Web.ViewModels.Administration.Products;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Logging;

    public class ProductsController : AdministrationController
    {
        private readonly IProductsService productsService;
        private readonly ICategoriesService categoriesService;
        private readonly ILogger<ProductsController> logger;

        public ProductsController(
            IProductsService productsService,
            ICategoriesService categoriesService,
            ILogger<ProductsController> logger)
        {
            this.productsService = productsService;
            this.categoriesService = categoriesService;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await this.productsService.GetAllAsync();

            return this.View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var product = await this.productsService.GetByIdAsync((int)id);

            if (product == null)
            {
                return this.NotFound();
            }

            return this.View(product);
        }

        public IActionResult Create()
        {
            var viewModel = new CreateViewModel()
            {
                Categories = this.categoriesService
                .GetAll<CategoriesViewModel>()
                .Select(x => new SelectListItem { Text = x.Title, Value = x.Id.ToString() }),
            };

            return this.View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInputModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.productsService.CreateProduct(
                        viewModel.Name,
                        viewModel.Description,
                        viewModel.Price,
                        viewModel.Image,
                        viewModel.Categories);

                    this.ViewBag.Message = "Succesfully created product.";
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Could not add product to the database.\n-{ex.Message}");
                }
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var product = await this.productsService.GetByIdAsync((int)id);

            if (product == null)
            {
                return this.NotFound();
            }

            return this.View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageUrl,Price")] Product product)
        {
            if (id != product.Id)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                await this.productsService.Edit(product);

                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var product = await this.productsService.GetByIdAsync((int)id);

            if (product == null)
            {
                return this.NotFound();
            }

            return this.View(product);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await this.productsService.GetByIdAsync(id);

            try
            {
                await this.productsService.Delete(product);

                this.ViewBag.Message = "Succesfully Deleted Product.";
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Could not delete product.\n-{ex.Message}");

                this.ViewBag.ErrorMessage = "Deleting Product Failed.";
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
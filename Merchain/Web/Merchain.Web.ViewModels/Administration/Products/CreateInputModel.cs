﻿namespace Merchain.Web.ViewModels.Administration.Products
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Http;

    public class CreateInputModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<IFormFile> Images { get; set; }

        public decimal Price { get; set; }

        public IEnumerable<int> Categories { get; set; }

        public IEnumerable<int> Colors { get; set; }
    }
}

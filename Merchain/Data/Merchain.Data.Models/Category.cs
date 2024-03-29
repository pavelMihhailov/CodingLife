﻿namespace Merchain.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Merchain.Data.Common.Models;

    public class Category : BaseDeletableModel<int>
    {
        public Category()
        {
            this.ProductsCategories = new HashSet<ProductCategory>();
        }

        [Required]
        [Display(Name = "Име")]
        public string Title { get; set; }

        [Display(Name = "Снимка за банер")]
        public string BannerImage { get; set; }

        public virtual ICollection<ProductCategory> ProductsCategories { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RookieShop.Data.Entities
{
    public class Product
    {
        public int Id { set; get; }
        public decimal Price { set; get; }
        public decimal OriginalPrice { set; get; }
        public int Stock { set; get; }
        public int ViewCount { set; get; }
        public DateTime DateCreated { set; get; }
        public string Name { set; get; }
        public string Detail { get; set; }
        public string Description { set; get; }

        public bool? IsFeatured { get; set; }

        public List<ProductInCategory> ProductInCategories { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }

        public List<Cart> Carts { get; set; }
        public List<ProductImage> ProductImages { get; set; }
    }
}

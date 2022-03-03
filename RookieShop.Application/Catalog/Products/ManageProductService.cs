﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RookieShop.Application.Common;
using RookieShop.Data.EF;
using RookieShop.Data.Entities;
using RookieShop.Utilities.Exceptions;
using RookieShop.ViewModels.Catalog.Products;
using RookieShop.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RookieShop.Application.Catalog.Products
{
    public class ManageProductService : IManageProductService
    {
        private readonly EShopDbContext _context;
        private readonly IStorageService _storageService;
        public ManageProductService(EShopDbContext context, IStorageService storageService) 
        {
            _context = context;
            _storageService = storageService;
        }

        public Task<int> AddImages(int productId, List<IFormFile> files)
        {
            throw new NotImplementedException();
        }

        public async Task AddViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            product.ViewCount += 1;
            await _context.SaveChangesAsync();
        }

        public async Task<int> Create(ProductCreateRequest request)
        {
            var product = new Product()
            {
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Stock = request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.Now,
                Name = request.Name,
                Detail = request.Detail,
                Description = request.Description
            };
            //Save image
            if(request.Image != null)
            {
                product.ProductImages = new List<ProductImage>()
                {
                    new ProductImage()
                    {
                        Caption = request.Name,
                        DateCreated = DateTime.Now,
                        FileSize = request.Image.Length,
                        ImagePath = await this.SaveFile(request.Image),
                        IsDefault = true,
                        SortOrder = 1
                    }
                };
            }
            _context.Products.Add(product);
            return await _context.SaveChangesAsync();

        }

        public async Task<int> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) 
                throw new EShopException($"Cannot find product: {productId}");

            var images = _context.ProductImages.Where(i => i.ProductId == productId);
            foreach(var image in images)
            {
                await _storageService.DeleteFileAsync(image.ImagePath);
            }
            _context.Products.Remove(product);
            return await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ProductViewModel>> GetAllPaging(GetManageProductPagedRequest request)
        {
            //1. select join
            var query = from p in _context.Products
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                        join c in _context.Categories on pic.CategoryId equals c.Id
                        select new {p, pic};
            //2. filter
            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(x => x.p.Name.Contains(request.Keyword));

            if(request.CategoryIds.Count > 0) //product search
                query = query.Where(p => request.CategoryIds.Contains(p.pic.CategoryId));
            //3. Paging
            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductViewModel()
                {
                    Id = x.p.Id,
                    Name = x.p.Name,
                    Price = x.p.Price,
                    OriginalPrice = x.p.OriginalPrice,
                    Stock = x.p.Stock,
                    ViewCount = x.p.ViewCount,
                    DateCreated = x.p.DateCreated,
                    Detail = x.p.Detail,
                    Description = x.p.Description,
                }).ToListAsync();
            //4. select and project
            var pagedResult = new PagedResult<ProductViewModel>()
            {
                TotalRecord = totalRow,
                Items = data,
            };
            return pagedResult;
        }

        public Task<List<ProductImageViewModel>> GetListImage(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveImages(int imageId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            if (product == null)
                throw new EShopException($"Cannot find product ID: {request.Id}");

            product.Name = request.Name;
            product.Detail = request.Detail;
            product.Description = request.Description;
            //Save image
            if (request.Image != null)
            {
                var image = await _context.ProductImages.FirstOrDefaultAsync(i => i.IsDefault == true && i.ProductId == request.Id);
                if(image != null)
                {

                    image.FileSize = request.Image.Length;
                    image.ImagePath = await this.SaveFile(request.Image);
                    _context.ProductImages.Update(image);
           
                }
            }
            return await _context.SaveChangesAsync();
        }

        public Task<int> UpdateImages(int imageId, string Caption, bool isDefault)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new EShopException($"Cannot find product ID: {productId}");

            product.Price = newPrice;
            return await _context.SaveChangesAsync() > 0; 
        }

        public async Task<bool> UpdateStock(int productId, int addQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new EShopException($"Cannot find product ID: {productId}");

            product.Stock += addQuantity;
            return await _context.SaveChangesAsync() > 0;
        }

        Task<List<ProductImageViewModel>> IManageProductService.GetListImage(int productId)
        {
            throw new NotImplementedException();
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
    }
}

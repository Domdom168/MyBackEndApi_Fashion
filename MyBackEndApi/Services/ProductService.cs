using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Product;
using MyBackEndApi.Helpers;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<ProductListDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).FirstOrDefault() ?? p.ImageUrl,
                    NameEnglish = p.NameEnglish,
                    NameKhmer= p.NameKhmer,
                    CategoryEnglish =p.Category.NameEnglish,
                    CategoryKhmer=p.Category.NameKhmer,
                    Price = p.Price,
                    Stock = p.Stock,
                    Rating = p.Rating,
                    Service = p.Stock > 0 ? "Available" : "Out of Stock"
                })
                .ToListAsync();
        }

        public async Task<ProductListDto?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.Id == id)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).FirstOrDefault() ?? p.ImageUrl,
                    NameEnglish = p.NameEnglish ,
                    NameKhmer= p.NameKhmer,
                    CategoryEnglish = p.Category.NameEnglish ,
                    CategoryKhmer=p.Category.NameKhmer,
                    Price = p.Price,
                    Stock = p.Stock,
                    Rating = p.Rating,
                    Service = p.Stock > 0 ? "Available" : "Out of Stock"
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductListDto> CreateProductAsync(CreateProductDto productDto)
        {
            // Validate category exists
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid category ID");

            // Create product
            var product = new Product
            {
                NameEnglish = productDto.NameEnglish,
                NameKhmer = productDto.NameKhmer, // adjust as needed
                DescriptionKhmer = productDto.DescriptionKhmer,
                DescriptionEnglish = productDto.DescriptionEnglish,
                category_id = productDto.CategoryId,
                Price = productDto.Price,
                Stock = productDto.Stock,
                Rating = productDto.Rating ?? 0,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // get product Id

            // Handle main image
            if (productDto.ImageFile != null)
            {
                var imageUrl = await FileHelper.SaveImageAsync(productDto.ImageFile, product.Id, _env);
                _context.ProductImages.Add(new ProductImage
                {
                    product_id = product.Id,
                    ImageUrl = imageUrl,
                    DisplayOrder = 0
                });
            }

            // Handle additional images
            if (productDto.AdditionalImages != null)
            {
                int order = 1;
                foreach (var img in productDto.AdditionalImages)
                {
                    var imageUrl = await FileHelper.SaveImageAsync(img, product.Id, _env);
                    _context.ProductImages.Add(new ProductImage
                    {
                        product_id = product.Id,
                        ImageUrl = imageUrl,
                        DisplayOrder = order++
                    });
                }
            }

            // Handle colors
            if (productDto.Colors != null)
            {
                foreach (var color in productDto.Colors)
                {
                    _context.ProductColors.Add(new ProductColor
                    {
                        product_id = product.Id,
                        ColorName = color,
                        ColorHex = null // or generate
                    });
                }
            }

            // Handle sizes
            if (productDto.Sizes != null)
            {
                foreach (var size in productDto.Sizes)
                {
                    _context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = product.Id,
                        Size = size
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id) ?? throw new Exception("Failed to retrieve created product");
        }

        public async Task<ProductListDto?> UpdateProductAsync(int id, CreateProductDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load product with related data
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductColors)
                    .Include(p => p.ProductSizes)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (product == null) return null;

                // Validate category
                var category = await _context.Categories.FindAsync(productDto.CategoryId);
                if (category == null) throw new ArgumentException("Invalid category ID");

                // Update scalar properties
                product.NameEnglish = productDto.NameEnglish;
                product.NameKhmer = productDto.NameKhmer;
                product.DescriptionKhmer = productDto.DescriptionKhmer;
                product.DescriptionEnglish = productDto.DescriptionEnglish;
                product.category_id = productDto.CategoryId;
                product.Price = productDto.Price;
                product.Stock = productDto.Stock;
                product.Rating = productDto.Rating ?? 0;
                product.updated_at = DateTime.UtcNow;

                // Delete existing images from disk and database
                //foreach (var img in product.ProductImages)
                //{
                //    FileHelper.DeleteImage(img.ImageUrl, _env);
                //}
                //_context.ProductImages.RemoveRange(product.ProductImages);

                //// Handle new images
                //var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                //if (!Directory.Exists(uploadsFolder))
                //    Directory.CreateDirectory(uploadsFolder);
                if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
                {
                    foreach (var img in product.ProductImages)
                        FileHelper.DeleteImage(img.ImageUrl, _env);
                    _context.ProductImages.RemoveRange(product.ProductImages);
                    var imageUrl = await FileHelper.SaveImageAsync(productDto.ImageFile, product.Id, _env);
                    _context.ProductImages.Add(new ProductImage
                    {
                        product_id = product.Id,
                        ImageUrl = imageUrl,
                        DisplayOrder = 0
                    });
                }

                if (productDto.AdditionalImages != null)
                {
                    int order = 1;
                    foreach (var img in productDto.AdditionalImages)
                    {
                        var imageUrl = await FileHelper.SaveImageAsync(img, product.Id, _env);
                        _context.ProductImages.Add(new ProductImage
                        {
                            product_id = product.Id,
                            ImageUrl = imageUrl,
                            DisplayOrder = order++
                        });
                    }
                }

                // Replace colors
                _context.ProductColors.RemoveRange(product.ProductColors);
                if (productDto.Colors != null)
                {
                    foreach (var color in productDto.Colors)
                    {
                        _context.ProductColors.Add(new ProductColor
                        {
                            product_id = product.Id,
                            ColorName = color,
                            ColorHex = null
                        });
                    }
                }

                // Replace sizes
                _context.ProductSizes.RemoveRange(product.ProductSizes);
                if (productDto.Sizes != null)
                {
                    foreach (var size in productDto.Sizes)
                    {
                        _context.ProductSizes.Add(new ProductSize
                        {
                            ProductId = product.Id,
                            Size = size
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetProductByIdAsync(id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            // Delete images from disk
            foreach (var img in product.ProductImages)
            {
                FileHelper.DeleteImage(img.ImageUrl, _env);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

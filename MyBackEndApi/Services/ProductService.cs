using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Product;
using MyBackEndApi.DTOs.ProductVariant;
using MyBackEndApi.Helpers;
using MyBackEndApi.Models;
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

        // ----- HELPERS -----
        private async Task UpdateProductTotalStockAsync(int productId)
        {
            var totalStock = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .SumAsync(v => v.Stock);
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Stock = totalStock;
                await _context.SaveChangesAsync();
            }
        }

        // ========== HELPER: Compute discounted price ==========
        private static decimal? GetDiscountedPrice(Product p)
        {
            if (string.IsNullOrEmpty(p.DiscountType) || !p.DiscountValue.HasValue)
                return null;
            var now = DateTime.UtcNow;
            if (p.DiscountStartDate.HasValue && now < p.DiscountStartDate.Value) return null;
            if (p.DiscountEndDate.HasValue && now > p.DiscountEndDate.Value) return null;
            return p.DiscountType == "percentage"
                ? p.Price - (p.Price * p.DiscountValue.Value / 100)
                : p.Price - p.DiscountValue.Value;
        }


        // ----- GET ALL -----
        public async Task<IEnumerable<ProductListDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).FirstOrDefault() ?? p.ImageUrl,
                    NameKhmer = p.NameKhmer,
                    NameEnglish = p.NameEnglish,
                    CategoryId = p.category_id,
                    CategoryEnglish = p.Category != null ? p.Category.NameEnglish : null,
                    CategoryKhmer = p.Category != null ? p.Category.NameKhmer : null,
                    DescriptionEnglish = p.DescriptionEnglish,
                    DescriptionKhmer = p.DescriptionKhmer,
                    Price = p.Price,
                    DiscountedPrice = GetDiscountedPrice(p),
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    DiscountStartDate = p.DiscountStartDate,
                    DiscountEndDate = p.DiscountEndDate,
                    Stock = p.Stock,
                    Rating = p.Rating,
                    Service = p.Stock > 0 ? "Available" : "Out of Stock"
                })
                .ToListAsync();
        }

        // ----- GET BY ID -----
        public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id && !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductSizes)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync();

            if (product == null) return null;

            return new ProductDetailDto
            {
                Id = product.Id,
                ImageUrl = product.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).FirstOrDefault() ?? product.ImageUrl,
                NameKhmer = product.NameKhmer,
                NameEnglish = product.NameEnglish,
                DescriptionKhmer = product.DescriptionKhmer,
                DescriptionEnglish = product.DescriptionEnglish,
                CategoryId = product.category_id,
                CategoryName = product.Category?.NameEnglish,
                Price = product.Price,
                DiscountedPrice = GetDiscountedPrice(product),
                DiscountType = product.DiscountType,
                DiscountValue = product.DiscountValue,
                DiscountStartDate = product.DiscountStartDate,
                DiscountEndDate = product.DiscountEndDate,
                Stock = product.Stock,
                Rating = product.Rating,
                Images = product.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => new ProductImageDto
                {
                    Id = pi.Id,
                    ImageUrl = pi.ImageUrl,
                    DisplayOrder = pi.DisplayOrder
                }).ToList(),
                Colors = product.ProductColors.Select(c => c.ColorName ?? "").ToList(),
                Sizes = product.ProductSizes.Select(s => s.Size ?? "").ToList(),
                Variants = product.Variants.Select(v => new VariantResponseDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    Stock = v.Stock,
                    Price = v.Price
                }).ToList()
            };
        }

        // ----- CREATE -----
        public async Task<ProductDetailDto> CreateProductAsync(ProductCreateDto dto)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid category ID");

            var product = new Product
            {
                NameKhmer = dto.NameKhmer,
                NameEnglish = dto.NameEnglish,
                DescriptionKhmer = dto.DescriptionKhmer,
                DescriptionEnglish = dto.DescriptionEnglish,
                category_id = dto.CategoryId,
                Price = dto.Price,
                Stock = 0, // will be updated from variants
                Rating = dto.Rating ?? 0,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                DiscountStartDate = dto.DiscountStartDate,
                DiscountEndDate = dto.DiscountEndDate,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                ProductImages = new List<ProductImage>()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Add colors
            if (dto.Colors != null)
            {
                foreach (var color in dto.Colors)
                {
                    _context.ProductColors.Add(new ProductColor
                    {
                        product_id = product.Id,
                        ColorName = color,
                        ColorHex = null
                    });
                }
            }

            // Add sizes
            if (dto.Sizes != null)
            {
                foreach (var size in dto.Sizes)
                {
                    _context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = product.Id,
                        Size = size
                    });
                }
            }

            // Add variants
            if (dto.Variants != null && dto.Variants.Any())
            {
                foreach (var v in dto.Variants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        Size = v.Size,
                        ColorName = v.ColorName,
                        ColorHex = v.ColorHex,
                        Stock = v.Stock,
                        Price = v.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
                await UpdateProductTotalStockAsync(product.Id);
            }

            // Add images
            if (dto.ImageFile != null)
            {
                Console.WriteLine("Saving main image...");
                var imageUrl = await FileHelper.SaveImageAsync(dto.ImageFile, product.Id, _env);
                _context.ProductImages.Add(new ProductImage
                {
                    product_id = product.Id,
                    ImageUrl = imageUrl,
                    DisplayOrder = 0
                });
            }
            // 4. Save additional images
            if (dto.AdditionalImages != null && dto.AdditionalImages.Any())
            {
                Console.WriteLine($"Saving {dto.AdditionalImages.Count} additional images...");
                int order = 1;
                foreach (var img in dto.AdditionalImages)
                {
                    if (img == null || img.Length == 0)
                    {
                        Console.WriteLine("Skipping empty file.");
                        continue;
                    }
                    try
                    {
                        var imageUrl = await FileHelper.SaveImageAsync(img, product.Id, _env);
                        _context.ProductImages.Add(new ProductImage
                        {
                            product_id = product.Id,
                            ImageUrl = imageUrl,
                            DisplayOrder = order++
                        });
                        Console.WriteLine($"Additional image saved: {imageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving additional image: {ex.Message}");
                        throw new Exception($"Failed to save additional image: {ex.Message}", ex);
                    }
                }
            }


            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id) ?? throw new Exception("Failed to retrieve created product");
        }

        // ----- UPDATE -----
        public async Task<ProductDetailDto?> UpdateProductAsync(int id,ProductCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductColors)
                    .Include(p => p.ProductSizes)
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
                if (product == null) return null;

                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null)
                    throw new ArgumentException("Invalid category ID");

                // Update scalars
                product.NameKhmer = dto.NameKhmer;
                product.NameEnglish = dto.NameEnglish;
                product.DescriptionKhmer = dto.DescriptionKhmer;
                product.DescriptionEnglish = dto.DescriptionEnglish;
                product.category_id = dto.CategoryId;
                product.Price = dto.Price;
                product.Rating = dto.Rating ?? 0;
                product.DiscountType = dto.DiscountType;
                product.DiscountValue = dto.DiscountValue;
                product.DiscountStartDate = dto.DiscountStartDate;
                product.DiscountEndDate = dto.DiscountEndDate;
                product.updated_at = DateTime.UtcNow;

                // Replace colors
                _context.ProductColors.RemoveRange(product.ProductColors);
                if (dto.Colors != null)
                {
                    foreach (var color in dto.Colors)
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
                if (dto.Sizes != null)
                {
                    foreach (var size in dto.Sizes)
                    {
                        _context.ProductSizes.Add(new ProductSize
                        {
                            ProductId = product.Id,
                            Size = size
                        });
                    }
                }

                // Replace variants
                _context.ProductVariants.RemoveRange(product.Variants);
                if (dto.Variants != null && dto.Variants.Any())
                {
                    foreach (var v in dto.Variants)
                    {
                        _context.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            Size = v.Size,
                            ColorName = v.ColorName,
                            ColorHex = v.ColorHex,
                            Stock = v.Stock,
                            Price = v.Price,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await UpdateProductTotalStockAsync(product.Id);

                // Replace images
                if (dto.ImageFile != null)
                {
                    foreach (var img in product.ProductImages)
                        FileHelper.DeleteImage(img.ImageUrl, _env);
                    _context.ProductImages.RemoveRange(product.ProductImages);
                    var imageUrl = await FileHelper.SaveImageAsync(dto.ImageFile, product.Id, _env);
                    _context.ProductImages.Add(new ProductImage
                    {
                        product_id = product.Id,
                        ImageUrl = imageUrl,
                        DisplayOrder = 0
                    });
                }
                if (dto.AdditionalImages != null)
                {
                    int order = 1;
                    foreach (var img in dto.AdditionalImages)
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

        // ----- SOFT DELETE -----
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;
            product.IsDeleted = true;
            product.updated_at = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;
            product.IsDeleted = false;
            product.updated_at = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            var image = await _context.ProductImages
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == imageId);
            if (image == null) return false;

            var product = image.Product;
            var imageUrl = image.ImageUrl;

            // Remove from DB
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // Delete physical file
            if (!string.IsNullOrEmpty(imageUrl))
                FileHelper.DeleteImage(imageUrl, _env);

            // Optionally, if this was the first image, update product.ImageUrl to the new first image
            if (product != null)
            {
                var firstImage = await _context.ProductImages
                    .Where(pi => pi.product_id == product.Id)
                    .OrderBy(pi => pi.DisplayOrder)
                    .FirstOrDefaultAsync();
                product.ImageUrl = firstImage?.ImageUrl; // update fallback
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
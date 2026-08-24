using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Category;
using MyBackEndApi.Helpers;
using MyBackEndApi.Models;
namespace MyBackEndApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    NameKhmer = c.NameKhmer,
                    NameEnglish = c.NameEnglish,
                    Icon = c.Icon,
                    Color = c.Color,
                    CreatedAt = c.created_at
                })
                .ToListAsync();
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryResponseDto
            {
                Id = category.Id,
                NameKhmer = category.NameKhmer,
                NameEnglish = category.NameEnglish,
                Icon = category.Icon,
                Color = category.Color,
                CreatedAt = category.created_at
            };
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto dto)
        {
            var category = new Category
            {
                NameKhmer = dto.NameKhmer,
                NameEnglish = dto.NameEnglish,
                Icon = dto.Icon,
                Color = dto.Color,
                created_at = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                NameKhmer = category.NameKhmer,
                NameEnglish = category.NameEnglish,
                Icon = category.Icon,
                Color = category.Color,
                CreatedAt = category.created_at
            };
        }

        public async Task<CategoryResponseDto?> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.NameKhmer = dto.NameKhmer;
            category.NameEnglish = dto.NameEnglish;
            category.Icon = dto.Icon;
            category.Color = dto.Color;

            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                NameKhmer = category.NameKhmer,
                NameEnglish = category.NameEnglish,
                Icon = category.Icon,
                Color = category.Color,
                CreatedAt = category.created_at
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
          .Include(c => c.Products)
          .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;

            // Check if category has any products
            if (category.Products.Any())
            {
                throw new InvalidOperationException(
                    "Cannot delete category because it has associated products. " +
                    "Please reassign or delete the products first."
                );
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}

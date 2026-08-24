using MyBackEndApi.DTOs.Product;

namespace MyBackEndApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListDto>> GetAllProductsAsync();
        Task<ProductDetailDto?> GetProductByIdAsync(int id);
        Task<ProductDetailDto> CreateProductAsync(ProductCreateDto dto);
        Task<ProductDetailDto?> UpdateProductAsync(int id, ProductCreateDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> RestoreProductAsync(int id);
        Task<bool> DeleteProductImageAsync(int imageId);
    }
}

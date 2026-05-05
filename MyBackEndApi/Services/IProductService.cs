using MyBackEndApi.DTOs.Product;

namespace MyBackEndApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListDto>> GetAllProductsAsync();
        Task<ProductListDto?> GetProductByIdAsync(int id);
        Task<ProductListDto> CreateProductAsync(CreateProductDto productDto);
        Task<ProductListDto?> UpdateProductAsync(int id, CreateProductDto productDto);
        Task<bool> DeleteProductAsync(int id);
    }
}

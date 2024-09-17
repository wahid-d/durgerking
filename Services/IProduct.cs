using DurgerKing.Entity;

namespace DurgerKing.Services;

public interface IProductService
{
    Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<List<Product>> GetProductsAsync(int categoryId, CancellationToken cancellationToken = default);
}


using DurgerKing.Data;
using DurgerKing.Entity;
using Microsoft.EntityFrameworkCore;

namespace DurgerKing.Services;

public class ProductService : IProductService
{
    private readonly ILogger<ProductService> logger;
    private readonly IAppDbContext dbContext;

    public ProductService(
        ILogger<ProductService> logger,
        IAppDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
    }

    public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => dbContext.Categories.ToListAsync(cancellationToken);

    public Task<List<Product>> GetProductsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return dbContext.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
    }
}
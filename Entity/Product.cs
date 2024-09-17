namespace DurgerKing.Entity;

public class Product
{
    internal int categoryId;

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public float DiscountPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public virtual List<Product> Items { get; set; }
    public virtual int CategoryId { get; set; }
    public virtual Category Category { get; set; } 
    public virtual List<ProductMedium> Media { get; set; } = new List<ProductMedium>();
}
using WebApi.Models;

namespace WebApi.Services;

public class ProductService : IProductService
{
    private readonly List<Product> products = new()
    {
        new Product { Id = 1, Name = "Яблоко", Price = 450000 },
        new Product { Id = 2, Name = "Золотой банан", Price = 250000 },
        new Product { Id = 3, Name = "Паприка", Price = 50000 }
    };

    public IEnumerable<Product> GetAll()
    {
        return products;
    }

    public Product? GetById(int id)
    {
        return products.FirstOrDefault(p => p.Id == id);
    }

    public Product Add(Product product)
    {
        product.Id = products.Count > 0
            ? products.Max(p => p.Id) + 1
            : 1;

        products.Add(product);

        return product;
    }

    public bool Delete(int id)
    {
        var product = products.FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return false;
        }

        products.Remove(product);

        return true;
    }
}
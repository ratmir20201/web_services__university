using WebApi.Models;

namespace WebApi.Services;

public interface IProductService
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);
    Product Add(Product product);
    bool Delete(int id);
}
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        _logger.LogInformation("Получение всех продуктов");

        return Ok(_productService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            _logger.LogWarning(
                "Продукт с ID {ProductId} не найден",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Продукт с ID {ProductId} найден",
            id);

        return Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Add(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || product.Price < 0)
        {
            return BadRequest();
        }

        var newProduct = _productService.Add(product);

        _logger.LogInformation(
            "Продукт {ProductName} был создан",
            product.Name);

        return CreatedAtAction(
            nameof(GetById),
            new { id = newProduct.Id },
            newProduct);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var deleted = _productService.Delete(id);

        if (!deleted)
        {
            _logger.LogWarning(
                "Продукт с ID {ProductId} не был найден",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Продукт с ID {ProductId} был удален",
            id);

        return Ok();
    }
}
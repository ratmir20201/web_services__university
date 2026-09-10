using Microsoft.AspNetCore.Mvc;
using WebApiPractice1.Models;

namespace WebApiPractice1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private static readonly List<Book> books = new()
    {
        new Book
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert Martin",
            Year = 2008,
            Genre = "Programming",
            Price = 25.99m
        },

        new Book
        {
            Id = 2,
            Title = "1984",
            Author = "George Orwell",
            Year = 1949,
            Genre = "Dystopian",
            Price = 15.99m
        },

        new Book
        {
            Id = 3,
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            Year = 1937,
            Genre = "Fantasy",
            Price = 20.50m
        }
    };

    [HttpGet]
    public ActionResult<List<Book>> GetBooks()
    {
        return Ok(books);
    }

    [HttpGet("{id}")]
    public ActionResult<Book> GetBook(int id)
    {
        var book = books.FirstOrDefault(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpPost]
    public ActionResult<Book> AddBook(Book book)
    {
        book.Id = books.Count + 1;

        books.Add(book);

        return Ok(book);
    }
}
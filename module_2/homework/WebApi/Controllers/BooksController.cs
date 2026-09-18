using Microsoft.AspNetCore.Mvc;
using WebApiPractice1.Models;

namespace WebApiPractice1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase {
    private static readonly List<Book> books = new() {
        new Book {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert Martin",
            Year = 2008
        },

        new Book {
            Id = 2,
        Title = "1984",
            Author = "George Orwell",
            Year = 1949
        }
    };

    [HttpGet]
    public ActionResult<List<Book>> GetBooks() {
        return Ok(books);
    }

    [HttpPost]
    public ActionResult<Book> AddBook(Book book) {
        book.Id = books.Count + 1;
        books.Add(book);
        return Ok(book);
    }
}
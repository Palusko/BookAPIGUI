using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Services
{
    public interface IBookRepositoryGUI
    {
        IEnumerable<BookDto> GetBooks();
        BookDto GetBookById(int bookId);
        BookDto GetBookByIsbn(string bookIsbn);
        decimal GetBookRating(int bookId);
    }
}

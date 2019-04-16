using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Services
{
    public interface IAuthorRepositoryGUI
    {
        IEnumerable<AuthorDto> GetAuthors();
        AuthorDto GetAuthorById(int authorId);
        IEnumerable<BookDto> GetBooksByAuthor(int authorId);
        IEnumerable<AuthorDto> GetAuthorsOfABook(int bookId);
    }
}

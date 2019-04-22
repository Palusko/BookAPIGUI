using BookApiProject.Dtos;
using BookGUI.Services;
using BookGUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Controllers
{
    public class AuthorsController : Controller
    {
        IAuthorRepositoryGUI _authorRepository;
        ICountryRepositoryGUI _countryRepository;
        ICategoryRepositoryGUI _categoryRepository;

        public AuthorsController(IAuthorRepositoryGUI authorRepository, ICountryRepositoryGUI countryRepository,
            ICategoryRepositoryGUI categoryRepository)
        {
            _authorRepository = authorRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            var authors = _authorRepository.GetAuthors();

            if (authors.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving authors from the database or no author exists";
            }

            return View(authors);
        }

        public IActionResult GetAuthorById(int authorId)
        {
            var author = _authorRepository.GetAuthorById(authorId);
            if (author == null)
            {
                ModelState.AddModelError("", "Some kind of error getting author");
                ViewBag.Message = $"There was a problem retrieving author from the " +
                                    $"database or no author with id {authorId}  exists";
                author = new AuthorDto();
            }

            var country = _countryRepository.GetCountryOfAnAuthor(authorId);
            if (country == null)
            {
                ModelState.AddModelError("", "Some kind of error getting country");
                ViewBag.Message += $"There was a problem retrieving country from the " +
                                    $"database or no country for author with id {authorId} exists";
                country = new CountryDto();
            }

            var bookCategories = new Dictionary<BookDto, IEnumerable<CategoryDto>>();
            var books = _authorRepository.GetBooksByAuthor(authorId);
            if (books.Count() <= 0)
            {
                ViewBag.BookMessage = $"No books for {author.FirstName} {author.LastName} exists.";
            }

            foreach(var book in books)
            {
                var categories = _categoryRepository.GetAllCategoriesOfABook(book.Id);
                bookCategories.Add(book, categories);
            }

            var authorCountryBooksCategoriesViewModel = new AuthorCountryBooksCategoriesViewModel
            {
                Author = author,
                Country = country,
                BookCategories = bookCategories
            };

            return View(authorCountryBooksCategoriesViewModel);
        }
    }
}

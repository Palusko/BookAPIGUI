using BookApiProject.Dtos;
using BookApiProject.Models;
using BookGUI.Components;
using BookGUI.Services;
using BookGUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookGUI.Controllers
{
    public class HomeController : Controller
    {
        IBookRepositoryGUI _bookRepository;
        IAuthorRepositoryGUI _authorRepository;
        ICountryRepositoryGUI _countryRepository;
        ICategoryRepositoryGUI _categoryRepository;
        IReviewRepositoryGUI _reviewRepository;
        IReviewerRepositoryGUI _reviewerRepository;

        public HomeController(IBookRepositoryGUI bookRepository, IAuthorRepositoryGUI authorRepository,
                              ICountryRepositoryGUI countryRepository, ICategoryRepositoryGUI categoryRepository,
                              IReviewRepositoryGUI reviewRepository, IReviewerRepositoryGUI reviewerRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _reviewerRepository = reviewerRepository;
        }

        public IActionResult Index()
        {
            var books = _bookRepository.GetBooks();

            if(books.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving books from the database or no book exists";
            }

            var bookAuthorsCategoriesRatingViewModel = new List<BookAuthorsCategoriesRatingViewModel>();

            foreach(var book in books)
            {
                var authors = _authorRepository.GetAuthorsOfABook(book.Id).ToList();
                if (authors.Count() <= 0)
                    ModelState.AddModelError("", "Some kind of error getting authors");

                var categories = _categoryRepository.GetAllCategoriesOfABook(book.Id).ToList();
                if (categories.Count() <= 0)
                    ModelState.AddModelError("", "Some kind of error getting categories");

                var rating = _bookRepository.GetBookRating(book.Id);

                bookAuthorsCategoriesRatingViewModel.Add(new BookAuthorsCategoriesRatingViewModel
                {
                    Book = book,
                    Authors = authors,
                    Categories = categories,
                    Rating = rating
                });
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(bookAuthorsCategoriesRatingViewModel);
        }

        public IActionResult GetBookById(int bookId)
        {
            var completeBookViewModel = new CompleteBookViewModel
            {
                AuthorsCountries = new Dictionary<AuthorDto, CountryDto>(),
                ReviewsReviewers = new Dictionary<ReviewDto, ReviewerDto>()
            };

            var book = _bookRepository.GetBookById(bookId);

            if (book == null)
            {
                ModelState.AddModelError("", "Some kind of error getting book");
                book = new BookDto();
            }

            var categories = _categoryRepository.GetAllCategoriesOfABook(bookId);
            if (categories.Count() <= 0)
            {
                ModelState.AddModelError("", "Some kind of error getting categories");
            }

            var rating = _bookRepository.GetBookRating(bookId);
            completeBookViewModel.Book = book;
            completeBookViewModel.Categories = categories;
            completeBookViewModel.Rating = rating;

            var authors = _authorRepository.GetAuthorsOfABook(bookId);
            if (authors.Count() <= 0)
            {
                ModelState.AddModelError("", "Some kind of error getting authors");
            }

            foreach (var author in authors)
            {
                var country = _countryRepository.GetCountryOfAnAuthor(author.Id);
                completeBookViewModel.AuthorsCountries.Add(author, country);
            }

            var reviews = _reviewRepository.GetReviewsOfABook(bookId);
            if (reviews.Count() <= 0)
            {
                ViewBag.ReviewsMessage = "There are no reviews yet";
            }

            foreach (var review in reviews)
            {
                var reviewer = _reviewerRepository.GetReviewerOfAReview(review.Id);
                completeBookViewModel.ReviewsReviewers.Add(review, reviewer);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BookMessage = "There was an error retrieving a complete book record";
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(completeBookViewModel);
        }

        [HttpGet]
        public IActionResult CreateBook()
        {
            var authors = _authorRepository.GetAuthors();
            var categories = _categoryRepository.GetCategories();

            if (authors.Count() <= 0 || categories.Count() <= 0)
            {
                ModelState.AddModelError("", "Some kind of error getting authors or categories");
            }

            var authorList = new AuthorsList(authors.ToList());
            var categoryList = new CategoriesList(categories.ToList());

            var createUpdateBook = new CreateUpdateBookViewModel
            {
                AuthorSelectListItems = authorList.GetAuthorsList(),
                CategorySelectListItems = categoryList.GetCategoriesList()
            };

            return View(createUpdateBook);

        }

        [HttpPost]
        public IActionResult CreateBook(IEnumerable<int> AuthorIds, IEnumerable<int> CategoryIds, 
            CreateUpdateBookViewModel bookToCreate)
        {
            using (var client = new HttpClient())
            {
                var book = new Book()
                {
                    Id = bookToCreate.Book.Id,
                    Isbn = bookToCreate.Book.Isbn,
                    Title = bookToCreate.Book.Title,
                    DatePublished = bookToCreate.Book.DatePublished

                    //books?authId=?&authId=?&catId=?
                };

                var uriParameters = GetAuthorsCategoriesUri(AuthorIds.ToList(), CategoryIds.ToList());

                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PostAsJsonAsync($"books?{uriParameters}", book);
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTaskNewBook = result.Content.ReadAsAsync<Book>();
                    readTaskNewBook.Wait();

                    var newBook = readTaskNewBook.Result;

                    TempData["SuccessMessage"] = $"Book {book.Title} was successfully created.";
                    return RedirectToAction("GetBookById", new { bookId = newBook.Id });
                }

                if((int)result.StatusCode == 422)
                {
                    ModelState.AddModelError("", "ISBN already Exists!");
                }
                else
                {
                    ModelState.AddModelError("", "Error! Book not created!");
                }                
            }

            var authorList = new AuthorsList(_authorRepository.GetAuthors().ToList());
            var categoryList = new CategoriesList(_categoryRepository.GetCategories().ToList());
            bookToCreate.AuthorSelectListItems = authorList.GetAuthorsList(AuthorIds.ToList());
            bookToCreate.CategorySelectListItems = categoryList.GetCategoriesList(CategoryIds.ToList());
            bookToCreate.AuthorIds = AuthorIds.ToList();
            bookToCreate.CategoryIds = CategoryIds.ToList();

            return View(bookToCreate);
        }

        [HttpGet]
        public IActionResult UpdateBook(int bookId)
        {
            var bookDto = _bookRepository.GetBookById(bookId);
            var authorList = new AuthorsList(_authorRepository.GetAuthors().ToList());
            var categoryList = new CategoriesList(_categoryRepository.GetCategories().ToList());

            var bookViewModel = new CreateUpdateBookViewModel
            {
                Book = bookDto,
                AuthorSelectListItems = authorList.GetAuthorsList(_authorRepository.GetAuthorsOfABook(bookId)
                                        .Select(a => a.Id).ToList()),
                CategorySelectListItems = categoryList.GetCategoriesList(_categoryRepository.GetAllCategoriesOfABook(bookId)
                                        .Select(c => c.Id).ToList())
            };

            return View(bookViewModel);
        }

        [HttpPost]
        public IActionResult UpdateBook(IEnumerable<int> AuthorIds, IEnumerable<int>CategoryIds, 
            CreateUpdateBookViewModel bookToUpdate)
        {
            using(var client = new HttpClient())
            {
                var book = new Book()
                {
                    Id = bookToUpdate.Book.Id,
                    Isbn = bookToUpdate.Book.Isbn,
                    Title = bookToUpdate.Book.Title,
                    DatePublished = bookToUpdate.Book.DatePublished
                };

                var uriParameters = GetAuthorsCategoriesUri(AuthorIds.ToList(), CategoryIds.ToList());

                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PutAsJsonAsync($"books/{book.Id}?{uriParameters}", book);
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Book {book.Title} was successfully updated.";
                    return RedirectToAction("GetBookById", new { bookId = book.Id });
                }

                if ((int)result.StatusCode == 422)
                {
                    ModelState.AddModelError("", "ISBN already Exists!");
                }
                else
                {
                    ModelState.AddModelError("", "Error! Book not created!");
                }
            }

            var authorList = new AuthorsList(_authorRepository.GetAuthors().ToList());
            var categoryList = new CategoriesList(_categoryRepository.GetCategories().ToList());
            bookToUpdate.AuthorSelectListItems = authorList.GetAuthorsList(AuthorIds.ToList());
            bookToUpdate.CategorySelectListItems = categoryList.GetCategoriesList(CategoryIds.ToList());
            bookToUpdate.AuthorIds = AuthorIds.ToList();
            bookToUpdate.CategoryIds = CategoryIds.ToList();

            return View(bookToUpdate);
        }

        [HttpGet]
        public IActionResult DeleteBook(int bookId)
        {
            var bookDto = _bookRepository.GetBookById(bookId);

            return View(bookDto);
        }

        [HttpPost]
        public IActionResult DeleteBook(int bookId, string bookTitle)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.DeleteAsync($"books/{bookId}");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Book {bookTitle} was successfully deleted.";

                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Some kind of error. Book not deleted!");
            }

            var bookDto = _bookRepository.GetBookById(bookId);
            return View(bookDto);
        }

        private string GetAuthorsCategoriesUri(List<int> authorIds, List<int> categoryIds)
        {
            var uri = "";
            foreach(var authorId in authorIds)
            {
                uri += $"authId={authorId}&";
            }

            foreach (var categoryId in categoryIds)
            {
                uri += $"catId={categoryId}&";
            }

            return uri;
        }
    }
}

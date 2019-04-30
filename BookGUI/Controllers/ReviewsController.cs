using BookApiProject.Dtos;
using BookApiProject.Models;
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
    public class ReviewsController : Controller
    {
        IReviewRepositoryGUI _reviewRepository;
        IReviewerRepositoryGUI _reviewerRepository;
        IBookRepositoryGUI _bookRepository;

        public ReviewsController(IReviewRepositoryGUI reviewRepository, IReviewerRepositoryGUI reviewerRepository,
            IBookRepositoryGUI bookRepository)
        {
            _reviewRepository = reviewRepository;
            _reviewerRepository = reviewerRepository;
            _bookRepository = bookRepository;
        }

        public IActionResult Index()
        {
            var reviews = _reviewRepository.GetReviews();

            if (reviews.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving reviews from the database or no review exists";
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(reviews);
        }

        public IActionResult GetReviewById(int reviewId)
        {
            var review = _reviewRepository.GetReviewById(reviewId);
            if (review == null)
            {
                ModelState.AddModelError("", "Some kind of error getting review");
                ViewBag.Message = $"There was a problem retrieving review from the " +
                                    $"database or no review with id {reviewId}  exists";
                review = new ReviewDto();
            }

            var reviewer = _reviewerRepository.GetReviewerOfAReview(reviewId);
            if (reviewer == null)
            {
                ModelState.AddModelError("", "Some kind of error getting reviewer");
                ViewBag.Message += $"There was a problem retrieving reviewer from the database " +
                                    $"or no reviewer for the review id {reviewId} exist";
                reviewer = new ReviewerDto();
            }

            var book = _reviewRepository.GetBookOfAReview(reviewId);
            if (book == null)
            {
                ModelState.AddModelError("", "Some kind of error getting book");
                ViewBag.Message += $"There was a problem retrieving book from the database " +
                                    $"or no book for the review id {reviewId} exist";
                book = new BookDto();
            }

            var reviewReviewerBookViewModel = new ReviewReviewerBookViewModel
            {
                Review = review,
                Reviewer = reviewer,
                Book = book
            };

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(reviewReviewerBookViewModel);
        }

        [HttpGet]
        public IActionResult CreateReview(int bookId, string bookTitle)
        {
            ViewBag.BookId = bookId;
            ViewBag.BookTitle = bookTitle;
            return View();
        }

        [HttpPost]
        public IActionResult CreateReview(int bookId, int ReviewerId, Review review)
        {
            using (var client = new HttpClient())
            {
                var reviewerDto = _reviewerRepository.GetReviewerById(ReviewerId);
                var bookDto = _bookRepository.GetBookById(bookId);

                if(reviewerDto == null || bookDto == null || review == null)
                {
                    ModelState.AddModelError("", "Invalid book, reviewer, or review. Cannot create review!");
                    ViewBag.BookId = bookId;
                    ViewBag.BookTitle = bookDto == null ? "" : bookDto.Title;
                    return View(review);
                }

                review.Reviewer = new Reviewer
                {
                    Id = reviewerDto.Id,
                    FirstName = reviewerDto.FirstName,
                    LastName = reviewerDto.LastName
                };

                review.Book = new Book
                {
                    Id = bookDto.Id,
                    Isbn = bookDto.Isbn,
                    Title = bookDto.Title
                };

                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PostAsJsonAsync("reviews", review);
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var newReviewTask = result.Content.ReadAsAsync<Review>();
                    newReviewTask.Wait();

                    var newReview = newReviewTask.Result;
                    TempData["SuccessMessage"] = $"Review for {review.Book.Title} was successfully created.";
                    return RedirectToAction("GetReviewById", new { reviewId = newReview.Id });
                }

                ModelState.AddModelError("", "Review not created");
                ViewBag.BookId = bookId;
                ViewBag.BookTitle = bookDto == null ? "" : bookDto.Title;
            }

            return View(review);
        }       

        [HttpGet]
        public IActionResult UpdateReview(int bookId, int reviewId, int ReviewerId)
        {
            var reviewDto = _reviewRepository.GetReviewById(reviewId);
            var reviewerDto = _reviewerRepository.GetReviewerOfAReview(reviewId);
            var bookDto = _bookRepository.GetBookById(bookId);

            Review review = null;

            if (reviewDto == null || reviewerDto == null || bookDto == null)
            {
                ModelState.AddModelError("", "Invalid book, reviewer, or review. Cannot update review!");
                review = new Review();
            }
            else
            {
                review = new Review
                {
                    Id = reviewDto.Id,
                    Headline = reviewDto.Headline,
                    Rating = reviewDto.Rating,
                    ReviewText = reviewDto.ReviewText,
                    Reviewer = new Reviewer
                    {
                        Id = reviewerDto.Id,
                        FirstName = reviewerDto.FirstName,
                        LastName = reviewerDto.LastName
                    },
                    Book = new Book
                    {
                        Id = bookDto.Id,
                        Title = bookDto.Title,
                        Isbn = bookDto.Isbn,
                        DatePublished = bookDto.DatePublished
                    }
                };
            }

            return View(review);
        }

        [HttpPost]
        public IActionResult UpdateReview(int ReviewerId, Review reviewToUpdate)
        {
            var reviewerDto = _reviewerRepository.GetReviewerById(ReviewerId);
            var bookDto = _bookRepository.GetBookById(reviewToUpdate.Book.Id);

            if (reviewToUpdate == null || reviewerDto == null || bookDto == null)
            {
                ModelState.AddModelError("", "Invalid book, reviewer, or review. Cannot update review!");
            }
            else
            {
                reviewToUpdate.Reviewer = new Reviewer
                {
                    Id = reviewerDto.Id,
                    FirstName = reviewerDto.FirstName,
                    LastName = reviewerDto.LastName
                };
                reviewToUpdate.Book = new Book
                {
                    Id = bookDto.Id,
                    Title = bookDto.Title,
                    Isbn = bookDto.Isbn,
                    DatePublished = bookDto.DatePublished
                };

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:60039/api/");
                    var responseTask = client.PutAsJsonAsync($"reviews/{reviewToUpdate.Id}", reviewToUpdate);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Review updated";
                        return RedirectToAction("GetReviewById", new { reviewId = reviewToUpdate.Id });
                    }

                    ModelState.AddModelError("", "Unexpected Error. Review Not Updated");
                }
            }

            return View(reviewToUpdate);
        }

        [HttpGet]
        public IActionResult DeleteReview(int reviewId)
        {
            var reviewDto = _reviewRepository.GetReviewById(reviewId);
            var reviewerDto = _reviewerRepository.GetReviewerOfAReview(reviewId);
            var bookDto = _reviewRepository.GetBookOfAReview(reviewId);

            Review review = null;

            if (reviewDto == null || reviewerDto == null || bookDto == null)
            {
                ModelState.AddModelError("", "Some kind of error getting review, reviewer, or book");
                review = new Review();
            }
            else
            {
                review = new Review
                {
                    Id = reviewDto.Id,
                    Headline = reviewDto.Headline,
                    Rating = reviewDto.Rating,
                    ReviewText = reviewDto.ReviewText,
                    Reviewer = new Reviewer
                    {
                        Id = reviewerDto.Id,
                        FirstName = reviewerDto.FirstName,
                        LastName = reviewerDto.LastName
                    },
                    Book = new Book
                    {
                        Id = bookDto.Id,
                        Title = bookDto.Title,
                        Isbn = bookDto.Isbn,
                        DatePublished = bookDto.DatePublished
                    }
                };
            }

            return View(review);
        }

        [HttpPost, ActionName("DeleteReview")]
        public IActionResult DeleteReviewPost(int reviewId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.DeleteAsync($"reviews/{reviewId}");
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Review was deleted";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Some kind of error. Review not deleted!");
            }

            var reviewDto = _reviewRepository.GetReviewById(reviewId);
            var reviewerDto = _reviewerRepository.GetReviewerOfAReview(reviewId);
            var bookDto = _reviewRepository.GetBookOfAReview(reviewId);

            Review review = null;

            if (reviewDto == null || reviewerDto == null || bookDto == null)
            {
                ModelState.AddModelError("", "Some kind of error getting review, reviewer, or book");
                review = new Review();
            }
            else
            {
                review = new Review
                {
                    Id = reviewDto.Id,
                    Headline = reviewDto.Headline,
                    Rating = reviewDto.Rating,
                    ReviewText = reviewDto.ReviewText,
                    Reviewer = new Reviewer
                    {
                        Id = reviewerDto.Id,
                        FirstName = reviewerDto.FirstName,
                        LastName = reviewerDto.LastName
                    },
                    Book = new Book
                    {
                        Id = bookDto.Id,
                        Title = bookDto.Title,
                        Isbn = bookDto.Isbn,
                        DatePublished = bookDto.DatePublished
                    }
                };
            }

            return View(review);
        }
    }
}

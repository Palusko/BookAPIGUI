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
    public class ReviewsController : Controller
    {
        IReviewRepositoryGUI _reviewRepository;
        IReviewerRepositoryGUI _reviewerRepository;

        public ReviewsController(IReviewRepositoryGUI reviewRepository, IReviewerRepositoryGUI reviewerRepository)
        {
            _reviewRepository = reviewRepository;
            _reviewerRepository = reviewerRepository;
        }

        public IActionResult Index()
        {
            var reviews = _reviewRepository.GetReviews();

            if (reviews.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving reviews from the database or no review exists";
            }

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

            return View(reviewReviewerBookViewModel);
        }
    }
}

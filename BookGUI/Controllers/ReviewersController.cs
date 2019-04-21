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
    public class ReviewersController : Controller
    {
        IReviewerRepositoryGUI _reviewerRepository;
        IReviewRepositoryGUI _reviewRepository;

        public ReviewersController(IReviewerRepositoryGUI reviewerRepository, IReviewRepositoryGUI reviewRepository)
        {
            _reviewerRepository = reviewerRepository;
            _reviewRepository = reviewRepository;
        }

        public IActionResult Index()
        {
            var reviewers = _reviewerRepository.GetReviewers();

            if (reviewers.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving reviewers from the database or no reviewer exists";
            }

            return View(reviewers);
        }

        public IActionResult GetReviewerById(int reviewerId)
        {
            var reviewer = _reviewerRepository.GetReviewerById(reviewerId);
            if (reviewer == null)
            {
                ModelState.AddModelError("", "Some kind of error getting reviewer");
                ViewBag.ReviewerMessage = $"There was a problem retrieving reviewer from the database " +
                                          $"or no reviewer with id {reviewerId} exist";
                reviewer = new ReviewerDto();
            }

            var reviews = _reviewerRepository.GetReviewsByReviewer(reviewerId);
            if (reviews.Count() <= 0)
            {
                ViewBag.ReviewMessage = $"Reviewer {reviewer.FirstName} {reviewer.LastName} has no reviews";
            }

            IDictionary<ReviewDto, BookDto> reviewAndBook = new Dictionary<ReviewDto, BookDto>();
            foreach(var review in reviews)
            {
                var book = _reviewRepository.GetBookOfAReview(review.Id);
                reviewAndBook.Add(review, book);
            }

            var reviewerReviewsBooksViewModel = new ReviewerReviewsBooksViewModel
            {
                Reviewer = reviewer,
                ReviewBook = reviewAndBook
            };

            return View(reviewerReviewsBooksViewModel);
        }
    }
}

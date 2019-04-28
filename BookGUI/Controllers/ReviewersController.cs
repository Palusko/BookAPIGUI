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

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
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

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(reviewerReviewsBooksViewModel);
        }

        [HttpGet]
        public IActionResult CreateReviewer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateReviewer(Reviewer reviewer)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PostAsJsonAsync("reviewers", reviewer);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var newReviewerTask = result.Content.ReadAsAsync<Reviewer>();
                    newReviewerTask.Wait();

                    var newReviewer = newReviewerTask.Result;
                    TempData["SuccessMessage"] = $"Reviewer {newReviewer.FirstName} {newReviewer.LastName} " +
                        $"was successfully created.";

                    return RedirectToAction("GetReviewerById", new { reviewerId = newReviewer.Id });
                }
                
                ModelState.AddModelError("", "Some kind of error. Reviewer not created!");
            }

            return View();
        }

        [HttpGet]
        public IActionResult UpdateReviewer(int reviewerId)
        {
            var reviewerToUpdate = _reviewerRepository.GetReviewerById(reviewerId);
            if (reviewerToUpdate == null)
            {
                ModelState.AddModelError("", "Error getting reviewer");
                reviewerToUpdate = new ReviewerDto();
            }

            return View(reviewerToUpdate);
        }

        [HttpPost]
        public IActionResult UpdateReviewer(Reviewer reviewerToUpdate)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PutAsJsonAsync($"reviewers/{reviewerToUpdate.Id}", reviewerToUpdate);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Reviewer was successfully updated.";

                    return RedirectToAction("GetReviewerById", new { reviewerId = reviewerToUpdate.Id });
                }
                
                ModelState.AddModelError("", "Some kind of error. Reviewer not updated!");
            }

            var reviewerDto = _reviewerRepository.GetReviewerById(reviewerToUpdate.Id);
            return View(reviewerDto);
        }

        [HttpGet]
        public IActionResult DeleteReviewer(int reviewerId)
        {
            var reviewer = _reviewerRepository.GetReviewerById(reviewerId);
            if (reviewer == null)
            {
                ModelState.AddModelError("", "Some kind of error. Reviewer doesn't exist!");
                reviewer = new ReviewerDto();
            }

            return View(reviewer);
        }

        [HttpPost]
        public IActionResult DeleteReviewer(int reviewerId, string firstName, string lastName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.DeleteAsync($"reviewers/{reviewerId}");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Reviewer {firstName} {lastName} was successfully deleted.";

                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Some kind of error. Reviewer not deleted!");
            }

            var reviewerDto = _reviewerRepository.GetReviewerById(reviewerId);
            return View(reviewerDto);
        }
    }
}

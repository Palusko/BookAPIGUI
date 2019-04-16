using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Services
{
    public interface IReviewRepositoryGUI
    {
        IEnumerable<ReviewDto> GetReviews();
        ReviewDto GetReviewById(int reviewId);
        IEnumerable<ReviewDto> GetReviewsOfABook(int bookId);
        BookDto GetBookOfAReview(int reviewId);
    }
}

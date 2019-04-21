using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.ViewModels
{
    public class ReviewerReviewsBooksViewModel
    {
        public ReviewerDto Reviewer { get; set; }
        public IDictionary<ReviewDto,BookDto> ReviewBook { get; set; }
    }
}

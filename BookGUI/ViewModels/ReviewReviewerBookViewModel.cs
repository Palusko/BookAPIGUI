using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.ViewModels
{
    public class ReviewReviewerBookViewModel
    {
        public ReviewDto Review { get; set; }
        public ReviewerDto Reviewer { get; set; }
        public BookDto Book { get; set; }
    }
}

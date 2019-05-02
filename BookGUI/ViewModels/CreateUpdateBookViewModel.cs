using BookApiProject.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.ViewModels
{
    public class CreateUpdateBookViewModel
    {
        public BookDto Book { get; set; }

        public List<int> AuthorIds { get; set; }
        public List<SelectListItem> AuthorSelectListItems { get; set; }

        public List<int> CategoryIds { get; set; }
        public List<SelectListItem> CategorySelectListItems { get; set; }
    }
}

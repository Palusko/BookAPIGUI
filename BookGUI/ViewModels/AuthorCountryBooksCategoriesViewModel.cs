using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.ViewModels
{
    public class AuthorCountryBooksCategoriesViewModel
    {
        public AuthorDto Author { get; set; }
        public CountryDto Country { get; set; }
        public IDictionary<BookDto, IEnumerable<CategoryDto>> BookCategories { get; set; }
    }
}

using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.ViewModels
{
    public class CountryAuthorsViewModel
    {
        public CountryDto Country { get; set; }
        public IEnumerable<AuthorDto> Authors { get; set; }
    }
}

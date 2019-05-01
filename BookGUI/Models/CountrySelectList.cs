using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Models
{
    public class CountrySelectList
    {
        public int CountryId { get; set; }
        public SelectList CountriesList { get; set; }
    }
}

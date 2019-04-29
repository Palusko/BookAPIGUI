using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Models
{
    public class ReviewerSelectList
    {
        public int ReviewerId { get; set; }
        public SelectList ReviewersList { get; set; }
    }
}

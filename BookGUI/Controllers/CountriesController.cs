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
    public class CountriesController : Controller
    {
        ICountryRepositoryGUI _countryRepository;
        IAuthorRepositoryGUI _authorRespository;

        public CountriesController(ICountryRepositoryGUI countryRepository, IAuthorRepositoryGUI authorRespository)
        {
            _countryRepository = countryRepository;
            _authorRespository = authorRespository;
        }

        public IActionResult Index()
        {
            var countries = _countryRepository.GetCountries();
            //var countries = new List<CountryDto>();
            if (countries.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving countries from " +
                    "the database or no country exists";
            }

            return View(countries);
        }

        public IActionResult GetCountryById(int countryId)
        {
            var country = _countryRepository.GetCountryById(countryId);
            //country = null;
            if(country == null)
            {
                ModelState.AddModelError("","Error getting a country");
                ViewBag.Message = $"There was a problem retrieving country with id {countryId} " +
                    $"from the database or no country with that id exists";
                country = new CountryDto();
            }

            var authors = _countryRepository.GetAuthorsFromACountry(countryId);
            if (authors.Count() <= 0)
            {
                ViewBag.AuthorMessage = $"There are no authors from country with id {country.Id}";                    
            }

            var countryAuthorsViewModel = new CountryAuthorsViewModel
            {
                Country = country,
                Authors = authors
            };

            return View(countryAuthorsViewModel);
        }
    }
}

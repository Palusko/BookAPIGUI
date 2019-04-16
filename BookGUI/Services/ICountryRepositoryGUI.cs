using BookApiProject.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Services
{
    public interface ICountryRepositoryGUI
    {
        IEnumerable<CountryDto> GetCountries();
        CountryDto GetCountryById(int countryId);
        CountryDto GetCountryOfAnAuthor(int authorId);
        IEnumerable<AuthorDto> GetAuthorsFromACountry(int countryId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BookApiProject.Dtos;

namespace BookGUI.Services
{
    public class CountryRepositoryGUI : ICountryRepositoryGUI
    {
        public IEnumerable<AuthorDto> GetAuthorsFromACountry(int countryId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CountryDto> GetCountries()
        {
            IEnumerable<CountryDto> countries = new List<CountryDto>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");

                var response = client.GetAsync("countries");
                response.Wait();

                var result = response.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<CountryDto>>();
                    readTask.Wait();

                    countries = readTask.Result;
                }
            }

            return countries;
        }

        public CountryDto GetCountryById(int countryId)
        {
            throw new NotImplementedException();
        }

        public CountryDto GetCountryOfAnAuthor(int authorId)
        {
            throw new NotImplementedException();
        }
    }
}

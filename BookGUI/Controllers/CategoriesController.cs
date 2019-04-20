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
    public class CategoriesController : Controller
    {
        ICategoryRepositoryGUI _categoryRepository;
        public CategoriesController(ICategoryRepositoryGUI categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            var categories = _categoryRepository.GetCategories();

            if (categories.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving categories from the database or no category exists";
            }

            return View(categories);
        }

        public IActionResult GetCategoryById(int categoryId)
        {
            var category = _categoryRepository.GetCategoryById(categoryId);
            
            if (category == null)
            {
                ModelState.AddModelError("", "Error getting a category");
                ViewBag.Message = $"There was a problem retrieving category with id {categoryId} " +
                    $"from the database or no category with that id exists";
                category = new CategoryDto();
            }

            var books = _categoryRepository.GetAllBooksForCategory(categoryId);

            if (books.Count() <= 0)
            {
                ViewBag.BookMessage = $"{category.Name} category has no books";
            }

            var bookCategoryViewModel = new CategoryBooksViewModel()
            {
                Category = category,
                Books = books
            };

            return View(bookCategoryViewModel);
        }
    }
}

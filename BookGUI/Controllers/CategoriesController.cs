using BookApiProject.Dtos;
using BookApiProject.Models;
using BookGUI.Services;
using BookGUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
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

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(bookCategoryViewModel);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PostAsJsonAsync("categories", category);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var newCategoryTask = result.Content.ReadAsAsync<Category>();
                    newCategoryTask.Wait();

                    var newCategory = newCategoryTask.Result;
                    TempData["SuccessMessage"] = $"Category {newCategory.Name} was successfully created.";

                    return RedirectToAction("GetCategoryById", new { categoryId = newCategory.Id });
                }

                if ((int)result.StatusCode == 422)
                {
                    ModelState.AddModelError("", "Category Already Exists!");
                }
                else
                {
                    ModelState.AddModelError("", "Some kind of error. Category not created!");
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult UpdateCategory(int categoryId)
        {
            var categoryToUpdate = _categoryRepository.GetCategoryById(categoryId);
            if (categoryToUpdate == null)
            {
                ModelState.AddModelError("", "Error getting category");
                categoryToUpdate = new CategoryDto();
            }

            return View(categoryToUpdate);
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category categoryToUpdate)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.PutAsJsonAsync($"categories/{categoryToUpdate.Id}", categoryToUpdate);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Category was successfully updated.";

                    return RedirectToAction("GetCategoryById", new { categoryId = categoryToUpdate.Id });
                }

                if ((int)result.StatusCode == 422)
                {
                    ModelState.AddModelError("", "Category Already Exists!");
                }
                else
                {
                    ModelState.AddModelError("", "Some kind of error. Category not updated!");
                }
            }

            var categoryDto = _categoryRepository.GetCategoryById(categoryToUpdate.Id);
            return View(categoryDto);
        }

        [HttpGet]
        public IActionResult DeleteCategory(int categoryId)
        {
            var category = _categoryRepository.GetCategoryById(categoryId);
            if (category == null)
            {
                ModelState.AddModelError("", "Some kind of error. Category doesn't exist!");
                category = new CategoryDto();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int categoryId, string categoryName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60039/api/");
                var responseTask = client.DeleteAsync($"categories/{categoryId}");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Category {categoryName} was successfully deleted.";

                    return RedirectToAction("Index");
                }

                if ((int)result.StatusCode == 409)
                {
                    ModelState.AddModelError("", $"Category {categoryName} cannot be deleted because " +
                                                $"it is used by at least one book");
                }
                else
                {
                    ModelState.AddModelError("", "Some kind of error. Category not deleted!");
                }
            }

            var categoryDto = _categoryRepository.GetCategoryById(categoryId);
            return View(categoryDto);
        }
    }
}

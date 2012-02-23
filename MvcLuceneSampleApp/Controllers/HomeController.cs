using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using MvcLuceneSampleApp.Search;
using MvcLuceneSampleApp.ViewModels;

namespace MvcLuceneSampleApp.Controllers {
	public class HomeController : Controller {
		private IEnumerable<SampleData> _searchResults;

		public ActionResult Index(string searchTerm, string searchField, string type) {
			// create default Lucene search index directory
			if (!Directory.Exists(LuceneSearch._luceneDir)) Directory.CreateDirectory(LuceneSearch._luceneDir);

			// perform Lucene search
			if (string.IsNullOrEmpty(type))
				_searchResults = string.IsNullOrEmpty(searchField)
				                 	? LuceneSearch.Search(searchTerm)
				                 	: LuceneSearch.Search(searchTerm, searchField);
			else if (type == "default")
				_searchResults = string.IsNullOrEmpty(searchField)
				                 	? LuceneSearch.SearchDefault(searchTerm)
				                 	: LuceneSearch.SearchDefault(searchTerm, searchField);
			
			// setup and return view model
			var search_field_list = new
				List<SelectListItem> {
				                     	new SelectListItem {Text = "(All Fields)", Value = ""},
				                     	new SelectListItem {Text = "Id", Value = "Id"},
				                     	new SelectListItem {Text = "Name", Value = "Name"},
				                     	new SelectListItem {Text = "Description", Value = "Description"}
				                     };
			return View(new IndexViewModel {
			                               	AllSampleData = SampleDataRepository.GetAll(),
			                               	AllSearchIndexData = LuceneSearch.GetAllIndexRecords(),
			                               	SampleData = new SampleData {Id = 9, Name = "El-Paso", Description = "City in Texas"},
			                               	SampleSearchResults = _searchResults,
			                               	SearchFieldList = search_field_list,
			                               });
		}

		public ActionResult Search(string searchTerm, string searchField) {
			return RedirectToAction("Index", new {searchTerm, searchField});
		}

		public ActionResult SearchDefault(string searchTerm, string searchField) {
			return RedirectToAction("Index", new {type = "default", searchTerm, searchField});
		}

		public ActionResult CreateIndex() {
			LuceneSearch.AddUpdateLuceneIndex(SampleDataRepository.GetAll());
			TempData["Result"] = "Search index was created successfully!";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult AddToIndex(SampleData sampleData) {
			LuceneSearch.AddUpdateLuceneIndex(sampleData);
			TempData["Result"] = "Record was added to search index successfully!";
			return RedirectToAction("Index");
		}

		public ActionResult ClearIndex() {
			if (LuceneSearch.ClearLuceneIndex())
				TempData["Result"] = "Search index was cleared successfully!";
			else
				TempData["ResultFail"] = "Index is locked and cannot be cleared, try again later or clear manually!";
			return RedirectToAction("Index");
		}

		public ActionResult ClearIndexRecord(int id) {
			LuceneSearch.ClearLuceneIndexRecord(id);
			TempData["Result"] = "Search index record was deleted successfully!";
			return RedirectToAction("Index");
		}

		public ActionResult OptimizeIndex() {
			LuceneSearch.Optimize();
			TempData["Result"] = "Search index was optimized successfully!";
			return RedirectToAction("Index");
		}

	}
}
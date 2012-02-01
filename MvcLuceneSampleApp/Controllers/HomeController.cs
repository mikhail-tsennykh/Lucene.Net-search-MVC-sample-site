using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using MvcLuceneSampleApp.Search;
using MvcLuceneSampleApp.ViewModels;

namespace MvcLuceneSampleApp.Controllers {
	public class HomeController : Controller {
		private IEnumerable<SampleData> _searchResults;

		public ActionResult Index(string searchTerm, string searchField) {
			if (!Directory.Exists(LuceneSearch._luceneDir))
				Directory.CreateDirectory(LuceneSearch._luceneDir);

			_searchResults = string.IsNullOrEmpty(searchField)
			                 	? LuceneSearch.Search(searchTerm)
			                 	: LuceneSearch.Search(searchTerm, searchField);

			return View(new IndexViewModel {
			                               	AllSampleData = SampleDataRepository.GetAll(),
			                               	AllSearchIndexData = LuceneSearch.GetAllIndexRecords(),
			                               	SampleData = new SampleData {Id = 9, Name = "El-Paso", Description = "City in Texas"},
			                               	SampleSearchResults = _searchResults,
			                               }
				);
		}

		public ActionResult Search(string searchTerm, string searchField) {
			return RedirectToAction("Index", new {searchTerm, searchField});
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

	}
}
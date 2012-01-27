using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcLuceneSampleApp.Search;
using MvcLuceneSampleApp.ViewModels;

namespace MvcLuceneSampleApp.Controllers {
	public class HomeController : Controller {

		public ActionResult Index(string searchTerm) {
			if (!Directory.Exists(LuceneSearch._luceneDir))
				Directory.CreateDirectory(LuceneSearch._luceneDir);

			return View(new IndexViewModel {
			                               	AllSampleData = SampleDataRepository.GetAll(),
			                               	AllSearchIndexData = LuceneSearch.GetAllIndexRecords(),
			                               	SampleData = new SampleData {Id = 9, Name = "El-Paso", Description = "City in Texas"},
			                               	SampleSearchResults = LuceneSearch.SearchByAll(searchTerm),
			                               }
				);
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

		[HttpPost]
		public ActionResult SearchIndex(string searchTerm) {
			return RedirectToAction("Index", new {searchTerm});
		}

		public ActionResult ClearIndex() {
			if (LuceneSearch.ClearLuceneIndex())
				TempData["Result"] = "Search index was cleared successfully!";
			else
				TempData["ResultFail"] = "Index is locked and cannot be cleared, try again later or clear manually!";
			return RedirectToAction("Index");
		}

	}
}
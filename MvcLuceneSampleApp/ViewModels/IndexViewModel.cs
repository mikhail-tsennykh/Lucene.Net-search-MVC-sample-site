using System.Collections.Generic;
using System.Web.Mvc;
using MvcLuceneSampleApp.Search;

namespace MvcLuceneSampleApp.ViewModels {
	public class IndexViewModel {
		public SampleData SampleData { get; set; }
		public IEnumerable<SampleData> AllSampleData { get; set; }
		public IEnumerable<SampleData> AllSearchIndexData { get; set; }
		public IEnumerable<SampleData> SampleSearchResults { get; set; }
		public IList<SelectListItem> SearchFieldList { get; set; }
		public string SearchTerm { get; set; }
		public string SearchField { get; set; }
	}
}
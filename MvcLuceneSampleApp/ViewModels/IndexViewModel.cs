using System.Collections.Generic;
using MvcLuceneSampleApp.Search;

namespace MvcLuceneSampleApp.ViewModels {
	public class IndexViewModel {
		public SampleData SampleData { get; set; }
		public IEnumerable<SampleData> AllSampleData { get; set; }
		public IEnumerable<SampleData> AllSearchIndexData { get; set; }
		public IEnumerable<SampleData> SampleSearchResults { get; set; }
		public string SearchTerm { get; set; }
		public string SearchField { get; set; }
	}
}
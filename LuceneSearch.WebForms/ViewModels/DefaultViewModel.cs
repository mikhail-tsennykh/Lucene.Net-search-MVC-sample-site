using System.Collections.Generic;
using LuceneSearch.Model;

namespace WebFormsLuceneSampleApp.ViewModels {
	public class DefaultViewModel {
		public SampleData SampleData { get; set; }
		public IEnumerable<SampleData> AllSampleData { get; set; }
		public IEnumerable<SampleData> AllSearchIndexData { get; set; }
		public IEnumerable<SampleData> SampleSearchResults { get; set; }
		public IList<SelectedList> SearchFieldList { get; set; }
		public string SearchTerm { get; set; }
		public string SearchField { get; set; }
	}
}
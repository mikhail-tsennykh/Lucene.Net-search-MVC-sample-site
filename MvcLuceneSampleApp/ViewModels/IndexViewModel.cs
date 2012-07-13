using System.Collections.Generic;
using LuceneSearch.Model;

namespace MvcLuceneSampleApp.ViewModels {
	public class IndexViewModel {
	  public int Limit { get; set; }
	  public bool SearchDefault { get; set; }
	  public SampleData SampleData { get; set; }
		public IEnumerable<SampleData> AllSampleData { get; set; }
		public IEnumerable<SampleData> SearchIndexData { get; set; }
		public IList<SelectedList> SearchFieldList { get; set; }
		public string SearchTerm { get; set; }
		public string SearchField { get; set; }
	}
}
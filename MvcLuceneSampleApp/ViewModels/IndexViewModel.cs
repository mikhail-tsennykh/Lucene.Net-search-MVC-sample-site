using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcLuceneSampleApp.Search;

namespace MvcLuceneSampleApp.ViewModels {
	public class IndexViewModel {
		public SampleData SampleData { get; set; }
		public IEnumerable<SampleData> AllSampleData { get; set; }
		public IEnumerable<SampleData> AllSearchIndexData { get; set; }
		public IEnumerable<SampleData> SampleSearchResults { get; set; }
		public string SearchTerm { get; set; }
	}
}
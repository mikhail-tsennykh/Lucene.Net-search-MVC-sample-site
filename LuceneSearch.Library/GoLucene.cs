using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneSearch.Model;
using Version = Lucene.Net.Util.Version;

namespace LuceneSearch.Service {
	public static class GoLucene {
		// Properties
		public static string _luceneDir =
			Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_index");
		private static FSDirectory _directoryTemp;
		private static FSDirectory _directory {
			get {
				if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
				if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
				var lockFilePath = Path.Combine(_luceneDir, "write.lock");
				if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
				return _directoryTemp;
			}
		}

		// Search methods
    public static IEnumerable<SampleData> Search(string input, string fieldName = "") {
			if (string.IsNullOrEmpty(input)) return new List<SampleData>();
			
			var terms = input.Trim().Replace("-", " ").Split(' ')
				.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
			input = string.Join(" ", terms);

			return _search(input, fieldName);
		}
		public static IEnumerable<SampleData> SearchDefault(string input, string fieldName = "") {
			return string.IsNullOrEmpty(input) ? new List<SampleData>() : _search(input, fieldName);
		}
		public static IEnumerable<SampleData> GetAllIndexRecords() {
			// validate search index
			if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<SampleData>();

			// set up lucene searcher
			var searcher = new IndexSearcher(_directory, false);
			var reader = IndexReader.Open(_directory, false);
			var docs = new List<Document>();
			var term = reader.TermDocs();
      // v 2.9.4: use 'term.Doc()'
      // v 3.0.3: use 'term.Doc'
			while (term.Next()) docs.Add(searcher.Doc(term.Doc));
			reader.Dispose();
			searcher.Dispose();
		  return docs.MapToDataList();
		}

		// Main search method
		private static IEnumerable<SampleData> _search(string searchQuery, string searchField = "") {
			// validation
			if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<SampleData>();

			// set up lucene searcher
			using (var searcher = new IndexSearcher(_directory, false)) {
				var hits_limit = 1000;
				var analyzer = new StandardAnalyzer(Version.LUCENE_30);

				// search by single field
				if (!string.IsNullOrEmpty(searchField)) {
					var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
					var query = parseQuery(searchQuery, parser);
					var hits = searcher.Search(query, hits_limit).ScoreDocs;
					var results = hits.MapToDataList(searcher);
					analyzer.Close();
					searcher.Dispose();
					return results;
				}
				// search by multiple fields (ordered by RELEVANCE)
				else {
					var parser = new MultiFieldQueryParser
						(Version.LUCENE_30, new[] {"Id", "Name", "Description"}, analyzer);
					var query = parseQuery(searchQuery, parser);
					var hits = searcher.Search(query, null, hits_limit, Sort.INDEXORDER).ScoreDocs;
				  var results = hits.MapToDataList(searcher);
					analyzer.Close();
					searcher.Dispose();
					return results;
				}
			}
		}
		private static Query parseQuery(string searchQuery, QueryParser parser) {
			Query query;
			try {
				query = parser.Parse(searchQuery.Trim());
			}
			catch (ParseException) {
				query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
			}
			return query;
		}

		// Add/update/clear search index data 
		public static void AddUpdateLuceneIndex(SampleData sampleData) {
			AddUpdateLuceneIndex(new List<SampleData> {sampleData});
		}
		public static void AddUpdateLuceneIndex(IEnumerable<SampleData> sampleDatas) {
			// init lucene
			var analyzer = new StandardAnalyzer(Version.LUCENE_30);
			using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED)) {
				// add data to lucene search index (replaces older entries if any)
				foreach (var sampleData in sampleDatas) _addToLuceneIndex(sampleData, writer);

				// close handles
				analyzer.Close();
				writer.Dispose();
			}
		}
		public static void ClearLuceneIndexRecord(int record_id) {
			// init lucene
			var analyzer = new StandardAnalyzer(Version.LUCENE_30);
			using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED)) {
				// remove older index entry
				var searchQuery = new TermQuery(new Term("Id", record_id.ToString()));
				writer.DeleteDocuments(searchQuery);

				// close handles
				analyzer.Close();
				writer.Dispose();
			}
		}
		public static bool ClearLuceneIndex() {
			try {
				var analyzer = new StandardAnalyzer(Version.LUCENE_30);
				using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED)) {
					// remove older index entries
					writer.DeleteAll();

					// close handles
					analyzer.Close();
					writer.Dispose();
				}
			}
			catch (Exception) {
				return false;
			}
			return true;
		}
		public static void Optimize() {
			var analyzer = new StandardAnalyzer(Version.LUCENE_30);
			using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED)) {
				analyzer.Close();
				writer.Optimize();
				writer.Dispose();
			}
		}
		private static void _addToLuceneIndex(SampleData sampleData, IndexWriter writer) {
			// remove older index entry
			var searchQuery = new TermQuery(new Term("Id", sampleData.Id.ToString()));
			writer.DeleteDocuments(searchQuery);

			// add new index entry
			var doc = new Document();

			// add lucene fields mapped to db fields
			doc.Add(new Field("Id", sampleData.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			doc.Add(new Field("Name", sampleData.Name, Field.Store.YES, Field.Index.ANALYZED));
			doc.Add(new Field("Description", sampleData.Description, Field.Store.YES, Field.Index.ANALYZED));

			// add entry to index
			writer.AddDocument(doc);
		}

    // Map Lucene search index to data
		private static IEnumerable<SampleData> 
      MapToDataList(this IEnumerable<Document> hits) {
		  return hits.Select(hit => hit.MapToData()).ToList();
		}
		private static IEnumerable<SampleData> 
      MapToDataList(this IEnumerable<ScoreDoc> hits, IndexSearcher searcher) {
      // v 2.9.4: use 'hit.doc'
      // v 3.0.3: use 'hit.Doc'
		  return hits.Select(hit => searcher.Doc(hit.Doc).MapToData()).ToList();
		}
		private static SampleData 
      MapToData(this Document doc) {
			return new SampleData {
			                      	Id = Convert.ToInt32(doc.Get("Id")),
			                      	Name = doc.Get("Name"),
			                      	Description = doc.Get("Description")
			                      };
		}

	}
}
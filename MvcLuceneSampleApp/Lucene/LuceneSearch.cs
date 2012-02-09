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
using Version = Lucene.Net.Util.Version;

namespace MvcLuceneSampleApp.Search {
	public static class LuceneSearch {
		// properties
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
		

		// search methods
		public static IEnumerable<SampleData> GetAllIndexRecords() {
			// validate search index
			if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<SampleData>();

			// set up lucene searcher
			var searcher = new IndexSearcher(_directory, false);
			var reader = IndexReader.Open(_directory, false);
			var docs = new List<Document>();
			var term = reader.TermDocs();
			while (term.Next()) docs.Add(searcher.Doc(term.Doc()));
			reader.Close();
			reader.Dispose();
			searcher.Close();
			searcher.Dispose();
			return _mapLuceneToDataList(docs);
		}
		public static IEnumerable<SampleData> Search(string input, string fieldName = "") {
			if (string.IsNullOrEmpty(input)) return new List<SampleData>();
			input = input.Replace("-", " ").Replace("\"", "").Trim() + "*";
			if (input.IndexOf("*") == 0) input = input.Replace("*", "");
			return _search(input, fieldName);
		}


		// main search method
		private static IEnumerable<SampleData> _search(string searchQuery, string searchField = "") {
			// validation
			if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<SampleData>();

			// set up lucene searcher
			using (var searcher = new IndexSearcher(_directory, false)) {
				var hits_limit = 1000;
				var analyzer = new StandardAnalyzer(Version.LUCENE_29);

				// search by single field
				if (!string.IsNullOrEmpty(searchField)) {
					var parser = new QueryParser(Version.LUCENE_29, searchField, analyzer);
					var query = parser.Parse(searchQuery.Trim());
					var hits = searcher.Search(query, hits_limit).ScoreDocs;
					var results = _mapLuceneToDataList(hits, searcher);
					analyzer.Close();
					searcher.Close();
					searcher.Dispose();
					return results;
				}
				// search by multiple fields (ordered by RELEVANCE)
				else {
					var parser = new MultiFieldQueryParser
						(Version.LUCENE_29, new[] {"Id", "Name", "Description"}, analyzer);
					var query = parser.Parse(searchQuery.Trim());
					var hits = searcher.Search(query, null, hits_limit, Sort.INDEXORDER).ScoreDocs;
					var results = _mapLuceneToDataList(hits, searcher);
					analyzer.Close();
					searcher.Close();
					searcher.Dispose();
					return results;
				}
			}
		}


		// map Lucene search index to data
		private static IEnumerable<SampleData> _mapLuceneToDataList(IEnumerable<Document> hits) {
			return hits.Select(_mapLuceneDocumentToData).ToList();
		}
		private static IEnumerable<SampleData> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher) {
			return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.doc))).ToList();
		}
		private static SampleData _mapLuceneDocumentToData(Document doc) {
			return new SampleData {
			                      	Id = Convert.ToInt32(doc.Get("Id")),
			                      	Name = doc.Get("Name"),
			                      	Description = doc.Get("Description")
			                      };
		}
		

		// add/update/clear search index data 
		public static void AddUpdateLuceneIndex(SampleData sampleData) {
			AddUpdateLuceneIndex(new List<SampleData> {sampleData});
		}
		public static void AddUpdateLuceneIndex(IEnumerable<SampleData> sampleDatas) {
			// init lucene
			var analyzer = new StandardAnalyzer(Version.LUCENE_29);
			using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED)) {
				// add data to lucene search index (replaces older entries if any)
				foreach (var sampleData in sampleDatas) _addToLuceneIndex(sampleData, writer);

				// close handles
				analyzer.Close();
				writer.Close();
				writer.Dispose();
			}
		}
		public static void ClearLuceneIndexRecord(int record_id) {
			// init lucene
			var analyzer = new StandardAnalyzer(Version.LUCENE_29);
			using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED)) {
				// remove older index entry
				var searchQuery = new TermQuery(new Term("Id", record_id.ToString()));
				writer.DeleteDocuments(searchQuery);

				// close handles
				analyzer.Close();
				writer.Close();
				writer.Dispose();
			}
		}
		public static bool ClearLuceneIndex() {
			try {
				var analyzer = new StandardAnalyzer(Version.LUCENE_29);
				using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED)) {
					// remove older index entries
					writer.DeleteAll();

					// close handles
					analyzer.Close();
					writer.Close();
					writer.Dispose();
				}
			}
			catch (Exception) {
				return false;
			}
			return true;
		}
		public static void Optimize() {
			var analyzer = new StandardAnalyzer(Version.LUCENE_29);
			using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED)) {
				analyzer.Close();
				writer.Optimize();
				writer.Close();
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

	}
}
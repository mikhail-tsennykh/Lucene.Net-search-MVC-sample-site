using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using LuceneSearch.Service;
using LuceneSearch.Data;
using LuceneSearch.Model;
using WebFormsLuceneSampleApp.ViewModels;

namespace WebFormsLuceneSampleApp {
  public partial class _DefaultPage : Page {
    protected void Page_Load(object sender, EventArgs e) {
      if (!IsPostBack)
        bindData();
    }

    public DefaultViewModel Model;

    private string getSearchTerm() {
      return Request.QueryString["searchTerm"] ?? "";
    }
    private string getSearchField() {
      return Request.QueryString["searchField"] ?? "";
    }
    private string getSearchType() {
      return Request.QueryString["type"] ?? "";
    }
    private string getLimit() {
      return Request.QueryString["limit"] ?? "";
    }

    private void bindData() {
      Model = getDefaultViewModel(getSearchTerm(), getSearchField(), getSearchType());
      
      var limit = getLimit() == "" ? 3 : Convert.ToInt32(getLimit());
      if (limit > 0) {
        litDatabaseRecordsCount.Text =
          "<div class=\"margin_top10\">And <b>" + (Model.AllSampleData.Count() - limit) + "</b> more records... " +
          "(<a href=\"" + Request.ApplicationPath + "?limit=0\">See All</a>)</div>";
        lvDatabase.DataSource = Model.AllSampleData.Take(limit);
      }
      else
        lvDatabase.DataSource = Model.AllSampleData;
      lvDatabase.DataBind();
      
      var searchIndex = Model.AllSearchIndexData;
      if (getSearchTerm() != string.Empty)
        searchIndex = Model.SampleSearchResults;

      lvSearchIndex.DataSource = searchIndex;
      lvSearchIndex.DataBind();

      ddlSearchFields.DataValueField = "Value";
      ddlSearchFields.DataTextField = "Text";
      ddlSearchFields.DataSource = Model.SearchFieldList;
      ddlSearchFields.DataBind();

      txtSearch.Text = getSearchTerm();
      ddlSearchFields.SelectedValue = getSearchField();
      chkSearchDefault.Checked = getSearchType() == "default";
    }

    private DefaultViewModel getDefaultViewModel(string searchTerm, string searchField, string type = "") {
      // create default Lucene search index directory
      if (!Directory.Exists(GoLucene._luceneDir)) Directory.CreateDirectory(GoLucene._luceneDir);

      // perform Lucene search
      IEnumerable<SampleData> searchResults = new List<SampleData>();
      if (string.IsNullOrEmpty(type))
        searchResults = string.IsNullOrEmpty(searchField)
                           ? GoLucene.Search(searchTerm)
                           : GoLucene.Search(searchTerm, searchField);
      else if (type == "default")
        searchResults = string.IsNullOrEmpty(searchField)
                           ? GoLucene.SearchDefault(searchTerm)
                           : GoLucene.SearchDefault(searchTerm, searchField);

      // setup and return view model
      var search_field_list = new
        List<SelectedList> {
                             new SelectedList {Text = "(All Fields)", Value = ""},
                             new SelectedList {Text = "Id", Value = "Id"},
                             new SelectedList {Text = "Name", Value = "Name"},
                             new SelectedList {Text = "Description", Value = "Description"}
                           };
      return new DefaultViewModel {
                                  AllSampleData = SampleDataRepository.GetAll(),
                                  AllSearchIndexData = GoLucene.GetAllIndexRecords(),
                                  SampleData = new SampleData {Id = 9, Name = "El-Paso", Description = "City in Texas"},
                                  SampleSearchResults = searchResults,
                                  SearchFieldList = search_field_list,
                                };
    }

    private void createIndex() {
      GoLucene.AddUpdateLuceneIndex(SampleDataRepository.GetAll());
      litResult.Text = "Search index was created successfully!";
      bindData();
    }

    private void addToIndex(SampleData sampleData) {
      GoLucene.AddUpdateLuceneIndex(sampleData);
      litResult.Text = "Record was added to search index successfully!";
      bindData();
    }

    private void clearIndex() {
      if (GoLucene.ClearLuceneIndex())
        litResult.Text = "Search index was cleared successfully!";
      else
        litResult.Text = "Index is locked and cannot be cleared, try again later or clear manually!";
      bindData();
    }

    private void clearIndexRecord(int id) {
			GoLucene.ClearLuceneIndexRecord(id);
      litResult.Text = "Search index record was deleted successfully!";
      bindData();
		}

    private void optimizeIndex() {
			GoLucene.Optimize();
      litResult.Text = "Search index was optimized successfully!";
      bindData();
		}
    
    // List views
    protected void lvSearchIndex_OnItemDeleting(object sender, ListViewDeleteEventArgs e) {
      var recordId = (int) lvSearchIndex.DataKeys[e.ItemIndex].Value;
      clearIndexRecord(recordId);
    }
    
    // Form events
    protected void CreateIndex_Click(object sender, EventArgs e) {
      createIndex();
    }
    protected void lnkOptimizeIndex_Click(object sender, EventArgs e) {
      optimizeIndex();
    }
    protected void lnkClearIndex_Click(object sender, EventArgs e) {
      clearIndex();
    }
    protected void btnAddUpdate_Click(object sender, EventArgs e) {
      int id;
      if (!int.TryParse(txtId.Text, out id)) {
        litResultFail.Text = "'Id' must be a number!";
        return;
      }
      addToIndex
        (new SampleData {
                          Id = id,
                          Name = txtName.Text,
                          Description = txtDescription.Text
                        });
    }
    protected void btnSearch_Click(object sender, EventArgs e) {
      Response.Redirect(Request.ApplicationPath +
                        "?searchTerm=" + txtSearch.Text +
                        "&searchField=" + ddlSearchFields.SelectedValue +
                        "&type=" + (chkSearchDefault.Checked ? "default" : "")
        );
    }

  }
}
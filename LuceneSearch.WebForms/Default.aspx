<%@ Page Language="C#" MasterPageFile="~/Layout.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="WebFormsLuceneSampleApp._DefaultPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" runat="server">
  <div class="content_left">
    <span style="color: green;"><asp:Literal ID="litResult" runat="server" /></span>
	  <span style="color: red;"><asp:Literal ID="litResultFail" runat="server" /></span>
  </div>
  <div class="clear"></div>

  <div class="col_12">
    <fieldset>
	    <legend>
	      Database records (<asp:LinkButton ID="lnkCreateIndex" OnClick="CreateIndex_Click" runat="server">Create Search Index From Database [+]</asp:LinkButton>)
	    </legend> 
      
      <asp:ListView ID="lvDatabase" DataKeyNames="Id" runat="server">
        <LayoutTemplate>
 	        <table class="grid">
		        <tr>
			        <th>Id</th>
			        <th>Name</th>
			        <th>Description</th>
		        </tr>         
            <tr id="itemPlaceHolder" runat="server"></tr>
          </table>
        </LayoutTemplate>
        <ItemTemplate>
          <tr>
			      <td><%# Eval("Id") %></td>
			      <td><%# Eval("Name") %></td>
			      <td><%# Eval("Description") %></td>
		      </tr>
        </ItemTemplate>
      </asp:ListView>
      
      <asp:Literal ID="litDatabaseRecordsCount" runat="server" />

    </fieldset>
  </div>
  
  <div class="searchBox col_12">
    <fieldset>
	    <legend>Search (custom, useful for most basic scenarios)</legend>
      <div class="content_left margin_top5">
	      <p>Try these searches: <em>"1 3 5", "City", "Russia India", "bel mos ind"</em></p>
      </div>
      <div class="content_left margin_top13 margin_left30">
        <asp:CheckBox ID="chkSearchDefault" Text="Use default Lucene query" runat="server" />
      </div>
          
      <div class="content_left margin_right20">
        <asp:TextBox ID="txtSearch" Width="650px" autocomplete="off" CssClass="big" runat="server" />
      </div>
      <div class="content_left margin_top15 margin_right30">
        <asp:DropDownList ID="ddlSearchFields" Width="150px" runat="server" />
      </div>
      <div class="content_right margin_top8">
        <asp:Button ID="btnSearch" Text="Search" OnClick="btnSearch_Click" runat="server" />
      </div>
      <div class="clear"></div>

    </fieldset>
  </div>
  
  <div class="col_8">
    <fieldset>
	    <legend>
		    Search index
        (<asp:LinkButton ID="lnkOptimizeIndex" OnClick="lnkOptimizeIndex_Click" runat="server">Optimize [+]</asp:LinkButton>)
        (<asp:LinkButton ID="lnkClearIndex" OnClick="lnkClearIndex_Click" runat="server">Clear [+]</asp:LinkButton>)
	    </legend> 
      
      <asp:ListView ID="lvSearchIndex" DataKeyNames="Id" runat="server"
      OnItemDeleting="lvSearchIndex_OnItemDeleting">
        <LayoutTemplate>
 	        <table class="grid">
		        <tr>
			        <th>Id</th>
			        <th>Name</th>
			        <th>Description</th>
			        <th></th>
		        </tr>         
            <tr id="itemPlaceHolder" runat="server"></tr>
          </table>
        </LayoutTemplate>
        <ItemTemplate>
          <tr>
			      <td><%# Eval("Id") %></td>
			      <td><%# Eval("Name") %></td>
			      <td><%# Eval("Description") %></td>
            <td>
              <asp:LinkButton ID="lnkClearIndexRecord" CommandName="Delete" runat="server">Delete</asp:LinkButton>
            </td>
		      </tr>
        </ItemTemplate>
        <EmptyDataTemplate>
          <br/>No search index records found...<br/>
        </EmptyDataTemplate>
      </asp:ListView>

    </fieldset>
  </div>
  
  <div class="addRecord col_4">
    <fieldset>
	    <legend>Add/Update search index record</legend>
      Use Id of existing one to update
      <div class="form_horizontal">
        <p>
          Id<br/><asp:TextBox ID="txtId" Text="9" Width="30" runat="server" />
        </p>
        <p>
          Name<br/><asp:TextBox ID="txtName" Text="El-Paso" Width="100" runat="server" />
        </p>
        <p>
          Description<br/><asp:TextBox ID="txtDescription" Text="City in Texas" Width="120" runat="server" />
        </p>
      </div>
      <div class="clear"></div>
      <asp:Button ID="btnAddUpdate" Text="Add/Update Record" OnClick="btnAddUpdate_Click" runat="server" />
    </fieldset>
  </div>
  <div class="clear"></div>
  
  <script type="text/javascript">
    $(document).ready(function () {
      $('#PageBody_txtSearch').focus();
      $('.grid tr:even').css("background", "silver");
      
      var key_codes = { 'enter_key': 13 };
      $('.addRecord').keypress(function (e) {
        if (e.which == key_codes.enter_key) { $('#PageBody_btnAddUpdate').click(); }
      });
      $('.searchBox').keypress(function (e) {
        if (e.which == key_codes.enter_key) { $('#PageBody_btnSearch').click(); }
      });

    });
  </script>
</asp:Content>
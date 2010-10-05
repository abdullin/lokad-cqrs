<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Sample_05.Web.Controllers.RegistrationForm>" %>


<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Create account - Salescast
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div id="left">
	<% if (ViewData["Message"] != null) { %>
	<div class="box">
		<%= Html.Encode(ViewData["Message"].ToString())%>
	</div>
	<% } %>

	<h2>Create a Salecast account</h2>
	<p>We don't have <em><%=Html.Encode(Model.Identity) %></em> recorded in Salecast.</p>
	<p>You need to register before proceeding.</p>

	<% =Html.ValidationSummary(true) %>
	<% using (Html.BeginForm("register","user")){%>
		
		<%=Html.HiddenFor(r => r.Identity) %>
		<%=Html.HiddenFor(r=> r.Name) %>

		<fieldset>
			<legend>Enter account info:</legend>

			<div class="editor-label">Email to notify about results:</div>

			<div class="editor-field">
				<%=Html.EditorFor(r => r.Email) %>
				<%= Html.ValidationMessageFor(model => model.Email) %>
			</div>
			
			<input type="submit" value="Register" />
		</fieldset>
	<%}%>
</div>
<div id="right">

</div>
</asp:Content>

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
    <h3>Big Picture</h3>
    <p>Salecast is a client app delivering sales forecasts. In order to use
    Salecast, you need a <a href="http://app.lokad.com/">Lokad Account</a>. 
    After opening your Lokad Account, you will receive the Forecasting API key
    in your registration email.</p>

    <h3>Why two registrations?</h3>
    <p>The forecasting technology of Lokad can be used for many purposes.
    Your Lokad account keeps track of your overal forecasts consumption, 
    while Salescast is strictly specialized on importing sales data
    and delivering sales forecast reports.
    </p>

    <h3>How much does it cost?</h3>
    <p>Salecast itself is provided free of charge, yet, Salescast relies on the
    Forecasting Service of Lokad to build its reports. Basically, Lokad has a
    pay-as-you pricing for each forecast you consume. See our 
    <a href="http://www.lokad.com/pricing.ashx">pricing page</a> for more 
    details.</p>
</div>
</asp:Content>

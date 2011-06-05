<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MasterPage.master" Inherits="ViewPage<UserDashboardView>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>






<h3>Add Contact</h3>
    <% =Html.ValidationSummary(true) %>

  <%--  <% using (Html.BeginForm("CreateContact","Home")) {%>
        <%=Html.HiddenFor(f => f.SolutionId) %>
        <%=Html.HiddenFor(f => f.OldName) %>

        <%=Html.EditorFor(x => x.Name) %>
        <%=Html.ValidationMessageFor(x=> x.Name) %>

        <p><input type="submit" value="Update" /></p>
    <%}%>
--%>



</asp:Content>

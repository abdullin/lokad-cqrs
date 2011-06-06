<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MasterPage.master" Inherits="ViewPage<UserDashboardView>" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>

    <%=Html
        .Grid(Model.Contacts)
        .Columns(c => c.For(x => x.DisplayName))
        .Empty("There are no contacts")
            .Attributes(id => "contactTable")%>

<script>

    function htmlEncode(value) {
        if (value) {
            return jQuery('<div/>').text(value).html();
        } else {
            return '';
        }
    }
    function addContact() {

        var str = $("#addContactForm").serialize();
        $.post('<%=Url.Action("AddContact") %>', str, function (data) {
            var newRow = $("<tr><td>" + htmlEncode($("#name").attr("value")) + "</td></tr>");
            $('#contactTable tr:last').after(newRow);
        });
    }
</script>
        <div id="myDiv">
        <h3>Add</h3>
        <form action="" name="addContactForm">
            <input type="text" id="name" />
            <input onclick="addContact(); return false" type="button" value="ok" />
        </form>

</div>
        

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

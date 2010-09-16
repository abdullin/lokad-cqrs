<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Guid>" %>


<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Register - Create new Account - Salescast
</asp:Content>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
<div id="left">
	<h2>Create a Salecast account</h2>
	<fieldset>
		<p>Please, wait a few moments, while Lokad creates your account.</p>

		<div id="updateTarget">
			<p><strong id="message">Starting the registration...</strong></p>
			<img src="../../Content/Processing-32x32.gif" alt="Waiting" />
		</div>
	</fieldset>

</div>
<div id="right">

</div>
	
<script type="text/javascript">
	var timeoutInterval = 3000;

	$(function () {
		setRefresh(); 
	});

	function setRefresh() {
		setTimeout(function () { callRefresh(); }, timeoutInterval);
	}

	function callRefresh() {
		$.post(
			'<%=Url.Action("CheckJson",new {requestId=Model}) %>',
			null,
			function (data, status) {
				$("#message").text(data.Status);
				if (data.Redirect)
					document.location = data.Redirect;
				else
					setRefresh();
			}, "json");
	}
	
</script>

</asp:Content>

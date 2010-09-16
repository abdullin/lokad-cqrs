<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Salescast - Sales Forecasting Software - Get started in 5min
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" src="../../Content/OpenId/openid-jquery.js"></script>
	<script type="text/javascript">
		$(function () { $("#homeTab").addClass('active'); });
		$(document).ready(function () {
			openid.init('openId');
		});
	</script>
    

	<div id="onecolumn">
	<h1><span style="font-size:180%;"><span style="color:#AA4400">Salescast</span><br /></span>Sales Forecasting made WAY easier!</h1>
	<div style="height:2em;"></div>
	<table width="100%" style="border: solid 0px #FFFFFF;">
	<tr valign="top" style="border: solid 0px #FFFFFF;">
	<td style="width:50%;border: solid 0px #FFFFFF;">
	
    
    <% if (ViewData["Message"] != null) { %>
	<div class="box">
		<%= Html.Encode(ViewData["Message"].ToString())%>
	</div>
	<% } %>
    
    <%--Simple OpenID Selector --%>
    <%= Html.ValidationSummary("Login was unsuccessful. Please correct the errors and try again.") %>

    <form id="openid_form" action="<%=Url.Action("authenticate","user", new { ReturnUrl = Request.QueryString["ReturnUrl"]}) %>" method="post">

	<input type="hidden" name="action" value="verify" />
	<fieldset>    		
	
			<noscript>
            <p>
              No need to register, just log on with you OpenID!
            </p>
            </noscript>
            <legend>Enter your OpenID in order to proceed:</legend>
    		<div id="openid_choice">
	    		<p>You don't need to register, just use one of your existing accounts:</p>
	    		<div id="openid_btns"></div>
			</div>
			
			<div id="openid_input_area">
				<input id="openId" name="openId" type="text" value="http://" />
				<input id="openid_submit" type="submit" value="Sign-In" />
			</div>
        <%-- [vermorel] Remember Me is not implemented at this point.
        <p>
          <%= Html.CheckBox("rememberMe", (bool)(ViewData["RememberMe"] ?? false)) %> <label class="inline" for="rememberMe">Remember me?</label>
        </p>--%>
        
	</fieldset>
    </form>

	</td>
	<td class="noborder" style="padding:2em;border: solid 0px #FFFFFF;">	
		<h2>About OpenID</h2>
		<p>If you don't have an account on the existing providers, you can 
			<a href="https://www.myopenid.com/signup">get one in 2min</a> <img src="../../Content/OpenId/myopenid.ico" alt="MyOpenId" />.
		</p>
		<p>OpenID is service that enables you to login to many different websites using a <b>single identity</b>, 
			managed by an account provider (no more duplicated passwords). As you'll find out, you may already have an OpenID! </p>


		</td>
	</tr>
	</table>
	
    
</div>
</asp:Content>

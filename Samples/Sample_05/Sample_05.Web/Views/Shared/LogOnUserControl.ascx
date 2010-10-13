<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Sample_05.Web" %>
<%
    if (GlobalState.IsAuthenticated) {%>
        Welcome <b><%= Html.Encode(GlobalState.Identity.UserName)%></b>!
        [ <%= Html.ActionLink("Logout", "logout", "user") %> ]
<%
    }
    else {
%> 
        [ <%= Html.ActionLink("Login", "login", "user") %> ]
<%
    }
%>

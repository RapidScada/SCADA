﻿<%@ Page Title="About - Rapid SCADA" Language="C#" MasterPageFile="~/MasterMain.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Scada.Web.WFrmAbout" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainHead" runat="server">
    <link href="css/about.min.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphMainContent" runat="server">
    <div id="divContainer">
        <div id="divAppName"><asp:HyperLink ID="hlAppName" runat="server" NavigateUrl="http://rapidscada.org" Target="_blank">Webstation</asp:HyperLink>
            <span id="spanVersion">5.0.0.2</span></div>
    </div>
</asp:Content>

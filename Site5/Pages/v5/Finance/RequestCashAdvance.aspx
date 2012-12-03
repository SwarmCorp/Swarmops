﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Master-v5.master" AutoEventWireup="true" CodeFile="RequestCashAdvance.aspx.cs" Inherits="Pages_v5_Finance_RequestCashAdvance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderHead" Runat="Server">
    <script src="http://hostedscripts.falkvinge.net/easyui/jquery.easyui.min.js" type="text/javascript"></script>
	<link rel="stylesheet" type="text/css" href="https://hostedscripts.falkvinge.net/easyui/themes/icon.css">
        $(document).ready(function () {
            $('#DropBudgets').combotree({
                required: true,
                animate: true
            });  // Is this init call even necessary?
        });

    </script>


<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" Runat="Server">
    <div class="entryFields">
        <asp:TextBox runat="server" ID="TextAmount" CssClass="alignRight" />&nbsp;<br/>
        <asp:TextBox runat="server" ID="TextPurpose" />&nbsp;<br/>
        <select class="easyui-combotree" url="Json-ExpensableBudgetsTree.aspx" name="DropBudgets" id="DropBudgets" animate="true" style="width:300px"></select>&nbsp;<br/>
        &nbsp;<br/><!-- placeholder for label-side H2 -->
        <asp:TextBox runat="server" ID="TextBank" />&nbsp;<br/>
        <asp:TextBox runat="server" ID="TextClearing" />&nbsp;<br/>
        <asp:TextBox runat="server" ID="TextAccount" />&nbsp;<br/>
        <asp:Button ID="Button1" runat="server" CssClass="buttonAccentColor" Text="Request"/>
    </div>
    <div class="entryLabels">
        Amount (SEK)<br/>
        Purpose<br/>
        Budget<br/>
        <h2>Your bank details</h2>
        Bank<br/>
        Clearing#<br/>
        Account#
    </div>
    <div style="clear:both"></div>
</asp:Content>



<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderSide" Runat="Server">
</asp:Content>

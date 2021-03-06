﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Master-v5.master" AutoEventWireup="true" CodeFile="ProfitLossStatement.aspx.cs" Inherits="Pages_v5_Ledgers_ProfitLossStatement" %>
<%@ Register src="~/Controls/v5/UI/ExternalScripts.ascx" tagname="ExternalScripts" tagprefix="Swarmops5" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderHead" Runat="Server">
    <Swarmops5:ExternalScripts ID="ExternalScripts1" Package="easyui" Control="tree" runat="server" />

	<script type="text/javascript">

	    $(document).ready(function () { 

	        $('#tableProfitLoss').treegrid(
	        {
	            onBeforeExpand: function (foo) {
	                $('span.profitlossdata-collapsed-' + foo.id).fadeOut('fast', function () {
	                    $('span.profitlossdata-expanded-' + foo.id).fadeIn('slow');
	                });
	            },

	            onBeforeCollapse: function (foo) {
	                $('span.profitlossdata-expanded-' + foo.id).fadeOut('fast', function () {
	                    $('span.profitlossdata-collapsed-' + foo.id).fadeIn('slow');
	                });
	            },

	            onLoadSuccess: function () {
	                $('div.datagrid').css('opacity', 1);
	                $('#imageLoadIndicator').hide();
	                $('span.loadingHeader').hide();

	                var selectedYear = $('#<%=DropYears.ClientID %>').val();

	                $('div#linkDownloadReport').attr("onclick", "document.location='Csv-ProfitLossData.aspx?Year=" + selectedYear + "';");
	                $('#spanDownloadText').text('<%=Resources.Pages.Ledgers.ProfitLossStatement_DownloadFileName %>' + selectedYear + "-<%=DateTime.Today.ToString("yyyyMMdd") %>.csv");
	                
                    if (selectedYear == currentYear) 
                    {
                        $('span.previousYearsHeader').hide();
                        $('span.currentYearHeader').show();
                    }
                    else
                    {
                        $('#previousLastYear').text(selectedYear - 1);
                        $('#previousQ1').text(selectedYear + "-" + $('#currentQ1').text());
                        $('#previousQ2').text(selectedYear + "-" + $('#currentQ2').text());
                        $('#previousQ3').text(selectedYear + "-" + $('#currentQ3').text());
                        $('#previousQ4').text(selectedYear + "-" + $('#currentQ4').text());
                        $('#previousYtd').text(selectedYear);

                        $('span.currentYearHeader').hide();
                        $('span.previousYearsHeader').show();
                    }
	            }
	        });

	        $('#<%=DropYears.ClientID %>').change(function () {
	            var selectedYear = $('#<%=DropYears.ClientID %>').val();

	            $('#tableProfitLoss').treegrid({ url: 'Json-ProfitLossData.aspx?Year=' + selectedYear });
        	    $('#imageLoadIndicator').show();
	            $('div.datagrid').css('opacity', 0.4);

	            $('#tableProfitLoss').treegid('reload');
	        });


	        $('div.datagrid').css('opacity', 0.4);
	    });

	    var currentYear = <%=DateTime.Today.Year %>;

	</script>
    <style>
	    .content h2 select {
		    font-size: 16px;
            font-weight: bold;
            color: #1C397E;
            letter-spacing: 1px;
	    }
	    table.datagrid-ftable {
		    font-weight: 500;
	    }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" Runat="Server">
    <h2><asp:Label ID="LabelContentHeader" runat="server" /> <asp:DropDownList runat="server" ID="DropYears"/>&nbsp;<img alt="Loading" src="/Images/Abstract/ajaxloader-blackcircle.gif" ID="imageLoadIndicator" /></h2>
    <table id="tableProfitLoss" title="" class="easyui-treegrid" style="width:680px;height:600px"
        url="Json-ProfitLossData.aspx"
        rownumbers="false"
        animate="true" showFooter="true" fitColumns="true"
        idField="id" treeField="name">
    <thead>  
        <tr>  
            <th field="name" width="178"><asp:Literal ID="LiteralHeaderAccountName" runat="server"/></th>  
            <th field="lastYear" width="80" align="right"><span class="previousYearsHeader" id="previousLastYear" style="display:none"></span><span class="currentYearHeader" style="display:none"><asp:Literal ID="LiteralHeaderLastYear" runat="server" /></span><span class="loadingHeader">&mdash;</span></th>  
            <th field="q1" width="80" align="right"><span class="previousYearsHeader" id="previousQ1" style="display:none"></span><span class="currentYearHeader" style="display:none" id="currentQ1"><asp:Literal ID="LiteralHeaderQ1" runat="server" /></span><span class="loadingHeader">&mdash;</span></th>
            <th field="q2" width="80" align="right"><span class="previousYearsHeader" id="previousQ2" style="display:none"></span><span class="currentYearHeader" style="display:none" id="currentQ2"><asp:Literal ID="LiteralHeaderQ2" runat="server" /></span><span class="loadingHeader">&mdash;</span></th>
            <th field="q3" width="80" align="right"><span class="previousYearsHeader" id="previousQ3" style="display:none"></span><span class="currentYearHeader" style="display:none" id="currentQ3"><asp:Literal ID="LiteralHeaderQ3" runat="server" /></span><span class="loadingHeader">&mdash;</span></th>  
            <th field="q4" width="80" align="right"><span class="previousYearsHeader" id="previousQ4" style="display:none"></span><span class="currentYearHeader" style="display:none" id="currentQ4"><asp:Literal ID="LiteralHeaderQ4" runat="server" /></span><span class="loadingHeader">&mdash;</span></th>
            <th field="ytd" width="80" align="right"><span class="previousYearsHeader" id="previousYtd" style="display:none"></span><span class="currentYearHeader" style="display:none"><asp:Literal ID="LiteralHeaderYtd" runat="server" /></span><span class="loadingHeader">&mdash;</span></th>
        </tr>  
    </thead>  
</table> 
<div style="clear:both"></div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderSide" Runat="Server">
    <h2 class="blue"><asp:Label ID="LabelSidebarDownload" Text="Download This" runat="server" /><span class="arrow"></span></h2>
    
    <div class="box">
        <div class="content">
            <div class="link-row-encaps" id="linkDownloadReport" onclick="document.location='placeholder';" >
                <div class="link-row-icon" style="background-image:url('/Images/Icons/iconshock-downarrow-16px.png');background-position:-1px -1px"></div>
                <span id="spanDownloadText">ProfitLossxxxx-yyyymmdd.csv</span>
            </div>
        </div>
    </div>

</asp:Content>


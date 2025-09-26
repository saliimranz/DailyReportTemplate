<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DP.aspx.vb" Inherits="DailyReportTemplate.DP" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>LiMove Daily Report</title>
    <meta charset="utf-8" />
    <style>
        :root{
            --purple:#6C2BD9;        /* primary */
            --purple-d:#4C1D95;      /* darker hover */
            --bg:#ffffff;
            --ink:#111827;           /* gray-900 */
            --muted:#6B7280;         /* gray-500 */
            --line:#E5E7EB;          /* gray-200 */
            --chip:#EFE9FF;          /* light purple */
        }
        *{box-sizing:border-box}
        html,body{height:100%}
        body{
            margin:0; background:var(--bg); color:var(--ink);
            font:14px/1.45 "Segoe UI", Arial, Helvetica, sans-serif;
        }

        /* Header */
        .appbar{
            position:sticky; top:0; z-index:10;
            background:var(--purple); color:#fff; border-bottom:1px solid rgba(255,255,255,.15);
            padding:16px 24px; display:flex; align-items:center; justify-content:space-between;
        }
        .brand{
            display:flex; align-items:center; gap:10px; font-weight:700; letter-spacing:.2px;
        }
        .brand-mark{
            width:12px; height:12px; border-radius:3px; background:#fff; box-shadow:0 0 0 3px rgba(255,255,255,.35) inset;
            transform: rotate(45deg);
        }
        .brand h1{margin:0; font-size:18px}
        .subtle{opacity:.9; font-weight:500}

        .container{padding:20px 24px; max-width:1200px; margin:0 auto}

        /* Toolbar / Controls */
        .toolbar{
            display:flex; flex-wrap:wrap; gap:12px; align-items:flex-end;
            background:#fff; border:1px solid var(--line); border-radius:12px; padding:16px;
            box-shadow:0 2px 10px rgba(17,24,39,.04);
        }
        .field{display:flex; flex-direction:column; gap:6px; min-width:160px}
        .field label{font-size:12px; color:var(--muted); text-transform:uppercase; letter-spacing:.4px}
        .text, .select{
            height:36px; padding:0 10px; border:1px solid var(--line); border-radius:8px; outline:none;
        }
        .text:focus, .select:focus{border-color:var(--purple); box-shadow:0 0 0 3px rgba(108,43,217,.15)}
        .btn{
            height:36px; padding:0 14px; border-radius:8px; border:1px solid transparent;
            background:#f8f8ff; color:var(--purple); cursor:pointer; font-weight:600;
            display:inline-flex; align-items:center; justify-content:center; text-decoration:none;
        }
        .btn:hover{background:#f1edff}
        .btn.primary{background:var(--purple); color:#fff}
        .btn.primary:hover{background:var(--purple-d)}
        .btn.ghost{background:transparent; border-color:var(--line); color:var(--ink)}
        .btn.ghost:hover{background:#f9f9fb}

        .spacer{flex:1}

        /* Status / chips */
        .status{margin-top:12px; color:#B91C1C; font-weight:600}
        .chip{background:var(--chip); color:var(--purple-d); padding:2px 10px; border-radius:999px; font-size:12px; display:inline-block}

        /* Grid */
        .table-wrap{margin-top:16px; border:1px solid var(--line); border-radius:12px; overflow:hidden; background:#fff}
        .grid{width:100%; border-collapse:separate; border-spacing:0}
        .grid thead th{
            position:sticky; top:0; z-index:5;
            background:#F5F3FF; color:#3B0764; text-align:left; font-weight:700; padding:10px; border-bottom:1px solid var(--line);
        }
        .grid tbody td{padding:10px; border-bottom:1px solid var(--line); vertical-align:top}
        .grid tbody tr:nth-child(even){background:#FCFAFF}
        .grid tbody tr:hover{background:#F5F3FF55}
        .num{text-align:right}
        .unit{color:var(--muted)}

        /* ASP.NET validators */
        .val{color:#B91C1C; font-weight:700; margin-left:4px}
        @media (max-width:720px){
            .field{min-width:120px}
        }
        .pager{ padding:10px; text-align:right; border-top:1px solid var(--line); }
.pager a, .pager span{
  display:inline-block; margin-left:6px; padding:6px 10px; border-radius:6px; border:1px solid var(--line);
  text-decoration:none; color:var(--ink); background:#fff;
}
.pager a:hover{ background:#F5F3FF; border-color:var(--purple); }
.pager span{ background:#6C2BD9; color:#fff; border-color:#6C2BD9; } /* current page */
.grid thead th{ text-align:left !important; }

    </style>
</head>
<body>
<form id="form1" runat="server">

    <!-- Top Bar -->
    <div class="appbar">
        <div class="brand">
            <span class="brand-mark" aria-hidden="true"></span>
            <h1>LiMove <span class="subtle">Daily Report</span></h1>
        </div>
        <span class="chip" id="modeChip" runat="server">FROM DB</span>
    </div>

    <div class="container">

        <!-- Toolbar -->
<div class="toolbar">
    <div class="field">
        <label for="txtDate">Report Date</label>
        <asp:TextBox ID="txtDate" runat="server" CssClass="text" TextMode="Date"
            AutoPostBack="true" OnTextChanged="txtDate_TextChanged" />
        <asp:RequiredFieldValidator ID="rfvDate" runat="server" ControlToValidate="txtDate"
            ErrorMessage="*" CssClass="val" Display="Dynamic" />
    </div>

    <div class="field">
        <label for="ddlMode">Source Mode</label>
        <asp:DropDownList ID="ddlMode" runat="server" CssClass="select">
            <asp:ListItem Text="Fetch via Helper" Value="HELPER" />
            <asp:ListItem Text="Copy Previous Day" Value="COPY_PREV" />
        </asp:DropDownList>
    </div>

    <div class="spacer"></div>

    <div class="field" style="min-width:120px">
        <label>&nbsp;</label>
        <!-- Keep this as your “build/preview” action -->
        <asp:Button ID="btnRefresh" runat="server" CssClass="btn ghost" Text="Preview Report" />
    </div>

    <div class="field" style="min-width:160px">
        <label>&nbsp;</label>
        <asp:HyperLink ID="lnkManualEntry" runat="server" CssClass="btn" Text="Manual Entry" />
    </div>

    <div class="field" style="min-width:200px">
        <label>&nbsp;</label>
        <!-- Export button -->
        <asp:Button ID="btnExportExcel" runat="server" CssClass="btn primary" Text="Export to Excel" />
    </div>
</div>

        <!-- Status -->
        <div class="status">
            <asp:Label ID="lblInfo" runat="server" />
        </div>

<asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" CssClass="grid"
              AllowPaging="true" PageSize="30" GridLines="None"
              OnPageIndexChanging="gv_PageIndexChanging">
    <PagerSettings Mode="NumericFirstLast" Position="Bottom" PageButtonCount="10"
                   FirstPageText="« First" LastPageText="Last »" NextPageText="›" PreviousPageText="‹" />
    <PagerStyle CssClass="pager" />
    <Columns>
        <asp:BoundField DataField="ReportDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}">
            <HeaderStyle HorizontalAlign="Left" />
            <ItemStyle HorizontalAlign="Left" />
        </asp:BoundField>

        <asp:BoundField DataField="Section" HeaderText="Section">
            <HeaderStyle HorizontalAlign="Left" />
            <ItemStyle HorizontalAlign="Left" />
        </asp:BoundField>

        <asp:BoundField DataField="SubSection" HeaderText="Sub-Section">
            <HeaderStyle HorizontalAlign="Left" />
            <ItemStyle HorizontalAlign="Left" />
        </asp:BoundField>

        <asp:BoundField DataField="Item" HeaderText="Item">
            <HeaderStyle HorizontalAlign="Left" />
            <ItemStyle HorizontalAlign="Left" />
        </asp:BoundField>

        <asp:BoundField DataField="MeasureGroup" HeaderText="Group">
            <HeaderStyle HorizontalAlign="Left" />
            <ItemStyle HorizontalAlign="Left" />
        </asp:BoundField>

        <asp:BoundField DataField="MeasureName" HeaderText="Measure">
            <HeaderStyle HorizontalAlign="Left" />
            <ItemStyle HorizontalAlign="Left" />
        </asp:BoundField>

        <asp:BoundField DataField="ValueNum" HeaderText="Value" DataFormatString="{0:n0}">
            <HeaderStyle HorizontalAlign="Right" />
            <ItemStyle HorizontalAlign="Right" />
        </asp:BoundField>
    </Columns>
</asp:GridView>
</div>
</form>
</body>
</html>

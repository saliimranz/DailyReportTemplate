<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManualEntry.aspx.vb" Inherits="DailyReportTemplate.ManualEntry" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>LiMove Daily Report - Manual Entry</title>
    <meta charset="utf-8" />
    <style>
        :root{
            --purple:#6C2BD9;
            --purple-d:#4C1D95;
            --bg:#ffffff;
            --ink:#111827;
            --muted:#6B7280;
            --line:#E5E7EB;
            --chip:#EFE9FF;
        }
        *{box-sizing:border-box}
        html,body{height:100%}
        body{
            margin:0; background:var(--bg); color:var(--ink);
            font:14px/1.45 "Segoe UI", Arial, Helvetica, sans-serif;
        }
        .appbar{
            position:sticky; top:0; z-index:10;
            background:var(--purple); color:#fff; border-bottom:1px solid rgba(255,255,255,.15);
            padding:16px 24px; display:flex; align-items:center; justify-content:space-between;
        }
        .brand{display:flex; align-items:center; gap:10px; font-weight:700; letter-spacing:.2px;}
        .brand-mark{
            width:12px; height:12px; border-radius:3px; background:#fff; box-shadow:0 0 0 3px rgba(255,255,255,.35) inset;
            transform: rotate(45deg);
        }
        .brand h1{margin:0; font-size:18px}
        .subtle{opacity:.9; font-weight:500}
        .container{padding:20px 24px; max-width:1400px; margin:0 auto}
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
        }
        .btn:hover{background:#f1edff}
        .btn.primary{background:var(--purple); color:#fff}
        .btn.primary:hover{background:var(--purple-d)}
        .btn.ghost{background:transparent; border-color:var(--line); color:var(--ink)}
        .btn.ghost:hover{background:#f9f9fb}
        .spacer{flex:1}
        .status{margin-top:12px; color:#2563EB; font-weight:600}
        .status.error{color:#B91C1C}
        .chip{background:var(--chip); color:var(--purple-d); padding:2px 10px; border-radius:999px; font-size:12px; display:inline-block}
        .table-wrap{margin-top:16px; border:1px solid var(--line); border-radius:12px; background:#fff}
        .table-scroll{max-height:560px; overflow-y:auto; overflow-x:hidden}
        .table-scroll::-webkit-scrollbar{width:10px}
        .table-scroll::-webkit-scrollbar-thumb{background:rgba(108,43,217,.3); border-radius:999px}
        .table-scroll::-webkit-scrollbar-track{background:rgba(108,43,217,.08)}
        .grid{width:100%; border-collapse:separate; border-spacing:0}
        .grid thead th{
            position:sticky; top:0; z-index:5;
            background:#F5F3FF; color:#3B0764; text-align:left; font-weight:700; padding:10px; border-bottom:1px solid var(--line);
            font-size:12px;
        }
        .grid tbody td{padding:8px 10px; border-bottom:1px solid var(--line); vertical-align:top}
        .grid tbody tr:nth-child(even){background:#FCFAFF}
        .grid tbody tr:hover{background:#F5F3FF55}
        .grid .num{text-align:right}
        .input-small{width:110px}
        .input-medium{width:140px}
        .input-wide{width:220px}
        .section-label{font-weight:600; color:var(--purple-d)}
        .subsection-label{color:var(--muted); font-size:12px}
        @media (max-width:960px){
            .container{padding:12px}
            .field{min-width:140px}
            .input-wide{width:160px}
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="appbar">
            <div class="brand">
                <span class="brand-mark" aria-hidden="true"></span>
                <h1>LiMove <span class="subtle">Daily Report</span></h1>
            </div>
            <span class="chip" id="modeChip" runat="server">MANUAL ENTRY</span>
        </div>

        <div class="container">
            <div class="toolbar">
                <div class="field">
                    <label for="txtDate">Report Date</label>
                    <asp:TextBox ID="txtDate" runat="server" CssClass="text" TextMode="Date" />
                </div>
                <div class="field" style="min-width:140px">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnLoad" runat="server" CssClass="btn ghost" Text="Load Template" />
                </div>
                <div class="spacer"></div>
                <div class="field" style="min-width:160px">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnSave" runat="server" CssClass="btn primary" Text="Save Report" />
                </div>
                <div class="field" style="min-width:160px">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnBack" runat="server" CssClass="btn" Text="Back to Report" CausesValidation="False" />
                </div>
            </div>

            <div class="status">
                <asp:Label ID="lblInfo" runat="server" />
            </div>

            <div class="table-wrap">
                <div class="table-scroll">
                    <asp:GridView ID="gvManual" runat="server" AutoGenerateColumns="False" CssClass="grid" GridLines="None" AllowPaging="False">
                    <Columns>
                        <asp:TemplateField HeaderText="Section">
                            <ItemTemplate>
                                <span class="section-label"><%# Eval("Section") %></span><br />
                                <span class="subsection-label"><%# Eval("SubSection") %></span>
                                <asp:HiddenField ID="hfSection" runat="server" Value='<%# Eval("SectionKey") %>' />
                                <asp:HiddenField ID="hfSubSection" runat="server" Value='<%# Eval("SubSectionKey") %>' />
                                <asp:HiddenField ID="hfItem" runat="server" Value='<%# Eval("ItemKey") %>' />
                                <asp:HiddenField ID="hfMeasureGroup" runat="server" Value='<%# Eval("MeasureGroupKey") %>' />
                                <asp:HiddenField ID="hfMeasureName" runat="server" Value='<%# Eval("MeasureNameKey") %>' />
                                <asp:HiddenField ID="hfUnit" runat="server" Value='<%# Eval("Unit") %>' />
                                <asp:HiddenField ID="hfSortSection" runat="server" Value='<%# Eval("SortSection") %>' />
                                <asp:HiddenField ID="hfSortSubSection" runat="server" Value='<%# Eval("SortSubSection") %>' />
                                <asp:HiddenField ID="hfSortItem" runat="server" Value='<%# Eval("SortItem") %>' />
                                <asp:HiddenField ID="hfSortMeasure" runat="server" Value='<%# Eval("SortMeasure") %>' />
                                <asp:HiddenField ID="hfValueText" runat="server" Value='<%# Eval("ValueText") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Item" HeaderText="Item" />
                        <asp:BoundField DataField="MeasureGroup" HeaderText="Group" />
                        <asp:BoundField DataField="MeasureName" HeaderText="Measure" />
                        <asp:BoundField DataField="Unit" HeaderText="Unit" />
                        <asp:TemplateField HeaderText="Value (Number)">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValueNum" runat="server" CssClass="text input-small" Text='<%# Eval("ValueNumDisplay") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Remarks">
                            <ItemTemplate>
                                <asp:TextBox ID="txtRemarks" runat="server" CssClass="text input-wide" Text='<%# Eval("Remarks") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Oracle C1">
                            <ItemTemplate>
                                <asp:TextBox ID="txtOracleC1" runat="server" CssClass="text input-small" Text='<%# Eval("Oracle_C1_Display") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Oracle C2">
                            <ItemTemplate>
                                <asp:TextBox ID="txtOracleC2" runat="server" CssClass="text input-small" Text='<%# Eval("Oracle_C2_Display") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>

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
        .brand-mark{width:12px; height:12px; border-radius:3px; background:#fff; box-shadow:0 0 0 3px rgba(255,255,255,.35) inset; transform:rotate(45deg);}
        .brand h1{margin:0; font-size:18px}
        .subtle{opacity:.9; font-weight:500}
        .container{padding:20px 24px; max-width:1200px; margin:0 auto}
        .toolbar{
            display:flex; flex-wrap:wrap; gap:12px; align-items:flex-end;
            background:#fff; border:1px solid var(--line); border-radius:12px; padding:16px;
            box-shadow:0 2px 10px rgba(17,24,39,.04);
        }
        .field{display:flex; flex-direction:column; gap:6px; min-width:180px}
        .field label{font-size:12px; color:var(--muted); text-transform:uppercase; letter-spacing:.4px}
        .text{height:36px; padding:0 10px; border:1px solid var(--line); border-radius:8px; outline:none;}
        .text:focus{border-color:var(--purple); box-shadow:0 0 0 3px rgba(108,43,217,.15)}
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
        .status{margin-top:16px; font-weight:600; color:var(--purple-d)}
        .section-card{
            margin-top:24px; background:#fff; border:1px solid var(--line); border-radius:12px;
            padding:18px 20px; box-shadow:0 2px 10px rgba(17,24,39,.04);
        }
        .section-title{margin:0 0 8px; font-size:18px}
        .subsection-title{margin:16px 0 6px; font-size:15px; color:var(--muted); text-transform:uppercase; letter-spacing:.6px}
        .item-row{padding:12px 0; border-top:1px solid var(--line)}
        .item-row:first-of-type{border-top:none}
        .item-label{font-weight:600; display:flex; gap:8px; align-items:center}
        .unit{font-size:12px; color:var(--muted); background:#F3F4F6; border-radius:999px; padding:2px 8px}
        .item-inputs{display:flex; flex-wrap:wrap; gap:16px; margin-top:10px}
        .input-group{display:flex; flex-direction:column; gap:6px}
        .input-label{font-size:12px; color:var(--muted); text-transform:uppercase; letter-spacing:.4px}
        .input-box{
            width:140px; height:36px; padding:0 10px; border:1px solid var(--line); border-radius:8px; outline:none;
        }
        .input-box:focus{border-color:var(--purple); box-shadow:0 0 0 3px rgba(108,43,217,.15)}
        .input-box.num{text-align:right}
        .nav-link{color:#fff; text-decoration:none; font-weight:600;}
        .nav-link:hover{text-decoration:underline}
        @media (max-width:720px){
            .field{min-width:140px}
            .input-box{width:120px}
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="appbar">
            <div class="brand">
                <span class="brand-mark" aria-hidden="true"></span>
                <h1>LiMove <span class="subtle">Manual Entry</span></h1>
            </div>
            <a class="nav-link" href="DP.aspx">Back to Daily Report</a>
        </div>
        <div class="container">
            <div class="toolbar">
                <div class="field">
                    <label for="txtDate">Report Date</label>
                    <asp:TextBox ID="txtDate" runat="server" CssClass="text" TextMode="Date" />
                </div>
                <div class="field" style="min-width:150px">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnLoadPrevious" runat="server" CssClass="btn ghost" Text="Load Previous Day" />
                </div>
                <div class="spacer"></div>
                <div class="field" style="min-width:160px">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnSave" runat="server" CssClass="btn primary" Text="Save" />
                </div>
                <div class="field" style="min-width:200px">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnSaveAndExport" runat="server" CssClass="btn" Text="Save &amp; Export" />
                </div>
            </div>
            <div class="status">
                <asp:Label ID="lblStatus" runat="server" />
            </div>
            <asp:PlaceHolder ID="phFields" runat="server" />
        </div>
    </form>
</body>
</html>

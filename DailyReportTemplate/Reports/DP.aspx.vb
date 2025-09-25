Imports DailyReportTemplate.BLF
Imports DailyReportTemplate.DailyReport.BLF
Imports NPOI.SS.Formula.Functions

Public Class DP
    Inherits System.Web.UI.Page

    Private repo As New ReportRepository()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' Default to today
            txtDate.Text = Date.Today.ToString("yyyy-MM-dd")
            ddlMode.SelectedValue = "PLACEHOLDER"
            BindGrid()
        End If
    End Sub

    Private Function SelectedDate() As Date
        Dim d As Date
        If Not Date.TryParse(txtDate.Text, d) Then
            d = Date.Today
            txtDate.Text = d.ToString("yyyy-MM-dd")
        End If
        Return d
    End Function

    Private Sub BindGrid()
        Dim d = SelectedDate()
        Dim dt = repo.GetFacts(d)
        gv.DataSource = dt
        gv.DataBind()

        If dt.Rows.Count = 0 Then
            lblInfo.Text = $"No data for {d:yyyy-MM-dd}. Build it first."
        Else
            lblInfo.Text = $"Loaded {dt.Rows.Count:n0} rows for {d:yyyy-MM-dd}."
        End If
    End Sub

    ' === Buttons ===


    ' Refresh

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        BindGrid()
    End Sub

    Protected Sub txtDate_TextChanged(sender As Object, e As EventArgs) Handles txtDate.TextChanged
        BindGrid()
    End Sub

    Protected Sub btnExportTemplate_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click
        Try
            Dim d = SelectedDate()
            Dim dt = repo.GetFacts(d)
            If dt.Rows.Count = 0 Then
                lblInfo.Text = $"Nothing to export for {d:yyyy-MM-dd}."
                Return
            End If
            Dim templatePath = Server.MapPath("~/Templates/DailyReportTemplate.xlsx")
            Dim bytes = TemplateExporter.ExportFromTemplate(templatePath, dt, d)
            PushDownload(bytes, $"DailyReport_{d:yyyyMMdd}.xlsx")
        Catch ex As Exception
            lblInfo.Text = "Export (template) failed: " & ex.Message
        End Try
    End Sub

    Private Sub PushDownload(bytes As Byte(), fileName As String)
        Response.Clear()
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        Response.AddHeader("Content-Disposition", $"attachment; filename=""{fileName}""")
        Response.BinaryWrite(bytes)
        Response.End()
    End Sub

    Protected Sub gv_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        gv.PageIndex = e.NewPageIndex

        ' Rebind from preview (Session) if available, else from DB
        Dim dt As DataTable = TryCast(Session("DR_PREVIEW_DT"), DataTable)
        If dt Is Nothing Then
            dt = repo.GetFacts(SelectedDate())
        End If

        gv.DataSource = dt
        gv.DataBind()
    End Sub


End Class
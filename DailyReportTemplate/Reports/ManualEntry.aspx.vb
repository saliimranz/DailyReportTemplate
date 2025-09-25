Imports System.Globalization
Imports System.Data
Imports System.Collections.Generic
Imports DailyReportTemplate.DailyReport.BLF

Public Class ManualEntry
    Inherits System.Web.UI.Page

    Private ReadOnly repo As New ReportRepository()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        modeChip.InnerText = "MANUAL ENTRY"
        If Not IsPostBack Then
            Dim d = GetInitialDate()
            txtDate.Text = d.ToString("yyyy-MM-dd")
            BindGrid(d)
        End If
    End Sub

    Private Function GetInitialDate() As Date
        Dim initial = Date.Today
        Dim qs = Request.QueryString("date")
        Dim parsed As Date
        If Not String.IsNullOrWhiteSpace(qs) AndAlso Date.TryParse(qs, parsed) Then
            initial = parsed
        End If
        Return initial
    End Function

    Private Function SelectedDate() As Date
        Dim d As Date
        If Not Date.TryParse(txtDate.Text, d) Then
            d = Date.Today
            txtDate.Text = d.ToString("yyyy-MM-dd")
        End If
        Return d
    End Function

    Private Sub BindGrid(reportDate As Date)
        Try
            Dim dt = repo.GetFacts(reportDate)
            If dt.Rows.Count = 0 Then
                repo.BuildDailyReport(reportDate, "PLACEHOLDER")
                dt = repo.GetFacts(reportDate)
            End If

            If Not dt.Columns.Contains("ValueNumDisplay") Then
                dt.Columns.Add("ValueNumDisplay", GetType(String))
            End If
            If Not dt.Columns.Contains("Oracle_C1_Display") Then
                dt.Columns.Add("Oracle_C1_Display", GetType(String))
            End If
            If Not dt.Columns.Contains("Oracle_C2_Display") Then
                dt.Columns.Add("Oracle_C2_Display", GetType(String))
            End If

            For Each row As DataRow In dt.Rows
                row("ValueNumDisplay") = FormatNullableDecimal(row, "ValueNum")
                row("Oracle_C1_Display") = FormatNullableDecimal(row, "Oracle_C1")
                row("Oracle_C2_Display") = FormatNullableDecimal(row, "Oracle_C2")
            Next

            gvManual.DataSource = dt
            gvManual.DataBind()

            If dt.Rows.Count = 0 Then
                lblInfo.CssClass = "status error"
                lblInfo.Text = $"No template rows were found for {reportDate:yyyy-MM-dd}."
            Else
                lblInfo.CssClass = "status"
                lblInfo.Text = $"Editing {dt.Rows.Count:n0} fields for {reportDate:yyyy-MM-dd}."
            End If
        Catch ex As Exception
            lblInfo.CssClass = "status error"
            lblInfo.Text = "Failed to load template: " & ex.Message
        End Try
    End Sub

    Private Shared Function FormatNullableDecimal(row As DataRow, columnName As String) As String
        If row.Table.Columns.Contains(columnName) AndAlso Not row.IsNull(columnName) Then
            Dim value As Decimal
            If Decimal.TryParse(row(columnName).ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return value.ToString("G29", CultureInfo.InvariantCulture)
            End If
        End If
        Return String.Empty
    End Function

    Protected Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        BindGrid(SelectedDate())
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim reportDate = SelectedDate()
        Dim updates As New List(Of ReportFactUpdate)()

        Try
            For Each row As GridViewRow In gvManual.Rows
                If row.RowType <> DataControlRowType.DataRow Then Continue For

                Dim section = GetHiddenValue(row, "hfSection")
                Dim subSection = GetHiddenValue(row, "hfSubSection")
                Dim item = GetHiddenValue(row, "hfItem")
                Dim measureGroup = GetHiddenValue(row, "hfMeasureGroup")
                Dim measureName = GetHiddenValue(row, "hfMeasureName")

                Dim valueNum = ParseNullableDecimal(TryCast(row.FindControl("txtValueNum"), TextBox))
                Dim valueText = GetTextValue(row, "txtValueText")
                Dim remarks = GetTextValue(row, "txtRemarks")
                Dim oracleC1 = ParseNullableDecimal(TryCast(row.FindControl("txtOracleC1"), TextBox))
                Dim oracleC2 = ParseNullableDecimal(TryCast(row.FindControl("txtOracleC2"), TextBox))

                Dim updateRow As New ReportFactUpdate() With {
                    .ReportDate = reportDate,
                    .Section = section,
                    .SubSection = subSection,
                    .Item = item,
                    .MeasureGroup = measureGroup,
                    .MeasureName = measureName,
                    .ValueNum = valueNum,
                    .ValueText = If(String.IsNullOrWhiteSpace(valueText), Nothing, valueText.Trim()),
                    .Remarks = If(String.IsNullOrWhiteSpace(remarks), Nothing, remarks.Trim()),
                    .OracleC1 = oracleC1,
                    .OracleC2 = oracleC2
                }

                updates.Add(updateRow)
            Next

            If updates.Count = 0 Then
                lblInfo.CssClass = "status error"
                lblInfo.Text = "Nothing to save."
                Return
            End If

            repo.SaveManualReport(reportDate, updates)
            Response.Redirect($"DP.aspx?date={reportDate:yyyy-MM-dd}")
        Catch ex As Exception
            lblInfo.CssClass = "status error"
            lblInfo.Text = "Failed to save report: " & ex.Message
        End Try
    End Sub

    Protected Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim d = SelectedDate()
        Response.Redirect($"DP.aspx?date={d:yyyy-MM-dd}")
    End Sub

    Private Shared Function GetHiddenValue(row As GridViewRow, controlId As String) As String
        Dim ctrl = TryCast(row.FindControl(controlId), HiddenField)
        If ctrl IsNot Nothing Then
            Return ctrl.Value
        End If
        Return String.Empty
    End Function

    Private Shared Function GetTextValue(row As GridViewRow, controlId As String) As String
        Dim ctrl = TryCast(row.FindControl(controlId), TextBox)
        If ctrl IsNot Nothing Then
            Return ctrl.Text
        End If
        Return String.Empty
    End Function

    Private Shared Function ParseNullableDecimal(ctrl As TextBox) As Decimal?
        If ctrl Is Nothing Then Return Nothing
        Dim input = ctrl.Text
        If String.IsNullOrWhiteSpace(input) Then Return Nothing

        Dim value As Decimal
        If Decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
            Return value
        End If

        If Decimal.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, value) Then
            Return value
        End If

        Throw New FormatException($"Value '{input}' is not a valid number.")
    End Function

    Protected Sub gvManual_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gvManual.PageIndexChanging
        gvManual.PageIndex = e.NewPageIndex
        BindGrid(SelectedDate())
    End Sub

End Class

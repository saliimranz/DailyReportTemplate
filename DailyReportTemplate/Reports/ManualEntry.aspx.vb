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
            Dim existing = repo.GetFacts(reportDate)
            Dim template = repo.GetTemplateFacts(reportDate)

            Dim dt As DataTable = Nothing
            Dim usedTemplate As Boolean = False

            If template IsNot Nothing AndAlso template.Rows.Count > 0 Then
                dt = template
                If existing IsNot Nothing AndAlso existing.Rows.Count > 0 Then
                    MergeExistingValues(dt, existing)
                End If
                usedTemplate = True
            Else
                dt = existing
            End If

            EnsureKeyColumns(dt)

            If dt IsNot Nothing Then
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
                    NormalizePickupRow(row)
                    row("ValueNumDisplay") = FormatNullableDecimal(row, "ValueNum")
                    row("Oracle_C1_Display") = FormatNullableDecimal(row, "Oracle_C1")
                    row("Oracle_C2_Display") = FormatNullableDecimal(row, "Oracle_C2")
                Next
            End If

            gvManual.DataSource = dt
            gvManual.DataBind()

            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                lblInfo.CssClass = "status error"
                lblInfo.Text = $"No template rows were found for {reportDate:yyyy-MM-dd}."
            Else
                lblInfo.CssClass = "status"
                If existing IsNot Nothing AndAlso existing.Rows.Count > 0 Then
                    lblInfo.Text = $"Editing {dt.Rows.Count:n0} fields for {reportDate:yyyy-MM-dd}. {existing.Rows.Count:n0} rows already saved."
                ElseIf usedTemplate Then
                    lblInfo.Text = $"Loaded template for {reportDate:yyyy-MM-dd}. Enter values and click Save."
                Else
                    lblInfo.Text = $"Loaded {dt.Rows.Count:n0} fields for {reportDate:yyyy-MM-dd}."
                End If
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

    Private Shared Sub EnsureKeyColumns(table As DataTable)
        If table Is Nothing Then Return

        Dim required = New String() {"SectionKey", "SubSectionKey", "ItemKey", "MeasureGroupKey", "MeasureNameKey"}

        For Each name In required
            If Not table.Columns.Contains(name) Then
                table.Columns.Add(name, GetType(String))
            End If
        Next

        For Each row As DataRow In table.Rows
            row("SectionKey") = row("Section").ToString()
            row("SubSectionKey") = row("SubSection").ToString()
            row("ItemKey") = row("Item").ToString()
            row("MeasureGroupKey") = row("MeasureGroup").ToString()
            row("MeasureNameKey") = row("MeasureName").ToString()
        Next
    End Sub

    Private Shared Sub MergeExistingValues(template As DataTable, existing As DataTable)
        If template Is Nothing OrElse existing Is Nothing Then Return

        Dim lookup As New Dictionary(Of String, DataRow)(StringComparer.OrdinalIgnoreCase)

        For Each existingRow As DataRow In existing.Rows
            Dim key = BuildRowKey(existingRow)
            If Not lookup.ContainsKey(key) Then
                lookup.Add(key, existingRow)
            End If
        Next

        For Each templateRow As DataRow In template.Rows
            Dim key = BuildRowKey(templateRow)
            Dim match As DataRow = Nothing
            If lookup.TryGetValue(key, match) Then
                CopyColumnIfExists(templateRow, match, "ValueNum")
                CopyColumnIfExists(templateRow, match, "ValueText")
                CopyColumnIfExists(templateRow, match, "Remarks")
                CopyColumnIfExists(templateRow, match, "Oracle_C1")
                CopyColumnIfExists(templateRow, match, "Oracle_C2")
                CopyColumnIfExists(templateRow, match, "Unit")
                CopyColumnIfExists(templateRow, match, "SortSection")
                CopyColumnIfExists(templateRow, match, "SortSubSection")
                CopyColumnIfExists(templateRow, match, "SortItem")
                CopyColumnIfExists(templateRow, match, "SortMeasure")
            End If
        Next
    End Sub

    Private Shared Sub CopyColumnIfExists(target As DataRow, source As DataRow, columnName As String)
        If target Is Nothing OrElse source Is Nothing Then Return
        If target.Table.Columns.Contains(columnName) AndAlso source.Table.Columns.Contains(columnName) Then
            target(columnName) = source(columnName)
        End If
    End Sub

    Private Shared Function BuildRowKey(row As DataRow) As String
        If row Is Nothing Then Return String.Empty

        Dim parts = New String() {
            SafeDataRowString(row, "Section"),
            SafeDataRowString(row, "SubSection"),
            SafeDataRowString(row, "Item"),
            SafeDataRowString(row, "MeasureGroup"),
            SafeDataRowString(row, "MeasureName")
        }

        Return String.Join("|", parts)
    End Function

    Private Shared Function SafeDataRowString(row As DataRow, columnName As String) As String
        If row.Table.Columns.Contains(columnName) AndAlso Not row.IsNull(columnName) Then
            Return row(columnName).ToString()
        End If
        Return String.Empty
    End Function

    Private Shared Sub NormalizePickupRow(row As DataRow)
        If row Is Nothing Then Return

        Dim itemValue = row("Item").ToString()
        If Not String.Equals(itemValue, "01TonPickup", StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        row("Item") = "01Ton Pickup"

        If row.Table.Columns.Contains("SubSection") Then
            row("SubSection") = "Pickup Loading"
        End If

        Dim measureGroup = row("MeasureGroup").ToString()
        Dim measureName = row("MeasureName").ToString()

        Dim preferred As String = Nothing
        If Not String.IsNullOrWhiteSpace(measureGroup) Then
            preferred = measureGroup
        ElseIf Not String.IsNullOrWhiteSpace(measureName) Then
            preferred = measureName
        End If

        If String.IsNullOrWhiteSpace(preferred) Then
            preferred = measureName
        End If

        If Not String.IsNullOrWhiteSpace(preferred) Then
            row("MeasureGroup") = preferred
            row("MeasureName") = preferred
        End If
    End Sub

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
                Dim unit = GetHiddenValue(row, "hfUnit")
                Dim sortSection = ParseNullableInt(row, "hfSortSection")
                Dim sortSubSection = ParseNullableInt(row, "hfSortSubSection")
                Dim sortItem = ParseNullableInt(row, "hfSortItem")
                Dim sortMeasure = ParseNullableInt(row, "hfSortMeasure")
                Dim valueText = GetHiddenValue(row, "hfValueText")

                Dim valueNum = ParseNullableDecimal(TryCast(row.FindControl("txtValueNum"), TextBox))
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
                    .ValueText = NullIfWhiteSpace(valueText),
                    .Remarks = NullIfWhiteSpace(remarks),
                    .OracleC1 = oracleC1,
                    .OracleC2 = oracleC2,
                    .Unit = NullIfWhiteSpace(unit),
                    .SortSection = sortSection,
                    .SortSubSection = sortSubSection,
                    .SortItem = sortItem,
                    .SortMeasure = sortMeasure
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

    Private Shared Function ParseNullableInt(row As GridViewRow, controlId As String) As Integer?
        Dim raw = GetHiddenValue(row, controlId)
        If String.IsNullOrWhiteSpace(raw) Then Return Nothing

        Dim value As Integer
        If Integer.TryParse(raw, value) Then
            Return value
        End If

        Return Nothing
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

    Private Shared Function NullIfWhiteSpace(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Return value.Trim()
    End Function

End Class

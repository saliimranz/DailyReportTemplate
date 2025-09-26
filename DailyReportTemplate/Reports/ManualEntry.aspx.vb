Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports DailyReportTemplate.BLF
Imports DailyReportTemplate.DailyReport.BLF
Imports System.Web.UI.WebControls

Partial Public Class ManualEntry
    Inherits System.Web.UI.Page

    Private ReadOnly repo As New ReportRepository()
    Private fieldDefs As List(Of DailyReportFieldDefinition)
    Private inputControls As Dictionary(Of String, TextBox)

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Init
        fieldDefs = DailyReportFieldCatalog.GetFields()
        inputControls = New Dictionary(Of String, TextBox)(StringComparer.OrdinalIgnoreCase)
        BuildDynamicForm()
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Dim requested = RequestedDateFromQuery()
            Dim initial = If(requested.HasValue, requested.Value, Date.Today)
            txtDate.Text = initial.ToString("yyyy-MM-dd")
            LoadValuesForDate(initial)
        End If
    End Sub

    Private Function RequestedDateFromQuery() As Nullable(Of Date)
        Dim raw = Request.QueryString("date")
        Dim parsed As Date
        If Not String.IsNullOrWhiteSpace(raw) AndAlso Date.TryParse(raw, parsed) Then
            Return parsed
        End If
        Return Nothing
    End Function

    Private Function SelectedDate() As Date
        Dim d As Date
        If Not Date.TryParse(txtDate.Text, d) Then
            d = Date.Today
            txtDate.Text = d.ToString("yyyy-MM-dd")
        End If
        Return d
    End Function

    Private Sub BuildDynamicForm()
        phFields.Controls.Clear()
        Dim sections = fieldDefs.OrderBy(Function(f) f.SortSection).GroupBy(Function(f) f.Section)

        For Each secGroup In sections
            Dim sectionPanel As New Panel With {.CssClass = "section-card"}
            sectionPanel.Controls.Add(New LiteralControl("<h2 class='section-title'>" & FriendlySection(secGroup.Key) & "</h2>"))

            Dim subGroups = secGroup.OrderBy(Function(f) f.SortSubSection).GroupBy(Function(f) f.SubSection)
            For Each subGroup In subGroups
                sectionPanel.Controls.Add(New LiteralControl("<h3 class='subsection-title'>" & FriendlySubSection(secGroup.Key, subGroup.Key) & "</h3>"))

                Dim itemGroups = subGroup.OrderBy(Function(f) f.SortItem).GroupBy(Function(f) f.Item)
                For Each itemGroup In itemGroups
                    sectionPanel.Controls.Add(BuildItemBlock(secGroup.Key, subGroup.Key, itemGroup.ToList()))
                Next
            Next

            phFields.Controls.Add(sectionPanel)
        Next
    End Sub

    Private Function BuildItemBlock(sectionKey As String, subKey As String, fieldsForItem As List(Of DailyReportFieldDefinition)) As Control
        Dim wrapper As New Panel With {.CssClass = "item-row"}
        Dim labelText = fieldsForItem.First().DisplayLabel
        Dim unitText = fieldsForItem.First().Unit
        Dim unitMarkup = If(String.IsNullOrWhiteSpace(unitText), String.Empty, "<span class='unit'>" & unitText & "</span>")
        wrapper.Controls.Add(New LiteralControl("<div class='item-label'>" & labelText & unitMarkup & "</div>"))

        Dim inputsWrap As New Panel With {.CssClass = "item-inputs"}
        For Each field In fieldsForItem.OrderBy(Function(f) f.SortMeasure)
            Dim inputWrap As New Panel With {.CssClass = "input-group"}
            inputWrap.Controls.Add(New LiteralControl("<label class='input-label'>" & FriendlyMeasure(field) & "</label>"))

            Dim tb As New TextBox With {
                .ID = "fld_" & field.Key,
                .CssClass = If(field.InputType = "number", "input-box num", "input-box"),
                .ClientIDMode = ClientIDMode.Static
            }
            tb.Attributes("autocomplete") = "off"
            If field.InputType = "number" Then
                tb.Attributes("type") = "number"
                tb.Attributes("inputmode") = "decimal"
                tb.Attributes("step") = "any"
            End If

            inputControls(field.Key) = tb
            inputWrap.Controls.Add(tb)
            inputsWrap.Controls.Add(inputWrap)
        Next
        wrapper.Controls.Add(inputsWrap)
        Return wrapper
    End Function

    Private Sub LoadValuesForDate(reportDate As Date)
        Dim dict = repo.GetFactsAsDictionary(reportDate)
        ApplyValues(dict)
        If dict.Count = 0 Then
            lblStatus.Text = "No saved values for " & reportDate.ToString("yyyy-MM-dd") & "."
        Else
            lblStatus.Text = "Loaded " & dict.Count.ToString(CultureInfo.InvariantCulture) & " values for " & reportDate.ToString("yyyy-MM-dd") & "."
        End If
    End Sub

    Private Sub ApplyValues(values As IDictionary(Of Tuple(Of String, String, String, String, String), DailyReportFactValue))
        For Each tb In inputControls.Values
            tb.Text = String.Empty
        Next
        If values Is Nothing Then Return

        For Each field In fieldDefs
            Dim key As Tuple(Of String, String, String, String, String) = Tuple.Create(
                field.Section,
                If(field.SubSection, String.Empty),
                If(field.Item, String.Empty),
                field.MeasureGroup,
                field.MeasureName)
            Dim dto As DailyReportFactValue = Nothing
            If values.TryGetValue(key, dto) AndAlso dto IsNot Nothing Then
                Dim tb = inputControls(field.Key)
                If field.UseValueNum Then
                    If dto.ValueNum.HasValue Then
                        tb.Text = dto.ValueNum.Value.ToString("0.##", CultureInfo.InvariantCulture)
                    Else
                        tb.Text = String.Empty
                    End If
                Else
                    tb.Text = If(dto.ValueText, String.Empty)
                End If
            End If
        Next
    End Sub

    Protected Sub txtDate_TextChanged(sender As Object, e As EventArgs) Handles txtDate.TextChanged
        LoadValuesForDate(SelectedDate())
    End Sub

    Protected Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        LoadValuesForDate(SelectedDate())
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        SaveInternal(exportAfter:=False)
    End Sub

    Protected Sub btnSaveAndExport_Click(sender As Object, e As EventArgs) Handles btnSaveAndExport.Click
        SaveInternal(exportAfter:=True)
    End Sub

    Private Sub SaveInternal(exportAfter As Boolean)
        Dim selected = SelectedDate()
        Dim errors As New List(Of String)()
        Dim result As New List(Of DailyReportFactValue)()

        For Each field In fieldDefs
            Dim tb = inputControls(field.Key)
            Dim raw = tb.Text
            Dim dto As New DailyReportFactValue With {
                .ReportDate = selected,
                .Section = field.Section,
                .SubSection = field.SubSection,
                .Item = field.Item,
                .MeasureGroup = field.MeasureGroup,
                .MeasureName = field.MeasureName,
                .Unit = field.Unit,
                .SortSection = field.SortSection,
                .SortSubSection = field.SortSubSection,
                .SortItem = field.SortItem,
                .SortMeasure = field.SortMeasure
            }

            If field.UseValueNum Then
                Dim trimmed = If(raw, String.Empty).Trim()
                If trimmed.Length = 0 Then
                    dto.ValueNum = Nothing
                Else
                    Dim dec As Decimal
                    If Decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, dec) Then
                        dto.ValueNum = dec
                    Else
                        errors.Add(FriendlySection(field.Section) & " / " & FriendlySubSection(field.Section, field.SubSection) & " / " & field.DisplayLabel & " (" & FriendlyMeasure(field) & ") must be numeric.")
                    End If
                End If
            Else
                dto.ValueText = If(raw, String.Empty).Trim()
            End If

            result.Add(dto)
        Next

        If errors.Count > 0 Then
            lblStatus.Text = String.Join("<br/>", errors)
            Return
        End If

        Try
            repo.ReplaceFacts(selected, result)
            If exportAfter Then
                Dim dt = repo.GetFacts(selected)
                Dim templatePath = Server.MapPath("~/Templates/DailyReportTemplate.xlsx")
                Dim bytes = TemplateExporter.ExportFromTemplate(templatePath, dt, selected)
                PushDownload(bytes, "DailyReport_" & selected.ToString("yyyyMMdd") & ".xlsx")
                Return
            Else
                LoadValuesForDate(selected)
                lblStatus.Text = "Saved " & result.Count.ToString(CultureInfo.InvariantCulture) & " values for " & selected.ToString("yyyy-MM-dd") & "."
            End If
        Catch ex As Exception
            lblStatus.Text = "Save failed: " & ex.Message
        End Try
    End Sub

    Private Sub PushDownload(bytes As Byte(), fileName As String)
        Response.Clear()
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        Response.AddHeader("Content-Disposition", "attachment; filename=\"" & fileName & " \ "")
        Response.BinaryWrite(bytes)
        Response.Flush()
        Context.ApplicationInstance.CompleteRequest()
    End Sub

    Private Shared Function FriendlySection(section As String) As String
        Select Case section
            Case "GoodsReceiving" : Return "Goods Receiving"
            Case "PickingDelivery" : Return "Picking & Delivery"
            Case "Storage" : Return "Storage"
            Case "UsedBatteries" : Return "Used Batteries"
            Case "Others" : Return "Others"
            Case Else : Return section
        End Select
    End Function

    Private Shared Function FriendlySubSection(section As String, subSection As String) As String
        Select Case section
            Case "GoodsReceiving"
                Select Case subSection
                    Case "AutoPartsContainers" : Return "Auto Parts Containers"
                    Case "TireContainers" : Return "Tire Containers"
                    Case "BatteriesContainers" : Return "Batteries Containers"
                    Case "LubeContainers" : Return "Lube Containers"
                End Select
            Case "PickingDelivery"
                Select Case subSection
                    Case "Picking" : Return "Picking"
                    Case "MTD" : Return "MTD"
                    Case "Packing" : Return "Packing"
                    Case "Labelling/Special Marketing" : Return "Labelling / Special Marketing"
                    Case "TotalOrders" : Return "Total Orders"
                End Select
            Case "Storage"
                Select Case subSection
                    Case "Palletizing" : Return "Palletizing"
                    Case "Auto Parts" : Return "Auto Parts"
                    Case "Battery Testing & Charging: In Coming Shipment" : Return "Battery Testing & Charging: Incoming Shipment"
                    Case "In Stock (Battery)" : Return "In Stock (Battery)"
                End Select
            Case "UsedBatteries"
                If subSection = "Used Batteries Activities" Then Return "Used Batteries Activities"
            Case "Others"
                Select Case subSection
                    Case "Pickup Loading" : Return "Pickup Loading"
                    Case "Consignment Receiving (Offday/After Off)" : Return "Consignment Receiving (Offday/After Off)"
                End Select
        End Select
        Return subSection
    End Function

    Private Shared Function FriendlyMeasure(field As DailyReportFieldDefinition) As String
        Select Case field.MeasureGroup
            Case "QTY"
                Return "Qty"
            Case "MTD"
                Return "MTD"
            Case "Line"
                Return "Line"
            Case "Carton"
                Return "Carton"
            Case "Loose"
                Return "Loose"
            Case "Pallet"
                Return "Pallet"
            Case "TotalPickingOrders"
                Return "Total Picking Orders"
            Case "TotalMTDOrders"
                Return "Total MTD Orders"
            Case Else
                Return field.MeasureName
        End Select
    End Function

End Class

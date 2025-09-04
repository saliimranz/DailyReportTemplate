Imports NPOI.SS.UserModel
Imports NPOI.XSSF.UserModel
Imports System.Data
Imports System.IO

Namespace BLF
    Public Class TemplateExporter

        Public Shared Function ExportFromTemplate(templatePath As String,
                                                  dt As DataTable,
                                                  reportDate As Date) As Byte()
            Using fs As New FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim wb As IWorkbook = New XSSFWorkbook(fs)

                ' Optional: write date if you created a name HDR_Date
                Try
                    Dim loc = GetCellByDefinedName(wb, "Date")
                    If loc IsNot Nothing Then
                        loc.Item2.SetCellValue(reportDate.ToString("dd/MM/yy"))
                    End If
                Catch
                End Try

                ' Iterate facts and write values
                For Each r As DataRow In dt.Rows
                    Dim section = SafeStr(r, "Section")
                    Dim subsec = SafeStr(r, "SubSection")
                    Dim item = SafeStr(r, "Item")
                    Dim grp = SafeStr(r, "MeasureGroup")
                    Dim meas = SafeStr(r, "MeasureName")
                    Dim name = TemplateMap.BuildName(section, subsec, item, grp, meas)

                    Dim loc = GetCellByDefinedName(wb, name)
                    If loc Is Nothing Then
                        ' name not found in template → skip
                        Continue For
                    End If

                    ' Prefer numeric value
                    If Not r.IsNull("ValueNum") Then
                        loc.Item2.SetCellValue(Convert.ToDouble(r("ValueNum")))
                    ElseIf Not r.IsNull("ValueText") Then
                        loc.Item2.SetCellValue(r("ValueText").ToString())
                    Else
                        loc.Item2.SetCellValue("")
                    End If
                Next

                Using ms As New MemoryStream()
                    wb.Write(ms)
                    Return ms.ToArray()
                End Using
            End Using
        End Function

        Private Shared Function SafeStr(dr As DataRow, col As String) As String
            If dr Is Nothing OrElse Not dr.Table.Columns.Contains(col) OrElse dr.IsNull(col) Then Return ""
            Return dr(col).ToString()
        End Function

        ' Returns (sheet, cell) for a single-cell defined name
        Private Shared Function GetCellByDefinedName(wb As IWorkbook, definedName As String) As Tuple(Of ISheet, ICell)
            Dim nm = wb.GetName(definedName)
            If nm Is Nothing Then Return Nothing

            Dim ref = nm.RefersToFormula
            If String.IsNullOrWhiteSpace(ref) Then Return Nothing

            ' Parse the name reference, e.g. 'Report'!$C$18 or a small area like 'Report'!$C$18:$C$18
            Dim ar = New NPOI.SS.Util.AreaReference(ref, wb.SpreadsheetVersion())
            Dim fr = ar.FirstCell

            Dim sh = wb.GetSheet(fr.SheetName)
            If sh Is Nothing Then Return Nothing

            Dim row = sh.GetRow(fr.Row)
            If row Is Nothing Then row = sh.CreateRow(fr.Row)

            Dim cell = row.GetCell(fr.Col)
            If cell Is Nothing Then cell = row.CreateCell(fr.Col)

            Return Tuple.Create(sh, cell)
        End Function

    End Class
End Namespace

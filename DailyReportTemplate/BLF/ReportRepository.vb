Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic


Namespace DailyReport.BLF
    Public Class ReportRepository
        Private ReadOnly _cs As String
        Public Sub New()
            _cs = System.Configuration.ConfigurationManager.ConnectionStrings("DBCS").ConnectionString
        End Sub

        Public Sub BuildDailyReport(reportDate As Date)
            Using con As New SqlConnection(_cs)
                Using cmd As New SqlCommand("dbo.BuildDailyReport", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ReportDate", reportDate)
                    con.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        Public Function GetFacts(reportDate As Date) As DataTable
            Dim dt As New DataTable()
            Using con As New SqlConnection(_cs)
                Using cmd As New SqlCommand("
SELECT ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit, Remarks, Oracle_C1, Oracle_C2
FROM dbo.DailyReportFact
WHERE ReportDate = @d
ORDER BY SortSection, SortSubSection, SortItem, SortMeasure;", con)
                    cmd.Parameters.AddWithValue("@d", reportDate)
                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
            End Using
            Return dt
        End Function
        ' App_Code/ReportRepository.vb
        Public Sub BuildDailyReport(reportDate As Date, Optional mode As String = "PLACEHOLDER")
            Using con As New SqlConnection(_cs)
                Using cmd As New SqlCommand("dbo.BuildDailyReport", con)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ReportDate", reportDate)
                    cmd.Parameters.AddWithValue("@Mode", mode)
                    con.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        Public Sub SaveManualReport(reportDate As Date, rows As IEnumerable(Of ReportFactUpdate))
            Using con As New SqlConnection(_cs)
                con.Open()

                For Each row In rows
                    Using cmd As New SqlCommand("UPDATE dbo.DailyReportFact SET ValueNum = @ValueNum, ValueText = @ValueText, Remarks = @Remarks, Oracle_C1 = @OracleC1, Oracle_C2 = @OracleC2 WHERE ReportDate = @ReportDate AND Section = @Section AND SubSection = @SubSection AND Item = @Item AND MeasureGroup = @MeasureGroup AND MeasureName = @MeasureName;", con)
                        cmd.Parameters.AddWithValue("@ReportDate", reportDate)
                        cmd.Parameters.AddWithValue("@Section", row.Section)
                        cmd.Parameters.AddWithValue("@SubSection", row.SubSection)
                        cmd.Parameters.AddWithValue("@Item", row.Item)
                        cmd.Parameters.AddWithValue("@MeasureGroup", row.MeasureGroup)
                        cmd.Parameters.AddWithValue("@MeasureName", row.MeasureName)

                        cmd.Parameters.AddWithValue("@ValueNum", If(row.ValueNum.HasValue, CType(row.ValueNum.Value, Object), DBNull.Value))
                        cmd.Parameters.AddWithValue("@ValueText", If(row.ValueText IsNot Nothing, CType(row.ValueText, Object), DBNull.Value))
                        cmd.Parameters.AddWithValue("@Remarks", If(row.Remarks IsNot Nothing, CType(row.Remarks, Object), DBNull.Value))
                        cmd.Parameters.AddWithValue("@OracleC1", If(row.OracleC1.HasValue, CType(row.OracleC1.Value, Object), DBNull.Value))
                        cmd.Parameters.AddWithValue("@OracleC2", If(row.OracleC2.HasValue, CType(row.OracleC2.Value, Object), DBNull.Value))

                        Dim affected = cmd.ExecuteNonQuery()
                        If affected = 0 Then
                            Throw New InvalidOperationException($"Template row not found for {row.Section}/{row.SubSection}/{row.Item}/{row.MeasureGroup}/{row.MeasureName} on {reportDate:yyyy-MM-dd}.")
                        End If
                    End Using
                Next
            End Using
        End Sub
    End Class
End Namespace

Imports System.Data
Imports System.Data.SqlClient


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
SELECT ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit
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
    End Class
End Namespace

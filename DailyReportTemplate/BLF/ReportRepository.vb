Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Namespace DailyReport.BLF

    Public Class DailyReportFactValue
        Public Property ReportDate As Date
        Public Property Section As String
        Public Property SubSection As String
        Public Property Item As String
        Public Property MeasureGroup As String
        Public Property MeasureName As String
        Public Property ValueNum As Decimal?
        Public Property ValueText As String
        Public Property Unit As String
        Public Property SortSection As Integer
        Public Property SortSubSection As Integer
        Public Property SortItem As Integer
        Public Property SortMeasure As Integer

        Public Function KeyTuple() As Tuple(Of String, String, String, String, String)
            Return Tuple.Create(Section, SubSection, Item, MeasureGroup, MeasureName)
        End Function
    End Class

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
                Using cmd As New SqlCommand("SELECT ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit FROM dbo.DailyReportFact WHERE ReportDate = @d ORDER BY SortSection, SortSubSection, SortItem, SortMeasure;", con)
                    cmd.Parameters.AddWithValue("@d", reportDate)
                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
            End Using
            Return dt
        End Function

        Public Function GetFactsAsDictionary(reportDate As Date) As Dictionary(Of Tuple(Of String, String, String, String, String), DailyReportFactValue)
            Dim result As New Dictionary(Of Tuple(Of String, String, String, String, String), DailyReportFactValue)()
            Using con As New SqlConnection(_cs)
                Using cmd As New SqlCommand("SELECT ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit, SortSection, SortSubSection, SortItem, SortMeasure FROM dbo.DailyReportFact WHERE ReportDate = @d", con)
                    cmd.Parameters.AddWithValue("@d", reportDate)
                    con.Open()
                    Using rdr = cmd.ExecuteReader()
                        While rdr.Read()
                            Dim dto As New DailyReportFactValue With {
                                .ReportDate = reportDate,
                                .Section = rdr("Section").ToString(),
                                .SubSection = rdr("SubSection").ToString(),
                                .Item = rdr("Item").ToString(),
                                .MeasureGroup = rdr("MeasureGroup").ToString(),
                                .MeasureName = rdr("MeasureName").ToString(),
                                .Unit = If(rdr("Unit") Is DBNull.Value, String.Empty, rdr("Unit").ToString()),
                                .SortSection = If(rdr("SortSection") Is DBNull.Value, 0, Convert.ToInt32(rdr("SortSection"))),
                                .SortSubSection = If(rdr("SortSubSection") Is DBNull.Value, 0, Convert.ToInt32(rdr("SortSubSection"))),
                                .SortItem = If(rdr("SortItem") Is DBNull.Value, 0, Convert.ToInt32(rdr("SortItem"))),
                                .SortMeasure = If(rdr("SortMeasure") Is DBNull.Value, 0, Convert.ToInt32(rdr("SortMeasure")))
                            }
                            If rdr("ValueNum") IsNot DBNull.Value Then
                                dto.ValueNum = Convert.ToDecimal(rdr("ValueNum"))
                            End If
                            If rdr("ValueText") IsNot DBNull.Value Then
                                dto.ValueText = rdr("ValueText").ToString()
                            End If
                            result(dto.KeyTuple()) = dto
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Public Sub ReplaceFacts(reportDate As Date, values As IEnumerable(Of DailyReportFactValue))
            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))

            Using con As New SqlConnection(_cs)
                con.Open()
                Using tran = con.BeginTransaction()
                    Using delCmd As New SqlCommand("DELETE FROM dbo.DailyReportFact WHERE ReportDate = @d", con, tran)
                        delCmd.Parameters.Add("@d", SqlDbType.Date).Value = reportDate
                        delCmd.ExecuteNonQuery()
                    End Using

                    Using insCmd As New SqlCommand("INSERT INTO dbo.DailyReportFact (ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit, SortSection, SortSubSection, SortItem, SortMeasure) VALUES (@ReportDate, @Section, @SubSection, @Item, @MeasureGroup, @MeasureName, @ValueNum, @ValueText, @Unit, @SortSection, @SortSubSection, @SortItem, @SortMeasure)", con, tran)
                        insCmd.Parameters.Add("@ReportDate", SqlDbType.Date)
                        insCmd.Parameters.Add("@Section", SqlDbType.VarChar, 40)
                        insCmd.Parameters.Add("@SubSection", SqlDbType.VarChar, 60)
                        insCmd.Parameters.Add("@Item", SqlDbType.VarChar, 120)
                        insCmd.Parameters.Add("@MeasureGroup", SqlDbType.VarChar, 40)
                        insCmd.Parameters.Add("@MeasureName", SqlDbType.VarChar, 40)
                        insCmd.Parameters.Add("@ValueNum", SqlDbType.Decimal).Precision = 18
                        insCmd.Parameters("@ValueNum").Scale = 4
                        insCmd.Parameters.Add("@ValueText", SqlDbType.NVarChar, 200)
                        insCmd.Parameters.Add("@Unit", SqlDbType.VarChar, 20)
                        insCmd.Parameters.Add("@SortSection", SqlDbType.Int)
                        insCmd.Parameters.Add("@SortSubSection", SqlDbType.Int)
                        insCmd.Parameters.Add("@SortItem", SqlDbType.Int)
                        insCmd.Parameters.Add("@SortMeasure", SqlDbType.Int)

                        For Each v In values
                            If v Is Nothing Then Continue For
                            insCmd.Parameters("@ReportDate").Value = reportDate
                            insCmd.Parameters("@Section").Value = v.Section
                            insCmd.Parameters("@SubSection").Value = v.SubSection
                            insCmd.Parameters("@Item").Value = v.Item
                            insCmd.Parameters("@MeasureGroup").Value = v.MeasureGroup
                            insCmd.Parameters("@MeasureName").Value = v.MeasureName
                            If v.ValueNum.HasValue Then
                                insCmd.Parameters("@ValueNum").Value = v.ValueNum.Value
                                insCmd.Parameters("@ValueText").Value = DBNull.Value
                            Else
                                insCmd.Parameters("@ValueNum").Value = DBNull.Value
                                insCmd.Parameters("@ValueText").Value = If(String.IsNullOrWhiteSpace(v.ValueText), CType(DBNull.Value, Object), v.ValueText)
                            End If
                            insCmd.Parameters("@Unit").Value = If(String.IsNullOrWhiteSpace(v.Unit), CType(DBNull.Value, Object), v.Unit)
                            insCmd.Parameters("@SortSection").Value = v.SortSection
                            insCmd.Parameters("@SortSubSection").Value = v.SortSubSection
                            insCmd.Parameters("@SortItem").Value = v.SortItem
                            insCmd.Parameters("@SortMeasure").Value = v.SortMeasure
                            insCmd.ExecuteNonQuery()
                        Next
                    End Using

                    tran.Commit()
                End Using
            End Using
        End Sub

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

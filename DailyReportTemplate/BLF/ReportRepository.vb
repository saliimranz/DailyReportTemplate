Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic

Namespace DailyReport.BLF
    Public Class ReportRepository
        Private ReadOnly _cs As String
        Private Const FactsQuery As String = "SELECT ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit, Remarks, Oracle_C1, Oracle_C2, SortSection, SortSubSection, SortItem, SortMeasure" & vbCrLf &
                                            "FROM dbo.DailyReportFact" & vbCrLf &
                                            "WHERE ReportDate = @d" & vbCrLf &
                                            "ORDER BY SortSection, SortSubSection, SortItem, SortMeasure;"

        Public Sub New()
            _cs = System.Configuration.ConfigurationManager.ConnectionStrings("DBCS").ConnectionString
        End Sub

        Public Sub BuildDailyReport(reportDate As Date)
            BuildDailyReport(reportDate, "PLACEHOLDER")
        End Sub

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

        Public Function GetFacts(reportDate As Date) As DataTable
            Dim dt As New DataTable()
            Using con As New SqlConnection(_cs)
                Using cmd As New SqlCommand(FactsQuery, con)
                    cmd.Parameters.AddWithValue("@d", reportDate)
                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
            End Using
            Return dt
        End Function

        Public Function GetTemplateFacts(reportDate As Date) As DataTable
            Dim dt = LoadFactsViaBuilder(reportDate, "COPY_PREV")

            If dt.Rows.Count = 0 Then
                dt = LoadFactsViaBuilder(reportDate, "PLACEHOLDER")
            End If

            ClearEditableColumns(dt)

            Return dt
        End Function

        Private Function LoadFactsViaBuilder(reportDate As Date, mode As String) As DataTable
            Dim dt As New DataTable()

            Using con As New SqlConnection(_cs)
                con.Open()

                Using tx = con.BeginTransaction(IsolationLevel.ReadCommitted)
                    Try
                        Using buildCmd As New SqlCommand("dbo.BuildDailyReport", con, tx)
                            buildCmd.CommandType = CommandType.StoredProcedure
                            buildCmd.Parameters.AddWithValue("@ReportDate", reportDate)
                            buildCmd.Parameters.AddWithValue("@Mode", mode)
                            buildCmd.ExecuteNonQuery()
                        End Using

                        Using selectCmd As New SqlCommand(FactsQuery, con, tx)
                            selectCmd.Parameters.AddWithValue("@d", reportDate)
                            Using da As New SqlDataAdapter(selectCmd)
                                da.Fill(dt)
                            End Using
                        End Using

                        tx.Rollback()
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            Return dt
        End Function

        Private Shared Sub ClearEditableColumns(table As DataTable)
            If table Is Nothing Then Return

            For Each row As DataRow In table.Rows
                If table.Columns.Contains("ValueNum") Then row("ValueNum") = DBNull.Value
                If table.Columns.Contains("ValueText") Then row("ValueText") = DBNull.Value
                If table.Columns.Contains("Remarks") Then row("Remarks") = DBNull.Value
                If table.Columns.Contains("Oracle_C1") Then row("Oracle_C1") = DBNull.Value
                If table.Columns.Contains("Oracle_C2") Then row("Oracle_C2") = DBNull.Value
            Next
        End Sub

        Public Sub SaveManualReport(reportDate As Date, rows As IEnumerable(Of ReportFactUpdate))
            Using con As New SqlConnection(_cs)
                con.Open()

                Using tx = con.BeginTransaction(IsolationLevel.ReadCommitted)
                    Try
                        Dim existingCount As Integer
                        Using countCmd As New SqlCommand("SELECT COUNT(1) FROM dbo.DailyReportFact WHERE ReportDate = @ReportDate;", con, tx)
                            countCmd.Parameters.AddWithValue("@ReportDate", reportDate)
                            existingCount = Convert.ToInt32(countCmd.ExecuteScalar())
                        End Using

                        If existingCount = 0 Then
                            Using buildCmd As New SqlCommand("dbo.BuildDailyReport", con, tx)
                                buildCmd.CommandType = CommandType.StoredProcedure
                                buildCmd.Parameters.AddWithValue("@ReportDate", reportDate)
                                buildCmd.Parameters.AddWithValue("@Mode", "PLACEHOLDER")
                                buildCmd.ExecuteNonQuery()
                            End Using
                        End If

                        For Each row In rows
                            Using cmd As New SqlCommand("UPDATE dbo.DailyReportFact SET ValueNum = @ValueNum, ValueText = @ValueText, Remarks = @Remarks, Oracle_C1 = @OracleC1, Oracle_C2 = @OracleC2, Unit = @Unit, SortSection = @SortSection, SortSubSection = @SortSubSection, SortItem = @SortItem, SortMeasure = @SortMeasure WHERE ReportDate = @ReportDate AND Section = @Section AND SubSection = @SubSection AND Item = @Item AND MeasureGroup = @MeasureGroup AND MeasureName = @MeasureName;", con, tx)
                                cmd.Parameters.AddWithValue("@ReportDate", reportDate)
                                cmd.Parameters.AddWithValue("@Section", row.Section)
                                cmd.Parameters.AddWithValue("@SubSection", row.SubSection)
                                cmd.Parameters.AddWithValue("@Item", row.Item)
                                cmd.Parameters.AddWithValue("@MeasureGroup", row.MeasureGroup)
                                cmd.Parameters.AddWithValue("@MeasureName", row.MeasureName)

                                cmd.Parameters.AddWithValue("@ValueNum", If(row.ValueNum.HasValue, CType(row.ValueNum.Value, Object), DBNull.Value))
                                cmd.Parameters.AddWithValue("@ValueText", If(String.IsNullOrWhiteSpace(row.ValueText), CType(DBNull.Value, Object), row.ValueText))
                                cmd.Parameters.AddWithValue("@Remarks", If(String.IsNullOrWhiteSpace(row.Remarks), CType(DBNull.Value, Object), row.Remarks))
                                cmd.Parameters.AddWithValue("@OracleC1", If(row.OracleC1.HasValue, CType(row.OracleC1.Value, Object), DBNull.Value))
                                cmd.Parameters.AddWithValue("@OracleC2", If(row.OracleC2.HasValue, CType(row.OracleC2.Value, Object), DBNull.Value))
                                cmd.Parameters.AddWithValue("@Unit", If(String.IsNullOrWhiteSpace(row.Unit), CType(DBNull.Value, Object), row.Unit))
                                cmd.Parameters.AddWithValue("@SortSection", If(row.SortSection.HasValue, CType(row.SortSection.Value, Object), DBNull.Value))
                                cmd.Parameters.AddWithValue("@SortSubSection", If(row.SortSubSection.HasValue, CType(row.SortSubSection.Value, Object), DBNull.Value))
                                cmd.Parameters.AddWithValue("@SortItem", If(row.SortItem.HasValue, CType(row.SortItem.Value, Object), DBNull.Value))
                                cmd.Parameters.AddWithValue("@SortMeasure", If(row.SortMeasure.HasValue, CType(row.SortMeasure.Value, Object), DBNull.Value))

                                Dim affected = cmd.ExecuteNonQuery()
                                If affected = 0 Then
                                    Using insertCmd As New SqlCommand("INSERT INTO dbo.DailyReportFact (ReportDate, Section, SubSection, Item, MeasureGroup, MeasureName, ValueNum, ValueText, Unit, SortSection, SortSubSection, SortItem, SortMeasure, Remarks, Oracle_C1, Oracle_C2) VALUES (@ReportDate, @Section, @SubSection, @Item, @MeasureGroup, @MeasureName, @ValueNum, @ValueText, @Unit, @SortSection, @SortSubSection, @SortItem, @SortMeasure, @Remarks, @OracleC1, @OracleC2);", con, tx)
                                        insertCmd.Parameters.AddWithValue("@ReportDate", reportDate)
                                        insertCmd.Parameters.AddWithValue("@Section", row.Section)
                                        insertCmd.Parameters.AddWithValue("@SubSection", row.SubSection)
                                        insertCmd.Parameters.AddWithValue("@Item", row.Item)
                                        insertCmd.Parameters.AddWithValue("@MeasureGroup", row.MeasureGroup)
                                        insertCmd.Parameters.AddWithValue("@MeasureName", row.MeasureName)
                                        insertCmd.Parameters.AddWithValue("@ValueNum", If(row.ValueNum.HasValue, CType(row.ValueNum.Value, Object), DBNull.Value))
                                        insertCmd.Parameters.AddWithValue("@ValueText", If(String.IsNullOrWhiteSpace(row.ValueText), CType(DBNull.Value, Object), row.ValueText))
                                        insertCmd.Parameters.AddWithValue("@Unit", If(String.IsNullOrWhiteSpace(row.Unit), CType(DBNull.Value, Object), row.Unit))
                                        insertCmd.Parameters.AddWithValue("@SortSection", If(row.SortSection.HasValue, CType(row.SortSection.Value, Object), DBNull.Value))
                                        insertCmd.Parameters.AddWithValue("@SortSubSection", If(row.SortSubSection.HasValue, CType(row.SortSubSection.Value, Object), DBNull.Value))
                                        insertCmd.Parameters.AddWithValue("@SortItem", If(row.SortItem.HasValue, CType(row.SortItem.Value, Object), DBNull.Value))
                                        insertCmd.Parameters.AddWithValue("@SortMeasure", If(row.SortMeasure.HasValue, CType(row.SortMeasure.Value, Object), DBNull.Value))
                                        insertCmd.Parameters.AddWithValue("@Remarks", If(String.IsNullOrWhiteSpace(row.Remarks), CType(DBNull.Value, Object), row.Remarks))
                                        insertCmd.Parameters.AddWithValue("@OracleC1", If(row.OracleC1.HasValue, CType(row.OracleC1.Value, Object), DBNull.Value))
                                        insertCmd.Parameters.AddWithValue("@OracleC2", If(row.OracleC2.HasValue, CType(row.OracleC2.Value, Object), DBNull.Value))

                                        Dim inserted = insertCmd.ExecuteNonQuery()
                                        If inserted = 0 Then
                                            Throw New InvalidOperationException($"Template row not found for {row.Section}/{row.SubSection}/{row.Item}/{row.MeasureGroup}/{row.MeasureName} on {reportDate:yyyy-MM-dd}.")
                                        End If
                                    End Using
                                End If
                            End Using
                        Next

                        tx.Commit()
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        End Sub
    End Class
End Namespace

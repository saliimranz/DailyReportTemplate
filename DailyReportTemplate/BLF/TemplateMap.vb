Imports System.Data

Namespace BLF
    Public Module TemplateMap

        ' Builds the expected Excel defined-name for a fact row
        Public Function BuildName(section As String, subSec As String, item As String,
                                  measureGroup As String, measureName As String) As String
            ' normalize
            Dim S = section
            Dim SS = subSec
            Dim I = If(item, "")
            Dim G = measureGroup
            Dim M = measureName

            ' canonicalize tokens → short codes that match the Excel names you created
            S = S.Trim()

            Select Case S
                Case "GoodsReceiving" : S = "GR"
                Case "PickingDelivery" : S = "PD"
                Case "Storage" : S = "ST"
                Case "UsedBatteries" : S = "UB"
                Case "Others" : S = "OT"
                Case Else : S = "N"
            End Select

            If SS IsNot Nothing Then
                SS = SS.Trim()
                SS = SS.Replace("AutoPartsContainers", "AP").
                         Replace("TireContainers", "TI").
                         Replace("BatteriesContainers", "BA").
                         Replace("LubeContainers", "LU").
                         Replace("Picking", "PK").
                         Replace("MTD", "MTD").
                         Replace("TotalOrders", "TO").
                         Replace("Packing", "PCK").
                         Replace("Labelling/Special Marketing", "LAB").
                         Replace("Palletizing", "PL").
                         Replace("Auto Parts", "AP").
                         Replace("Battery Testing & Charging: In Coming Shipment", "BTC").
                         Replace("Battery Testing & Charging", "BTC").
                         Replace("In Stock (Battery)", "IS").
                         Replace("Used Batteries Activities", "UBA").
                         Replace("Pickup Loading", "PL").
                         Replace("Consignment Receiving (Offday/After Off)", "CR")
            Else
                SS = ""
            End If

            If I IsNot Nothing Then
                I = I.Trim().
                      Replace(" ", "_").
                      Replace(".", "_").
                      Replace("(", "_").
                      Replace(")", "_").
                      Replace("&", "").
                      Replace("/", "_").
                      Replace("__", "_")

                ' Align a few special cases with our examples
                I = I.Replace("No of Containers", "NoOfContainers").
                      Replace("No_of_Containers", "NoOfContainers").
                      Replace("A.Filter", "A_Filter").
                      Replace("B.Shock_Absorber", "B_ShockAbsorber").
                      Replace("C.Brake_Disc", "C_BrakeDisc").
                      Replace("TotalOrders", "TO").
                      Replace("D.Others", "D_Others").
                      Replace("Battery Scanning", "BatteryScanning").
                      Replace("AMAP_Sticker_Labeling", "AMAPStickerLabeling").
                      Replace("Labeling_on_Shocks_Disc_Batt", "LabelingShocksDiscBatt").
                      Replace("Pallet_Add__Replace", "PalletAddReplace").
                      Replace("Shrink Wrap & Repalletized", "ShrinkWrapRepalletized").
                      Replace("Checked_Pallet_", "Checked_Pallet").
                      Replace("Checked (Pallet)", "Checked_Pallet").
                      Replace("Found OK", "FoundOK").
                      Replace("Found_OK", "FoundOK").
                      Replace("Recharged_Customer_", "Recharged_Customer").
                      Replace("Recharged (Stock)", "Recharged_Stock").
                      Replace("Recharged_Stock_", "Recharged_Stock").
                      Replace("Shrink_Wrap_Repalletized", "ShrinkWrapRepalletized").
                      Replace("Total_Shrink_Wrap__Repalletized", "TotalShrinkWrapRepalletized").
                      Replace("Qty_Received", "QtyReceived").
                      Replace("Qty_checked", "QtyChecked").
                      Replace("Qty_Repalletized", "QtyRepalletized").
                      Replace("Pallet_Qty_Shrink_Wraped_", "PalletQtyShrinkWraped").
                      Replace("Pallet Qty (Shrink/Wraped)", "PalletQtyShrinkWraped").
                      Replace("Batteries_OffLoading", "BatteriesOffLoading").
                      Replace("Batteries_Loading", "BatteriesLoading").
                      Replace("01Ton_Pickup", "01TonPickup").
                      Replace("03Ton_Pickup", "03TonPickup").
                      Replace("10Ton_Pickup_20ft_40ft", "10TonPickup_20ft_40ft").
                      Replace("Over_Time_Hrs_", "OverTime_Hrs")

            Else
                I = ""
            End If

            ' Measures → tokens
            G = G.Trim()
            M = M.Trim()
            Dim colToken As String

            Select Case G
                Case "QTY" : colToken = "QTY"
                Case "MTD" : colToken = "MTD"
                Case "Line" : colToken = "Line"    ' Line / Qty
                Case "Qty" : colToken = "QTY"        ' Line / Qty
                Case "Carton" : colToken = "Carton"
                Case "Loose" : colToken = "Loose"
                Case "Pallet" : colToken = "Pallet"
                Case "Labelling" : colToken = "QTY"
                Case "TotalPickingOrders" : colToken = "TotalPickingOrders"
                Case "TotalMTDOrders" : colToken = "TotalMTDOrders"
                Case "Unnamed3" : colToken = "U3"
                Case "Unnamed4" : colToken = "U4"
                Case "Total" : colToken = M                 ' e.g., PickingOrders / MTD / MTDOrders
                Case Else : colToken = M
            End Select

            ' Compose final name
            Dim parts As New List(Of String)
            parts.Add(S)
            If SS <> "" Then parts.Add(SS)
            If I <> "" Then parts.Add(I)
            If colToken <> "" Then parts.Add(colToken)

            Return String.Join("_", parts)
        End Function

    End Module
End Namespace

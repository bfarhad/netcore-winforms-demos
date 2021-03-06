using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Range = DevExpress.Spreadsheet.Range;

namespace DevExpress.DevAV.Reports.Spreadsheet {
    public static class CellsHelper {

        public static List<OrderCellInfo> OrderCells { get { return orderCells; } }
        public static List<OrderCellInfo> OrderItemCells { get { return orderItemCells; } }
        public static Dictionary<CustomEditorIds, CustomEditorInfo> CustomEditorsConfig { get { return customEditorsConfig; } }
        public static string InvoiceWorksheetName { get { return "Invoice"; } }
        public static string InvoiceWorksheetPassword { get { return "123"; } }

        static Dictionary<CustomEditorIds, CustomEditorInfo> customEditorsConfig = CreateCustomEditorsConfig();
        static List<OrderCellInfo> orderCells = CreateOrderCells();
        static List<OrderCellInfo> orderItemCells = CreateOrderItemCells();
        static Dictionary<CellsKind, string> cellsPosition = orderCells.ToDictionary<OrderCellInfo, CellsKind, string>(x => x.Cell, y => y.CellRange);

        public static void GenerateEditors(List<OrderCellInfo> cellsInfo, Worksheet invoice) {
            cellsInfo.Where(x => x.IsAutoGeneratedEditor()).ToList().ForEach(info => {
                ValueObject value = info.EditorId ?? (info.FixedValues != null ? ValueObject.CreateListSource(info.FixedValues) : null);
                invoice.CustomCellInplaceEditors.Add(invoice[info.CellRange], info.EditorType.Value, value);
            });
        }
        public static void CreateCollectionEditor<T>(CellsKind cell, Worksheet invoice, IEnumerable<T> source, Func<T, string> getValue) {
            var cellInfo = FindCell(cell);
            var cellValues = source.Select(x => CellValue.FromObject(getValue(x))).ToArray();
            invoice.CustomCellInplaceEditors.Add(invoice[cellInfo.CellRange], cellInfo.EditorType.Value,
                ValueObject.CreateListSource(cellValues));
        }
        public static void RemoveEditor(CellsKind cell, Worksheet invoice) {
            var storeCell = FindCell(cell);
            invoice.CustomCellInplaceEditors.Remove(invoice[storeCell.CellRange]);
        }
        public static void RemoveAllEditors(string region, Worksheet invoice) {
            invoice.CustomCellInplaceEditors.Remove(invoice[region]);
        }

        public static bool IsOrderItemProductCell(Cell cell, Range orderItemRange) {
            return cell.LeftColumnIndex - orderItemRange.LeftColumnIndex == CellsHelper.FindCell(CellsKind.ProductDescription).Offset;
        }
        public static bool IsRemoveItemRange(Cell cell, Range orderItemsRange) {
            return IsSingleCellSelected(cell)
                && cell.LeftColumnIndex > orderItemsRange.RightColumnIndex
                && cell.LeftColumnIndex < orderItemsRange.RightColumnIndex + 4
                && cell.TopRowIndex >= orderItemsRange.TopRowIndex
                && cell.TopRowIndex <= orderItemsRange.BottomRowIndex;
        }
        public static bool IsAddItemRange(Cell cell, Range orderItemsRange) {
            return IsSingleCellSelected(cell)
                && cell.TopRowIndex == orderItemsRange.BottomRowIndex + 1
                && cell.LeftColumnIndex - orderItemsRange.LeftColumnIndex == 1;
        }
        public static bool IsInvoiceSheetActive(Cell cell) {
            return cell != null && cell.Worksheet.Name == CellsHelper.InvoiceWorksheetName;
        }

        public static CellValue GetOrderCellValue(CellsKind cell, List<OrderItem> orderItems, Worksheet invoice) {
            var range = CellsHelper.GetActualCellRange(CellsHelper.FindLeftCell(cell), orderItems.Any() ? orderItems.Count : 0);
            return invoice.Cells[range].Value;
        }
        public static CellValue GetOrderItemCellValue(CellsKind cell, Range orderItemRange, Worksheet invoice) {
            int offset = CellsHelper.GetOffset(cell);
            var cellRange = invoice.Range.FromLTRB(orderItemRange.LeftColumnIndex + offset,
                orderItemRange.TopRowIndex, orderItemRange.LeftColumnIndex + offset, orderItemRange.BottomRowIndex);
            return cellRange.Value;
        }

        public static void CopyOrderItemRange(Range itemRange) {
            Range range = itemRange.Offset(-1, 0);
            range.CopyFrom(itemRange, PasteSpecial.All, true);
        }

        public static void UpdateEditableCells(Worksheet invoice, Order order, OrderCollections source) {
            invoice.Cells[FindLeftCell(CellsKind.Date)].Value = order.OrderDate.Millisecond != 0 ? order.OrderDate : DateTime.FromBinary(0);
            invoice.Cells[FindLeftCell(CellsKind.InvoiceNumber)].Value = order.InvoiceNumber;
            invoice.Cells[FindLeftCell(CellsKind.CustomerName)].Value = GetCustomer(order, source) != null ? GetCustomer(order, source).Name : string.Empty;
            invoice.Cells[FindLeftCell(CellsKind.CustomerStoreName)].Value = GetStore(order, source) != null ? GetStore(order, source).City : string.Empty;
            invoice.Cells[FindLeftCell(CellsKind.EmployeeName)].Value = GetEmployee(order, source) != null ? GetEmployee(order, source).FullName : string.Empty;
            invoice.Cells[FindLeftCell(CellsKind.CustomerHomeOfficeName)].Value = "Home Office";
            invoice.Cells[FindLeftCell(CellsKind.PONumber)].Value = order.PONumber;
            invoice.Cells[FindLeftCell(CellsKind.ShipDate)].Value = order.ShipDate;
            invoice.Cells[FindLeftCell(CellsKind.ShipVia)].Value = order.ShipmentCourier.ToString();
            invoice.Cells[FindLeftCell(CellsKind.FOB)].Value = string.Empty;
            invoice.Cells[FindLeftCell(CellsKind.Terms)].Value = order.OrderTerms != null ? int.Parse(new Regex(@"\d+").Match(order.OrderTerms).Value) : 5;
            invoice.Cells[FindLeftCell(CellsKind.Shipping)].Value = (double)order.ShippingAmount;
            invoice.Cells[FindLeftCell(CellsKind.Comments)].Value = order.Comments;
        }
        public static void UpdateDependentCells(Worksheet invoice, Order order, OrderCollections source) {
            var customer = GetCustomer(order, source);
            var store = GetStore(order, source);
            if(customer != null) {
                invoice.Cells[FindLeftCell(CellsKind.ShippingCustomerName)].Value = customer.Name;
                invoice.Cells[FindLeftCell(CellsKind.CustomerStreetLine)].Value = customer.HomeOffice.Line;
                invoice.Cells[FindLeftCell(CellsKind.CustomerCityLine)].Value = customer.HomeOffice.CityLine;
            }
            if(store != null) {
                invoice.Cells[FindLeftCell(CellsKind.CustomerStoreStreetLine)].Value = store.Address.Line;
                invoice.Cells[FindLeftCell(CellsKind.CustomerStoreCityLine)].Value = store.Address.CityLine;
            }
        }
        public static OrderCellInfo FindCell(CellsKind cell) {
            return OrderCells.FirstOrDefault(x => x.Cell == cell) ?? OrderItemCells.FirstOrDefault(x => x.Cell == cell);
        }

        public static string FindLeftCell(CellsKind cell) {
            return FindLeftCell(cellsPosition[cell]);
        }
        public static string GetActualCellRange(string defaultRange, int shiftValue, int initialShiftIndex = 23) {
            var defaultRow = int.Parse(new Regex(@"\d+").Match(defaultRange).Value);
            var absShiftValue = Math.Abs(shiftValue);

            var actualShiftValue = absShiftValue < 2 ? 0 : Math.Sign(shiftValue) * (absShiftValue - 1);
            if(defaultRow < initialShiftIndex + (actualShiftValue < 0 ? Math.Abs(actualShiftValue) : 0))
                return defaultRange;
            return string.Format("{0}{1}", defaultRange.First(), defaultRow + actualShiftValue);
        }
        public static int GetOffset(CellsKind kind) {
            return FindCell(kind).Offset.Value;
        }
        public static bool HasDependentCells(string range) {
            return OrderCells.FirstOrDefault(x => FindLeftCell(x.Cell) == range).HasDependentCells;
        }

        public static CustomEditorInfo FindEditor(string name) {
            return CustomEditorsConfig.Values.SingleOrDefault(x => x.Name == name);
        }
        static Customer GetCustomer(Order order, OrderCollections source) {
            return order.Customer ?? (order.CustomerId == null ? null : source.Customers.FirstOrDefault(x => x.Id == order.CustomerId));
        }
        static CustomerStore GetStore(Order order, OrderCollections source) {
            return order.Store ?? (order.StoreId == null ? null : source.CustomerStores.FirstOrDefault(x => x.Id == order.StoreId));
        }
        static Employee GetEmployee(Order order, OrderCollections source) {
            return order.Employee ?? (order.EmployeeId == null ? null : source.Employees.FirstOrDefault(x => x.Id == order.EmployeeId));
        }
        static bool IsSingleCellSelected(Cell cell) {
            return cell.LeftColumnIndex == cell.RightColumnIndex && cell.TopRowIndex == cell.BottomRowIndex;
        }

        #region Cells Config
        static List<OrderCellInfo> CreateOrderCells() {
            var result = new List<OrderCellInfo>();
            result.Add(new OrderCellInfo(CellsKind.Date, "B4", CustomCellInplaceEditorType.DateEdit));
            result.Add(new OrderCellInfo(CellsKind.InvoiceNumber, "C5"));
            result.Add(new OrderCellInfo(CellsKind.CustomerName, "B10:E10", CustomCellInplaceEditorType.ComboBox, hasDependentCells: true));
            result.Add(new OrderCellInfo(CellsKind.CustomerHomeOfficeName, "B12"));
            result.Add(new OrderCellInfo(CellsKind.CustomerStreetLine, "B13"));
            result.Add(new OrderCellInfo(CellsKind.CustomerCityLine, "B14"));
            result.Add(new OrderCellInfo(CellsKind.ShippingCustomerName, "H10:M10"));
            result.Add(new OrderCellInfo(CellsKind.CustomerStoreName, "H12:M12", CustomCellInplaceEditorType.ComboBox, hasDependentCells: true));
            result.Add(new OrderCellInfo(CellsKind.CustomerStoreStreetLine, "H13"));
            result.Add(new OrderCellInfo(CellsKind.CustomerStoreCityLine, "H14"));
            result.Add(new OrderCellInfo(CellsKind.EmployeeName, "B18:C18", CustomCellInplaceEditorType.ComboBox));
            result.Add(new OrderCellInfo(CellsKind.PONumber, "D18:E18"));
            result.Add(new OrderCellInfo(CellsKind.ShipDate, "F18:G18", CustomCellInplaceEditorType.DateEdit));
            result.Add(new OrderCellInfo(CellsKind.ShipVia, "H18:I18", CustomCellInplaceEditorType.ComboBox, fixedValues: new CellValue[] { ShipmentCourier.FedEx.ToString(), ShipmentCourier.DHL.ToString(), ShipmentCourier.UPS.ToString(), ShipmentCourier.None.ToString() }));
            result.Add(new OrderCellInfo(CellsKind.FOB, "J18:K18", CustomCellInplaceEditorType.Custom, editorId: customEditorsConfig[CustomEditorIds.FOBSpinEdit].Name));
            result.Add(new OrderCellInfo(CellsKind.Terms, "L18:M18", CustomCellInplaceEditorType.Custom, editorId: customEditorsConfig[CustomEditorIds.TermsSpinEdit].Name));
            result.Add(new OrderCellInfo(CellsKind.SubTotal, "K23:M23"));
            result.Add(new OrderCellInfo(CellsKind.Shipping, "K24:M24", CustomCellInplaceEditorType.Custom, editorId: customEditorsConfig[CustomEditorIds.ShippingSpinEdit].Name, hasDependentCells: true));
            result.Add(new OrderCellInfo(CellsKind.TotalDue, "K25:M25"));
            result.Add(new OrderCellInfo(CellsKind.Comments, "B28:E31"));
            result.Add(new OrderCellInfo(CellsKind.AmountPaid, "K27:M27"));

            return result;
        }

        static List<OrderCellInfo> CreateOrderItemCells() {
            var result = new List<OrderCellInfo>();
            result.Add(new OrderCellInfo(CellsKind.Quantity, "B22:B23", CustomCellInplaceEditorType.Custom, editorId: customEditorsConfig[CustomEditorIds.QuantitySpinEdit].Name, offset: 0));
            result.Add(new OrderCellInfo(CellsKind.ProductDescription, "C22:F23", CustomCellInplaceEditorType.ComboBox, offset: 1));
            result.Add(new OrderCellInfo(CellsKind.UnitPrice, "G22:H23", offset: 5));
            result.Add(new OrderCellInfo(CellsKind.Discount, "I22:J23", CustomCellInplaceEditorType.Custom, editorId: customEditorsConfig[CustomEditorIds.DiscountSpinEdit].Name, offset: 7));
            result.Add(new OrderCellInfo(CellsKind.Total, "K22:M23", offset: 9));
            return result;
        }
        static Dictionary<CustomEditorIds, CustomEditorInfo> CreateCustomEditorsConfig() {
            var result = new Dictionary<CustomEditorIds, CustomEditorInfo>();
            result.Add(CustomEditorIds.FOBSpinEdit, new CustomEditorInfo("FOBSpinEdit", minValue: 0, maxValue: 500, increment: 500));
            result.Add(CustomEditorIds.TermsSpinEdit, new CustomEditorInfo("TermsSpinEdit", minValue: 5, maxValue: 200, increment: 1));
            result.Add(CustomEditorIds.QuantitySpinEdit, new CustomEditorInfo("QuantitySpinEdit", minValue: 1, maxValue: 100, increment: 1));
            result.Add(CustomEditorIds.DiscountSpinEdit, new CustomEditorInfo("DiscountSpinEdit", minValue: 0, maxValue: 1000, increment: 10));
            result.Add(CustomEditorIds.ShippingSpinEdit, new CustomEditorInfo("ShippingSpinEdit", minValue: 0, maxValue: 1000, increment: 5));
            return result;
        }

        #endregion

        static string FindLeftCell(string range) {
            return range.Substring(0, Math.Min(range.Length, 3));
        }
    }

    public class CustomEditorInfo {
        public CustomEditorInfo(string name, int minValue, int maxValue, int increment) {
            Name = name;
            MinValue = minValue;
            MaxValue = maxValue;
            Increment = increment;
        }
        public string Name { get; private set; }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        public int Increment { get; private set; }
    }

    public class OrderCellInfo {
        public OrderCellInfo(CellsKind cell, string cellRange, CustomCellInplaceEditorType? editorType = null,
            string editorId = null, CellValue[] fixedValues = null, int? offset = null, bool hasDependentCells = false) {
            Cell = cell;
            CellRange = cellRange;
            EditorType = editorType;
            EditorId = editorId;
            FixedValues = fixedValues;
            Offset = offset;
            HasDependentCells = hasDependentCells;
        }
        public CellsKind Cell { get; private set; }
        public string CellStringId { get; set; }
        public string CellRange { get; private set; }
        public CustomCellInplaceEditorType? EditorType { get; private set; }
        public string EditorId { get; private set; }
        public CellValue[] FixedValues { get; private set; }
        public int? Offset { get; private set; }
        public bool HasDependentCells { get; private set; }

        public bool IsUnitOfWorkSelector() {
            return EditorType == CustomCellInplaceEditorType.ComboBox && FixedValues == null;
        }
        public bool IsAutoGeneratedEditor() {
            return (EditorType != null && EditorType != CustomCellInplaceEditorType.ComboBox) || FixedValues != null;
        }
    }


}

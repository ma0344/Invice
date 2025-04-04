using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace Invoice
{
    // T_CUSTOMER テーブルに対応するクラス
    public class CustomerTable
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerKana { get; set; }
        public int Balance { get; set; }
        public bool Visible { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public static List<CustomerTable> GetAllCustomers()
        {
            var customers = new List<CustomerTable>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_CUSTOMER", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var customer = new CustomerTable
                {
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    CustomerName = reader.GetString("CUSTOMER_NAME"),
                    CustomerKana = reader.IsDBNull("CUSTOMER_KANA") ? "" : reader.GetString("CUSTOMER_KANA"),
                    Balance = reader.GetInt32("BALANCE"),
                    Visible = reader.GetBoolean("VISIBLE"),
                    CreateDate = reader.GetDateTime("CREATE_DATE"),
                    UpdateDate = reader.GetDateTime("UPDATE_DATE")
                };
                customers.Add(customer);
            }
            return customers;
        }

        public void AddCustomer()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_CUSTOMER (CUSTOMER_NAME, CUSTOMER_KANA, BALANCE, VISIBLE)
                             VALUES (@CustomerName, @CustomerKana, @Balance, @Visible)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerName", CustomerName);
            command.Parameters.AddWithValue("@CustomerKana", CustomerKana);
            command.Parameters.AddWithValue("@Balance", Balance);
            command.Parameters.AddWithValue("@Visible", Visible);
            command.ExecuteNonQuery();
            CustomerId = (int)command.LastInsertedId;
        }

        public void UpdateCustomer()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_CUSTOMER SET CUSTOMER_NAME = @CustomerName, CUSTOMER_KANA = @CustomerKana,
                             BALANCE = @Balance, VISIBLE = @Visible WHERE CUSTOMER_ID = @CustomerId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerName", CustomerName);
            command.Parameters.AddWithValue("@CustomerKana", CustomerKana);
            command.Parameters.AddWithValue("@Balance", Balance);
            command.Parameters.AddWithValue("@Visible", Visible);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.ExecuteNonQuery();
        }

        public void DeleteCustomer()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_CUSTOMER WHERE CUSTOMER_ID = @CustomerId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.ExecuteNonQuery();
        }
    }

    // T_INVOICE テーブルに対応するクラス
    public class InvoiceTable
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Subject { get; set; }
        public string SlipNumber { get; set; }
        public int? Subtotal { get; set; }
        public double? Tax { get; set; }
        public int? Total { get; set; }
        public int? PaymentMethodId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int InvoiceStatusId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public static List<InvoiceTable> GetAllInvoices()
        {
            var invoices = new List<InvoiceTable>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var invoice = new InvoiceTable
                {
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    IssueDate = reader.IsDBNull("ISSUE_DATE") ? null : reader.GetDateTime("ISSUE_DATE"),
                    DueDate = reader.IsDBNull("DUE_DATE") ? null : reader.GetDateTime("DUE_DATE"),
                    Subject = reader.IsDBNull("SUBJECT") ? "" : reader.GetString("SUBJECT"),
                    SlipNumber = reader.IsDBNull("SLIP_NUMBER") ? "" : reader.GetString("SLIP_NUMBER"),
                    Subtotal = reader.IsDBNull("SUBTOTAL") ? null : reader.GetInt32("SUBTOTAL"),
                    Tax = reader.IsDBNull("TAX") ? null : reader.GetDouble("TAX"),
                    Total = reader.IsDBNull("TOTAL") ? null : reader.GetInt32("TOTAL"),
                    PaymentMethodId = reader.IsDBNull("PAYMENT_METHOD_ID") ? null : reader.GetInt32("PAYMENT_METHOD_ID"),
                    PaymentDate = reader.IsDBNull("PAYMENT_DATE") ? null : reader.GetDateTime("PAYMENT_DATE"),
                    InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID"),
                    CreateDate = reader.GetDateTime("CREATE_DATE"),
                    UpdateDate = reader.GetDateTime("UPDATE_DATE")
                };
                invoices.Add(invoice);
            }
            return invoices;
        }

        public void AddInvoice()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_INVOICE (CUSTOMER_ID, ISSUE_DATE, DUE_DATE, SUBJECT, SLIP_NUMBER, SUBTOTAL, TAX, TOTAL, PAYMENT_METHOD_ID, PAYMENT_DATE, INVOICE_STATUS_ID)
                             VALUES (@CustomerId, @IssueDate, @DueDate, @Subject, @SlipNumber, @Subtotal, @Tax, @Total, @TransactionTypeId, @PaymentDate, @InvoiceStatusId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@Subtotal", Subtotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@Total", Total);
            command.Parameters.AddWithValue("@TransactionTypeId", PaymentMethodId);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
            command.ExecuteNonQuery();
            InvoiceId = (int)command.LastInsertedId;
        }

        public void UpdateInvoice()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_INVOICE SET CUSTOMER_ID = @CustomerId, ISSUE_DATE = @IssueDate, DUE_DATE = @DueDate, SUBJECT = @Subject,
                             SLIP_NUMBER = @SlipNumber, SUBTOTAL = @Subtotal, TAX = @Tax, TOTAL = @Total, 
                             PAYMENT_METHOD_ID = @TransactionTypeId, PAYMENT_DATE = @PaymentDate, INVOICE_STATUS_ID = @InvoiceStatusId
                             WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@Subtotal", Subtotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@Total", Total);
            command.Parameters.AddWithValue("@TransactionTypeId", PaymentMethodId);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }

        public void DeleteInvoice()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }
    }

    // T_INVOICE_ITEMS テーブルに対応するクラス
    public class InvoiceItemTable
    {
        public int InvoiceItemsId { get; set; }
        public int InvoiceId { get; set; }
        public int? ItemOrder { get; set; }
        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public int? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public int? ItemSubtotal { get; set; }
        public int? TaxTypeId { get; set; }
        public int? Tax { get; set; }
        public int? ItemTotal { get; set; }

        public static List<InvoiceItemTable> GetInvoiceItemsByInvoiceId(int invoiceId)
        {
            var items = new List<InvoiceItemTable>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM T_INVOICE_ITEMS WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new InvoiceItemTable
                {
                    InvoiceItemsId = reader.GetInt32("INVOICE_ITEMS_ID"),
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    ItemOrder = reader.IsDBNull("ITEM_ORDER") ? null : reader.GetInt32("ITEM_ORDER"),
                    ItemId = reader.IsDBNull("ITEM_ID") ? null : reader.GetInt32("ITEM_ID"),
                    ItemName = reader.IsDBNull("ITEM_NAME") ? "" : reader.GetString("ITEM_NAME"),
                    UnitPrice = reader.IsDBNull("UNIT_PRICE") ? null : reader.GetInt32("UNIT_PRICE"),
                    Quantity = reader.IsDBNull("QUANTITY") ? null : reader.GetInt32("QUANTITY"),
                    ItemSubtotal = reader.IsDBNull("ITEM_SUBTOTAL") ? null : reader.GetInt32("ITEM_SUBTOTAL"),
                    TaxTypeId = reader.IsDBNull("TAX_TYPE_ID") ? null : reader.GetInt32("TAX_TYPE_ID"),
                    Tax = reader.IsDBNull("TAX") ? null : reader.GetInt32("TAX"),
                    ItemTotal = reader.IsDBNull("ITEM_TOTAL") ? null : reader.GetInt32("ITEM_TOTAL")
                };
                items.Add(item);
            }
            return items;
        }

        public void AddInvoiceItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_INVOICE_ITEMS (INVOICE_ID, ITEM_ORDER, ITEM_ID, ITEM_NAME, UNIT_PRICE, QUANTITY, ITEM_SUBTOTAL, TAX_TYPE_ID, TAX, ITEM_TOTAL)
                             VALUES (@InvoiceId, @ItemOrder, @ItemId, @ItemName, @UnitPrice, @Quantity, @ItemSubtotal, @TaxTypeId, @Tax, @ItemTotal)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@ItemOrder", ItemOrder);
            command.Parameters.AddWithValue("@ItemId", ItemId);
            command.Parameters.AddWithValue("@ItemName", ItemName);
            command.Parameters.AddWithValue("@UnitPrice", UnitPrice);
            command.Parameters.AddWithValue("@Quantity", Quantity);
            command.Parameters.AddWithValue("@ItemSubtotal", ItemSubtotal);
            command.Parameters.AddWithValue("@TaxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@ItemTotal", ItemTotal);
            command.ExecuteNonQuery();
            InvoiceItemsId = (int)command.LastInsertedId;
        }

        public void UpdateInvoiceItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_INVOICE_ITEMS SET INVOICE_ID = @InvoiceId, ITEM_ORDER = @ItemOrder, ITEM_ID = @ItemId, ITEM_NAME = @ItemName,
                             UNIT_PRICE = @UnitPrice, QUANTITY = @Quantity, ITEM_SUBTOTAL = @ItemSubtotal, TAX_TYPE_ID = @TaxTypeId,
                             TAX = @Tax, ITEM_TOTAL = @ItemTotal WHERE INVOICE_ITEMS_ID = @InvoiceItemsId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@ItemOrder", ItemOrder);
            command.Parameters.AddWithValue("@ItemId", ItemId);
            command.Parameters.AddWithValue("@ItemName", ItemName);
            command.Parameters.AddWithValue("@UnitPrice", UnitPrice);
            command.Parameters.AddWithValue("@Quantity", Quantity);
            command.Parameters.AddWithValue("@ItemSubtotal", ItemSubtotal);
            command.Parameters.AddWithValue("@TaxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@ItemTotal", ItemTotal);
            command.Parameters.AddWithValue("@InvoiceItemsId", InvoiceItemsId);
            command.ExecuteNonQuery();
        }

        public void DeleteInvoiceItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ITEMS_ID = @InvoiceItemsId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceItemsId", InvoiceItemsId);
            command.ExecuteNonQuery();
        }
    }

    // T_INVOICE_STATUS テーブルに対応するクラス
    public class InvoiceStatus
    {
        public int InvoiceStatusId { get; set; }
        public string Status { get; set; }

        public static List<InvoiceStatus> GetAllInvoiceStatuses()
        {
            var statuses = new List<InvoiceStatus>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE_STATUS", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var status = new InvoiceStatus
                {
                    InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID"),
                    Status = reader.GetString("INVOICE_STATUS")
                };
                statuses.Add(status);
            }
            return statuses;
        }
    }

    // T_ITEM テーブルに対応するクラス
    public class Item
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string Unit { get; set; }
        public int UnitPrice { get; set; }
        public int TaxTypeId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public static List<Item> GetAllItems()
        {
            var items = new List<Item>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_ITEM", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new Item
                {
                    ItemId = reader.GetInt32("ITEM_ID"),
                    ItemName = reader.GetString("ITEM_NAME"),
                    ItemCode = reader.IsDBNull("ITEM_CODE") ? "" : reader.GetString("ITEM_CODE"),
                    Unit = reader.IsDBNull("UNIT") ? "" : reader.GetString("UNIT"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID"),
                    CreateDate = reader.GetDateTime("CREATE_DATE"),
                    UpdateDate = reader.GetDateTime("UPDATE_DATE")
                };
                items.Add(item);
            }
            return items;
        }

        public void AddItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_ITEM (ITEM_NAME, ITEM_CODE, UNIT, UNIT_PRICE, TAX_TYPE_ID)
                             VALUES (@ItemName, @ItemCode, @Unit, @UnitPrice, @TaxTypeId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ItemName", ItemName);
            command.Parameters.AddWithValue("@ItemCode", ItemCode);
            command.Parameters.AddWithValue("@Unit", Unit);
            command.Parameters.AddWithValue("@UnitPrice", UnitPrice);
            command.Parameters.AddWithValue("@TaxTypeId", TaxTypeId);
            command.ExecuteNonQuery();
            ItemId = (int)command.LastInsertedId;
        }

        public void UpdateItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_ITEM SET ITEM_NAME = @ItemName, ITEM_CODE = @ItemCode, UNIT = @Unit,
                             UNIT_PRICE = @UnitPrice, TAX_TYPE_ID = @TaxTypeId WHERE ITEM_ID = @ItemId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ItemName", ItemName);
            command.Parameters.AddWithValue("@ItemCode", ItemCode);
            command.Parameters.AddWithValue("@Unit", Unit);
            command.Parameters.AddWithValue("@UnitPrice", UnitPrice);
            command.Parameters.AddWithValue("@TaxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@ItemId", ItemId);
            command.ExecuteNonQuery();
        }

        public void DeleteItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_ITEM WHERE ITEM_ID = @ItemId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ItemId", ItemId);
            command.ExecuteNonQuery();
        }
    }

    // T_PAYMENT_METHOD テーブルに対応するクラス
    public class PaymentMethodTable
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; }

        public static List<PaymentMethodClass> GetAllPaymentMethods()
        {
            var methods = new List<PaymentMethodClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_PAYMENT_METHOD", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var method = new PaymentMethodClass
                {
                    PaymentMethodId = reader.GetInt32("PAYMENT_METHOD_ID"),
                    MethodName = reader.GetString("METHOD_NAME")
                };
                methods.Add(method);
            }
            return methods;
        }
    }

    // T_SLIP_NUMBER_INFO テーブルに対応するクラス
    public class SlipNumberInfoTable
    {
        public int SlipInfoId { get; set; }
        public string InvoicePrefix { get; set; }
        public string InvoiceSuffix { get; set; }
        public string ReceiptPrefix { get; set; }
        public string ReceiptSuffix { get; set; }
        public int InvoiceLatest { get; set; }
        public int ReceiptLatest { get; set; }

        public static SlipNumberInfoTable GetSlipNumberInfo()
        {
            SlipNumberInfoTable info = null;
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM T_SLIP_NUMBER_INFO LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                info = new SlipNumberInfoTable
                {
                    SlipInfoId = reader.GetInt32("T_SLIP_INFO_ID"),
                    InvoicePrefix = reader.GetString("INVOICE_PREFIX"),
                    InvoiceSuffix = reader.GetString("INVOICE_SUFFIX"),
                    ReceiptPrefix = reader.GetString("RECEIPT_PREFIX"),
                    ReceiptSuffix = reader.GetString("RECEIPT_SUFFIX"),
                    InvoiceLatest = reader.GetInt32("INVOICE_LATEST"),
                    ReceiptLatest = reader.GetInt32("RECEIPT_LATEST")
                };
            }
            return info;
        }

        public void UpdateSlipNumberInfo()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_SLIP_NUMBER_INFO SET INVOICE_PREFIX = @InvoicePrefix, INVOICE_SUFFIX = @InvoiceSuffix,
                             RECEIPT_PREFIX = @ReceiptPrefix, RECEIPT_SUFFIX = @ReceiptSuffix, INVOICE_LATEST = @InvoiceLatest,
                             RECEIPT_LATEST = @ReceiptLatest WHERE T_SLIP_INFO_ID = @SlipInfoId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoicePrefix", InvoicePrefix);
            command.Parameters.AddWithValue("@InvoiceSuffix", InvoiceSuffix);
            command.Parameters.AddWithValue("@ReceiptPrefix", ReceiptPrefix);
            command.Parameters.AddWithValue("@ReceiptSuffix", ReceiptSuffix);
            command.Parameters.AddWithValue("@InvoiceLatest", InvoiceLatest);
            command.Parameters.AddWithValue("@ReceiptLatest", ReceiptLatest);
            command.Parameters.AddWithValue("@SlipInfoId", SlipInfoId);
            command.ExecuteNonQuery();
        }
    }

    // T_TAX_TYPE テーブルに対応するクラス
    public class TaxTypeTable
    {
        public int TaxTypeId { get; set; }
        public string TaxTypeName { get; set; }
        public decimal TaxRate { get; set; }

        public static List<TaxTypeTable> GetAllTaxTypes()
        {
            var taxTypes = new List<TaxTypeTable>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_TAX_TYPE", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var taxType = new TaxTypeTable
                {
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID"),
                    TaxTypeName = reader.GetString("TAX_TYPE_NAME"),
                    TaxRate = reader.GetDecimal("TAX_RATE")
                };
                taxTypes.Add(taxType);
            }
            return taxTypes;
        }
    }

}


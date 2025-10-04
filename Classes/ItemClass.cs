using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Classes
{

    // T_ITEM テーブルに対応するクラス
    public class ItemClass : INotifyPropertyChanged, ILoggable
    {
        public int ItemId { get; set; } = 0;

        private string _itemName = "";
        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        private string _itemCode = "";
        public string ItemCode
        {
            get => _itemCode;
            set
            {
                _itemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        private string _unit = "";
        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        private int _unitPrice = 0;
        public virtual int UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        private int _taxTypeId = 1;
        public int TaxTypeId
        {
            get => _taxTypeId;
            set
            {
                _taxTypeId = value;
                OnPropertyChanged(nameof(TaxTypeId));
            }
        }

        private string _taxTypeName = "";
        public string TaxTypeName
        {
            get => _taxTypeName;
            set
            {
                _taxTypeName = value;
                OnPropertyChanged(nameof(TaxTypeName));
            }
        }

        public static List<ItemClass> GetItems()
        {
            var items = new List<ItemClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_ITEM", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new ItemClass
                {
                    ItemId = reader.GetInt32("ITEM_ID"),
                    ItemName = reader.GetString("ITEM_NAME"),
                    ItemCode = reader.GetString("ITEM_CODE"),
                    Unit = reader.GetString("UNIT"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID")
                };
                items.Add(item);
            }
            return items;
        }

        public ItemClass Copy()
        {

            return new ItemClass
            {
                ItemId = ItemId,
                ItemName = ItemName,
                ItemCode = ItemCode,
                Unit = Unit,
                UnitPrice = UnitPrice,
                TaxTypeId = TaxTypeId,
                TaxTypeName = TaxTypeName
            };
        }

        public void AddItem()
        {

            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "INSERT INTO T_ITEM (ITEM_NAME, ITEM_CODE, UNIT, UNIT_PRICE, TAX_TYPE_ID) " + "\r\n" + "VALUES (@name, @code, @unit, @price, @taxTypeId)", connection);
            command.Parameters.AddWithValue("@name", ItemName);
            command.Parameters.AddWithValue("@code", ItemCode);
            command.Parameters.AddWithValue("@unit", Unit);
            command.Parameters.AddWithValue("@price", UnitPrice);
            command.Parameters.AddWithValue("@taxTypeId", TaxTypeId);
            command.ExecuteNonQuery();
        }

        public void UpdateItem()
        {

            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE T_ITEM SET ITEM_NAME=@name, ITEM_CODE=@code, UNIT=@unit, " +
                "UNIT_PRICE=@price, TAX_TYPE_ID=@taxTypeId WHERE ITEM_ID=@id", connection);
            command.Parameters.AddWithValue("@name", ItemName);
            command.Parameters.AddWithValue("@code", ItemCode);
            command.Parameters.AddWithValue("@unit", Unit);
            command.Parameters.AddWithValue("@price", UnitPrice);
            command.Parameters.AddWithValue("@taxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@id", ItemId);
            command.ExecuteNonQuery();
        }

        public void DeleteItem()
        {

            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM T_ITEM WHERE ITEM_ID=@id", connection);
            command.Parameters.AddWithValue("@id", ItemId);
            command.ExecuteNonQuery();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // T_DEFAULT_ITEMS テーブルに対応するクラス
    public class DefaultItemsClass : ItemClass, INotifyPropertyChanged, ILoggable
    {
        public int DefaultItemsId { get; set; }

        // ItemOrder
        private int _ItemOrder = 0;
        public int ItemOrder
        {
            get => _ItemOrder;
            set
            {
                _ItemOrder = value;
                OnPropertyChanged(nameof(ItemOrder));
            }
        }

        // ItemId
        // ItemName
        // UnitPrice
        // Quantity
        private int _Quantity = 1;
        public int Quantity
        {
            get => _Quantity;
            set
            {
                _Quantity = value;
                OnPropertyChanged(nameof(Quantity));
                ReTotal();
            }
        }

        // Unit
        // ItemSubTotal
        private int _ItemSubTotal = 0;
        public int ItemSubTotal
        {
            get => _ItemSubTotal;
            set
            {
                _ItemSubTotal = UnitPrice * Quantity;
                OnPropertyChanged(nameof(ItemSubTotal));
            }
        }

        // TaxTypeId
        // SelectedTax
        private TaxTypeClass? _selectedTax;
        public TaxTypeClass? SelectedTax
        {
            get => _selectedTax;
            set
            {
                if (_selectedTax != value)
                {
                    _selectedTax = value;
                    OnPropertyChanged(nameof(SelectedTax));
                    if (_selectedTax != null && TaxTypeId != _selectedTax.TaxTypeId)
                    {
                        TaxTypeId = _selectedTax.TaxTypeId;
                        OnPropertyChanged(nameof(TaxTypeId));
                        TaxTypeName = _selectedTax.TaxTypeName;
                    }
                    ReTotal();
                }
            }
        }

        // TaxTypeId
        // Tax
        private int _Tax = 0;
        public int Tax
        {
            get => _Tax;
            set
            {
                if (_selectedTax == null) return;
                _Tax = (int)Math.Round(_selectedTax.TaxRate * _ItemSubTotal, 0, MidpointRounding.ToEven);
                OnPropertyChanged(nameof(Tax));
            }
        }

        // ItemTotal
        private int _ItemTotal = 0;
        public int ItemTotal
        {
            get => _ItemTotal;
            set
            {
                _ItemTotal = _ItemSubTotal + Tax;
                OnPropertyChanged(nameof(ItemTotal));
            }
        }

        public void ReTotal()
        {

            ItemSubTotal = Quantity * UnitPrice;
            var taxRate = SelectedTax?.TaxRate ?? 0;
            Tax = (int)(ItemSubTotal * taxRate);
            ItemTotal = ItemSubTotal + Tax;
        }

        public void SetItem(ItemClass item)
        {

            if (item == null) return;
            ItemId = item.ItemId;
            ItemName = item.ItemName;
            UnitPrice = item.UnitPrice;
            Quantity = 1;
            Unit = item.Unit;
            TaxTypeId = item.TaxTypeId;
            TaxTypeName = new TaxTypeClass().GetTaxTypeName(TaxTypeId);

        }

        public static List<DefaultItemsClass> GetDefaultItems()
        {
            var items = new List<DefaultItemsClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_DEFAULT_ITEMS", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new DefaultItemsClass
                {
                    DefaultItemsId = reader.GetInt32("DEFAULT_ITEMS_ID"),
                    ItemOrder = reader.GetInt32("ITEM_ORDER"),
                    ItemId = reader.GetInt32("ITEM_ID"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    Quantity = reader.GetInt32("QUANTITY"),
                    Unit = reader.GetString("UNIT"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID")
                };
                items.Add(item);
            }
            return items;
        }

        public InvoiceItemClass ToInvoiceItem()
        {
            return new InvoiceItemClass()
            {
                ItemOrder = ItemOrder,
                ItemId = ItemId,
                ItemName = ItemName,
                UnitPrice = UnitPrice,
                Quantity = Quantity,
                Unit = Unit,
                TaxTypeId = TaxTypeId,
                TaxTypeName = TaxTypeName,
            };
        }

        public static void AddDefaultItems(List<DefaultItemsClass> items)
        {
            var unitOfWork = new UnitOfWork();
            CrearDefaultitemsTable(unitOfWork);
            var command = unitOfWork.CreateCommand();
            command.CommandText = "INSERT INTO T_DEFAULT_ITEMS (ITEM_ORDER, ITEM_ID, UNIT_PRICE, QUANTITY, UNIT, TAX_TYPE_ID) " + "\r\n" + "VALUES (@itemOrder, @itemId, @unitPrice, @quantity, @unit, @taxTypeId)";
            foreach (var item in items)
            {
                command.Parameters.AddWithValue("@itemOrder", item.ItemOrder);
                command.Parameters.AddWithValue("@itemId", item.ItemId);
                command.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue("@unit", item.Unit);
                command.Parameters.AddWithValue("@taxTypeId", item.TaxTypeId);
                command.ExecuteNonQuery();
            }
        }

        public static bool CrearDefaultitemsTable(UnitOfWork unitOfWork)
        {
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var command = unitOfWork.CreateCommand();
                command.CommandText = "TRUNCATE TABLE T_DEFAULT_ITEMS";
                command.ExecuteNonQuery();
                return true;
            }, unitOfWork);
        }

    }

}

using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Invoice
{
    // T_INVOICE_ITEMS テーブルに対応するクラス
    public class InvoiceItemClass : ItemClass, INotifyPropertyChanged, ILoggable
    {

        //public EventHandler ItemChanged;
        //public EventHandler ItemAdded;
        //public EventHandler ItemDeleted;

        //public DataGridComboBoxColumn ComboboxCulumn { get; set; }

        // InvoiceItemI
        private int _InvoiceItemId = 0;
        public int InvoiceItemId
        {
            get => _InvoiceItemId;
            set
            {
                _InvoiceItemId = value;
                OnPropertyChanged(nameof(InvoiceItemId));
            }
        }

        //InvoiceId
        private int _InvoiceId = 0;
        public int InvoiceId
        {
            get => _InvoiceId;
            set
            {
                _InvoiceId = value;
                OnPropertyChanged(nameof(InvoiceId));
            }
        }

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

        public override int UnitPrice
        {
            get => base.UnitPrice;
            set
            {
                if (base.UnitPrice == value) return;
                base.UnitPrice = value;
                ReTotal();
            }
        }

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

        // ItemSubTotal
        public int ItemSubTotal
        {
            get => UnitPrice * Quantity;
        }

        // SelectedTax
        public TaxTypeClass SelectedTax
        {
            get => TaxTypeClass.TaxTypes.FirstOrDefault(t => t.TaxTypeId == TaxTypeId);
        }

        // Tax
        public int Tax
        {
            get => (int)(ItemSubTotal * (SelectedTax?.TaxRate ?? 0));
        }

        // ItemTotal
        public int ItemTotal
        {
            get => ItemSubTotal + Tax;
        }

        public void ReTotal()
        {

            OnPropertyChanged(nameof(UnitPrice));
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Unit));
            OnPropertyChanged(nameof(ItemSubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(ItemTotal));
        }

        private ItemClass _selectedItem;
        public ItemClass SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    ReTotal();
                    if (_selectedItem != null)
                    {
                        // 選択されたアイテムの情報をInvoiceItemのプロパティに反映
                        ItemId = _selectedItem.ItemId;
                        ItemName = _selectedItem.ItemName;
                        UnitPrice = _selectedItem.UnitPrice;
                        Quantity = 1;
                        Unit = _selectedItem.Unit;
                        TaxTypeId = _selectedItem.TaxTypeId;
                        TaxTypeName = new TaxTypeClass().getTaxTypeName(TaxTypeId);
                    }
                }
            }
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
            TaxTypeName = new TaxTypeClass().getTaxTypeName(TaxTypeId);
            ReTotal();
        }

        public static List<InvoiceItemClass> GetInvoiceItemsByInvoiceId(int invoiceId, bool forCopyInvoice = false)
        {
            var items = new List<InvoiceItemClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM T_INVOICE_ITEMS WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new InvoiceItemClass
                {
                    InvoiceItemId = reader.GetInt32("INVOICE_ITEM_ID"),
                    InvoiceId = forCopyInvoice ? 0 : reader.GetInt32("INVOICE_ID"),
                    ItemOrder = reader.GetInt32("ITEM_ORDER"),
                    ItemId = reader.GetInt32("ITEM_ID"),
                    ItemName = reader.GetString("ITEM_NAME"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    Quantity = reader.GetInt32("QUANTITY"),
                    Unit = reader.IsDBNull("UNIT") ? "" : reader.GetString("UNIT"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID"),
                };
                items.Add(item);
            }
            return items;
        }

        public static List<InvoiceItemClass> GetInvoiceItems()
        {
            var items = new List<InvoiceItemClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM T_INVOICE_ITEMS";
            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new InvoiceItemClass
                {
                    InvoiceItemId = reader.GetInt32("INVOICE_ITEM_ID"),
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    ItemOrder = reader.GetInt32("ITEM_ORDER"),
                    ItemId = reader.GetInt32("ITEM_ID"),
                    ItemName = reader.GetString("ITEM_NAME"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    Quantity = reader.GetInt32("QUANTITY"),
                    Unit = reader.IsDBNull("UNIT") ? "" : reader.GetString("UNIT"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID"),
                };
                items.Add(item);
            }
            return items;

        }

        public static bool CompareInvoiceItems(InvoiceItemClass oldItem, InvoiceItemClass? newItem)
        {
            if (newItem == null) return false;
            return oldItem.InvoiceItemId == newItem.InvoiceItemId &&
                   oldItem.InvoiceId == newItem.InvoiceId &&
                   oldItem.ItemOrder == newItem.ItemOrder &&
                   oldItem.ItemId == newItem.ItemId &&
                   oldItem.ItemName == newItem.ItemName &&
                   oldItem.UnitPrice == newItem.UnitPrice &&
                   oldItem.Quantity == newItem.Quantity &&
                   oldItem.Unit == newItem.Unit &&
                   oldItem.TaxTypeId == newItem.TaxTypeId;
        }

        public static void ProcessInvoiceItemsBatch(List<InvoiceItemClass> deleteItems, List<InvoiceItemClass> updateItems, List<InvoiceItemClass> addItems, int invoiceId, UnitOfWork unitOfWork)
        {
            UnitOfWork.ExecuteWithTransaction(uow =>
            {
                // 一括削除
                if (deleteItems != null && deleteItems.Count > 0)
                {
                    DeleteInvoiceItemsBatch(deleteItems, uow);
                }

                // 一括更新
                if (updateItems != null && updateItems.Count > 0)
                {
                    UpdateInvoiceItemsBatch(updateItems, uow);
                }

                // 一括挿入
                if (addItems != null && addItems.Count > 0)
                {
                    AddInvoiceItemsBatch(invoiceId, addItems, uow);
                }

                return true;
            }, unitOfWork);
        }

        public static void AddInvoiceItems(int invoiceId, ObservableCollection<InvoiceItemClass> ItemList, UnitOfWork unitOfWork)
        {
            int order = 1;
            string query = @"INSERT INTO T_INVOICE_ITEMS (INVOICE_ID, ITEM_ORDER, ITEM_ID, ITEM_NAME, UNIT_PRICE, QUANTITY, UNIT,ITEM_SUBTOTAL, TAX_TYPE_ID, TAX, ITEM_TOTAL) "
                + "\r\n" +
                "VALUES (@InvoiceId, @ItemOrder, @ItemId, @ItemName, @UnitPrice, @Quantity, @Unit, @ItemSubtotal, @TaxTypeId, @Tax, @ItemTotal)";
            var command = unitOfWork.CreateCommand(query);
            foreach (var item in ItemList)
            {
                item.ItemOrder = order++;
                command.Parameters.AddWithValue("@InvoiceId", invoiceId);
                command.Parameters.AddWithValue("@ItemOrder", item.ItemOrder);
                command.Parameters.AddWithValue("@ItemId", item.ItemId);
                command.Parameters.AddWithValue("@ItemName", item.ItemName);
                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@Unit", item.Unit);
                command.Parameters.AddWithValue("@ItemSubtotal", item.ItemSubTotal);
                command.Parameters.AddWithValue("@TaxTypeId", item.TaxTypeId);
                command.Parameters.AddWithValue("@Tax", item.Tax);
                command.Parameters.AddWithValue("@ItemTotal", item.ItemTotal);
                command.ExecuteNonQuery();
                item.InvoiceItemId = (int)command.LastInsertedId;
                command.Parameters.Clear();
            }


        }
        public static void AddInvoiceItems(ObservableCollection<InvoiceItemClass> ItemList, int invoiceId, UnitOfWork unitOfWork)
        {
            AddInvoiceItemsBatch(invoiceId, ItemList.ToList(), unitOfWork);
        }

        public static void AddInvoiceItemsBatch(int invoiceId, List<InvoiceItemClass> items, UnitOfWork unitOfWork)
        {
            if (items == null || items.Count == 0) return;

            var query = new StringBuilder("INSERT INTO T_INVOICE_ITEMS (INVOICE_ID, ITEM_ORDER, ITEM_ID, ITEM_NAME, UNIT_PRICE, QUANTITY, UNIT, ITEM_SUBTOTAL, TAX_TYPE_ID, TAX, ITEM_TOTAL) VALUES ");

            foreach (var item in items)
            {
                query.Append($"({invoiceId}, {item.ItemOrder}, {item.ItemId}, '{item.ItemName}', {item.UnitPrice}, {item.Quantity}, '{item.Unit}', {item.ItemSubTotal}, {item.TaxTypeId}, {item.Tax}, {item.ItemTotal}),");
            }

            // 最後のカンマを削除
            query.Length--;

            var command = unitOfWork.CreateCommand(query.ToString());
            command.ExecuteNonQuery();
            var lastInsertedId = (int)command.LastInsertedId;
            var insertedIds = Enumerable.Range(lastInsertedId, items.Count).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].InvoiceItemId = insertedIds[i];
            }

        }

        public static void UpdateInvoiceItem(InvoiceItemClass invoiceItem, UnitOfWork unitOfWork)
        {
            string query = @"UPDATE T_INVOICE_ITEMS SET INVOICE_ID = @InvoiceId, ITEM_ORDER = @ItemOrder, ITEM_ID = @ItemId, ITEM_NAME = @ItemName, UNIT_PRICE = @UnitPrice, QUANTITY = @Quantity, UNIT = @Unit, ITEM_SUBTOTAL = @ItemSubtotal, TAX_TYPE_ID = @TaxTypeId, TAX = @Tax, ITEM_TOTAL = @ItemTotal WHERE INVOICE_ITEM_ID = @InvoiceItemId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@InvoiceId", invoiceItem.InvoiceId);
            command.Parameters.AddWithValue("@ItemOrder", invoiceItem.ItemOrder);
            command.Parameters.AddWithValue("@ItemId", invoiceItem.ItemId);
            command.Parameters.AddWithValue("@ItemName", invoiceItem.ItemName);
            command.Parameters.AddWithValue("@UnitPrice", invoiceItem.UnitPrice);
            command.Parameters.AddWithValue("@Quantity", invoiceItem.Quantity);
            command.Parameters.AddWithValue("@Unit", invoiceItem.Unit);
            command.Parameters.AddWithValue("@ItemSubtotal", invoiceItem.ItemSubTotal);
            command.Parameters.AddWithValue("@TaxTypeId", invoiceItem.TaxTypeId);
            command.Parameters.AddWithValue("@Tax", invoiceItem.Tax);
            command.Parameters.AddWithValue("@ItemTotal", invoiceItem.ItemTotal);
            command.Parameters.AddWithValue("@InvoiceItemId", invoiceItem.InvoiceItemId);
            command.ExecuteNonQuery();
        }

        private static List<InvoiceItemClass> CreateUpdateItems(List<InvoiceItemClass> notExistNewItems, List<InvoiceItemClass> notExistOldItems, int invoiceId)
        {
            return notExistNewItems.Zip(notExistOldItems, (newItem, oldItem) =>
            {
                var addingItem = newItem.DeepClone();
                addingItem.InvoiceItemId = oldItem.InvoiceItemId;
                addingItem.InvoiceId = invoiceId;
                return addingItem;
            }).ToList();
        }

        public static void UpdateInvoiceItems(int invoiceId, ObservableCollection<InvoiceItemClass> newItems, UnitOfWork unitOfWork)
        {
            var oldItems = GetInvoiceItemsByInvoiceId(invoiceId);
            if (oldItems.Count == 0)
            {
                AddInvoiceItems(newItems, invoiceId, unitOfWork);
                return;
            }

            bool isSameCount = newItems.Count == oldItems.Count; // 新旧アイテムの数が同じかどうか
            // 存在しないアイテムを特定
            // oldItemsに存在しないnewItemのリスト
            var notExistNewItems = newItems.Where(newItem => !oldItems.Any(oldItem => CompareInvoiceItems(oldItem, newItem))).ToList();
            // newItemsに存在しないoldItemのリスト
            var notExistOldItems = oldItems.Where(oldItem => !newItems.Any(newItem => CompareInvoiceItems(oldItem, newItem))).ToList();

            var updateItems = CreateUpdateItems(notExistNewItems, notExistOldItems, invoiceId);

            // 追加リストや削除リストの処理
            var addingItems = notExistNewItems.Skip(notExistOldItems.Count).ToList();
            var deleteItems = notExistOldItems.Skip(notExistNewItems.Count).ToList();

            ProcessInvoiceItemsBatch(deleteItems, updateItems, addingItems, invoiceId, unitOfWork);
        }

        public static void UpdateInvoiceItemsBatch(List<InvoiceItemClass> items, UnitOfWork unitOfWork)
        {
            if (items == null || items.Count == 0) return;

            // 更新対象の列を定義
            var columnsToUpdate = new Dictionary<string, Func<InvoiceItemClass, object>>
                {
                    { "ITEM_ORDER", item => item.ItemOrder },
                    { "QUANTITY", item => item.Quantity },
                    { "UNIT_PRICE", item => item.UnitPrice },
                    { "ITEM_SUBTOTAL", item => item.ItemSubTotal },
                    { "TAX_TYPE_ID", item => item.TaxTypeId },
                    { "TAX", item => item.Tax },
                    { "ITEM_TOTAL", item => item.ItemTotal },
                    { "UNIT", item => item.Unit }
                };

            // クエリの動的生成
            var queryBuilder = new StringBuilder("UPDATE T_INVOICE_ITEMS SET ");

            foreach (var column in columnsToUpdate.Keys)
            {
                queryBuilder.Append($"{column} = CASE ");
                foreach (var item in items)
                {
                    queryBuilder.Append($"WHEN INVOICE_ITEM_ID = {item.InvoiceItemId} THEN @{column}_{item.InvoiceItemId} ");
                }
                queryBuilder.Append("END, ");
            }

            // 最後のカンマを削除
            queryBuilder.Length -= 2;

            // WHERE 条件を追加
            var idList = string.Join(",", items.Select(i => i.InvoiceItemId));
            queryBuilder.Append($" WHERE INVOICE_ITEM_ID IN ({idList})");

            // コマンドの作成
            var command = unitOfWork.CreateCommand(queryBuilder.ToString());

            // パラメータを追加
            foreach (var column in columnsToUpdate)
            {
                foreach (var item in items)
                {
                    var parameterName = $"@{column.Key}_{item.InvoiceItemId}";
                    var value = column.Value(item) ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, value);
                }
            }

            // クエリの実行
            command.ExecuteNonQuery();
        }

        public static void DeleteInvoiceItemsByInvoiceId(int invoiceId, UnitOfWork unitOfWork)
        {
            string query = "DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ID = @InvoiceId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            command.ExecuteNonQuery();
        }

        public static void DeleteInvoiceItems(List<InvoiceItemClass> items, UnitOfWork unitOfWork)
        {
            foreach (var item in items)
            {
                DeleteInvoiceItemByInvoiceItemId(item.InvoiceItemId, unitOfWork);
            }
        }

        public static void DeleteInvoiceItemByInvoiceItemId(int invoiceItemId, UnitOfWork unitOfWork)
        {
            string query = "DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ITEM_ID = @InvoiceItemId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@InvoiceItemId", invoiceItemId);
            command.ExecuteNonQuery();
        }

        public static void DeleteInvoiceItemsBatch(List<InvoiceItemClass> items, UnitOfWork unitOfWork)
        {
            if (items == null || items.Count == 0) return;

            // InvoiceItemId を抽出してカンマ区切りの文字列に変換
            var idList = string.Join(",", items.Select(item => item.InvoiceItemId));
            var query = $"DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ITEM_ID IN ({idList})";

            var command = unitOfWork.CreateCommand(query);
            command.ExecuteNonQuery();
        }

        public InvoiceItemClass DeepClone(InvoiceItemClass item)
        {
            return new InvoiceItemClass()
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                ItemCode = item.ItemCode,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                TaxTypeId = item.TaxTypeId,
                TaxTypeName = item.TaxTypeName,
                InvoiceItemId = item.InvoiceItemId,
                InvoiceId = item.InvoiceId,
                ItemOrder = item.ItemOrder,
                Quantity = item.Quantity,
                SelectedItem = item.SelectedItem,
            };
        }

        public InvoiceItemClass DeepClone()
        {

            return new InvoiceItemClass
            {
                ItemId = this.ItemId,
                ItemName = this.ItemName,
                ItemCode = this.ItemCode,
                Unit = this.Unit,
                UnitPrice = this.UnitPrice,
                TaxTypeId = this.TaxTypeId,
                TaxTypeName = this.TaxTypeName,
                InvoiceItemId = this.InvoiceItemId,
                InvoiceId = this.InvoiceId,
                ItemOrder = this.ItemOrder,
                Quantity = this.Quantity,
                SelectedItem = this.SelectedItem,
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

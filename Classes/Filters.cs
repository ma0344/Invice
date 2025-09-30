using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Classes
{
    public class BalanceFilterParam : ILoggable
    {
        public int? BalanceId { get; set; } = null;
        public int? CustomerId { get; set; } = null;
        public int? InvoiceId { get; set; } = null;
        public int? PaymentId { get; set; } = null;
        public int? DepositId { get; set; } = null;
        public string? SlipNumber { get; set; } = null;
        public int? DebOrCreId { get; set; } = null;
        public DateTime? TransactionDate { get; set; } = null;
        public int? TransactionTypeId { get; set; } = null;
        public int? TransactionAmount { get; set; } = null;
    }

    public class CustomerFilterParam
    {
        public int? CustomerId { get; set; } = null;
        public string? CustomerName { get; set; } = null;
        public string? CustomerKana { get; set; } = null;
        public int? CustomerBalance { get; set; } = null;
        public bool? CustomerVisible { get; set; } = null;
    }
    
    public class InvoiceFiterParam
    {

        private int _CustomerId = 0;
        public int CustomerId
        {
            get => _CustomerId;
            set
            {
                _CustomerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }

        private int _InvoiceStatusId = 0;
        public int InvoiceStatusId
        {
            get => _InvoiceStatusId;
            set
            {
                _InvoiceStatusId = value;
                OnPropertyChanged(nameof(InvoiceStatusId));
            }
        }

        private int _TransactionTypeId = 0;
        public int TransactionTypeId
        {
            get => _TransactionTypeId;
            set
            {
                _TransactionTypeId = value;
                OnPropertyChanged(nameof(TransactionTypeId));
            }
        }

        private DateTime? _IssueDate = null;
        public DateTime? IssueDate
        {
            get => _IssueDate;
            set
            {
                _IssueDate = value;
                OnPropertyChanged(nameof(IssueDate));
            }
        }

        private DateTime? _DueDate = null;
        public DateTime? DueDate
        {
            get => _DueDate;
            set
            {
                _DueDate = value;
                OnPropertyChanged(nameof(DueDate));
            }
        }

        private DateTime? _PaymentDate = null;
        public DateTime? PaymentDate
        {
            get => _PaymentDate;
            set
            {
                _PaymentDate = value;
                OnPropertyChanged(nameof(PaymentDate));
            }
        }

        private string? _Subject = string.Empty;
        public string? Subject
        {
            get => _Subject;
            set
            {
                _Subject = value;
                OnPropertyChanged(nameof(Subject));
            }
        }

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

        private int _PaymentId = 0;
        public int PaymentId
        {
            get => _PaymentId;
            set
            {
                _PaymentId = value;
                OnPropertyChanged(nameof(PaymentId));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class PaymentFilterParam
    {
        public int? PaymentId { get; set; } = null;
        public string? SlipNumber { get; set; } = null;
        public int? CustomerId { get; set; } = null;
        public int? InvoiceId { get; set; } = null;
        public int? TransactionTypeId { get; set; } = null;
        public DateTime? PaymentDate { get; set; } = null;
        public int? PaymentAmount { get; set; } = null;
        public string? Subject { get; set; } = null;

    }

}

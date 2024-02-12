﻿namespace BeltmanSoftwareDesign.Shared.Jsons
{
    public class Invoice
    {
        public long id { get; set; }

        public long? InvoiceTypeId { get; set; }
        public string? InvoiceTypeName { get; set; }

        public long? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        
        public long? CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public long? TaxRateId { get; set; }
        public string? TaxRateName { get; set; }
        public double? TaxRatePercentage { get; set; }

        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; }        
        public string Description { get; set; }
        
        public bool IsPayedInCash { get; set; }
        public bool IsPayed { get; set; }

        public DateTime? DatePayed { get; set; }
        public string PaymentCode { get; set; }

        public InvoiceWorkorder[] InvoiceWorkorders { get; set; }
        //public Transaction[] Transacties { get; set; }

        //public virtual ICollection<InvoiceProduct> InvoiceProducts { get; set; }
        //public virtual ICollection<InvoiceRow> InvoiceRegels { get; set; }
        //public virtual ICollection<InvoiceAttachment> InvoiceAttachments { get; set; }
        //public virtual ICollection<InvoiceEmail> InvoiceEmails { get; set; }
        //public virtual ICollection<TransactionLog> TransactieLogs { get; set; }
        //public virtual ICollection<BankStatementInvoice> BankStatementInvoices { get; set; }

    }
}
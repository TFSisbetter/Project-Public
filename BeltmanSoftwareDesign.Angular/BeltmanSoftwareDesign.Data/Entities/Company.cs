using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltmanSoftwareDesign.Data.Entities
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? CountryId { get; set; }
        public virtual Country? Country { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = string.Empty;
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;
        [StringLength(256)]
        public string? Address { get; set; }
        [StringLength(256)]
        public string? Postalcode { get; set; }
        [StringLength(256)]
        public string? Place { get; set; }
        [StringLength(256)]
        public string? PhoneNumber { get; set; }
        [StringLength(256)]
        public string? Website { get; set; }
        [StringLength(64)]
        public string? BtwNumber { get; set; }
        [StringLength(64)]
        public string? KvkNumber { get; set; }
        [StringLength(64)]
        public string? Iban { get; set; }
        [StringLength(256)]
        public string? PayNL_ApiToken { get; set; }
        [StringLength(256)]
        public string? PayNL_ServiceId { get; set; }

        public DateTime DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public DateTime? DateDelete { get; set; }

        public virtual ICollection<CompanyUser>? CompanyUsers { get; set; } = new List<CompanyUser>();
        public virtual ICollection<User>? CurrentUsers { get; set; }
        public virtual ICollection<Workorder>? Workorders { get; set; }
        public virtual ICollection<Customer>? Customers { get; set; }
        public virtual ICollection<Project>? Projects { get; set; }
        public virtual ICollection<Invoice>? Invoices { get; set; }
        public virtual ICollection<InvoiceType>? InvoiceTypes { get; set; }
        public virtual ICollection<TaxRate>? TaxRates { get; set; }
        public virtual ICollection<Rate>? Rates { get; set; }

        //public virtual ICollection<Product> Products { get; set; }

        //public virtual ICollection<Supplier> Suppliers { get; set; }
        //public virtual ICollection<ExpenseType> ExpenseTypes { get; set; }
        //public virtual ICollection<Expense> Expenses { get; set; }
        //public virtual ICollection<WorkRate> WerkTarieven { get; set; }
        //public virtual ICollection<BankStatement> BankStatements { get; set; }
        //public virtual ICollection<Residence> Huisvestingen { get; set; }
        //public virtual ICollection<Setting> Instellingen { get; set; }
        //public virtual ICollection<TrafficRegistration> RitRegistraties { get; set; }
        //public virtual ICollection<ExpenseAttachment> ExpenseAttachments { get; set; }
        //public virtual ICollection<Document> Documents { get; set; }
        //public virtual ICollection<DocumentAttachment> DocumentAttachments { get; set; }
    }
}
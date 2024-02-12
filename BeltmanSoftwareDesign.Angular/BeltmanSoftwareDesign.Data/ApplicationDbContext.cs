﻿using BeltmanSoftwareDesign.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeltmanSoftwareDesign.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<BankStatement> BankStatements { get; set; }
        public DbSet<BankStatementExpense> BankStatementExpenses { get; set; }
        public DbSet<BankStatementInvoice> BankStatementInvoices { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }
        public DbSet<ClientBearer> ClientBearers { get; set; }
        public DbSet<ClientDevice> ClientDevices { get; set; }
        public DbSet<ClientDeviceProperty> ClientDeviceProperties { get; set; }
        public DbSet<ClientIpAddress> ClientLocations { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; }
        public DbSet<ExpenseProduct> ExpenseProducts { get; set; }
        public DbSet<ExpenseTaxRatePrice> ExpenseTaxRatePrices { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceAttachment> InvoiceAttachments { get; set; }
        public DbSet<InvoiceEmail> InvoiceEmails { get; set; }
        public DbSet<InvoiceProduct> InvoiceProducts { get; set; }
        public DbSet<InvoiceRow> InvoiceRows { get; set; }
        public DbSet<InvoiceType> InvoiceTypes { get; set; }
        public DbSet<InvoiceWorkorder> InvoiceWorkorders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Rate> WorkRates { get; set; }
        public DbSet<Residence> Residences { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<TrafficRegistration> TrafficRegistrations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Workorder> Workorders { get; set; }
        public DbSet<WorkorderAttachment> WorkorderAttachments { get; set; }



        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionLog> TransactionLogs { get; set; }


        public DbSet<Change> Changes { get; set; }
        public DbSet<ChangePath> ChangePaths { get; set; }
        public DbSet<ChangeSet> ChangeSets { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentAttachment> DocumentAttachments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<Technology> Technologies { get; set; }
        public DbSet<TechnologyAttachment> TechnologyAttachments { get; set; }

        public DbSet<Experience> Experiences { get; set; }
        public DbSet<ExperienceAttachment> ExperienceAttachments { get; set; }
        public DbSet<ExperienceTechnology> ExperienceTechnologies { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientBearer>()
                .HasOne(cb => cb.ClientDevice)
                .WithMany(cd => cd.ClientBearers)
                .HasForeignKey(cb => cb.ClientDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClientBearer>()
                .HasOne(cb => cb.ClientIpAddress)
                .WithMany(cd => cd.ClientBearers)
                .HasForeignKey(cb => cb.ClientIpAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClientBearer>()
                .HasOne(cb => cb.User)
                .WithMany(u => u.ClientBearers)
                .HasForeignKey(cb => cb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClientDeviceProperty>()
                .HasOne(p => p.ClientDevice)
                .WithMany(d => d.ClientDeviceProperties)
                .HasForeignKey(p => p.ClientDeviceId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<User>()
                .HasOne(u => u.CurrentCompany)
                .WithMany(c => c.CurrentUsers)
                .HasForeignKey(p => p.CurrentCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CompanyUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.CompanyUsers)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyUser>()
                .HasOne(u => u.User)
                .WithMany(c => c.CompanyUsers)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workorder>()
                .HasOne(u => u.Rate)
                .WithMany(c => c.Workorders)
                .HasForeignKey(p => p.RateId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workorder>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Workorders)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workorder>()
                .HasOne(u => u.Project)
                .WithMany(c => c.Workorders)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workorder>()
                .HasOne(u => u.Customer)
                .WithMany(c => c.Workorders)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InvoiceWorkorder>()
                .HasOne(u => u.Workorder)
                .WithMany(c => c.InvoiceWorkorders)
                .HasForeignKey(p => p.WorkorderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceWorkorder>()
                .HasOne(u => u.Invoice)
                .WithMany(c => c.InvoiceWorkorders)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceWorkorder>()
                .HasOne(u => u.Rate)
                .WithMany(c => c.InvoiceWorkorders)
                .HasForeignKey(p => p.RateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkorderAttachment>()
                .HasOne(u => u.Workorder)
                .WithMany(c => c.WorkorderAttachments)
                .HasForeignKey(p => p.WorkorderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasOne(u => u.Country)
                .WithMany(c => c.Customers)
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Customer>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Customers)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasOne(u => u.Customer)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Project>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasOne(u => u.Country)
                .WithMany(c => c.Companies)
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceRow>()
                .HasOne(u => u.Invoice)
                .WithMany(c => c.InvoiceRows)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Invoices)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Invoices)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.InvoiceType)
                .WithMany(c => c.Invoices)
                .HasForeignKey(p => p.InvoiceTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.Project)
                .WithMany(c => c.Invoices)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.TaxRate)
                .WithMany(c => c.Invoices)
                .HasForeignKey(p => p.TaxRateId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(u => u.SetToPayed_By_CompanyUser)
                .WithMany(c => c.Invoices_SetToPayed)
                .HasForeignKey(p => p.SetToPayed_By_CompanyUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InvoiceType>()
                .HasOne(u => u.Company)
                .WithMany(c => c.InvoiceTypes)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaxRate>()
                .HasOne(u => u.Company)
                .WithMany(c => c.TaxRates)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaxRate>()
                .HasOne(u => u.Country)
                .WithMany(c => c.TaxRates)
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rate>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Rates)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rate>()
                .HasOne(u => u.TaxRate)
                .WithMany(c => c.Rates)
                .HasForeignKey(p => p.TaxRateId)
                .OnDelete(DeleteBehavior.NoAction);



            base.OnModelCreating(modelBuilder);
        }
    }
}
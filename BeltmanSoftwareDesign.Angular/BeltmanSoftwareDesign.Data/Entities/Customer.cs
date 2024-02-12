﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeltmanSoftwareDesign.Data.Entities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public long? CountryId { get; set; }
        public virtual Country? Country { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Postalcode { get; set; }
        public string Place { get; set; }
        public string PhoneNumber { get; set; }
        public string InvoiceEmail { get; set; }

        public bool Publiekelijk { get; set; }
        
        public virtual ICollection<Workorder>? Workorders { get; set; }
        public virtual ICollection<Project>? Projects { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }

        //public virtual ICollection<Expense> Expenses { get; set; }
        //public virtual ICollection<Document> Documents { get; set; }

        public override string ToString()
            => Name;
    }
}
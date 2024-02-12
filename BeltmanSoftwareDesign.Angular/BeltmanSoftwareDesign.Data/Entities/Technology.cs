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
    public class Technology
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long CompanyId { get; set; }

        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(1024)]
        public string Description { get; set; }
        public bool IsProgrammeerTaal { get; set; }
        //public virtual Company Company { get; set; }
        //public virtual ICollection<TechnologyAttachment> TechnologyAttachments { get; set; }
        //public virtual ICollection<ExperienceTechnology> ExperienceTechnologyen { get; set; }



    }
}
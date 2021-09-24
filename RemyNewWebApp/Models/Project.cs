using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RemyNewWebApp.Models
{
    public class Project
    {
        // -- Primary Key -- //

        public int Id { get; set; }

        // -- Properties -- //

        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Start Date")]
        public DateTimeOffset StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTimeOffset EndDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created")]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Updated")]
        public DateTimeOffset? Updated { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile ImageFormFile { get; set; }

        [DisplayName("File Name")]
        public string ImageFileName { get; set; }

        public byte[] ImageFileData { get; set; } 

        [DisplayName("File Extension")]
        public string ImageFileContentType { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        // -- Foreign Keys -- //

        [DisplayName("Company")]
        public int? CompanyId { get; set; }

        [DisplayName("Priority")]
        public int? ProjectPriorityId { get; set; }

        // -- Navigation Properties -- //

        public virtual Company Company { get; set; }

        public virtual ProjectPriority ProjectPriority { get; set; }

        public ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}

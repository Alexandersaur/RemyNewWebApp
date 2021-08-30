using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Models
{
    public class TicketAttachment
    {
        // -- Primary Key -- //

        public int Id { get; set; }

        // -- Properties -- //

        [DisplayName("Date")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("File Description")]
        public string Description { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile FormFile { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }

        [DisplayName("File Extension")]
        public string FileContentType { get; set; }

        public byte[] FileData { get; set; }

        // -- Foreign Keys -- //

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        // -- Navigation Properties -- //

        public virtual Ticket Ticket { get; set; }

        public virtual BTUser User { get; set; }
    }
}

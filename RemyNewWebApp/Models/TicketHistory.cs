using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Models
{
    public class TicketHistory
    {
        // -- Primary Key -- //
        public int Id { get; set; }

        // -- Properties -- //

        [DisplayName("Updated Item")]
        public string Property { get; set; }

        [DisplayName("Previous")]
        public string OldValue { get; set; }

        [DisplayName("Current")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Description of Change")]
        public string Description { get; set; }

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

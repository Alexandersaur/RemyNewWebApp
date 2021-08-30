using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Models
{
    public class TicketPriority
    {
        // -- Primary Key -- //
        public int Id { get; set; }

        // -- Properties -- //

        [DisplayName("Priority Name")]
        public string Name { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Models
{
    public class TicketType
    {
        // -- Primary Key -- //
        public int Id { get; set; }

        // -- Properties -- //

        [DisplayName("Type Name")]
        public string Name { get; set; }
    }
}

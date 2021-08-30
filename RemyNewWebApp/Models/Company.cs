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
    public class Company
    {
        // -- Primary Key -- //
        public int Id { get; set; }

        // -- Properties -- //

        [DisplayName("Company Name")]
        public string Name { get; set; }

        [DisplayName("Company Description")]
        public string Description { get; set; }

        // -- Navigation Properties -- //

        public ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();

    }
}

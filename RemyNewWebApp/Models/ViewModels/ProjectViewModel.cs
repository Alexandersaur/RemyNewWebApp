using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Models.ViewModels
{
    public class ProjectViewModel
    {
        public Project Project { get; set; }
        public BTUser ProjectManager { get; set; }
    }
}

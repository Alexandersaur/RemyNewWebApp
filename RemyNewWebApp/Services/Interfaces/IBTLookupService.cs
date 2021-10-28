using RemyNewWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Services.Interfaces
{
    public interface IBTLookupService
    {
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();
        public Task<List<TicketStatus>> GetTicketStatusesAsync();
        public Task<List<TicketType>> GetTicketTypesAsync();
        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();


    }
}

using Microsoft.EntityFrameworkCore;
using RemyNewWebApp.Data;
using RemyNewWebApp.Models;
using RemyNewWebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Services
{
    public class BTLookupService : IBTLookupService
    {
        private readonly ApplicationDbContext _context;
        public BTLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationDbContext Context { get; }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketType>> GetTicketTypesAsync()
        {
            throw new NotImplementedException();
        }
    }
}

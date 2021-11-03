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

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

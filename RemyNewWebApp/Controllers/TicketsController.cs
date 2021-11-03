using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemyNewWebApp.Data;
using RemyNewWebApp.Extensions;
using RemyNewWebApp.Models;
using RemyNewWebApp.Models.Enums;
using RemyNewWebApp.Models.ViewModels;
using RemyNewWebApp.Services;
using RemyNewWebApp.Services.Interfaces;

namespace RemyNewWebApp.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTTicketHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTLookupService _lookupService;

        public TicketsController(ApplicationDbContext context,
                                 IBTProjectService projectService,
                                 UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTTicketHistoryService historyService,
                                 IBTNotificationService notificationService,
                                 IBTLookupService lookupService)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _ticketService = ticketService;
            _historyService = historyService;
            _notificationService = notificationService;
            _lookupService = lookupService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MY Tickets
        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);
            return View(tickets);
        }

        // GET: ALL Tickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            if (User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
            {
                return View(tickets.Where(t => t.Archived == false));
            }
            else
            {
                return View(tickets);
            }


            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            return View(tickets);
        }

        // GET: Archived Tickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);
            return View(tickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Unassigned Tickets
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string btUserId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);
            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }
            else
            {
                List<Ticket> pmTickets = new();
                foreach(Ticket ticket in tickets)
                {
                    if(await _projectService.IsAssignedProjectManagerAsync(btUserId, ticket.ProjectId))
                    {
                        pmTickets.Add(ticket);
                    }
                }
                return View(pmTickets);
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        public IActionResult ShowFile(int id)
        {
            TicketAttachment ticketAttachment = _context.TicketAttachments.Find(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            //Get current user's companyId
            int companyId = User.Identity.GetCompanyId().Value;
            //Get current user
            BTUser btUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole(Roles.Admin.ToString()))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = btUser.Id;
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(BTTicketStatus.New.ToString())).Value;
                await _ticketService.AddNewTicketAsync(ticket);
                #region Ticket History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _historyService.AddHistoryAsync(null, newTicket, btUser.Id);

                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                int companyId = User.Identity.GetCompanyId().Value;
                Notification notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "New Ticket",
                    Message = $"New Ticket: {ticket?.Title}, was created by {btUser?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = btUser?.Id,
                    RecipientId = projectManager?.Id
                };
                if (projectManager != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationsByRole(notification, companyId, Roles.Admin.ToString());
                }
                #endregion
                return RedirectToAction(nameof(AllTickets));
            }
            if (User.IsInRole(Roles.Admin.ToString()))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(btUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,TicketTypeId,TicketStatusId,TicketPriorityId,ProjectId,Created,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                try
                {
                    ticket.Updated = DateTimeOffset.Now;

                    await _ticketService.UpdateTicketAsync(ticket);

                    //TODO: Send notification
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExistsAsync(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                Ticket newticket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newticket, btUser.Id);

                return RedirectToAction(nameof(AllTickets));
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int id)
        {
            AssignDeveloperViewModel model = new();
            model.Ticket = await _ticketService.GetTicketByIdAsync(id);
            model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, Roles.Developer.ToString()),
                                              "Id", "FullName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId != null)
            {
                //TODO: Get oldticket
                try
                {
                    await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            //TODO: Add history
            //TODO: Get Newticket
            //TODO: Send notifications
            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket.Id });
        }


        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(AllTickets));
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = true;
            await _ticketService.UpdateTicketAsync(ticket);
            return RedirectToAction(nameof(AllTickets));
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(AllTickets));
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);
            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExistsAsync(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemyNewWebApp.Data;
using RemyNewWebApp.Models;
using RemyNewWebApp.Services.Interfaces;

namespace RemyNewWebApp.Controllers
{
    public class TicketAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;

        public TicketAttachmentsController(ApplicationDbContext context, IBTFileService fileService, UserManager<BTUser> userManager)
        {
            _context = context;
            _fileService = fileService;
            _userManager = userManager;
        }

        // GET: TicketAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketAttachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachments
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FormFile,Image,Description,Created,TicketId,UserId")] TicketAttachment ticketAttachment)
        {
            if (ModelState.IsValid)
            {
                if (ticketAttachment.FormFile != null)
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;
                }

                ticketAttachment.Created = DateTimeOffset.Now;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                _context.Add(ticketAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttachment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

            // POST: TicketAttachments/Create
            // To protect from overposting attacks, enable the specific properties you want to bind to.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //    [HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Created,Description,FileName,FileContentType,FileData,TicketId,UserId")] TicketAttachment ticketAttachment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(ticketAttachment);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttachment.TicketId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
        //    return View(ticketAttachment);
        //}

        // GET: TicketAttachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachments.FindAsync(id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttachment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Created,Description,FileName,FileContentType,FileData,TicketId,UserId")] TicketAttachment ticketAttachment)
        {
            if (id != ticketAttachment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketAttachment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketAttachmentExists(ticketAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttachment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachments
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketAttachment = await _context.TicketAttachments.FindAsync(id);
            _context.TicketAttachments.Remove(ticketAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketAttachmentExists(int id)
        {
            return _context.TicketAttachments.Any(e => e.Id == id);
        }
    }
}

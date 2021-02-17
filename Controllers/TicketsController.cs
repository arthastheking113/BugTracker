using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ticket.Include(t => t.CustomUser).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.CustomUser)
                .Include(t => t.Priority)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id");
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Id");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,Updated,CustomUserId,ProjectId,StatusId,PriorityId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.CustomUserId);
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Id", ticket.PriorityId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", ticket.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Id", ticket.StatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.CustomUserId);
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Id", ticket.PriorityId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", ticket.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Id", ticket.StatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,Updated,CustomUserId,ProjectId,StatusId,PriorityId,TicketTypeId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.CustomUserId);
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Id", ticket.PriorityId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", ticket.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Id", ticket.StatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.CustomUser)
                .Include(t => t.Priority)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}

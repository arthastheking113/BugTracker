using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin, ProjectManager, Developer")]
    public class TicketHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketHistoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketHistories
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketHistory.Include(t => t.CustomUser).Include(t => t.Ticket);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistory
                .Include(t => t.CustomUser)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketHistory == null)
            {
                return NotFound();
            }

            return View(ticketHistory);
        }

        // GET: TicketHistories/Create
        public IActionResult Create()
        {
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id");
            return View();
        }

        // POST: TicketHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Created,Property,OldValue,NewValue,TicketId,CustomUserId")] TicketHistory ticketHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticketHistory.CustomUserId);
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", ticketHistory.TicketId);
            return View(ticketHistory);
        }

        // GET: TicketHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistory.FindAsync(id);
            if (ticketHistory == null)
            {
                return NotFound();
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticketHistory.CustomUserId);
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", ticketHistory.TicketId);
            return View(ticketHistory);
        }

        // POST: TicketHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Created,Property,OldValue,NewValue,TicketId,CustomUserId")] TicketHistory ticketHistory)
        {
            if (id != ticketHistory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketHistoryExists(ticketHistory.Id))
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
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticketHistory.CustomUserId);
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", ticketHistory.TicketId);
            return View(ticketHistory);
        }

        // GET: TicketHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistory
                .Include(t => t.CustomUser)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketHistory == null)
            {
                return NotFound();
            }

            return View(ticketHistory);
        }

        // POST: TicketHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketHistory = await _context.TicketHistory.FindAsync(id);
            _context.TicketHistory.Remove(ticketHistory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketHistoryExists(int id)
        {
            return _context.TicketHistory.Any(e => e.Id == id);
        }
    }
}

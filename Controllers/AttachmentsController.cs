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
    public class AttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttachmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Attachment.Include(a => a.Ticket);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Attachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attachment = await _context.Attachment
                .Include(a => a.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attachment == null)
            {
                return NotFound();
            }

            return View(attachment);
        }

        // GET: Attachments/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id");
            return View();
        }

        // POST: Attachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Created,FileName,FileData,TicketId,CustomUserId")] Attachment attachment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attachment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", attachment.TicketId);
            return View(attachment);
        }

        // GET: Attachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attachment = await _context.Attachment.FindAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", attachment.TicketId);
            return View(attachment);
        }

        // POST: Attachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Created,FileName,FileData,TicketId,CustomUserId")] Attachment attachment)
        {
            if (id != attachment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attachment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttachmentExists(attachment.Id))
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
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", attachment.TicketId);
            return View(attachment);
        }

        // GET: Attachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attachment = await _context.Attachment
                .Include(a => a.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attachment == null)
            {
                return NotFound();
            }

            return View(attachment);
        }

        // POST: Attachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attachment = await _context.Attachment.FindAsync(id);
            _context.Attachment.Remove(attachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttachmentExists(int id)
        {
            return _context.Attachment.Any(e => e.Id == id);
        }
    }
}

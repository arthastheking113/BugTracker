using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Data.Enums;

namespace BugTracker.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IEmailSender _emailSender;

        public CommentsController(ApplicationDbContext context, UserManager<CustomUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comment.Include(c => c.CustomUser).Include(c => c.Ticket);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.CustomUser)
                .Include(c => c.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Created,Updated,IsModerated,Moderated,ModeratedReason,ModeratedContent,CustomUserId,TicketId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Created = DateTime.Now;
                comment.Updated = comment.Created;
                var id = comment.TicketId;
                _context.Add(comment);
                await _context.SaveChangesAsync();

                var messageContent = comment.Content;

                Notification notification = new Notification
                {
                    Name = "New Comment on your ticket",
                    TicketId = comment.TicketId,
                    Description = $"You have a new comment from {(await _userManager.FindByIdAsync(comment.CustomUserId)).FullName} on your ticket {(_context.Ticket.FirstOrDefault(t => t.Id == comment.TicketId)).Name}, comment : {messageContent}",
                    Created = DateTime.Now,
                    SenderId = comment.CustomUserId,
                    RecipientId = (_context.Ticket.FirstOrDefault(t => t.Id == comment.TicketId)).DeveloperId
                };
                await _context.Notification.AddAsync(notification);
                await _context.SaveChangesAsync();

               
                var receiver = (_context.Ticket.FirstOrDefault(t => t.Id == comment.TicketId)).DeveloperId;
                string devEmail = (await _userManager.FindByIdAsync(receiver)).Email;
                string subject = $"New Comment From {(await _userManager.FindByIdAsync(comment.CustomUserId)).FullName} on your Ticket.";
                string message = $"You have a new Comment from {(await _userManager.FindByIdAsync(comment.CustomUserId)).FullName} on Ticket {(_context.Ticket.FirstOrDefault(t => t.Id == comment.TicketId)).Name}, comment : {messageContent}";

                await _emailSender.SendEmailAsync(devEmail, subject, message);

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Tickets", new { id });
            }
            else
            {
                var id = comment.TicketId;
                ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", comment.CustomUserId);
                ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", comment.TicketId);
                return RedirectToAction("Details", "Tickets", new { id });
            }
            
       
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", comment.CustomUserId);
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", comment.TicketId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,Created,Updated,IsModerated,Moderated,ModeratedReason,ModeratedContent,CustomUserId,TicketId")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
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
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", comment.CustomUserId);
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", comment.TicketId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.CustomUser)
                .Include(c => c.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }
    }
}

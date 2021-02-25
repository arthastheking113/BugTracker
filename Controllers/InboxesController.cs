﻿using System;
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

namespace BugTracker.Controllers
{
    public class InboxesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InboxesController(ApplicationDbContext context, UserManager<CustomUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Inboxes
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Send()
        {
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var applicationDbContext2 = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext2.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Deleted()
        {
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == true).Include(i => i.Receiver).Include(i => i.Sender);
            var applicationDbContext2 = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext2.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            return View(await applicationDbContext.ToListAsync());
        }
       

        // GET: Inboxes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inbox = await _context.Inbox
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inbox == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            inbox.IsSeen = true;
            await _context.SaveChangesAsync();
            return View(inbox);
        }


        public async Task<IActionResult> ReaderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inbox = await _context.Inbox
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inbox == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            await _context.SaveChangesAsync();
            return View(inbox);
        }


        public async Task<IActionResult> ReplyAsync(string senderId)
        {
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["senderIds"] = senderId;
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([Bind("Id,Message,Created,Subject,IsSeen,SenderId,ReceiverId")] Inbox inbox)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                inbox.SenderId = userId;
                inbox.Created = DateTime.Now;
                _context.Add(inbox);
                await _context.SaveChangesAsync();

                var messageSubject = inbox.Subject;
                var messageContent = inbox.Message;
                var receiver = inbox.ReceiverId;
                string devEmail = (await _userManager.FindByIdAsync(receiver)).Email;
                string subject = $"New Reply Message From {(await _userManager.FindByIdAsync(userId)).FullName}";
                string message = $"You have a new Reply Message from {(await _userManager.FindByIdAsync(userId)).FullName} about {messageSubject}, message : {messageContent}";

                await _emailSender.SendEmailAsync(devEmail, subject, message);

                await _context.SaveChangesAsync();


                var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
                var unread = (await applicationDbContext.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
                var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
                var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
                ViewData["send"] = send;
                ViewData["unread"] = unread;
                ViewData["delete"] = delete;
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "FullName", inbox.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FullName", inbox.SenderId);
            return View(inbox);
        }


        public async Task<IActionResult> temporaryDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inbox = await _context.Inbox
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inbox == null)
            {
                return NotFound();
            }
            inbox.IsDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> restoreDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inbox = await _context.Inbox
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inbox == null)
            {
                return NotFound();
            }
            inbox.IsDeleted = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Deleted));
        }

        // GET: Inboxes/Create
        public async Task<IActionResult> CreateAsync()
        {
            var userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
            var unread = (await applicationDbContext.Where(i => i.IsSeen == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
            var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
            ViewData["send"] = send;
            ViewData["unread"] = unread;
            ViewData["delete"] = delete;
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: Inboxes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Message,Created,Subject,IsSeen,SenderId,ReceiverId")] Inbox inbox)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                inbox.SenderId = userId;
                inbox.Created = DateTime.Now;
                _context.Add(inbox);
                await _context.SaveChangesAsync();
                var messageSubject = inbox.Subject;
                var messageContent = inbox.Message;
                var receiver = inbox.ReceiverId;
                string devEmail = (await _userManager.FindByIdAsync(receiver)).Email;
                string subject = $"New Message From {(await _userManager.FindByIdAsync(userId)).FullName}";
                string message = $"You have a new Message from {(await _userManager.FindByIdAsync(userId)).FullName} about {messageSubject}, message : {messageContent}";

                await _emailSender.SendEmailAsync(devEmail, subject, message);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "FullName", inbox.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FullName", inbox.SenderId);
            return View(inbox);
        }

        // GET: Inboxes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inbox = await _context.Inbox.FindAsync(id);
            if (inbox == null)
            {
                return NotFound();
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", inbox.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", inbox.SenderId);
            return View(inbox);
        }

        // POST: Inboxes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Message,Created,Subject,IsSeen,SenderId,ReceiverId")] Inbox inbox)
        {
            if (id != inbox.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inbox);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InboxExists(inbox.Id))
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
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", inbox.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", inbox.SenderId);
            return View(inbox);
        }

        // GET: Inboxes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inbox = await _context.Inbox
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inbox == null)
            {
                return NotFound();
            }

            return View(inbox);
        }

        // POST: Inboxes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inbox = await _context.Inbox.FindAsync(id);
            _context.Inbox.Remove(inbox);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Deleted));
        }

        private bool InboxExists(int id)
        {
            return _context.Inbox.Any(e => e.Id == id);
        }
    }
}

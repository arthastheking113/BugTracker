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

namespace BugTracker.Controllers
{
    public class RepliesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public RepliesController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Replies
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reply.Include(r => r.Inbox).Include(r => r.Receiver).Include(r => r.Sender);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Replies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reply = await _context.Reply
                .Include(r => r.Inbox)
                .Include(r => r.Receiver)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reply == null)
            {
                return NotFound();
            }

            return View(reply);
        }

        // GET: Replies/Create
        public IActionResult Create()
        {
            ViewData["InboxId"] = new SelectList(_context.Inbox, "Id", "Id");
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Replies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Message,Created,Subject,IsSeen,IsDeleted,SenderId,ReceiverId,InboxId")] Reply reply)
        {
            if (ModelState.IsValid)
            {
                var inboxId = reply.InboxId;

                var inbox = (await _context.Inbox.FirstOrDefaultAsync(i => i.Id == inboxId));

                inbox.IsSeenBySender = true;
                await _context.SaveChangesAsync();

                var userId = _userManager.GetUserId(User);
                
                reply.SenderId = userId;
                reply.Created = DateTime.Now;
                reply.ReceiverId = (await _context.Inbox.FirstOrDefaultAsync(r => r.Id == inboxId)).SenderId;
                _context.Add(reply);
                await _context.SaveChangesAsync();

                var applicationDbContext = _context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender);
                var unread = (await applicationDbContext.Where(i => i.IsSeenByReceiver == false).Include(i => i.Receiver).Include(i => i.Sender).ToListAsync()).Count;
                var delete = (_context.Inbox.Where(i => i.IsDeleted == true).Where(i => i.ReceiverId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
                var send = (_context.Inbox.Where(i => i.IsDeleted == false).Where(i => i.SenderId == userId).Include(i => i.Receiver).Include(i => i.Sender)).Count();
                ViewData["send"] = send;
                ViewData["unread"] = unread;
                ViewData["delete"] = delete;

                return RedirectToAction("Index","Inboxes");
            }
            ViewData["InboxId"] = new SelectList(_context.Inbox, "Id", "Id", reply.InboxId);
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", reply.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", reply.SenderId);
            return View(reply);
        }

        // GET: Replies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reply = await _context.Reply.FindAsync(id);
            if (reply == null)
            {
                return NotFound();
            }
            ViewData["InboxId"] = new SelectList(_context.Inbox, "Id", "Id", reply.InboxId);
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", reply.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", reply.SenderId);
            return View(reply);
        }

        // POST: Replies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Message,Created,Subject,IsSeen,IsDeleted,SenderId,ReceiverId,InboxId")] Reply reply)
        {
            if (id != reply.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reply);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReplyExists(reply.Id))
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
            ViewData["InboxId"] = new SelectList(_context.Inbox, "Id", "Id", reply.InboxId);
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", reply.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", reply.SenderId);
            return View(reply);
        }

        // GET: Replies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reply = await _context.Reply
                .Include(r => r.Inbox)
                .Include(r => r.Receiver)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reply == null)
            {
                return NotFound();
            }

            return View(reply);
        }

        // POST: Replies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reply = await _context.Reply.FindAsync(id);
            _context.Reply.Remove(reply);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReplyExists(int id)
        {
            return _context.Reply.Any(e => e.Id == id);
        }
    }
}

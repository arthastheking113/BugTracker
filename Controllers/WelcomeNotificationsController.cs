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
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{
    [Authorize]
    public class WelcomeNotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public WelcomeNotificationsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: WelcomeNotifications
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.WelcomeNotification.Include(w => w.Recipient).Include(w => w.Sender);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: WelcomeNotifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var welcomeNotification = await _context.WelcomeNotification.Where(n => n.RecipientId == _userManager.GetUserId(User))
               .Include(w => w.Recipient)
               .Include(w => w.Sender)
               .FirstOrDefaultAsync(m => m.Id == id);
            if (welcomeNotification == null)
            {
                return NotFound();
            } 
            welcomeNotification.IsViewed = true;
            await _context.SaveChangesAsync();
            return View(welcomeNotification);
           
        }

        // GET: WelcomeNotifications/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: WelcomeNotifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsViewed,Created,SenderId,RecipientId")] WelcomeNotification welcomeNotification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(welcomeNotification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", welcomeNotification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", welcomeNotification.SenderId);
            return View(welcomeNotification);
        }

        // GET: WelcomeNotifications/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var welcomeNotification = await _context.WelcomeNotification.FindAsync(id);
            if (welcomeNotification == null)
            {
                return NotFound();
            }
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", welcomeNotification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", welcomeNotification.SenderId);
            return View(welcomeNotification);
        }

        // POST: WelcomeNotifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsViewed,Created,SenderId,RecipientId")] WelcomeNotification welcomeNotification)
        {
            if (id != welcomeNotification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(welcomeNotification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WelcomeNotificationExists(welcomeNotification.Id))
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
            ViewData["RecipientId"] = new SelectList(_context.Users, "Id", "Id", welcomeNotification.RecipientId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", welcomeNotification.SenderId);
            return View(welcomeNotification);
        }

        // GET: WelcomeNotifications/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var welcomeNotification = await _context.WelcomeNotification
                .Include(w => w.Recipient)
                .Include(w => w.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (welcomeNotification == null)
            {
                return NotFound();
            }

            return View(welcomeNotification);
        }

        // POST: WelcomeNotifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var welcomeNotification = await _context.WelcomeNotification.FindAsync(id);
            _context.WelcomeNotification.Remove(welcomeNotification);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WelcomeNotificationExists(int id)
        {
            return _context.WelcomeNotification.Any(e => e.Id == id);
        }
    }
}

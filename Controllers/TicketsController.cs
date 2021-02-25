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
using BugTracker.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using BugTracker.Data.Enums;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICustomHistoryService _customHistoryService;
        private readonly IEmailSender _emailSender;

        public TicketsController(ApplicationDbContext context, UserManager<CustomUser> userManager, ICustomHistoryService customHistoryService, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _customHistoryService = customHistoryService;
            _emailSender = emailSender;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            if (await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Admin.ToString())|| await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.ProjectManager.ToString()))
            {
                var applicationDbContext = _context.Ticket.Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var applicationDbContext = _context.Ticket.Where(u => u.DeveloperId == userId).Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
                return View(await applicationDbContext.ToListAsync());
            }

         
        }

        public async Task<IActionResult> ProjectIndex(int? id)
        {
          
            var applicationDbContext = _context.Ticket.Where(u => u.ProjectId == id).Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
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
                .Include(t => t.Developer)
                .Include(t => t.Ownner)
                .Include(t => t.Priority)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.TicketType)
                .Include(t => t.Attachments)
                .Include(t => t.Comments)
                .Include(t => t.TicketHistories).ThenInclude(t => t.CustomUser)
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
            ViewData["DeveloperId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsAssigned,Created,Updated,DeveloperId,OwnnerId,ProjectId,StatusId,PriorityId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
              
                ticket.OwnnerId = _userManager.GetUserId(User);
                ticket.Created = DateTime.Now;
                ticket.Updated = ticket.Created;
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                var currentStatus = _context.Status.FirstOrDefault(t => t.Name == "Closed").Id; // new 

                if (ticket.IsAssigned == true && ticket.StatusId != currentStatus)
                {
                    Notification notification = new Notification
                    {
                        Name = "New Ticket Assign",
                        TicketId = ticket.Id,
                        Description = "You have a new ticket.",
                        Created = DateTime.Now,
                        SenderId = ticket.OwnnerId,
                        RecipientId = ticket.DeveloperId
                    };
                    await _context.Notification.AddAsync(notification);
                    await _context.SaveChangesAsync();


                    string devEmail = (await _userManager.FindByIdAsync(ticket.DeveloperId)).Email;
                    string subject = "New Ticket Assignment";
                    string message = $"You have been Assigned a new ticket {ticket.Name} about {ticket.Description} for project: {_context.Project.FirstOrDefault(p => p.Id == ticket.ProjectId).Name}";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);

                    await _context.SaveChangesAsync();
                }



                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperId);
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnnerId);
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
            ViewData["DeveloperId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperId);
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "FullName", ticket.OwnnerId);
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Name", ticket.PriorityId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Name", ticket.StatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,IsAssigned,Updated,DeveloperId,OwnnerId,ProjectId,StatusId,PriorityId,TicketTypeId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }
            Ticket oldTicket = await _context.Ticket.Include(t => t.TicketType).Include(t => t.Status).Include(t => t.Priority).Include(t => t.Developer).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

            if (ModelState.IsValid)
            {
                try
                {

                    ticket.OwnnerId = _userManager.GetUserId(User);
                    ticket.Updated = DateTime.Now;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    var userId = _userManager.GetUserId(User);
                    Ticket newTicket = await _context.Ticket.Include(t => t.TicketType).Include(t => t.Status).Include(t => t.Priority).Include(t => t.Developer).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    await _customHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

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
            ViewData["DeveloperId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperId);
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnnerId);
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
                .Include(t => t.Developer)
                .Include(t => t.Ownner)
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

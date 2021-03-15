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
using BugTracker.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using BugTracker.Data.Enums;
using Microsoft.AspNetCore.Authorization;


namespace BugTracker.Controllers
{

    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICustomHistoryService _customHistoryService;
        private readonly IEmailSender _emailSender;
        private readonly ICustomProjectService _projectService;
        private readonly ICustomRoleService _roleService;
        private readonly SignInManager<CustomUser> _signInManager;

        public TicketsController(ApplicationDbContext context, 
            UserManager<CustomUser> userManager, 
            ICustomHistoryService customHistoryService, 
            IEmailSender emailSender, 
            ICustomProjectService projectService, 
            ICustomRoleService roleService,
            SignInManager<CustomUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _customHistoryService = customHistoryService;
            _emailSender = emailSender;
            _projectService = projectService;
            _roleService = roleService;
            _signInManager = signInManager;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            IEnumerable<CustomUser> allDeveloper = new List<CustomUser>();
            var allProject = _context.Project.ToList();
            foreach (var item in allProject)
            {
                var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                allDeveloper = allDeveloper.Union(developer);
            }

            List<Project> listProject = new List<Project>();
            if (!await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.Admin.ToString()))
            {
                foreach (var item in allProject)
                {
                    var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                    allDeveloper = allDeveloper.Union(developer);

                    if (await _projectService.IsUserOnProjectAsync(userId, item.Id))
                    {
                        listProject.Add(item);
                    }
                }
            }
            else
            {
                listProject = allProject;
            }
            ViewData["NumberOfTicket"] = _context.Ticket.OrderBy(t => t.Id).Last().Id + 1;
            ViewData["DeveloperId"] = new SelectList(allDeveloper, "Id", "FullName");
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(listProject, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name");

            var ticket = await _context.Ticket.Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created).ToListAsync();

            if (await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Admin.ToString()))
            {
                var applicationDbContext = _context.Ticket.Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
                return View(await applicationDbContext.ToListAsync());
            }
            else if (await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.ProjectManager.ToString()))
            {
                List<Project> listProjectOfProjectManager = new List<Project>();

                foreach (var item in allProject)
                {
                    if (await _projectService.IsUserOnProjectAsync(userId, item.Id))
                    {
                        listProjectOfProjectManager.Add(item);
                    }
                }
                List<Ticket> projectManagerView = new List<Ticket>();
                foreach (var item in listProjectOfProjectManager)
                {
                    foreach (var itemInTicket in ticket)
                    {
                        if (itemInTicket.ProjectId == item.Id)
                        {
                            projectManagerView.Add(itemInTicket);
                        }
                    }
                }
                return View(projectManagerView);
            }
            else if (await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Submitter.ToString()))
            {

                var ticketsView = ticket.Where(u => u.OwnnerId == userId);
                return View(ticketsView);
            }
            else
            {
                var ticketsView = ticket.Where(u => u.DeveloperId == userId || (u.OwnnerId == userId && !u.IsAssigned));
                return View(ticketsView);
            }


        }
        [Authorize(Roles = "Admin, ProjectManager, Developer")]
        public async Task<IActionResult> ProjectIndex(int id)
        {
            ViewData["ProjectName"] = _context.Project.FirstOrDefault(p => p.Id == id).Name;
            ViewData["DeveloperId"] = new SelectList(await _projectService.DeveloperOnProjectAsync(id), "Id", "FullName");
            ViewData["NumberOfTicket"] = _context.Ticket.OrderBy(t => t.Id).Last().Id + 1;
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Name");
            ViewData["ProjectId"] = id;
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name");
            var applicationDbContext = _context.Ticket.Where(u => u.ProjectId == id).Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Updated);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> AcceptInviteAsync(string userId, string code)
        {
            var realGuid = Guid.Parse(code);
            var invite = _context.Invite.FirstOrDefault(i => i.CompanyToken == realGuid && i.InviteeId == userId);
            if (invite is null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (invite.IsValid)
            {
                invite.IsValid = false;
                var user = await _context.Users.FindAsync(userId);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index","Home");
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Tickets/Details/5
        [Authorize]
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
            //var userId = _userManager.GetUserId(User);
           
                return View(ticket);
           
            
        }

        // GET: Tickets/Create
        [Authorize]
        public async Task<IActionResult> CreateAsync(int? projectId)
        {
            IEnumerable<CustomUser> allDeveloper = new List<CustomUser>();
            var allProject = _context.Project.ToList();
            foreach (var item in allProject)
            {
                var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                allDeveloper = allDeveloper.Union(developer);
            }



            ViewData["projectIdfromView"] = projectId;
            ViewData["DeveloperId"] = new SelectList(allDeveloper, "Id", "FullName");
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
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsAssigned,Created,Updated,DeveloperId,OwnnerId,ProjectId,StatusId,PriorityId,TicketTypeId")] Ticket ticket)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (ModelState.IsValid)
                {
                    var Id = ticket.ProjectId;
                    if (ticket.IsAssigned == null)
                    {
                        ticket.IsAssigned = false;
                    }

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
                    var projectName = _context.Project.FirstOrDefault(p => p.Id == ticket.ProjectId).Name;
                    if ((await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Submitter.ToString())))
                    {
                        Notification notification = new Notification
                        {
                            Name = "New Ticket is Created by Submitter",
                            TicketId = ticket.Id,
                            Description = $"New Ticket #{ticket.Id} is Created by Submitter in {projectName} and Waiting for being Assigned",
                            Created = DateTime.Now,
                            SenderId = ticket.OwnnerId,
                            RecipientId = (await _projectService.ProjectManagerOnProjectAsync(ticket.ProjectId)).Id
                        };
                        await _context.Notification.AddAsync(notification);
                        await _context.SaveChangesAsync();

                       
                        Notification notification2 = new Notification
                        {
                            Name = "You just Create a New Ticket",
                            TicketId = ticket.Id,
                            Description = $"You just create a new ticket #{ticket.Id} in project {projectName} and waiting for being Assigned",
                            Created = DateTime.Now,
                            SenderId = (await _projectService.ProjectManagerOnProjectAsync(ticket.ProjectId)).Id,
                            RecipientId = ticket.OwnnerId
                        };
                        await _context.Notification.AddAsync(notification2);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else if ((await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Developer.ToString())))
                    {
                        Notification notification = new Notification
                        {
                            Name = "New Ticket is Created by Developer",
                            TicketId = ticket.Id,
                            Description = $"New Ticket #{ticket.Id} is Created by Developer in {projectName} and Waiting for being Approve",
                            Created = DateTime.Now,
                            SenderId = ticket.OwnnerId,
                            RecipientId = (await _projectService.ProjectManagerOnProjectAsync(ticket.ProjectId)).Id
                        };
                        await _context.Notification.AddAsync(notification);
                        await _context.SaveChangesAsync();


                        Notification notification2 = new Notification
                        {
                            Name = "You just Create a New Ticket",
                            TicketId = ticket.Id,
                            Description = $"You just create a new ticket #{ticket.Id} in project {projectName} and waiting for being Assigned",
                            Created = DateTime.Now,
                            SenderId = (await _projectService.ProjectManagerOnProjectAsync(ticket.ProjectId)).Id,
                            RecipientId = ticket.OwnnerId
                        };
                        await _context.Notification.AddAsync(notification2);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    { 
                        return RedirectToAction("Details", "Projects", new { Id });
                    }
                  
                


                }
                IEnumerable<CustomUser> allDeveloper = new List<CustomUser>();
                var allProject = _context.Project.ToList();
                List<Project> listProject = new List<Project>();
                foreach (var item in allProject)
                {
                    var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                    allDeveloper = allDeveloper.Union(developer);


                    if (await _projectService.IsUserOnProjectAsync(_userManager.GetUserId(User), item.Id))
                    {
                        listProject.Add(item);
                    }
                }
                ViewData["DeveloperId"] = new SelectList(allDeveloper, "Id", "FullName", ticket.DeveloperId);
                ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "FullName", ticket.OwnnerId);
                ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Name", ticket.PriorityId);
                ViewData["ProjectId"] = new SelectList(listProject, "Id", "Name", ticket.ProjectId);
                ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Name", ticket.StatusId);
                ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name", ticket.TicketTypeId);
                return View(ticket);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: Tickets/Edit/5
        [Authorize]
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
            IEnumerable<CustomUser> allDeveloper = new List<CustomUser>();
            var allProject = _context.Project.ToList();
            foreach (var item in allProject)
            {
                var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                allDeveloper = allDeveloper.Union(developer);
            }
            ViewData["DeveloperId"] = new SelectList(allDeveloper, "Id", "FullName", ticket.DeveloperId);
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
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,IsAssigned,Updated,DeveloperId,OwnnerId,ProjectId,StatusId,PriorityId,TicketTypeId")] Ticket ticket)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
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
                IEnumerable<CustomUser> allDeveloper = new List<CustomUser>();
                var allProject = _context.Project.ToList();
                foreach (var item in allProject)
                {
                    var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                    allDeveloper = allDeveloper.Union(developer);
                }
                ViewData["DeveloperId"] = new SelectList(allProject, "Id", "Id", ticket.DeveloperId);
                ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnnerId);
                ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Id", ticket.PriorityId);
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", ticket.ProjectId);
                ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Id", ticket.StatusId);
                ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
                return View(ticket);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
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
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                var ticket = await _context.Ticket.FindAsync(id);
                _context.Ticket.Remove(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}

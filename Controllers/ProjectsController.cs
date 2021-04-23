using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services;
using BugTracker.Data.Enums;
using Microsoft.AspNetCore.Http;
using BugTracker.Service;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{

    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomProjectService _projectService;
        private readonly ICustomRoleService _roleService;
        private readonly IImageService _imageService;
        private readonly UserManager<CustomUser> _userManager;

        public ProjectsController(ApplicationDbContext context, ICustomProjectService projectService, ICustomRoleService roleService, 
            IImageService imageService, 
            UserManager<CustomUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _roleService = roleService;
            _imageService = imageService;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ManagerUserProject()
        {
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            ViewData["ProjectManagerId"] = new SelectList(await _roleService.UsersInRoleAsync(Roles.ProjectManager.ToString()), "Id", "FullName");
            ViewData["DevelopersId"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            ViewData["SubmittersId"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Submitter.ToString()), "Id", "FullName");

            return View();
        }
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerUserProject(string ProjectManagerId, int ProjectId, List<string> DevelopersId, List<string> SubmittersId)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                var currentlyOnProject = await _projectService.UserOnProjectAsync(ProjectId);
                foreach (var user in currentlyOnProject)
                {
                    await _projectService.RemoveUserFromProjectAsync(user.Id, ProjectId);
                }
                await _projectService.AddUserToProjectAsync(ProjectManagerId, ProjectId);
                foreach (var user in DevelopersId)
                {
                    await _projectService.AddUserToProjectAsync(user, ProjectId);
                }
                foreach (var user in SubmittersId)
                {
                    await _projectService.AddUserToProjectAsync(user, ProjectId);
                }
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                ViewData["ProjectManagerId"] = new SelectList(await _roleService.UsersInRoleAsync(Roles.ProjectManager.ToString()), "Id", "FullName");
                ViewData["DevelopersId"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
                ViewData["SubmittersId"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Submitter.ToString()), "Id", "FullName");
                return View();
            }
            return RedirectToAction("DemoUser", "Projects");
        }



        // GET: Projects
        [Authorize(Roles = "Admin, ProjectManager,Developer")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var applicationDbContext = await  _context.Project.Include(p => p.Company).Include(p => p.Tickets).Include(p => p.CustomUsers).ToListAsync();
            List<Project> projectList = new List<Project>();
            if (await _userManager.IsInRoleAsync(user, Roles.ProjectManager.ToString()))
            {
                foreach (var project in applicationDbContext)
                {
                    if (await _projectService.IsUserOnProjectAsync(userId, project.Id))
                    {
                        projectList.Add(project);
                    }
                }
            }
            else if (await _userManager.IsInRoleAsync(user, Roles.Developer.ToString()))
            {
                foreach (var project in applicationDbContext)
                {
                    var developerInProject = await _projectService.DeveloperOnProjectAsync(project.Id);
                    foreach (var developer in developerInProject)
                    {
                        if (developer.Id == userId)
                        {
                            projectList.Add(project);
                        }
                    }
                   
                }
            }
            
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name");
            if (await _userManager.IsInRoleAsync(user, Roles.Admin.ToString()))
            {
                return View(applicationDbContext);
            }
            else 
            {
                return View(projectList);
            }
            
           
        }

        // GET: Projects/Details/5
        [Authorize(Roles = "Admin, ProjectManager,Developer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.Attachments)
                .Include(p => p.Tickets)
                .Include(p => p.CustomUsers)
                .FirstOrDefaultAsync(m => m.Id == id);



            if (project == null)
            {
                return NotFound();
            }
            ViewData["NumberOfTicket"] = _context.Ticket.OrderBy(t => t.Id).Last().Id + 1;
            ViewData["DeveloperId"] = new SelectList(await _projectService.DeveloperOnProjectAsync(project.Id), "Id", "FullName");
            ViewData["OwnnerId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["PriorityId"] = new SelectList(_context.Priority, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.Status, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name");
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (await _projectService.IsUserOnProjectAsync(userId, project.Id) || await _roleService.IsUserInRoleAsync(user, Roles.Admin.ToString()))
            {
                return View(project);
            }
            else
            {
                return View();
            }

           
        }
        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,CompanyId,ImageData,ContentType")] Project project, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                var loginUser = await _userManager.GetUserAsync(User);
                project.CompanyId = loginUser.CompanyId;
                project.ImageData = await _imageService.EncodeFileAsync(image);
                project.ContentType = _imageService.RecordContentType(image);
                project.Created = DateTime.Now;
                _context.Add(project);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public IActionResult DemoUser()
        {
            return View();
        }


        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,CompanyId,ImageData,ContentType")] Project project, IFormFile image, Byte[]? imageData, string contentType)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (id != project.Id)
                {
                    return NotFound();
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (image != null)
                        {
                            project.ImageData = await _imageService.EncodeFileAsync(image);
                            project.ContentType = _imageService.RecordContentType(image);
                        }
                        else
                        {
                            if (imageData != null && contentType != null)
                            {
                                project.ImageData = imageData;
                                project.ContentType = contentType;
                            }
                            else
                            {
                                project.ImageData = null;
                                project.ContentType = null;
                            }
                        }
                        _context.Update(project);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProjectExists(project.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Details", "Projects", new { id });
                }
                ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
                return View(project);
            }
            return RedirectToAction("DemoUser", "Projects");
        }
        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                var project = await _context.Project.FindAsync(id);

                var ticketList = _context.Ticket.Include(t => t.Attachments).Include(t => t.Comments).Include(t => t.TicketHistories).Where(t => t.ProjectId == project.Id).ToList();
                //delete ticket of this project
                foreach (var ticket in ticketList)
                {
                    //delete all attachments in 1 ticket
                    foreach (var attachment in ticket.Attachments)
                    {
                        _context.Attachment.Remove(attachment);
                        await _context.SaveChangesAsync();
                    }
                    //delete all comment in 1 ticket
                    foreach (var comment in ticket.Comments)
                    {
                        _context.Comment.Remove(comment);
                        await _context.SaveChangesAsync();
                    }
                    //delete all History in 1 ticket
                    foreach (var history in ticket.TicketHistories)
                    {
                        _context.TicketHistory.Remove(history);
                        await _context.SaveChangesAsync();
                    }
                    _context.Ticket.Remove(ticket);
                    await _context.SaveChangesAsync();
                }
                //you don't need to do this foreach loop
                foreach (var projectAttachment in project.Attachments)
                {
                    _context.ProjectAttachment.Remove(projectAttachment);
                    await _context.SaveChangesAsync();
                }

                _context.Project.Remove(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}

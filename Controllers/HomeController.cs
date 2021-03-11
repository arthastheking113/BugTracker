using BugTracker.Data;
using BugTracker.Data.Enums;
using BugTracker.Models;
using BugTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICustomRoleService _customRoleService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICustomProjectService _projectService;

        public HomeController(ILogger<HomeController> logger, 
            ICustomRoleService customRoleService, 
            ApplicationDbContext dbContext, 
            IEmailSender emailSender, 
            UserManager<CustomUser> userManager,
            ICustomProjectService projectService)
        {
            _logger = logger;
           _customRoleService = customRoleService;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _userManager = userManager;
            _projectService = projectService;
        }
        public async Task<JsonResult> DevelopersOnProject(int id)
        {
            var devs = await _projectService.DeveloperOnProjectAsync(id);
            var model = new DeveloperOnProject();
            foreach (var dev in devs)
            {
                model.Ids.Add(dev.Id);
                model.FullName.Add(dev.FullName);
            }
            return Json(model);
        }

        public async Task<IActionResult> IndexAsync()
        {
            var currentTime = DateTime.Now;
            var role = await _customRoleService.ListUserRoleAsync((await _userManager.GetUserAsync(User)));
            var userId = (await _userManager.GetUserAsync(User)).Id;
            var projects = _dbContext.Project.ToList();
           
              
            var number_of_project = projects.Count;
            var number_of_ticket = _dbContext.Ticket.ToList().Count;
            var number_of_user = _dbContext.Users.ToList().Count;
            var number_of_close_ticket = _dbContext.Ticket.Where(t => t.StatusId == (_dbContext.Status.FirstOrDefault(t => t.Name == "Closed").Id)).ToList().Count;
            var persent_of_ticket_done = number_of_close_ticket * 100 / number_of_ticket;
            var number_of_urgent_ticket = _dbContext.Ticket.Where(t => t.PriorityId == (_dbContext.Priority.FirstOrDefault(t => t.Name == "Urgent").Id)).ToList().Count;
            var number_of_high_ticket = _dbContext.Ticket.Where(t => t.PriorityId == (_dbContext.Priority.FirstOrDefault(t => t.Name == "High").Id)).ToList().Count;
            var number_of_medium_ticket = _dbContext.Ticket.Where(t => t.PriorityId == (_dbContext.Priority.FirstOrDefault(t => t.Name == "Medium").Id)).ToList().Count;
            var number_of_low_ticket = _dbContext.Ticket.Where(t => t.PriorityId == (_dbContext.Priority.FirstOrDefault(t => t.Name == "Low").Id)).ToList().Count;
            var number_unassign_ticket = _dbContext.Ticket.Where(t => t.IsAssigned == false).ToList().Count;
            var number_new_ticket = _dbContext.Ticket.Where(t => t.Created >= currentTime.AddDays(-7)).ToList().Count;
            var number_resolve_ticket = _dbContext.Ticket.Where(t => t.Updated >= currentTime.AddDays(-7) && t.StatusId == (_dbContext.Status.FirstOrDefault(t => t.Name == "Closed").Id)).ToList().Count;
            var number_of_company = _dbContext.Company.ToList().Count;


            ViewData["currentTime"] = currentTime;
            ViewData["role"] = role;
            ViewData["userId"] = userId;
            ViewData["projects"] = projects;
            ViewData["number_of_project"] = number_of_project;
            ViewData["number_of_ticket"] = number_of_ticket;
            ViewData["number_of_user"] = number_of_user;
            ViewData["number_of_close_ticket"] = number_of_close_ticket;
            ViewData["persent_of_ticket_done"] = persent_of_ticket_done;
            ViewData["number_of_urgent_ticket"] = number_of_urgent_ticket;
            ViewData["number_of_high_ticket"] = number_of_high_ticket;
            ViewData["number_of_medium_ticket"] = number_of_medium_ticket;
            ViewData["number_of_low_ticket"] = number_of_low_ticket;
            ViewData["number_unassign_ticket"] = number_unassign_ticket;
            ViewData["number_new_ticket"] = number_new_ticket;
            ViewData["number_resolve_ticket"] = number_resolve_ticket;
            ViewData["number_of_company"] = number_of_company;

            IEnumerable<CustomUser> allDeveloper = new List<CustomUser>();
            var allProject = _dbContext.Project.ToList();

            List<Project> listProject = new List<Project>();
            if (!await _customRoleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.Admin.ToString()))
            {
                foreach (var item in allProject)
                {
                    var developer = await _projectService.DeveloperOnProjectAsync(item.Id);
                    allDeveloper = allDeveloper.Concat(developer);

                    if (await _projectService.IsUserOnProjectAsync(userId, item.Id))
                    {
                        listProject.Add(item);
                    }
                }
            }
            else
            {
                listProject = _dbContext.Project.ToList();
            }
           
            var listRole = _dbContext.Roles.Where(r => (r.Name != Roles.Admin.ToString() 
            && r.Name != Roles.ProjectManager.ToString() 
            && r.Name != Roles.DemoUser.ToString()) 
            && r.Name != Roles.NewUser.ToString()).ToList();

           
  

            ViewData["Roles"] = new SelectList(listRole, "Id", "Name");
            ViewData["CompanyId"] = new SelectList(_dbContext.Company, "Id", "Name");
            ViewData["DeveloperId"] = new SelectList(allDeveloper, "Id", "FullName");
            ViewData["OwnnerId"] = new SelectList(_dbContext.Users, "Id", "FullName");
            ViewData["PriorityId"] = new SelectList(_dbContext.Priority, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(listProject, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_dbContext.Status, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_dbContext.TicketType, "Id", "Name");



            if (await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Admin.ToString()) || await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.ProjectManager.ToString()))
            {
                var applicationDbContext = _dbContext.Ticket.Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
                return View(await applicationDbContext.ToListAsync());
            }
            else if (await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), Roles.Submitter.ToString()))
            {
          
                var applicationDbContext = _dbContext.Ticket.Where(u => u.OwnnerId == userId).Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
             
                var applicationDbContext = _dbContext.Ticket.Where(u => u.DeveloperId == userId).Include(t => t.Developer).Include(t => t.Ownner).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.TicketType).OrderByDescending(c => c.Created);
                return View(await applicationDbContext.ToListAsync());
            }
       
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public IActionResult UserOverview()
        {
            var user = _dbContext.Users.ToList();
            return View(user);
        }
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(string Name, string Email, string Subject, string Message)
        {
            string myEmail = "arthastheking113@gmail.com";

            string subject = $"{Name} Just Send You A Message About {Subject}";

            string body = $"Message From {Email}: {Message}";

            string customberSubject = $"I Just Received A Message From Lan's Blog With Name {Name} About {Subject}";
            string customberBody = $"I Received Message From {Email} About: {Subject}. I will contact back to you as soon as possible.";

            await _emailSender.SendEmailAsync(myEmail, subject, body);

            await _emailSender.SendEmailAsync(Email, customberSubject, customberBody);

            ModelState.Clear();
            return View("SendMessageSuccess");
        }

        public IActionResult SendMessageSuccess()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize(Roles = "Admin")]
        public IActionResult ManageRole()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(List<string> userId, string role)
        {
            if (!(await _customRoleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                foreach (var id in userId)
                {
                    CustomUser user = await _dbContext.Users.FindAsync(id);
                    if (!await _customRoleService.IsUserInRoleAsync(user, role))
                    {
                        var userRole = await _customRoleService.ListUserRoleAsync(user);
                        foreach (var roles in userRole)
                        {
                            await _customRoleService.RemoveUserFromRoleAsync(user, roles);
                        }
                        await _customRoleService.AddUserToRoleAsync(user, role);
                    }
                }
                return View();
            }
            return RedirectToAction("DemoUser", "Projects");
        }

    }
}

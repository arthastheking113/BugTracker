using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public HomeController(ILogger<HomeController> logger, ICustomRoleService customRoleService, ApplicationDbContext dbContext, IEmailSender emailSender)
        {
            _logger = logger;
           _customRoleService = customRoleService;
            _dbContext = dbContext;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            ViewData["CompanyId"] = new SelectList(_dbContext.Company, "Id", "Name");
            ViewData["DeveloperId"] = new SelectList(_dbContext.Users, "Id", "FullName");
            ViewData["OwnnerId"] = new SelectList(_dbContext.Users, "Id", "FullName");
            ViewData["PriorityId"] = new SelectList(_dbContext.Priority, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_dbContext.Project, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_dbContext.Status, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_dbContext.TicketType, "Id", "Name");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

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

        public IActionResult ManageRole()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(List<string> userId, string role)
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

    }
}

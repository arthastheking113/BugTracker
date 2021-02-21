using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, ICustomRoleService customRoleService, ApplicationDbContext dbContext)
        {
            _logger = logger;
           _customRoleService = customRoleService;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
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

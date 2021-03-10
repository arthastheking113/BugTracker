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
using BugTracker.Data.Enums;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using BugTracker.Services;

namespace BugTracker.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ICustomProjectService _projectService;

        public InvitesController(ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<CustomUser> userManager, 
            IEmailSender emailSender,
            ICustomProjectService projectService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _projectService = projectService;
        }

        // GET: Invites
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invite.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create

        public IActionResult Create()
        {
            var listRole = _context.Roles.Where(r => (r.Name != Roles.Admin.ToString()
           && r.Name != Roles.ProjectManager.ToString()
           && r.Name != Roles.DemoUser.ToString())
           && r.Name != Roles.NewUser.ToString()).ToList();
            ViewData["Roles"] = new SelectList(listRole, "Id", "Name");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,CompanyToken,InviteDate,IsValid,CompanyId,InvitorId,InviteeId")] Invite invite, string CompanyName, string Role)
        {
            if (ModelState.IsValid)
            {

                var loginUser = (await _userManager.GetUserAsync(User));
                Company company = new Company();
                if (await _userManager.IsInRoleAsync(loginUser, Roles.Admin.ToString()))
                {
                     company = new Company
                    {
                        Name = CompanyName,
                        Description = $"This is a temporary description for Company: {CompanyName}, you can change this description anytime."
                    };
                    await _context.Company.AddAsync(company);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    company = await _context.Company.FirstOrDefaultAsync(c => c.Id == loginUser.CompanyId);
                }
              
                invite.CompanyToken = Guid.NewGuid();
                CustomUser newUser = new CustomUser 
                { 
                    FirstName = "Invite",
                    LastName = "User",
                    UserName = invite.Email,
                    Email = invite.Email,
                    EmailConfirmed = true,
                    CompanyId = company.Id
                };
                try
                {
                    var newUserFind = await _userManager.FindByEmailAsync(newUser.Email);
                    if (newUserFind == null)
                    {
                        var result = await _userManager.CreateAsync(newUser, "Abc123!");
                        if (result.Succeeded)
                        {
                            
                            string returnUrl = null;
                            returnUrl  ??= Url.Content("~/");
                            var code = invite.CompanyToken;
                            var callbackUrl = Url.Page(
                                "AcceptInvite",
                                "Tickets",
                                values: new {newUser.Id, code },
                                protocol: Request.Scheme);
                            //var realGuid = Guid.Parse(code);
                            await _emailSender.SendEmailAsync(newUser.Email, "Invite Email From Lan's Bug Tracker",
                                $"You received a invite ticket from Lan's Bug Tracker <br> <a style='background-color: #555555;border: none;color: white;padding: 15px 32px;text-align: center;text-decoration: none;display: inline-block;font-size: 16px;' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a> to join our software. <br> Your UserName is: {newUser.Email} <br> Your Password is: Abc123!");

                            if (await _userManager.IsInRoleAsync(loginUser, Roles.Admin.ToString()))
                            {
                                await _userManager.AddToRoleAsync(newUser, Roles.ProjectManager.ToString());
                            }
                            else if (await _userManager.IsInRoleAsync(loginUser, Roles.ProjectManager.ToString()))
                            {

                                await _userManager.AddToRoleAsync(newUser, _context.Roles.FirstOrDefault(r => r.Id == Role).Name);
                                //await _projectService.AddUserToProjectAsync(newUser.Id, );
                            }
                            else
                            {
                                await _userManager.AddToRoleAsync(newUser, Roles.NewUser.ToString());
                            }

                           
                            var admin = await _userManager.FindByEmailAsync("arthastheking113@gmail.com");

                            Project project = new Project
                            {
                                Name = "Project Example",
                                Description = "This is your project Example, you can change anything in this project anytime you want",
                                Created = DateTime.Now,
                                CompanyId = company.Id
                            };
                            await _context.Project.AddAsync(project);
                            await _context.SaveChangesAsync();

                            await _projectService.AddUserToProjectAsync(newUser.Id, project.Id);
                            //notification for new user
                            WelcomeNotification welcomenotification = new WelcomeNotification
                            {
                                Name = "Welcome To The Bug Tracker",
                                Description = "You have been Invited to the bug tracker service, please wait our admin or project manager assign you a higher role in the system. You can contact our staff by the inbox system.",
                                Created = DateTime.Now,
                                SenderId = (admin).Id,
                                RecipientId = newUser.Id
                            };
                            await _context.WelcomeNotification.AddAsync(welcomenotification);
                            await _context.SaveChangesAsync();

                            //noification for admin
                            WelcomeNotification welcomenotification2 = new WelcomeNotification
                            {
                                Name = "New invite ticket has been sent",
                                Description = $"{loginUser.FullName} just send a Invite Ticket to {invite.Email}",
                                Created = DateTime.Now,
                                SenderId = (admin).Id,
                                RecipientId = (admin).Id
                            };
                            await _context.WelcomeNotification.AddAsync(welcomenotification2);
                            await _context.SaveChangesAsync();

                            //noification for person who send invite ticket
                            WelcomeNotification welcomenotification3 = new WelcomeNotification
                            {
                                Name = "New invite ticket has been sent",
                                Description = $"You just send a Invite Ticket to {invite.Email}",
                                Created = DateTime.Now,
                                SenderId = (admin).Id,
                                RecipientId = loginUser.Id
                            };
                            await _context.WelcomeNotification.AddAsync(welcomenotification3);
                            await _context.SaveChangesAsync();
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("*************  ERROR  *************");
                    Debug.WriteLine("Error Create User For Invite Ticket.");
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("***********************************");
                    throw;
                }
                invite.InvitorId = loginUser.Id;
                invite.InviteeId = newUser.Id;
                invite.InviteDate = DateTime.Now;
                invite.IsValid = true;
                _context.Add(invite);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index","Home");
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            return RedirectToAction("Index", "Home");
        }

        // GET: Invites/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,CompanyToken,InviteDate,IsValid,CompanyId,InvitorId,InviteeId")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            return RedirectToAction("Index", "Home");
        }

        // GET: Invites/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invite = await _context.Invite.FindAsync(id);
            _context.Invite.Remove(invite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
            return _context.Invite.Any(e => e.Id == id);
        }
    }
}

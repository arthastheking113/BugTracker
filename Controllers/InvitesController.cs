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

namespace BugTracker.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InvitesController(ILogger<HomeController> logger, ApplicationDbContext context,UserManager<CustomUser> userManager, IEmailSender emailSender)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
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
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,CompanyToken,InviteDate,IsValid,CompanyId,InvitorId,InviteeId")] Invite invite)
        {
            if (ModelState.IsValid)
            {
                CustomUser newUser = new CustomUser 
                { 
                    FirstName = "Invite",
                    LastName = "User",
                    UserName = invite.Email,
                    Email = invite.Email,
                    EmailConfirmed = false,
                    CompanyId = invite.CompanyId
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
                            _logger.LogInformation("User created a new account with password.");
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = newUser.Id, code = code, returnUrl = returnUrl },
                                protocol: Request.Scheme);

                            await _emailSender.SendEmailAsync(newUser.Email, "Invite Email From Lan's Bug Tracker",
                                $"You received a invite ticket from Lan's Bug Tracker <br> <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a> to join our software. <br> Your UserName is: {newUser.Email} <br> Your Password is: Abc123!");

                        }
                        await _userManager.AddToRoleAsync(newUser, Roles.NewUser.ToString());
                        var admin = await _userManager.FindByEmailAsync("arthastheking113@gmail.com");
                 
                        WelcomeNotification notification = new WelcomeNotification
                        {
                            Name = "Welcome To The Bug Tracker",
                            Description = "You have been Invited to the bug tracker service, please wait our admin and project manager assign you a differnt role in the system. You can contact our staff by the inbox system.",
                            Created = DateTime.Now,
                            SenderId = (admin).Id,
                            RecipientId = newUser.Id
                        };
                        await _context.WelcomeNotification.AddAsync(notification);
                        await _context.SaveChangesAsync();
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
                invite.InvitorId = (await _userManager.GetUserAsync(User)).Id;
                invite.InviteeId = (await _userManager.FindByEmailAsync(newUser.Email)).Id;
                invite.InviteDate = DateTime.Now;

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

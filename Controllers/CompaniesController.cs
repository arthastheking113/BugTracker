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
using BugTracker.Services;
using BugTracker.Data.Enums;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICustomRoleService _roleService;

        public CompaniesController(ApplicationDbContext context, UserManager<CustomUser> userManager, ICustomRoleService roleService)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Company.ToListAsync());
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Company company)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (ModelState.IsValid)
                {
                    _context.Add(company);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(company);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (id != company.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(company);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CompanyExists(company.Id))
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
                return View(company);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                var company = await _context.Company.FindAsync(id);

                var projectList = _context.Project.Include(p => p.Attachments).Where(p => p.CompanyId == id).ToList();
                // delete project of company
                foreach (var project in projectList)
                {
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
                }

                var listUser = _context.Users.Where(u => u.CompanyId == company.Id).ToList();
                var temporaryCompanyId = _context.Company.FirstOrDefault(c => c.Name == "Temporary Company").Id;
                foreach (var user in listUser)
                {
                    user.CompanyId = temporaryCompanyId;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                //delete company
                _context.Company.Remove(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        private bool CompanyExists(int id)
        {
            return _context.Company.Any(e => e.Id == id);
        }
    }
}

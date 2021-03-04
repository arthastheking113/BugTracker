using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services;
using BugTracker.Data.Enums;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICustomRoleService _roleService;

        public ProjectAttachmentsController(ApplicationDbContext context, UserManager<CustomUser> userManager, ICustomRoleService roleService)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
        }

        // GET: ProjectAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProjectAttachment.Include(p => p.CustomUser).Include(p => p.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProjectAttachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectAttachment = await _context.ProjectAttachment
                .Include(p => p.CustomUser)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectAttachment == null)
            {
                return NotFound();
            }

            return View(projectAttachment);
        }

        // GET: ProjectAttachments/Create
        public IActionResult Create()
        {
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id");
            return View();
        }

        // POST: ProjectAttachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FormFile,Image,Description,Created,FileName,FileData,ProjectId,CustomUserId")] ProjectAttachment projectAttachment)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (ModelState.IsValid)
                {
                    MemoryStream ms = new MemoryStream();
                    await projectAttachment.FormFile.CopyToAsync(ms);

                    projectAttachment.FileData = ms.ToArray();
                    projectAttachment.FileName = projectAttachment.FormFile.FileName;
                    projectAttachment.Created = DateTimeOffset.Now;
                    projectAttachment.CustomUserId = _userManager.GetUserId(User);
                    var id = projectAttachment.ProjectId;
                    _context.Add(projectAttachment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Projects", new { id });
                }
                ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", projectAttachment.CustomUserId);
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", projectAttachment.ProjectId);
                return View(projectAttachment);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: ProjectAttachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectAttachment = await _context.ProjectAttachment.FindAsync(id);
            if (projectAttachment == null)
            {
                return NotFound();
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", projectAttachment.CustomUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", projectAttachment.ProjectId);
            return View(projectAttachment);
        }

        // POST: ProjectAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Created,FileName,FileData,ProjectId,CustomUserId")] ProjectAttachment projectAttachment)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (id != projectAttachment.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(projectAttachment);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProjectAttachmentExists(projectAttachment.Id))
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
                ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", projectAttachment.CustomUserId);
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", projectAttachment.ProjectId);
                return View(projectAttachment);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: ProjectAttachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectAttachment = await _context.ProjectAttachment
                .Include(p => p.CustomUser)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectAttachment == null)
            {
                return NotFound();
            }

            return View(projectAttachment);
        }

        // POST: ProjectAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                var projectAttachment = await _context.ProjectAttachment.FindAsync(id);
                _context.ProjectAttachment.Remove(projectAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        private bool ProjectAttachmentExists(int id)
        {
            return _context.ProjectAttachment.Any(e => e.Id == id);
        }
    }
}

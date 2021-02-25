using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;

namespace BugTracker.Controllers
{
    public class ProjectAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectAttachmentsController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("Id,Description,Created,FileName,FileData,ProjectId,CustomUserId")] ProjectAttachment projectAttachment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", projectAttachment.CustomUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", projectAttachment.ProjectId);
            return View(projectAttachment);
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
            var projectAttachment = await _context.ProjectAttachment.FindAsync(id);
            _context.ProjectAttachment.Remove(projectAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectAttachmentExists(int id)
        {
            return _context.ProjectAttachment.Any(e => e.Id == id);
        }
    }
}

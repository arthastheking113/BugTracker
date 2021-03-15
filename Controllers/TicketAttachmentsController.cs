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
    [Authorize(Roles = "Admin, ProjectManager, Developer")]
    public class TicketAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICustomFileService _fileService;
        private readonly ICustomRoleService _roleService;
        private readonly ICustomProjectService _projectService;

        public TicketAttachmentsController(ApplicationDbContext context, 
            UserManager<CustomUser> userManager, 
            ICustomFileService fileService, 
            ICustomRoleService roleService,
            ICustomProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _roleService = roleService;
            _projectService = projectService;
        }

        // GET: TicketAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Attachment.Include(t => t.CustomUser).Include(t => t.Ticket);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketAttachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.Attachment
                .Include(t => t.CustomUser)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        public IActionResult Create()
        {
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id");
            return View();
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FormFile,Image,Description,Created,FileName,ContentType,FileData,TicketId,CustomUserId")] TicketAttachment ticketAttachment)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (ModelState.IsValid)
                {
                    MemoryStream ms = new MemoryStream();
                    await ticketAttachment.FormFile.CopyToAsync(ms);

                    ticketAttachment.FileData = ms.ToArray();
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.ContentType = ticketAttachment.FormFile.ContentType;
                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.CustomUserId = _userManager.GetUserId(User);
                    var id = ticketAttachment.TicketId;
                    _context.Add(ticketAttachment);
                    await _context.SaveChangesAsync();
                    var loginUser = await _userManager.GetUserAsync(User);
                    var projectId = _context.Ticket.FirstOrDefault(t => t.Id == id).ProjectId;
                    Notification notification = new Notification
                    {
                        Name = $"New Attachment on Ticket #{id}",
                        Description = $"A new attachment: {ticketAttachment.FileName} has been uploaded by {loginUser.FullName} into ticket #{id}",
                        Created = DateTime.Now,
                        SenderId = loginUser.Id,
                        RecipientId = (await _projectService.ProjectManagerOnProjectAsync(projectId)).Id,
                        TicketId = id
                    };
                    await _context.Notification.AddAsync(notification);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Tickets", new { id });
                }
                ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.CustomUserId);
                ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", ticketAttachment.TicketId);
                return View(ticketAttachment);
            }
            return RedirectToAction("DemoUser", "Projects");
        }


        public async Task<FileResult> DownloadFile(int? id)
        {
            if (id == null)
            {
                return null;
            }
            TicketAttachment attachment = await _context.Attachment.FirstOrDefaultAsync(t => t.Id == id);

            var ContentType = _fileService.ConvertByteArrayToFile(attachment.FileData, attachment.FileName);

            if (attachment == null)
            {
                return null;
            }
            return File(attachment.FileData, attachment.ContentType);
        }

        // GET: TicketAttachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.Attachment.FindAsync(id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.CustomUserId);
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", ticketAttachment.TicketId);
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FormFile,Image,Description,Created,FileName,ContentType,FileData,TicketId,CustomUserId")] TicketAttachment ticketAttachment)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                if (id != ticketAttachment.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        if (ticketAttachment.FormFile != null)
                        {
                            MemoryStream ms = new MemoryStream();
                            await ticketAttachment.FormFile.CopyToAsync(ms);
                            ticketAttachment.FileData = ms.ToArray();
                            ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                            ticketAttachment.ContentType = ticketAttachment.FormFile.ContentType;
                        }

                        ticketAttachment.Created = DateTimeOffset.Now;
                        ticketAttachment.CustomUserId = _userManager.GetUserId(User);
                        _context.Update(ticketAttachment);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TicketAttachmentExists(ticketAttachment.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Details", "Tickets", new { id });
                }
                ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttachment.CustomUserId);
                ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", ticketAttachment.TicketId);
                return View(ticketAttachment);
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        // GET: TicketAttachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.Attachment
                .Include(t => t.CustomUser)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(await _roleService.IsUserInRoleAsync(await _userManager.GetUserAsync(User), Roles.DemoUser.ToString())))
            {
                var ticketAttachment = await _context.Attachment.FindAsync(id);
                _context.Attachment.Remove(ticketAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("DemoUser", "Projects");
        }

        private bool TicketAttachmentExists(int id)
        {
            return _context.Attachment.Any(e => e.Id == id);
        }
    }
}

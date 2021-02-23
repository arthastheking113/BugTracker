using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Service;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class CustomHistoryService : ICustomHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public CustomHistoryService(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            if (oldTicket.Name != newTicket.Name)
            {
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Name",
                    OldValue = oldTicket.Name,
                    NewValue = oldTicket.Name,
                    Created = DateTime.Now,
                    CustomUserId = userId
                };
                await _context.TicketHistory.AddAsync(history);
            }
            if (oldTicket.Description != newTicket.Description)
            {
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Description",
                    OldValue = oldTicket.Description,
                    NewValue = oldTicket.Description,
                    Created = DateTime.Now,
                    CustomUserId = userId
                };
                await _context.TicketHistory.AddAsync(history);
            }
            if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
            {
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Ticket Type",
                    OldValue = oldTicket.TicketType.Name,
                    NewValue = oldTicket.TicketType.Name,
                    Created = DateTime.Now,
                    CustomUserId = userId
                };
                await _context.TicketHistory.AddAsync(history);
            }

            if (oldTicket.PriorityId != newTicket.PriorityId)
            {
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Priority",
                    OldValue = oldTicket.Priority.Name,
                    NewValue = oldTicket.Priority.Name,
                    Created = DateTime.Now,
                    CustomUserId = userId
                };
                await _context.TicketHistory.AddAsync(history);
            }


            if (oldTicket.StatusId != newTicket.StatusId)
            {
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Status",
                    OldValue = oldTicket.Status.Name,
                    NewValue = oldTicket.Status.Name,
                    Created = DateTime.Now,
                    CustomUserId = userId
                };
                await _context.TicketHistory.AddAsync(history);
            }

            if (oldTicket.DeveloperId != newTicket.DeveloperId)
            {
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "CustomUser",
                    OldValue = oldTicket.Developer.FullName,
                    NewValue = oldTicket.Developer.FullName,
                    Created = DateTime.Now,
                    CustomUserId = userId
                };
                await _context.TicketHistory.AddAsync(history);
            }

            Notification notification = new Notification
            {
                TicketId = newTicket.Id,
                Description = "You have a new ticket.",
                Created = DateTimeOffset.Now,
                SenderId = userId,
                RecipientId = newTicket.DeveloperId
            };

            //send email
            string devEmail = newTicket.Developer.Email;
            string subject = "New Ticket Assignment";
            string message = $"You have a new ticket for project: {newTicket.Project.Name}";

            await _emailSender.SendEmailAsync(devEmail, subject, message);

            await _context.SaveChangesAsync();
        }
    }
}

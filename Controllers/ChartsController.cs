using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ChartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly List<string> _backgroundColors;

        public ChartsController(ApplicationDbContext context)
        {
            _context = context;
            _backgroundColors = new List<string>
            {
                "#f56954", 
                "#00a65a", 
                "#f39c12", 
                "#00c0ef", 
                "#3c8dbc", 
                "#d2d6de",
                "#19D0FF",
                "#D20DFF",
                "#A0FF19",
                "#00FF06",
                "#000000"
            };
        }

        public JsonResult PriorityChart()
        {
            var result = new ChartJSModel();
            int count = 0;
            foreach (var priority in _context.Priority.ToList())
            {
                result.Labels.Add(priority.Name);
                result.Data.Add(_context.Ticket.Where(t => t.PriorityId == priority.Id).Count());
                if (count < 11)
                {
                    result.BackgroundColor.Add(_backgroundColors[count]);
                }
                else
                {
                    result.BackgroundColor.Add(_backgroundColors[(count % 10)]);
                }
                count++;
            }
            return Json(result);
        }

        public JsonResult StatusChart()
        {
            var result = new ChartJSModel();
            int count = 0;
            foreach (var status in _context.Status.ToList())
            {
                result.Labels.Add(status.Name);
                result.Data.Add(_context.Ticket.Where(t => t.StatusId == status.Id).Count());
                if (count < 10)
                {
                    result.BackgroundColor.Add(_backgroundColors[count]);
                }
                else
                {
                    result.BackgroundColor.Add(_backgroundColors[(count % 10)]);
                }
                count++;
            }
            return Json(result);
        }

        public JsonResult TypeChart()
        {
            var result = new ChartJSModel();
            int count = 0;
            foreach (var type in _context.TicketType.ToList())
            {
                result.Labels.Add(type.Name);
                result.Data.Add(_context.Ticket.Where(t => t.TicketTypeId == type.Id).Count());
                if (count < 10)
                {
                    result.BackgroundColor.Add(_backgroundColors[count]);
                }
                else
                {
                    result.BackgroundColor.Add(_backgroundColors[(count % 10)]);
                }
                count++;
            }
            return Json(result);

        }
    }
}

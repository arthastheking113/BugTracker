﻿using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public interface ICustomHistoryService
    {
        public Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsViewed { get; set; }
        public DateTime Created { get; set; }

        public string SenderId { get; set; }
        public virtual CustomUser Sender { get; set; }

        public string RecipientId { get; set; }
        public virtual CustomUser Recipient { get; set; }

        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}

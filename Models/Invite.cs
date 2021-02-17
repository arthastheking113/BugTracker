using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }
       
        public string Email { get; set; }
        public Guid CompanyToken { get; set; }
        public DateTimeOffset InviteDate { get; set; }
        
       
        public bool IsValid { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public string InvitorId { get; set; }
        public virtual CustomUser Invitor { get; set; }

        public string InviteeId { get; set; }
        public virtual CustomUser Invitee { get; set; }

    }
}

﻿using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public interface ICustomRoleService
    {
        public Task<bool> AddUserToRoleAsync(CustomUser user,string role);

        public Task<bool> IsUserInRoleAsync(CustomUser user, string role);

        public Task<IEnumerable<string>> ListUserRoleAsync(CustomUser user);

        public Task<bool> RemoveUserFromRoleAsync(CustomUser user, string role);

        public Task<IEnumerable<CustomUser>> UsersInRoleAsync(string role);

        public Task<IEnumerable<CustomUser>> UsersNotInRoleAsync(string role);

        public IEnumerable<IdentityRole> NonDemoRoles();
    }
}

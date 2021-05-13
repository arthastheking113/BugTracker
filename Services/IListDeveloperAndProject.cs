using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public interface IListDeveloperAndProject
    {
        public Task<IEnumerable<CustomUser>> ReturnDeveloperOnlyAsync();

        public Task<(IEnumerable<CustomUser>, List<Project>)> ReturnDeveloperAndProjectAsync(CustomUser User);
    }
}

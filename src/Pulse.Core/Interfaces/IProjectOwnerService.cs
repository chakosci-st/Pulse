using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project member data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectOwnerService
    {
        /// <summary>
        /// Update node to owner in the system.
        /// </summary>
        /// <param name="add">The node to add.</param> 
        /// <param name="remove">The node to remove.</param> 
        Task<(int addcount, int deletedcount)> UpdateNodeOwnerAsync(List<ProjectOwner> addnode, List<ProjectOwner> removenode);

        Task<ProjectOwner> GetAsync(string projectno, string memberid, string nodetype, string nodeid);
    }
}

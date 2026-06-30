using Pulse.Core.Entities;
using Pulse.Core.Enums;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Task = System.Threading.Tasks.Task;


namespace Pulse.Services.Implementations
{
    public class ProjectOwnerService : IProjectOwnerService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectOwnerRepository _projectownerRepository;
        public ProjectOwnerService(OracleDataAccessLayer dataAccess, IProjectOwnerRepository projectownerRepository)
        {
            _dataAccess = dataAccess;
            _projectownerRepository = projectownerRepository;
        }


        public async Task<(int addcount, int deletedcount)> UpdateNodeOwnerAsync(List<ProjectOwner> addnode, List<ProjectOwner> removenode)
        {
            _dataAccess.BeginTransaction();

            int rows_created = 0;
            int rows_deleted = 0;

            try
            {
                foreach (var node in addnode)
                {
                    await _projectownerRepository.AddAsync(node);
                    rows_created++;
                }

                foreach (var node in removenode)
                {
                    var ProjectOwner = await this.GetAsync(node.ProjectNo, node.UserId, node.ParentType, node.ParentSysId);

                    if (ProjectOwner != null)
                    {
                        await _projectownerRepository.DeleteAsync(ProjectOwner.ProjectOwnerSysId);
                        rows_deleted++;
                    }

                }

                _dataAccess.CommitTransaction();
            }
            catch (Exception e)
            {
                _dataAccess.RollbackTransaction();
            }



            return (rows_created, rows_deleted);

        }

        public async Task<ProjectOwner> GetAsync(string projectno, string memberid, string nodetype, string nodeid)
        {
            return await _projectownerRepository.GetAsync(projectno, memberid, nodetype, nodeid);
        }

    }
}

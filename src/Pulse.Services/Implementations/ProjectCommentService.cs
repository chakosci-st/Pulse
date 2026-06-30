using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Pulse.Services.Implementations
{
    public class ProjectCommentService : IProjectCommentService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectCommentRepository _projectcommentRepository;

        public ProjectCommentService(OracleDataAccessLayer dataAccess, IProjectCommentRepository projectcommentRepository)
        {
            _dataAccess = dataAccess;
            _projectcommentRepository = projectcommentRepository;
        }


        public async Task<string> AddAsync(ProjectComment obj)
        {
            return await _projectcommentRepository.AddAsync(obj);
        }

        public async Task<IEnumerable<ProjectComment>> GetByProjectAsync(string projectno)
        {
            return await _projectcommentRepository.GetListAsync(projectno);
        }

        public async Task<IEnumerable<ProjectComment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid)
        {
            return await _projectcommentRepository.GetByEntityAsync(projectno, entitytype, entitysysid);
        }

         
    }
}

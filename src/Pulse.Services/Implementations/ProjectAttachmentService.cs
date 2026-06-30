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
    public class ProjectAttachmentService : IProjectAttachmentService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectAttachmentRepository _projectattachmentRepository;

        public ProjectAttachmentService(OracleDataAccessLayer dataAccess, IProjectAttachmentRepository projectattachmentRepository)
        {
            _dataAccess = dataAccess;
            _projectattachmentRepository = projectattachmentRepository;
        }


        public async Task<string> AddAsync(ProjectAttachment obj)
        {
            return await _projectattachmentRepository.AddAsync(obj);
        }

        public async Task<IEnumerable<ProjectAttachment>> GetByProjectAsync(string projectno)
        {
            return await _projectattachmentRepository.GetListAsync(projectno);
        }

        public async Task<IEnumerable<ProjectAttachment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid)
        {
            return await _projectattachmentRepository.GetByEntityAsync(projectno, entitytype, entitysysid);
        }

        public async Task<ProjectAttachment> GetByIdAsync(string attachmentSysId)
        {
            return await _projectattachmentRepository.GetAsync(attachmentSysId);
        }

        public async Task<int> RemoveAsync(string attachmentSysId)
        {
            return await _projectattachmentRepository.DeleteAsync(attachmentSysId);
        }

         
    }
}

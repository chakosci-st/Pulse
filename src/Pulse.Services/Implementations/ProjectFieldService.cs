using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
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
    public class ProjectFieldService : IProjectFieldService
    {
        private readonly IProjectFieldRepository _projectfieldRepository;
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IEventPublisher _eventBus;

        private DateTime transactiondatetime = DateTime.UtcNow;
        public ProjectFieldService(OracleDataAccessLayer dataAccess, IEventPublisher eventBus, IProjectFieldRepository projectfieldRepository)
        {
            _dataAccess = dataAccess;
            _eventBus = eventBus;
            _projectfieldRepository = projectfieldRepository;
        }

        public async Task AddProjectFieldAsync(IEnumerable<ProjectField> projectfield, string loggeduserid)
        {
            
            _dataAccess.BeginTransaction();
            try
            {
                projectfield = projectfield.Select(pf => { pf.CreatedBy = loggeduserid; pf.CreatedDate = transactiondatetime; return pf; });

                var addprojectTaskCollection = projectfield.Select(f => _projectfieldRepository.AddAsync(f));
                await Task.WhenAll(addprojectTaskCollection);

                _dataAccess.CommitTransaction();

                 _eventBus.Publish(new ProjectFieldCreatedEventArgs(loggeduserid, transactiondatetime));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);

            }

        }
        public async Task UpdateProjectFieldAsync(IEnumerable<ProjectField> projectfield, string loggeduserid)
        {
            _dataAccess.BeginTransaction();
            try
            {
                projectfield = projectfield.Select(pf => { pf.ModifiedBy = loggeduserid; pf.ModifiedDate = transactiondatetime; return pf; });

                var updateprojectTaskCollection = projectfield.Select(f => _projectfieldRepository.UpdateAsync(f));
                await Task.WhenAll(updateprojectTaskCollection);

                _dataAccess.CommitTransaction();

                _eventBus.Publish(new ProjectFieldUpdatedEventArgs(loggeduserid, transactiondatetime));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);

            }

        }
        public async Task DeleteProjectFieldAsync(IEnumerable<string> projectfieldsysid, string loggeduserid)
        {
            _dataAccess.BeginTransaction();

            try
            {
                var getfieldsTaskCollection = projectfieldsysid.Select(f => _projectfieldRepository.GetAsync(f));
                var fields = await Task.WhenAll(getfieldsTaskCollection);

                Array.ForEach(fields, field =>
                {
                    field.ModifiedBy = loggeduserid;
                    field.ModifiedDate = transactiondatetime;
                });
 
                //SET USER WHO DELETES THE User
                var fieldstoupdateTaskCollection = fields.Select(f => _projectfieldRepository.UpdateAsync(f));
                await Task.WhenAll(fieldstoupdateTaskCollection);

                //DELETE RECORD
                var fieldstodeoleteTaskCollection = fields.Select(f => _projectfieldRepository.DeleteAsync(f.ProjectFieldSysId));
                await Task.WhenAll(fieldstodeoleteTaskCollection);


                _dataAccess.CommitTransaction();

                _eventBus.Publish(new ProjectFieldDeletedEventArgs(loggeduserid, transactiondatetime));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);

            }
        }

        public async Task<ProjectField> GetByIdAsync(string projectfieldsysid)
        {
            return await _projectfieldRepository.GetAsync(projectfieldsysid);
        }

        public async Task<IEnumerable<ProjectField>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null)
        {
            return await _projectfieldRepository.GetListAsync(projectno, milestonesysid, tasksysid);
        }






    }
}

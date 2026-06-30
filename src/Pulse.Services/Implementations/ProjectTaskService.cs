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
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectTaskRepository _projecttaskRepository;
        private readonly IProjectStatusChangeRepository _projectstatuschangeRepository;
        private readonly IProjectTargetRevisionRepository _projecttargetrevisionRepository;
        private readonly IEventPublisher _eventBus;

        private DateTime transactiondatetime = DateTime.UtcNow;
        public ProjectTaskService(OracleDataAccessLayer dataAccess, IEventPublisher eventBus, IProjectTaskRepository projecttaskRepository, IProjectStatusChangeRepository projectstatuschangeRepository, IProjectTargetRevisionRepository projecttargetrevisionRepository)
        {
            _dataAccess = dataAccess;
            _eventBus = eventBus;
            _projecttaskRepository = projecttaskRepository;
            _projectstatuschangeRepository = projectstatuschangeRepository;
            _projecttargetrevisionRepository = projecttargetrevisionRepository;
        }


        private async Task ApplyStatusAsync(ProjectTask obj, string status, string reason, DateTime? actualdate)
        {
            var _obj = await _projecttaskRepository.GetAsync(obj.ProjectTaskSysId);

            if (!string.IsNullOrEmpty(obj.TransactionKey))
            {
                _obj.TransactionKey = obj.TransactionKey;
            }

            _obj.Status = status;
            _obj.ModifiedBy = obj.ModifiedBy;
            _obj.ModifiedDate = DateTime.UtcNow;
            _obj.ActualStartDate = status == "NOT STARTED" ? null : (status == "ONGOING" ? actualdate : _obj.ActualStartDate);
            _obj.ActualStartedBy = status == "NOT STARTED" ? null : (status == "ONGOING" ? obj.ModifiedBy : _obj.ActualStartedBy);
            _obj.ActualCompletionDate = status == "NOT STARTED" ? null : ((status == "COMPLETED" || status == "ARCHIVED") ? actualdate : _obj.ActualCompletionDate);
            _obj.ActualCompletedBy = status == "NOT STARTED" ? null : ((status == "COMPLETED" || status == "ARCHIVED") ? obj.ModifiedBy : _obj.ActualCompletedBy);

            if (await _projecttaskRepository.UpdateAsync(_obj) > 0)
            {
                var statusActualDate = status == "NOT STARTED"
                    ? DateTime.UtcNow
                    : (status == "ONGOING"
                        ? (actualdate ?? DateTime.UtcNow)
                        : (actualdate ?? _obj.ActualCompletionDate ?? _obj.ActualStartDate ?? DateTime.UtcNow));

                await _projectstatuschangeRepository.AddAsync(new ProjectStatusChange
                {
                    Status = status,
                    ProjectNo = obj.ProjectNo,
                    ActualDate = statusActualDate,
                    CreatedBy = obj.ModifiedBy,
                    EntitySysId = obj.ProjectTaskSysId,
                    EntityType = "TASK",
                    Remarks = reason
                });
            }
            else
            {
                throw new Exception("Project is either recently updated or removed.");
            }
        }

        public async Task InitializeAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "NOT STARTED", reason, null);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskNotStartedEventArgs(obj.ProjectTaskSysId, obj.ModifiedBy, obj.ModifiedDate.Value, reason));

            }
            catch
            {

                throw;
            }
        }

        public async Task StartAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "ONGOING", reason, obj.ActualStartDate);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskStartedEventArgs(obj.ProjectTaskSysId, obj.ModifiedBy, obj.ActualStartDate.Value, reason));

            }
            catch
            {
                throw;
            }

        }

        public async Task HoldAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "HOLD", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskHoldEventArgs(obj.ProjectTaskSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task UnholdAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {

                await ApplyStatusAsync(obj, "ONGOING", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskResumedEventArgs(obj.ProjectTaskSysId, Core.Enums.Status.Hold, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task CancelAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "CANCEL", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskCanceledEventArgs(obj.ProjectTaskSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task ArchiveAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "ARCHIVED", reason, DateTime.UtcNow);
            }
            catch
            {
                throw;
            }
        }

        public async Task CompleteAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                //CHECK IF ALL REQUIRED TASKS ARE COMPLETE  

                await ApplyStatusAsync(obj, "COMPLETED", reason, obj.ActualCompletionDate);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskCompletedEventArgs(obj.ProjectTaskSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task ForceCompleteAsync(ProjectTask obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "COMPLETED", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectTaskCompletedEventArgs(obj.ProjectTaskSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task<ProjectTask> GetTaskByIdAsync(string id)
        {
            return await _projecttaskRepository.GetAsync(id);
        }


        public async Task SetTargetAsync(ProjectTask task, string reason, string loggeduser)
        {
            _dataAccess.BeginTransaction();
            try
            {

                if (await _projecttaskRepository.UpdateTargetDateAsync(task) > 0)
                {
                    await _projecttargetrevisionRepository.AddAsync(new ProjectTargetRevision
                    {
                        ProjectNo = task.ProjectNo,
                        ProjectNodeSysId = task.ProjectTaskSysId,
                        EntityType = "ACTIVITY",
                        EntitySysId = task.RoadmapActivitySysId,
                        TargetStartDate = task.TargetStartDate,
                        TargetCompletionDate = task.TargetCompletionDate,
                        Reason = reason,
                        CreatedBy = loggeduser
                    });
                    _dataAccess.CommitTransaction();
                }
                else
                {
                    _dataAccess.RollbackTransaction();
                    throw new Exception("Project is either recently updated or removed.");

                }

            }
            catch (Exception e)
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public Task ChangeMilestoneTargetAsync(string milestonesysid, string targetstart, string targetcompletion, string transactionkey, string loggeduser, string reason)
        {
            throw new NotImplementedException();
        }

        public async Task<string> AddTaskAsync(ProjectTask task, string loggeduser)
        {
            return await _projecttaskRepository.AddAsync(task);
        }

        public async Task UpdateTaskAsync(ProjectTask task, string loggeduser)
        {
            task.ModifiedBy = loggeduser;
            task.ModifiedDate = DateTime.UtcNow;

            if (await _projecttaskRepository.UpdateAsync(task) <= 0)
            {
                throw new Exception("Project is either recently updated or removed.");
            }
        }

        public Task DeleteTaskAsync(ProjectTask task, string loggeduser)
        {
            throw new NotImplementedException();
        }
        public async Task<ProjectTaskItem> GetItemDetailsAsync(string projecttasksysid, string userid)
        {
            return (await _projecttaskRepository.GetTaskItemListAsync(projecttasksysid: projecttasksysid)).Where(t => t.Members.IndexOf(userid) >= 0).SingleOrDefault();
        }

        public async Task<ProjectTaskItem> GetItemDetailsReadOnlyAsync(string projecttasksysid, string userid)
        {
            return (await _projecttaskRepository.GetTaskItemListAsync(projecttasksysid: projecttasksysid, userid: userid)).SingleOrDefault();
        }

        public async Task<IEnumerable<ProjectTaskItem>> GetItemListAsync(string userid)
        {
            return await _projecttaskRepository.GetTaskItemListAsync(userid: userid);
        }


    }
}

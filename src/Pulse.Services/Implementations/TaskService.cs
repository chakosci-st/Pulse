using AutoMapper;
using Pulse.Core;
using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using Pulse.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Pulse.Services.Implementations
{
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
    public class TaskService : ITaskService
    {
        private readonly IEventPublisher _eventBus;
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly ITaskRepository _taskRepository;
        private readonly IStatusChangeRepository _statuschangeRepository;
        private readonly ITargetRevisionRepository _targetrevisionRepository;

        public TaskService(IEventPublisher eventBus, OracleDataAccessLayer dataAccess, ITaskRepository taskRepository, IStatusChangeRepository statuschangeRepository, ITargetRevisionRepository targetrevisionRepository)
        {
            _eventBus = eventBus;
            _dataAccess = dataAccess;
            _taskRepository = taskRepository;
            _statuschangeRepository = statuschangeRepository;
            _targetrevisionRepository = targetrevisionRepository;
        }



        public async Task<string> AddTaskAsync(Core.Entities.Task task)
        {
            try
            {
                task.CreatedDate = DateTime.UtcNow;
                var id = await _taskRepository.AddAsync(task);

                // raise event that Task status was changed
                _eventBus.Publish(new TaskCreatedEventArgs(id, task.CreatedBy, task.CreatedDate));

                return id;
            }
            catch
            {
                throw;
            }

        }
        public async Task<int> UpdateTaskAsync(Core.Entities.Task task)
        {
            return await _taskRepository.UpdateAsync(task);
        }

        public async Task<int> DeleteTaskAsync(string taskid, string loggeduser)
        {
            _dataAccess.BeginTransaction();
            try
            {
                var task = await _taskRepository.GetAsync(taskid);
                if (task == null)
                {
                    throw new Exception("Delete Failed! Task is already removed.");
                }

                task.ModifiedBy = loggeduser;

                var rowsaffected = await _taskRepository.UpdateAsync(task);
                if (rowsaffected > 0)
                {
                    await _taskRepository.DeleteAsync(taskid);
                    _dataAccess.CommitTransaction();
                }
                else
                {
                    _dataAccess.RollbackTransaction();
                    throw new Exception("Delete Failed! Task is either updated recently or already removed.");
                }


            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw;
            }

            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Core.Entities.Task>> GetAllTasksAsync()
        {
            return (await _taskRepository.GetListAsync()).ToList().Select(Mapper.Map<Core.Entities.Task>);
        }

        public async Task<IEnumerable<Core.Entities.Task>> GetAllTasksPerActiveDirectoryGroupAsync(string activedirectorygroup)
        {
            return (await _taskRepository.GetListAsync(adgroup: activedirectorygroup)).ToList().Select(Mapper.Map<Core.Entities.Task>);
        }

        public async Task<IEnumerable<Core.Entities.Task>> GetAllTasksPerProjectAsync(string projectno)
        {
            return (await _taskRepository.GetListAsync(projectno: projectno)).ToList().Select(Mapper.Map<Core.Entities.Task>);
        }

        public async Task<IEnumerable<Core.Entities.Task>> GetAllTasksPerUserAsync(string userid)
        {
            return (await _taskRepository.GetListAsync(userid: userid)).ToList().Select(Mapper.Map<Core.Entities.Task>);
        }

        public async Task<IEnumerable<Core.Entities.Task>> GetAllTasksPerUserGroupAsync(int usergroupid)
        {
            return (await _taskRepository.GetListAsync(usergroupid: usergroupid)).ToList().Select(Mapper.Map<Core.Entities.Task>);
        }

        public async Task<Core.Entities.Task> GetTaskByIdAsync(string taskid)
        {
            return await _taskRepository.GetAsync(taskid);
        }


        private async Task ChangeStatus(Core.Enums.Status status, Core.Entities.Task task, string reason)
        {
            _dataAccess.BeginTransaction();
            try
            {
                var transactiondate = DateTime.Now;
                var _task = await _taskRepository.GetAsync(task.TaskSysId);
                _task.Status = status.ToString();
                _task.TransactionKey = task.TransactionKey;
                _task.ActualStartDate = status == Core.Enums.Status.Ongoing ? task.ActualStartDate ?? DateTime.Now : _task.ActualStartDate;
                _task.ActualCompletionDate = status == Core.Enums.Status.Completed ? task.ActualCompletionDate ?? DateTime.Now : _task.ActualCompletionDate;
                _task.ModifiedDate = transactiondate;
                var rowsaffected = await _taskRepository.UpdateAsync(_task);

                await _statuschangeRepository.AddAsync(new StatusChange
                {
                    ProjectNo = _task.ProjectNo,
                    MilestoneSysId = _task.MilestoneSysId,
                    TaskSysId = _task.TaskSysId,
                    Status = status.ToString(),
                    Reason = reason,
                    CreatedBy = _task.ModifiedBy,
                    CreatedDate = transactiondate
                });


                _dataAccess.CommitTransaction();

                // raise event that Task status was changed
                _eventBus.Publish(new TaskStatusChangeEventArgs(task.TaskSysId, (Core.Enums.Status)Enum.Parse(typeof(Core.Enums.Status), task.Status), Core.Enums.Status.NotStarted, task.ModifiedBy, transactiondate, reason));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw;
            }

        }

        public async Task ChangeStatusToNotStartedAsync(Core.Entities.Task task, string reason)
        {
            task.ModifiedDate = DateTime.UtcNow;
            await this.ChangeStatus(Core.Enums.Status.NotStarted, task, reason);
        }

        public async Task ChangeStatusToOngoingAsync(Core.Entities.Task task, string reason)
        {
            await this.ChangeStatus(Core.Enums.Status.Ongoing, task, reason);

        }

        public async Task ChangeStatusToCompletedAsync(Core.Entities.Task task, string reason)
        {
            await this.ChangeStatus(Core.Enums.Status.Completed, task, reason);
        }

        public async Task ChangeStatusToCancelledAsync(Core.Entities.Task task, string reason)
        {
            await this.ChangeStatus(Core.Enums.Status.Canceled, task, reason);
        }

        public async Task ChangeStatusToHoldAsync(Core.Entities.Task task, string reason)
        {
            await this.ChangeStatus(Core.Enums.Status.Hold, task, reason);
        }

        public async Task ChangeStatusToFailedAsync(Core.Entities.Task task, string reason)
        {
            await this.ChangeStatus(Core.Enums.Status.Failed, task, reason);
        }

        public async Task<IEnumerable<Core.Entities.TargetRevision>> GetAllTargetRevisionsAsync(string tasksysid)
        {
            return await _targetrevisionRepository.GetListAsync(tasksysid: tasksysid);
        }
    }
}

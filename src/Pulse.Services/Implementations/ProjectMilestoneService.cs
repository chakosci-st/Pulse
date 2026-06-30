using AutoMapper;
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
    public class ProjectMilestoneService : IProjectMilestoneService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMilestoneRepository _projectmilestoneRepository;
        private readonly IProjectTaskService _projecttaskService;
        private readonly IProjectStatusChangeRepository _projectstatuschangeRepository;
        private readonly IProjectTargetRevisionRepository _projecttargetrevisionRepository;
        private readonly ITargetRevisionRepository _targetrevisionRepository;
        private readonly IEventPublisher _eventBus;

        private DateTime transactiondatetime = DateTime.UtcNow;
        public ProjectMilestoneService(OracleDataAccessLayer dataAccess, IEventPublisher eventBus, IProjectRepository projectRepository, IProjectMilestoneRepository projectmilestoneRepository, IProjectTargetRevisionRepository projecttargetrevisionRepository, IProjectTaskService projecttaskService, IProjectStatusChangeRepository projectstatuschangeRepository, ITargetRevisionRepository targetrevisionRepository)
        {
            _dataAccess = dataAccess;
            _eventBus = eventBus;
            _projectRepository = projectRepository;
            _projectmilestoneRepository = projectmilestoneRepository;
            _projecttaskService = projecttaskService;
            _projectstatuschangeRepository = projectstatuschangeRepository;
            _projecttargetrevisionRepository = projecttargetrevisionRepository;
            _targetrevisionRepository = targetrevisionRepository;
        }

        public async Task UpdateMilestoneAsync(ProjectMilestone projectmilestone, string loggeduser)
        {
            projectmilestone.ModifiedBy = loggeduser;
            projectmilestone.ModifiedDate = DateTime.UtcNow;
            await _projectmilestoneRepository.UpdateAsync(projectmilestone);
        }

        public async Task SetTargetAsync(ProjectMilestone milestone, string reason, string loggeduser)
        {
            _dataAccess.BeginTransaction();
            try
            {

                if (await _projectmilestoneRepository.UpdateTargetDateAsync(milestone) > 0)
                {
                    await _projecttargetrevisionRepository.AddAsync(new ProjectTargetRevision
                    {
                        ProjectNo = milestone.ProjectNo,
                        ProjectNodeSysId = milestone.MilestoneSysId,
                        EntityType = "MILESTONE",
                        EntitySysId = milestone.RoadmapMilestoneSysId,
                        TargetStartDate = milestone.TargetStartDate,
                        TargetCompletionDate = milestone.TargetCompletionDate,
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

        ////public async Task ChangeMilestoneTargetAsync(string milestonesysid, string targetstart, string targetcompletion, string transactionkey, string loggeduser, string reason)
        ////{
        ////    var transactiondate = DateTime.UtcNow;
        ////    _dataAccess.BeginTransaction();
        ////    try
        ////    {
        ////        var currentMilestone = await _projectmilestoneRepository.GetAsync(milestonesysid);
        ////        if (currentMilestone.TransactionKey != transactionkey)
        ////        {
        ////            _dataAccess.RollbackTransaction();
        ////            throw new Exception($"Milestone was updated recently, please refresh the page again to get the latest update.");
        ////        }
        ////        currentMilestone.TargetStart = targetstart;
        ////        currentMilestone.TargetCompletion = targetcompletion;
        ////        currentMilestone.ModifiedBy = loggeduser;
        ////        currentMilestone.ModifiedDate = transactiondate;

        ////        // Update milestone
        ////        var rowsaffected = await _projectmilestoneRepository.UpdateAsync(currentMilestone);

        ////        if (rowsaffected > 0)
        ////        {
        ////            // add history
        ////            await _targetrevisionRepository.AddAsync(new TargetRevision
        ////            {
        ////                ProjectNo = currentMilestone.ProjectNo,
        ////                MilestoneSysId = currentMilestone.MilestoneSysId,
        ////                TargetStart = targetstart,
        ////                TargetCompletion = targetcompletion,
        ////                CreatedBy = loggeduser,
        ////                CreatedDate = transactiondate
        ////            });
        ////        }

        ////        _dataAccess.CommitTransaction();
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        _dataAccess.RollbackTransaction();
        ////        throw new Exception(ex.Message);
        ////    }
        ////}


        public async Task<IEnumerable<ProjectMilestone>> GetAllProjectMilestonesAsync()
        {
            return await _projectmilestoneRepository.GetListAsync();
        }


        public async Task<IEnumerable<ProjectMilestone>> GetAllProjectMilestonesAsync(string projectno)
        {
            return await _projectmilestoneRepository.GetListAsync(projectno);
        }

        public async Task<ProjectMilestone> GetProjectMilestoneByIdAsync(string id)
        {
            return await _projectmilestoneRepository.GetAsync(id);
        }

        public async Task<IEnumerable<TargetRevision>> GetAllTargetRevisionsAsync(string projectno, string milestonesysid)
        {
            return await _targetrevisionRepository.GetListAsync(projectno: projectno, milestonesysid: milestonesysid);
        }

        public Task ChangeMilestoneTargetAsync(string milestonesysid, string targetstart, string targetcompletion, string transactionkey, string loggeduser, string reason)
        {
            throw new NotImplementedException();
        }

        private async Task ApplyStatusAsync(ProjectMilestone obj, string status, string reason, DateTime? actualdate)
        {
            var _obj = await _projectmilestoneRepository.GetAsync(obj.MilestoneSysId);

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

            if (await _projectmilestoneRepository.UpdateAsync(_obj) > 0)
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
                    EntitySysId = obj.MilestoneSysId,
                    EntityType = "MILESTONE",
                    Remarks = reason
                });


            }
            else
            {
                throw new Exception("Project is either recently updated or removed.");
            }
        }

        public async Task InitializeAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "NOT STARTED", reason, null);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneNotStartedEventArgs(obj.MilestoneSysId, obj.ModifiedBy, obj.ModifiedDate.Value, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task StartAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            if (begintransaction.Value)
                _dataAccess.BeginTransaction();
            try
            {
                await ApplyStatusAsync(obj, "ONGOING", reason, obj.ActualStartDate);

                if (includechild.Value)
                {
                    var nodes = await _projectRepository.GetProjectNodeChildrenAsync(obj.ProjectNo, "MILESTONE", obj.RoadmapMilestoneSysId);
                    foreach (var node in nodes)
                    {
                        if (node.NodeType.ToLower() == "milestone")
                        {
                            var _milestone = Mapper.Map<ProjectMilestone>(node);
                            _milestone.ModifiedBy = obj.ModifiedBy;
                            _milestone.ModifiedDate = obj.ModifiedDate;
                            await ApplyStatusAsync(_milestone, "ONGOING", reason, obj.ActualStartDate);
                        }

                        else
                        {
                            var _task = Mapper.Map<ProjectTask>(node);
                            _task.ModifiedBy = obj.ModifiedBy;
                            _task.ModifiedDate = obj.ModifiedDate;
                            _task.ActualStartDate = obj.ActualStartDate;

                            await _projecttaskService.StartAsync(_task, reason, false, false, notify);
                        }
                    }
                }


                if (begintransaction.Value)
                    _dataAccess.CommitTransaction();

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneStartedEventArgs(obj.MilestoneSysId, obj.ModifiedBy, obj.ActualStartDate.Value, reason));

            }
            catch
            {
                if (begintransaction.Value)
                    _dataAccess.RollbackTransaction();
                throw;
            }

        }


        public async Task HoldAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "HOLD", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneHoldEventArgs(obj.MilestoneSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task UnholdAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            if (begintransaction.Value)
                _dataAccess.BeginTransaction();

            try
            {

                await ApplyStatusAsync(obj, "ONGOING", reason, DateTime.UtcNow);

                if (begintransaction.Value)
                    _dataAccess.CommitTransaction();

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneResumedEventArgs(obj.MilestoneSysId, Core.Enums.Status.Hold, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                if (begintransaction.Value)
                    _dataAccess.RollbackTransaction();

                throw;
            }
        }

        public async Task CancelAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "CANCEL", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneCanceledEventArgs(obj.MilestoneSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task ArchiveAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
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

        public async Task CompleteAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                //CHECK IF ALL REQUIRED TASKS ARE COMPLETE  

                await ApplyStatusAsync(obj, "COMPLETED", reason, obj.ActualCompletionDate);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneCompletedEventArgs(obj.MilestoneSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }

        public async Task ForceCompleteAsync(ProjectMilestone obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true)
        {
            try
            {
                await ApplyStatusAsync(obj, "COMPLETED", reason, DateTime.UtcNow);

                if (notify.Value)
                    // raise event that Task status was changed
                    await _eventBus.Publish(new ProjectMilestoneCompletedEventArgs(obj.MilestoneSysId, obj.ModifiedBy, DateTime.UtcNow, reason));

            }
            catch
            {
                throw;
            }
        }


    }
}

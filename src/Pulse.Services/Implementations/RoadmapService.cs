using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pulse.SharedUtilities.Helpers;
using AutoMapper;
using Pulse.DataTransformationObjects;
using Pulse.Infrastructure.Tools;
namespace Pulse.Services.Implementations
{
    public class RoadmapService : IRoadmapService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IRoadmapRepository _roadmapRepository;
        private readonly IRoadmapMilestoneRepository _roadmapmilestoneRepository;
        private readonly IRoadmapActivityRepository _roadmapactivityRepository;
        private readonly IRoadmapActivityPrerequisiteRepository _roadmapactivityprerequisiteRepository;
        private readonly IFormEntityLinkRepository _formentitylinkRepository;
        public RoadmapService(OracleDataAccessLayer dataAccess, IRoadmapRepository roadmapRepository, IRoadmapMilestoneRepository roadmapmilestoneRepository, IRoadmapActivityRepository roadmapactivityRepository,
            IRoadmapActivityPrerequisiteRepository roadmapactivityprerequisiteRepository, IFormEntityLinkRepository formentitylinkRepository)
        {
            _dataAccess = dataAccess;
            _roadmapRepository = roadmapRepository;
            _roadmapmilestoneRepository = roadmapmilestoneRepository;
            _roadmapactivityRepository = roadmapactivityRepository;
            _roadmapactivityprerequisiteRepository = roadmapactivityprerequisiteRepository;
            _formentitylinkRepository = formentitylinkRepository;
        }

        public async Task<IEnumerable<Roadmap>> GetAllRoadmapsAsync()
        {
            return await _roadmapRepository.GetListAsync();
        }

        public async Task<Roadmap> GetRoadmapByIdAsync(string roadmapsysid)
        {
            return await _roadmapRepository.GetAsync(roadmapsysid);
        }

        public Roadmap GetRoadmapById(string roadmapsysid)
        {
            return _roadmapRepository.Get(roadmapsysid);
        }

        public async Task<string> BuildRoadmapAsync(Roadmap roadmap, string loggeduser)
        {

            _dataAccess.BeginTransaction();
            try
            {
                roadmap.CreatedBy = loggeduser;

                var roadmapsysid = await this.AddRoadmapAsync(roadmap);

                var keymap = new List<KeyMap>();

                // *** InsertActivities ***
                // Local recursive function for activity and its subactivities
                async System.Threading.Tasks.Task InsertActivities(
                        RoadmapActivity activity)
                {

                    // Insert the activity and get its new SysId
                    var roadmapactivitysysid = await _roadmapactivityRepository.AddAsync(activity);

                    keymap.Add(new KeyMap
                    {
                        TempKey = activity.RoadmapActivitySysId,
                        Type = "activity",
                        NewKey = roadmapactivitysysid
                    });

                    //Sub-Activities
                    var _orderindex = 0;
                    foreach (var subActivity in activity.SubActivities)
                    {
                        subActivity.ParentType = "activity";
                        subActivity.ParentSysId = roadmapactivitysysid;
                        subActivity.RoadmapSysId = activity.RoadmapSysId;
                        subActivity.OrderIndex = _orderindex++;
                        subActivity.CreatedBy = activity.CreatedBy;

                        // Set the parent ID for the child to the new SysId
                        await InsertActivities(subActivity);

                    }

                    //Milestones
                    _orderindex = 0;
                    foreach (var milestone in activity.Milestones)
                    {
                        milestone.ParentType = "activity";
                        milestone.ParentSysId = roadmapactivitysysid;
                        milestone.OrderIndex = _orderindex++;
                        milestone.RoadmapSysId = activity.RoadmapSysId;
                        milestone.CreatedBy = activity.CreatedBy;
                        // Set the parent ID for the child to the new SysId
                        await InsertMilestone(milestone);
                    }



                    //Forms
                    _orderindex = 0;
                    foreach (var form in activity.Forms)
                    {
                        var link = new FormEntityLink
                        {
                            FormSysId = form.FormSysId,
                            EntityType = "activity",
                            EntitySysId = roadmapactivitysysid,
                            OrderIndex = _orderindex++,
                            CreatedBy = activity.CreatedBy
                        };

                        await _formentitylinkRepository.AddAsync(link);
                    }

                    //Prerequisites
                    _orderindex = 0;
                    foreach (var prerequisite in activity.Prerequisites)
                    {

                        var link = new RoadmapActivityPrerequisite
                        {
                            RoadMapActivitySysId = roadmapactivitysysid,
                            PrerequisiteSysId = keymap.Find(x => x.TempKey == prerequisite.PrerequisiteSysId).NewKey
                        };

                        await this.AddActivityPrerequisiteAsync(link);
                    }
                }


                // *** InsertMilestoneWithChildren ***
                // Local recursive function for milestone and its submilestone
                async Task InsertMilestone(RoadmapMilestone milestone)
                {


                    // Insert the milestone and get its new SysId
                    var roadmapmilestonesysid = await _roadmapmilestoneRepository.AddAsync(milestone);

                    keymap.Add(new KeyMap
                    {
                        TempKey = milestone.RoadmapMilestoneSysId,
                        Type = "milestone",
                        NewKey = roadmapmilestonesysid
                    });


                    //Sub-Activities
                    var _orderindex = 0;
                    foreach (var subActivity in milestone.Activities)
                    {
                        subActivity.ParentType = "milestone";
                        subActivity.ParentSysId = roadmapmilestonesysid;
                        subActivity.RoadmapSysId = milestone.RoadmapSysId;
                        subActivity.OrderIndex = _orderindex++;
                        subActivity.CreatedBy = milestone.CreatedBy;

                        // Set the parent ID for the child to the new SysId
                        await InsertActivities(subActivity);

                    }

                    //Milestones
                    _orderindex = 0;
                    foreach (var submilestone in milestone.SubMilestones)
                    {
                        submilestone.ParentType = "milestone";
                        submilestone.ParentSysId = roadmapmilestonesysid;
                        submilestone.OrderIndex = _orderindex++;
                        submilestone.RoadmapSysId = milestone.RoadmapSysId;
                        submilestone.CreatedBy = milestone.CreatedBy;
                        await InsertMilestone(submilestone);
                    }



                    //Forms
                    _orderindex = 0;
                    foreach (var form in milestone.Forms)
                    {
                        var link = new FormEntityLink
                        {
                            FormSysId = form.FormSysId,
                            EntityType = "milestone",
                            EntitySysId = roadmapmilestonesysid,
                            OrderIndex = _orderindex++,
                            CreatedBy = milestone.CreatedBy
                        };

                        await _formentitylinkRepository.AddAsync(link);
                    }


                }



                //Root Activities
                var orderindex = 0;
                foreach (var activity in roadmap.Activities)
                {
                    activity.ParentType = "roadmap";
                    activity.ParentSysId = roadmapsysid;
                    activity.RoadmapSysId = roadmapsysid;
                    activity.OrderIndex = orderindex++;
                    activity.CreatedBy = activity.CreatedBy;

                    // Set the parent ID for the child to the new SysId
                    await InsertActivities(activity);

                }

                //Root Milestones
                orderindex = 0;
                foreach (var milestone in roadmap.Milestones)
                {
                    milestone.ParentType = "roadmap";
                    milestone.ParentSysId = roadmapsysid;
                    milestone.OrderIndex = orderindex++;
                    milestone.RoadmapSysId = roadmapsysid;
                    milestone.CreatedBy = roadmap.CreatedBy;
                    // Set the parent ID for the child to the new SysId
                    await InsertMilestone(milestone);
                }


                //Root Forms
                orderindex = 0;
                foreach (var form in roadmap.Forms)
                {
                    var link = new FormEntityLink
                    {
                        FormSysId = form.FormSysId,
                        EntityType = "roadmap",
                        EntitySysId = roadmapsysid,
                        OrderIndex = orderindex++,
                        CreatedBy = roadmap.CreatedBy
                    };

                    await _formentitylinkRepository.AddAsync(link);
                }


                _dataAccess.CommitTransaction();

                return roadmapsysid;
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }

        }


        private static IEnumerable<RoadmapActivity> GetAllActivities(Roadmap roadmap)
        {
            var result = new List<RoadmapActivity>();

            if (roadmap.Activities != null)
            {
                foreach (var activity in roadmap.Activities)
                {
                    result.AddRange(GetActivityTree(activity));
                }
            }

            if (roadmap.Milestones != null)
            {
                foreach (var milestone in roadmap.Milestones)
                {
                    result.AddRange(GetActivitiesFromMilestone(milestone));
                }
            }

            return result;
        }

        private static IEnumerable<RoadmapActivity> GetActivitiesFromMilestone(RoadmapMilestone milestone)
        {
            var result = new List<RoadmapActivity>();

            if (milestone.Activities != null)
            {
                foreach (var activity in milestone.Activities)
                {
                    result.AddRange(GetActivityTree(activity));
                }
            }

            if (milestone.SubMilestones != null)
            {
                foreach (var subMilestone in milestone.SubMilestones)
                {
                    result.AddRange(GetActivitiesFromMilestone(subMilestone));
                }
            }

            return result;
        }

        private static IEnumerable<RoadmapActivity> GetActivityTree(RoadmapActivity activity)
        {
            var result = new List<RoadmapActivity> { activity };

            if (activity.SubActivities != null)
            {
                foreach (var subActivity in activity.SubActivities)
                {
                    result.AddRange(GetActivityTree(subActivity));
                }
            }

            if (activity.Milestones != null)
            {
                foreach (var milestone in activity.Milestones)
                {
                    result.AddRange(GetActivitiesFromMilestone(milestone));
                }
            }

            return result;
        }

        private static IEnumerable<RoadmapMilestone> GetAllMilestones(Roadmap roadmap)
        {
            var result = new List<RoadmapMilestone>();

            if (roadmap?.Milestones != null)
            {
                foreach (var milestone in roadmap.Milestones)
                {
                    result.AddRange(GetMilestoneTree(milestone));
                }
            }

            if (roadmap?.Activities != null)
            {
                foreach (var activity in roadmap.Activities)
                {
                    result.AddRange(GetMilestonesFromActivity(activity));
                }
            }

            return result;
        }

        private static IEnumerable<RoadmapMilestone> GetMilestonesFromActivity(RoadmapActivity activity)
        {
            var result = new List<RoadmapMilestone>();

            if (activity?.Milestones != null)
            {
                foreach (var milestone in activity.Milestones)
                {
                    result.AddRange(GetMilestoneTree(milestone));
                }
            }

            if (activity?.SubActivities != null)
            {
                foreach (var subActivity in activity.SubActivities)
                {
                    result.AddRange(GetMilestonesFromActivity(subActivity));
                }
            }

            return result;
        }

        private static IEnumerable<RoadmapMilestone> GetMilestoneTree(RoadmapMilestone milestone)
        {
            var result = new List<RoadmapMilestone> { milestone };

            if (milestone?.SubMilestones != null)
            {
                foreach (var subMilestone in milestone.SubMilestones)
                {
                    result.AddRange(GetMilestoneTree(subMilestone));
                }
            }

            if (milestone?.Activities != null)
            {
                foreach (var activity in milestone.Activities)
                {
                    result.AddRange(GetMilestonesFromActivity(activity));
                }
            }

            return result;
        }

        private static void RekeyImportedRoadmapNodes(
            Roadmap roadmap,
            ISet<string> currentMilestoneKeys,
            ISet<string> currentActivityKeys,
            ISet<string> currentFormKeys)
        {
            var activityKeyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var milestone in GetAllMilestones(roadmap))
            {
                if (string.IsNullOrWhiteSpace(milestone.RoadmapMilestoneSysId) || currentMilestoneKeys.Contains(milestone.RoadmapMilestoneSysId))
                {
                    continue;
                }

                milestone.RoadmapMilestoneSysId = Guid.NewGuid().ToString();
            }

            foreach (var activity in GetAllActivities(roadmap))
            {
                if (string.IsNullOrWhiteSpace(activity.RoadmapActivitySysId))
                {
                    activity.RoadmapActivitySysId = Guid.NewGuid().ToString();
                    continue;
                }

                if (currentActivityKeys.Contains(activity.RoadmapActivitySysId))
                {
                    continue;
                }

                var oldKey = activity.RoadmapActivitySysId;
                var newKey = Guid.NewGuid().ToString();
                activity.RoadmapActivitySysId = newKey;
                activityKeyMap[oldKey] = newKey;
            }

            foreach (var form in roadmap.GetAllFormLinks())
            {
                if (string.IsNullOrWhiteSpace(form.FormEntityLinkSysId) || currentFormKeys.Contains(form.FormEntityLinkSysId))
                {
                    continue;
                }

                form.FormEntityLinkSysId = Guid.NewGuid().ToString();
            }

            if (activityKeyMap.Count == 0)
            {
                return;
            }

            foreach (var activity in GetAllActivities(roadmap))
            {
                foreach (var prerequisite in activity.Prerequisites ?? Enumerable.Empty<RoadmapActivityPrerequisite>())
                {
                    if (!string.IsNullOrWhiteSpace(prerequisite.RoadMapActivitySysId) && activityKeyMap.TryGetValue(prerequisite.RoadMapActivitySysId, out var mappedActivityKey))
                    {
                        prerequisite.RoadMapActivitySysId = mappedActivityKey;
                    }

                    if (!string.IsNullOrWhiteSpace(prerequisite.PrerequisiteSysId) && activityKeyMap.TryGetValue(prerequisite.PrerequisiteSysId, out var mappedPrereqKey))
                    {
                        prerequisite.PrerequisiteSysId = mappedPrereqKey;
                    }
                }
            }
        }

        public async Task RebuildRoadmapAsync(Roadmap roadmap, string transactionkey, string loggeduser)
        {
            var commit = true;
            _dataAccess.BeginTransaction();
            //get existing roadmap
            var currentroadmap = _roadmapRepository.Get(roadmap.RoadmapSysId);
            currentroadmap.ModifiedBy = loggeduser;
            currentroadmap.TransactionKey = transactionkey;

            try
            {
                var rowsaffected = await _roadmapRepository.UpdateAsync(currentroadmap);
                if (rowsaffected == 0)
                    throw new Exception($"The roadmap was recently updated on {currentroadmap.ModifiedDate}. Please refresh the page to see the latest updates.");

                var keymap = new List<KeyMap>();

                // get all current nodes, will be used to identify if to add or update
                //var currentnodes = _roadmapRepository.GetNodes(roadmap.RoadmapSysId);
                var currentforms = await _formentitylinkRepository.GetListByRoadmapAsync(roadmap.RoadmapSysId);
                var currentactivities = await _roadmapactivityRepository.GetListAsync(roadmap.RoadmapSysId);
                var currentmilestones = await _roadmapmilestoneRepository.GetListAsync(roadmap.RoadmapSysId);
                var currentactivityprerequisites = await _roadmapactivityprerequisiteRepository.GetListAsync(roadmap.RoadmapSysId);
                // Build lookups for O(1) access
                var currentFormsByKey = currentforms
                    .ToDictionary(f => f.FormEntityLinkSysId, f => f);   // from db

                var currentActivitiesByKey = currentactivities
                    .ToDictionary(a => a.RoadmapActivitySysId, a => a);   // from db

                var currentMilestonesByKey = currentmilestones
                    .ToDictionary(a => a.RoadmapMilestoneSysId, a => a);   // from db

                var currentActivityPrerequisitesByKey = currentactivityprerequisites
                    .ToDictionary(a => a.RoadmapActivityPrereqSysId, a => a);   // from db

                RekeyImportedRoadmapNodes(
                    roadmap,
                    new HashSet<string>(currentmilestones.Select(m => m.RoadmapMilestoneSysId), StringComparer.OrdinalIgnoreCase),
                    new HashSet<string>(currentactivities.Select(a => a.RoadmapActivitySysId), StringComparer.OrdinalIgnoreCase),
                    new HashSet<string>(currentforms.Select(f => f.FormEntityLinkSysId), StringComparer.OrdinalIgnoreCase));

                //var roadmapFormsByKey = roadmap.Forms
                //    .ToDictionary(f => f.FormEntityLinkSysId, f => f);  // from input

                IEnumerable<FormEntityLink> allFormLinks = roadmap.GetAllFormLinks();

                var roadmapFormsByKey = allFormLinks
                    .ToDictionary(f => f.FormEntityLinkSysId, f => f);  // from input

                var roadmapMilestonesByKey = roadmap.Milestones
                    .ToDictionary(m => m.RoadmapMilestoneSysId, m => m);  // from input

                //var roadmapActivitiesByKey = roadmap.Activities
                //    .ToDictionary(a => a.RoadmapActivitySysId, a => a);  // from input
                var roadmapActivitiesByKey = GetAllActivities(roadmap)
                    .GroupBy(a => a.RoadmapActivitySysId)
                    .ToDictionary(g => g.Key, g => g.First());


                var orderIndex = 0;
                #region "Root - Forms"
                // 1) Remove forms that exist in DB but are no longer in roadmap.Forms
                foreach (var existing in currentforms)
                {
                    // if not found in roadmap => delete
                    if (!roadmapFormsByKey.ContainsKey(existing.FormEntityLinkSysId))
                    {
                        try
                        {
                            // set to user who deleted the record
                            existing.ModifiedBy = loggeduser;
                            await _formentitylinkRepository.UpdateAsync(existing);

                            //delete record
                            await _formentitylinkRepository.DeleteAsync(existing.FormEntityLinkSysId);
                        }
                        catch
                        {
                            // set to inactive if already inuse
                            existing.ModifiedBy = loggeduser;
                            existing.IsActive = 0;
                            await _formentitylinkRepository.UpdateAsync(existing);
                        }


                    }
                }

                // 2) Add or update forms from roadmap.Forms
                foreach (var form in roadmap.Forms)
                {
                    // Does this form already exist as a root form in DB?
                    if (!currentFormsByKey.TryGetValue(form.FormEntityLinkSysId, out var existingLink))
                    {
                        // Add new
                        var link = new FormEntityLink
                        {
                            FormSysId = form.FormSysId,
                            EntityType = "roadmap",
                            EntitySysId = roadmap.RoadmapSysId,
                            OrderIndex = orderIndex++,
                            CreatedBy = loggeduser
                        };

                        await _formentitylinkRepository.AddAsync(link);
                    }
                    else
                    {
                        // Update existing - change the orderindex only
                        var forUpdate = Mapper.Map<FormEntityLink>(existingLink);
                        forUpdate.OrderIndex = orderIndex++;
                        forUpdate.ModifiedBy = loggeduser;
                        forUpdate.IsActive = 1;
                        await _formentitylinkRepository.UpdateAsync(forUpdate);
                    }
                }
                #endregion

                #region "Recursive Functions"
                async Task ModifyFormRecursive(FormEntityLink form, int orderindex, string entityType, string entitySysId, string loggedUser)
                {
                    if (!currentFormsByKey.TryGetValue(form.FormEntityLinkSysId, out var existingLink))
                    {
                        form.OrderIndex = orderindex;
                        form.CreatedBy = loggedUser;
                        form.EntityType = entityType;
                        form.EntitySysId = entitySysId;

                        await _formentitylinkRepository.AddAsync(form);
                    }
                    else
                    {
                        form.OrderIndex = orderindex;
                        form.ModifiedBy = loggedUser;
                        await _formentitylinkRepository.UpdateAsync(form);
                    }
                }

                async Task DisableMilestoneRecursive(RoadmapMilestone roadmapmilestone, string loggedUser)
                {
                    roadmapmilestone.IsActive = 0;
                    roadmapmilestone.ModifiedBy = loggedUser;
                    await _roadmapmilestoneRepository.UpdateAsync(roadmapmilestone);

                    //all sub-milestones
                    foreach (var submilestone in currentmilestones.Where(m => m.ParentType == "milestone" && m.ParentSysId == roadmapmilestone.RoadmapMilestoneSysId))
                    {
                        await DisableMilestoneRecursive(submilestone, loggedUser);
                    }
                    //all activities
                    foreach (var activity in currentactivities.Where(m => m.ParentType == "milestone" && m.ParentSysId == roadmapmilestone.RoadmapMilestoneSysId))
                    {
                        await DisableActivityRecursive(activity, loggedUser);
                    }
                }

                async Task DisableActivityRecursive(RoadmapActivity roadmapactivity, string loggedUser)
                {
                    roadmapactivity.IsActive = 0;
                    roadmapactivity.ModifiedBy = loggedUser;
                    await _roadmapactivityRepository.UpdateAsync(roadmapactivity);
                    //all sub-activities
                    foreach (var subactivity in currentactivities.Where(m => m.ParentType == "activity" && m.ParentSysId == roadmapactivity.RoadmapActivitySysId))
                    {
                        await DisableActivityRecursive(subactivity, loggedUser);
                    }

                    //all milestones
                    foreach (var milestone in currentmilestones.Where(m => m.ParentType == "activity" && m.ParentSysId == roadmapactivity.RoadmapActivitySysId))
                    {
                        await DisableMilestoneRecursive(milestone, loggedUser);
                    }


                }

                async Task InsertMilestoneRecursive(RoadmapMilestone milestone, string roadmapsysid, string loggedUser)
                {
                    string type = "milestone";
                    milestone.CreatedBy = loggedUser;
                    milestone.RoadmapSysId = roadmapsysid;
                    // Insert the milestone and get its new SysId
                    var roadmapmilestonesysid = await _roadmapmilestoneRepository.AddAsync(milestone);

                    keymap.Add(new KeyMap
                    {
                        TempKey = milestone.RoadmapMilestoneSysId,
                        Type = type,
                        NewKey = roadmapmilestonesysid
                    });


                    //Sub-Activities
                    var _orderindex = 0;
                    foreach (var activity in milestone.Activities)
                    {
                        activity.ParentType = type;
                        activity.ParentSysId = roadmapmilestonesysid;
                        activity.RoadmapSysId = roadmapsysid;
                        activity.OrderIndex = _orderindex++;
                        // Set the parent ID for the child to the new SysId
                        await InsertActivityRecursive(activity, roadmapsysid, loggedUser);

                    }

                    //Milestones
                    _orderindex = 0;
                    foreach (var submilestone in milestone.SubMilestones)
                    {
                        submilestone.ParentType = type;
                        submilestone.ParentSysId = roadmapmilestonesysid;
                        submilestone.RoadmapSysId = milestone.RoadmapSysId;
                        submilestone.OrderIndex = _orderindex++;
                        await InsertMilestoneRecursive(submilestone, roadmapsysid, loggedUser);
                    }



                    //Forms
                    _orderindex = 0;
                    foreach (var form in milestone.Forms)
                    {
                        await ModifyFormRecursive(form, _orderindex++, type, roadmapmilestonesysid, loggedUser);
                    }


                }

                async Task ModifyMilestoneRecursive(RoadmapMilestone milestone, string roadmapsysid, string loggedUser)
                {
                    var _orderindex = 0;
                    string type = "milestone";
                    // Does this milestone already exist as a root form in DB?
                    if (!currentMilestonesByKey.TryGetValue(milestone.RoadmapMilestoneSysId, out var existingLink))
                    {
                        await InsertMilestoneRecursive(milestone, roadmapsysid, loggedUser);

                        return;
                    }
                    milestone.ModifiedBy = loggedUser;
                    await _roadmapmilestoneRepository.UpdateAsync(milestone);

                    keymap.Add(new KeyMap
                    {
                        TempKey = milestone.RoadmapMilestoneSysId,
                        Type = type,
                        NewKey = milestone.RoadmapMilestoneSysId
                    });

                    //Milestones
                    _orderindex = 0;
                    foreach (var submilestone in milestone.SubMilestones)
                    {
                        submilestone.ParentType = type;
                        submilestone.ParentSysId = milestone.RoadmapMilestoneSysId;
                        submilestone.RoadmapSysId = milestone.RoadmapSysId;
                        submilestone.OrderIndex = _orderindex++;
                        await ModifyMilestoneRecursive(submilestone, roadmapsysid, loggedUser);
                    }

                    //Activities
                    _orderindex = 0;
                    foreach (var activity in milestone.Activities)
                    {
                        activity.ParentType = type;
                        activity.ParentSysId = milestone.RoadmapMilestoneSysId;
                        activity.RoadmapSysId = milestone.RoadmapSysId;
                        activity.OrderIndex = _orderindex++;
                        // Set the parent ID for the child to the new SysId
                        await ModifyActivityRecursive(activity, roadmapsysid, loggedUser);

                    }



                    //Forms
                    _orderindex = 0;
                    foreach (var form in milestone.Forms)
                    {
                        await ModifyFormRecursive(form, _orderindex++, type, milestone.RoadmapMilestoneSysId, loggedUser);
                    }


                }

                async System.Threading.Tasks.Task ModifyPrerequisites(List<RoadmapActivityPrerequisite> prerequisites, string roadmapactivitysysid, string roadmapsysid, string loggedUser)
                {
                    var currentactivityprerequisiteByKey = currentactivityprerequisites.Where(p => p.RoadMapActivitySysId == roadmapactivitysysid)
                        .ToDictionary(
                            p => Tuple.Create(p.RoadMapActivitySysId, p.PrerequisiteSysId),
                            p => p.RoadmapActivityPrereqSysId
                        );

                    var modprerequisites = prerequisites.Select(p =>
                    {
                        var key = Tuple.Create(p.RoadMapActivitySysId, p.PrerequisiteSysId);
                        string existingId;

                        currentactivityprerequisiteByKey.TryGetValue(key, out existingId);

                        return new RoadmapActivityPrerequisite
                        {
                            RoadMapActivitySysId = p.RoadMapActivitySysId,
                            PrerequisiteSysId = p.PrerequisiteSysId,
                            RoadmapActivityPrereqSysId = existingId,
                        };
                    }).Distinct().ToList();

                    var prerequisitesByKey = modprerequisites
                          .ToDictionary(
                              p => Tuple.Create(p.RoadMapActivitySysId, p.PrerequisiteSysId),
                              p => p.RoadmapActivityPrereqSysId
                          );


                    foreach (var existing in currentactivityprerequisites.Where(p => p.RoadMapActivitySysId == roadmapactivitysysid))
                    {
                        var key = Tuple.Create(existing.RoadMapActivitySysId, existing.PrerequisiteSysId);

                        if (!prerequisitesByKey.TryGetValue(key, out var prereqSysId))
                        {
                            await _roadmapactivityprerequisiteRepository.DeleteAsync(prereqSysId);
                        }
                    }

                    foreach (var prerequisite in prerequisites)
                    {
                        var key = Tuple.Create(prerequisite.RoadMapActivitySysId, prerequisite.PrerequisiteSysId);
                        if (prerequisitesByKey.TryGetValue(key, out var prereqSysId))
                        {
                            if (string.IsNullOrEmpty(prereqSysId))
                            {
                                var link = new RoadmapActivityPrerequisite
                                {
                                    RoadMapActivitySysId = roadmapactivitysysid,
                                    PrerequisiteSysId = keymap.Find(x => x.TempKey == prerequisite.PrerequisiteSysId).NewKey
                                };

                                await this.AddActivityPrerequisiteAsync(link);
                            }
                        }
                    }
                }

                async System.Threading.Tasks.Task InsertActivityRecursive(RoadmapActivity activity, string roadmapsysid, string loggedUser)
                {
                    string type = "activity";
                    // Insert the activity and get its new SysId
                    var roadmapactivitysysid = await _roadmapactivityRepository.AddAsync(activity);

                    keymap.Add(new KeyMap
                    {
                        TempKey = activity.RoadmapActivitySysId,
                        Type = type,
                        NewKey = roadmapactivitysysid
                    });

                    //Sub-Activities
                    var _orderindex = 0;
                    foreach (var subActivity in activity.SubActivities)
                    {
                        subActivity.ParentType = type;
                        subActivity.ParentSysId = roadmapactivitysysid;
                        subActivity.RoadmapSysId = activity.RoadmapSysId;
                        subActivity.OrderIndex = _orderindex++;
                        subActivity.CreatedBy = activity.CreatedBy;

                        // Set the parent ID for the child to the new SysId
                        await ModifyActivityRecursive(subActivity, roadmapsysid, loggedUser);

                    }

                    //Milestones
                    _orderindex = 0;
                    foreach (var milestone in activity.Milestones)
                    {
                        milestone.ParentType = type;
                        milestone.ParentSysId = roadmapactivitysysid;
                        milestone.OrderIndex = _orderindex++;
                        milestone.RoadmapSysId = activity.RoadmapSysId;
                        milestone.CreatedBy = activity.CreatedBy;
                        // Set the parent ID for the child to the new SysId
                        await ModifyMilestoneRecursive(milestone, roadmapsysid, loggedUser);
                    }



                    //Forms
                    _orderindex = 0;
                    foreach (var form in activity.Forms)
                    {
                        await ModifyFormRecursive(form, _orderindex++, type, activity.RoadmapActivitySysId, loggedUser);
                    }

                    ////////Prerequisites
                    //////_orderindex = 0;
                    //////try
                    //////{
                    //////    await ModifyPrerequisites(activity.Prerequisites.ToList(), roadmapactivitysysid, roadmapsysid, loggedUser);


                    //////    ////                    var prerequisitesByKey = activity.Prerequisites
                    //////    ////.ToDictionary(
                    //////    ////    p => Tuple.Create(p.RoadMapActivitySysId, p.PrerequisiteSysId),
                    //////    ////    p => p.RoadmapActivityPrereqSysId
                    //////    ////);


                    //////    ////                    var currentactivityprerequisiteByKey = currentactivityprerequisites
                    //////    ////                        .ToDictionary(
                    //////    ////                            p => Tuple.Create(p.RoadMapActivitySysId, p.PrerequisiteSysId),
                    //////    ////                            p => p.RoadmapActivityPrereqSysId
                    //////    ////                        );
                    //////    ////                    foreach (var existing in currentactivityprerequisites)
                    //////    ////                    {
                    //////    ////                        var key = Tuple.Create(existing.RoadMapActivitySysId, existing.PrerequisiteSysId);

                    //////    ////                        if (!prerequisitesByKey.ContainsKey(key) && currentactivityprerequisiteByKey.TryGetValue(key, out var prereqSysId))
                    //////    ////                        {
                    //////    ////                            await _roadmapactivityprerequisiteRepository.DeleteAsync(prereqSysId);
                    //////    ////                        }
                    //////    ////                    }

                    //////    ////                    foreach (var prerequisite in activity.Prerequisites)
                    //////    ////                    {
                    //////    ////                        var key = Tuple.Create(prerequisite.RoadMapActivitySysId, prerequisite.PrerequisiteSysId);
                    //////    ////                        if (!prerequisitesByKey.ContainsKey(key) && currentactivityprerequisiteByKey.TryGetValue(key, out var prereqSysId))
                    //////    ////                        {
                    //////    ////                            var link = new RoadmapActivityPrerequisite
                    //////    ////                            {
                    //////    ////                                RoadMapActivitySysId = roadmapactivitysysid,
                    //////    ////                                PrerequisiteSysId = keymap.Find(x => x.TempKey == prerequisite.PrerequisiteSysId).NewKey
                    //////    ////                            };

                    //////    ////                            await this.AddActivityPrerequisiteAsync(link);
                    //////    ////                        }
                    //////    ////                    }
                    //////}
                    //////catch (Exception err)
                    //////{
                    //////    throw new Exception(err.Message);
                    //////}

                }

                async Task ModifyActivityRecursive(RoadmapActivity activity, string roadmapsysid, string loggedUser)
                {
                    var type = "activity";
                    var _orderindex = 0;
                    // Does this milestone already exist as a root form in DB?
                    if (!currentActivitiesByKey.TryGetValue(activity.RoadmapActivitySysId, out var existingLink))
                    {
                        await InsertActivityRecursive(activity, roadmapsysid, loggedUser);

                        return;
                    }


                    // Insert the activity and get its new SysId
                    await _roadmapactivityRepository.UpdateAsync(activity);

                    keymap.Add(new KeyMap
                    {
                        TempKey = activity.RoadmapActivitySysId,
                        Type = type,
                        NewKey = activity.RoadmapActivitySysId,
                    });

                    //Sub-Activities
                    _orderindex = 0;
                    foreach (var subActivity in activity.SubActivities)
                    {
                        subActivity.ParentType = type;
                        subActivity.ParentSysId = activity.RoadmapActivitySysId;
                        subActivity.RoadmapSysId = activity.RoadmapSysId;
                        subActivity.OrderIndex = _orderindex++;

                        // Set the parent ID for the child to the new SysId
                        await ModifyActivityRecursive(subActivity, roadmapsysid, loggedUser);

                    }

                    //Milestones
                    _orderindex = 0;
                    foreach (var milestone in activity.Milestones)
                    {
                        milestone.ParentType = type;
                        milestone.ParentSysId = activity.RoadmapActivitySysId;
                        milestone.RoadmapSysId = activity.RoadmapSysId;
                        milestone.OrderIndex = _orderindex++;
                        // Set the parent ID for the child to the new SysId
                        await ModifyMilestoneRecursive(milestone, roadmapsysid, loggedUser);
                    }



                    //Forms
                    _orderindex = 0;
                    foreach (var form in activity.Forms)
                    {
                        await ModifyFormRecursive(form, _orderindex++, type, activity.RoadmapActivitySysId, loggedUser);
                    }

                    ////////Prerequisites
                    //////_orderindex = 0;

                    //////try
                    //////{
                    //////    await ModifyPrerequisites(activity.Prerequisites.ToList(), activity.RoadmapActivitySysId, roadmapsysid, loggedUser);


                    //////    ////var prerequisitesByKey = activity.Prerequisites
                    //////    ////.ToDictionary(
                    //////    ////    p => $"{p.RoadMapActivitySysId}_{p.PrerequisiteSysId}",
                    //////    ////    p => p.RoadmapActivityPrereqSysId
                    //////    ////);

                    //////    ////foreach (var existing in currentactivityprerequisites)
                    //////    ////{
                    //////    ////    var key = $"{existing.RoadMapActivitySysId}_{existing.PrerequisiteSysId}";

                    //////    ////    if (!prerequisitesByKey.TryGetValue(key, out var prereqSysId))
                    //////    ////    {
                    //////    ////        await _roadmapactivityprerequisiteRepository.DeleteAsync(prereqSysId);
                    //////    ////    }
                    //////    ////}

                    //////    ////foreach (var prerequisite in activity.Prerequisites)
                    //////    ////{
                    //////    ////    var key = $"{prerequisite.RoadMapActivitySysId}_{prerequisite.PrerequisiteSysId}";
                    //////    ////    if (!prerequisitesByKey.TryGetValue(key, out var prereqSysId))
                    //////    ////    {
                    //////    ////        var link = new RoadmapActivityPrerequisite
                    //////    ////        {
                    //////    ////            RoadMapActivitySysId = activity.RoadmapActivitySysId,
                    //////    ////            PrerequisiteSysId = keymap.Find(x => x.TempKey == prerequisite.PrerequisiteSysId).NewKey
                    //////    ////        };

                    //////    ////        await this.AddActivityPrerequisiteAsync(link);
                    //////    ////    }
                    //////    ////}
                    //////}
                    //////catch (Exception err)
                    //////{
                    //////    throw new Exception(err.Message);
                    //////}



                }

                async Task prerequisiteRecursive(RoadmapActivity activity, string roadmapsysid, string loggedUser)
                {
                    //Prerequisites 
                    try
                    {
                        await ModifyPrerequisites(activity.Prerequisites.ToList(), activity.RoadmapActivitySysId, roadmapsysid, loggedUser);
                    }
                    catch (Exception err)
                    {
                        throw new Exception(err.Message);
                    }
                }
                #endregion


                #region "Milestones"
                orderIndex = 0;

                // 1) Remove milestones that exist in DB but are no longer in roadmap.Milestones
                foreach (var existing in currentmilestones.Where(m => m.ParentType == "roadmap"))
                {
                    // if not found in roadmap => delete
                    if (!roadmapMilestonesByKey.ContainsKey(existing.RoadmapMilestoneSysId))
                    {
                        try
                        {
                            // set to user who deleted the record
                            existing.ModifiedBy = loggeduser;
                            await _roadmapmilestoneRepository.UpdateAsync(existing);

                            //delete record
                            await _roadmapmilestoneRepository.DeleteAsync(existing.RoadmapMilestoneSysId, roadmap.RoadmapSysId);
                        }
                        catch
                        {
                            // set to inactive if already inuse
                            await DisableMilestoneRecursive(existing, loggeduser);
                        }
                    }
                }

                // 2) Add or update milestones from roadmap.Milestones
                foreach (var milestone in roadmap.Milestones)
                {
                    await ModifyMilestoneRecursive(milestone, roadmap.RoadmapSysId, loggeduser);
                }
                #endregion

                #region "Activities"
                orderIndex = 0;
                // 1) Remove activities that exist in DB but are no longer in roadmap.Activities
                foreach (var existing in currentactivities)
                {
                    // if not found in roadmap => delete
                    if (!roadmapActivitiesByKey.ContainsKey(existing.RoadmapActivitySysId))
                    {
                        try
                        {
                            // set to user who deleted the record
                            existing.ModifiedBy = loggeduser;
                            await _roadmapactivityRepository.UpdateAsync(existing);

                            //delete record
                            await _roadmapactivityRepository.DeleteAsync(existing.RoadmapActivitySysId, roadmap.RoadmapSysId);
                        }
                        catch
                        {
                            // set to inactive if already inuse
                            await DisableActivityRecursive(existing, loggeduser);
                        }
                    }
                }

          

                // 2) Add or update activities from roadmap.Activities
                foreach (var activity in roadmap.Activities)
                {
                    await ModifyActivityRecursive(activity, roadmap.RoadmapSysId, loggeduser);
                }
                #endregion

                #region "Prerequisites"
                //root activities
                foreach (var activity in roadmap.Activities)
                {
                    await prerequisiteRecursive(activity, roadmap.RoadmapSysId, loggeduser);
                }
                //milestone activities


                foreach (var milestone in roadmap.Milestones)
                {
                    foreach (var activity in milestone.Activities)
                    {
                        await prerequisiteRecursive(activity, roadmap.RoadmapSysId, loggeduser);
                    }
                }
                #endregion


                if (commit)
                    _dataAccess.CommitTransaction();
                else
                    _dataAccess.RollbackTransaction();

            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }







        }


        ////public async System.Threading.Tasks.Task RebuildRoadmapAsync(Roadmap incomingRoadmap, string transactionkey, string loggeduser)
        ////{
        ////    //Load existing roadmap and all children from DB
        ////    var existingRoadmap = JsonHelper.ParseRoadmapJson<Roadmap>(this.GetCompleteInfoRoadmapByIdAsync(incomingRoadmap.RoadmapSysId).GetAwaiter().GetResult().RoadmapJson);


        ////    _dataAccess.BeginTransaction();
        ////    try
        ////    {
        ////        //Update roadmap properties
        ////        //var affectedrows = await this.UpdateRoadmapAsync(existingRoadmap);
        ////        var index = 0;
        ////        //Process incoming fields
        ////        foreach (var incomingField in incomingRoadmap.Fields)
        ////        {

        ////            var existingField = existingRoadmap.Fields.FirstOrDefault(f => f.RoadmapFieldSysId == incomingField.RoadmapFieldSysId);

        ////            if (existingField != null)
        ////            {
        ////                // Update properties
        ////                existingField.FieldName = incomingField.FieldName;
        ////                existingField.FieldTitle = incomingField.FieldTitle;
        ////                existingField.Placeholder = incomingField.Placeholder;
        ////                existingField.Tooltip = incomingField.Tooltip;
        ////                existingField.IsRequired = incomingField.IsRequired;
        ////                existingField.MinLength = incomingField.MinLength;
        ////                existingField.MaxLength = incomingField.MaxLength;
        ////                existingField.CaseOption = incomingField.CaseOption;
        ////                existingField.FileType = incomingField.FileType;
        ////                existingField.FileMaxSize = incomingField.FileMaxSize;
        ////                existingField.ReadAccess = incomingField.ReadAccess;
        ////                existingField.WriteAccess = incomingField.WriteAccess;
        ////                existingField.FieldValidate = incomingField.FieldValidate;
        ////                existingField.DataSource = incomingField.DataSource;
        ////                existingField.DataSourceParamField = incomingField.DataSourceParamField;
        ////                existingField.ParentFieldSysId = incomingField.ParentFieldSysId;
        ////                existingField.RoadmapSysId = incomingField.RoadmapSysId;
        ////                existingField.OrderIndex = index;
        ////                await _roadmapmilestoneRepository.UpdateAsync(existingField);

        ////                // Update options
        ////                var optionIndex = 0;
        ////                foreach (var incomingOption in incomingField.Options)
        ////                {

        ////                    var existingOption = existingField.Options.FirstOrDefault(o => o.OptionValue == incomingOption.OptionValue);
        ////                    if (existingOption != null)
        ////                    {
        ////                        // Update
        ////                        existingOption.RoadmapFieldSysId = existingField.RoadmapFieldSysId;
        ////                        existingOption.OptionValue = incomingOption.OptionValue;
        ////                        existingOption.OptionLabel = incomingOption.OptionLabel;
        ////                        existingOption.OrderIndex = optionIndex;
        ////                        existingOption.ModifiedBy = loggeduser;

        ////                        await _roadmapactivityRepository.UpdateAsync(existingOption);
        ////                    }
        ////                    else
        ////                    {
        ////                        incomingOption.RoadmapFieldSysId = existingField.RoadmapFieldSysId;
        ////                        incomingOption.OrderIndex = optionIndex;
        ////                        incomingOption.CreatedBy = loggeduser;
        ////                        // Insert new option
        ////                        await _roadmapactivityRepository.AddAsync(incomingOption);
        ////                    }

        ////                    optionIndex++;
        ////                }

        ////                // Delete removed options
        ////                var optionsToDelete = existingField.Options
        ////                       .Where(o => !incomingField.Options.Select(i => i.OptionValue).Contains(o.OptionValue))
        ////                       .ToList();

        ////                foreach (var opt in optionsToDelete)
        ////                    await _roadmapactivityRepository.DeleteAsync(opt.RoadmapFieldOptionSysId);




        ////                // Update rules
        ////                foreach (var incomingRule in incomingField.Rules)
        ////                {

        ////                    var existingRule = existingField.Rules.FirstOrDefault(o => o.RoadmapFieldRuleSysId == incomingRule.RoadmapFieldRuleSysId ||
        ////                    (o.RuleField == incomingRule.RuleField
        ////                    && o.RuleOperator == incomingRule.RuleOperator
        ////                    && o.RuleValue == incomingRule.RuleValue
        ////                    && o.RuleAction == incomingRule.RuleAction));
        ////                    if (existingRule != null)
        ////                    {
        ////                        // Update
        ////                        existingRule.RuleField = incomingRule.RuleField;
        ////                        existingRule.RuleOperator = incomingRule.RuleOperator;
        ////                        existingRule.RuleValue = incomingRule.RuleValue;
        ////                        existingRule.RuleAction = incomingRule.RuleAction;
        ////                        existingRule.RuleActionValue = incomingRule.RuleActionValue;
        ////                        existingRule.ModifiedBy = loggeduser;
        ////                        await _roadmapactivityprerequisiteRepository.UpdateAsync(existingRule);
        ////                    }
        ////                    else
        ////                    {
        ////                        incomingRule.RoadmapFieldSysId = existingField.RoadmapFieldSysId;
        ////                        incomingRule.CreatedBy = loggeduser;
        ////                        // Insert new option
        ////                        await _roadmapactivityprerequisiteRepository.AddAsync(incomingRule);
        ////                    }
        ////                }
        ////                // Delete removed options
        ////                var rulesToDelete = existingField.Rules
        ////                    .Where(o => !incomingField.Rules.Any(io => (io.RoadmapFieldRuleSysId == o.RoadmapFieldRuleSysId)
        ////                    || (io.RuleField == o.RuleField
        ////                    && io.RuleOperator == o.RuleOperator
        ////                    && io.RuleValue == o.RuleValue
        ////                    && io.RuleAction == o.RuleAction
        ////                    && io.RuleActionValue == o.RuleActionValue)))
        ////                    .ToList();

        ////                foreach (var opt in rulesToDelete)
        ////                    await _roadmapactivityprerequisiteRepository.DeleteAsync(opt.RoadmapFieldRuleSysId);


        ////            }
        ////            else
        ////            {
        ////                // Insert new field
        ////                //existingRoadmap.Fields.Add(incomingField);

        ////                incomingField.RoadmapSysId = incomingRoadmap.RoadmapSysId;
        ////                incomingField.OrderIndex = index;
        ////                incomingField.CreatedBy = loggeduser;

        ////                var fieldsysid = await _roadmapmilestoneRepository.AddAsync(incomingField);
        ////                var optionIndex = 0;
        ////                foreach (var option in incomingField.Options)
        ////                {
        ////                    option.RoadmapFieldSysId = fieldsysid;
        ////                    option.OrderIndex = optionIndex;
        ////                    option.CreatedBy = loggeduser;
        ////                    await _roadmapactivityRepository.AddAsync(option);
        ////                }

        ////                foreach (var rule in incomingField.Rules)
        ////                {
        ////                    rule.RoadmapFieldSysId = fieldsysid;
        ////                    rule.CreatedBy = loggeduser;
        ////                    await _roadmapactivityprerequisiteRepository.AddAsync(rule);
        ////                }
        ////            }

        ////            index++;
        ////        }


        ////        _dataAccess.CommitTransaction();

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        _dataAccess.RollbackTransaction();
        ////        throw new Exception(ex.Message);
        ////    }


        ////}

        private async Task<string> AddRoadmapAsync(Roadmap roadmap)
        {
            return await _roadmapRepository.AddAsync(roadmap);

        }

        private async Task<int> UpdateRoadmapAsync(Roadmap roadmap)
        {

            var rowsaffected = await _roadmapRepository.UpdateAsync(roadmap);

            // Publish the UserUpdatedEvent
            //var roadmapUpdatedEvent = new RoadmapUpdatedEvent
            //{
            //    RoadmapCode = roadmap.RoadmapCode,
            //    ActionBy = roadmap.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(roadmapUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public async Task<int> DeleteRoadmapAsync(string roadmapsysid, string userid)
        {

            try
            {
                //GET FORM INFO
                var obj = await _roadmapRepository.GetAsync(roadmapsysid);


                //SET USER WHO DELETES THE Roadmap
                obj.ModifiedBy = userid;
                await _roadmapRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _roadmapRepository.DeleteAsync(roadmapsysid);


                // Publish the UserUpdatedEvent
                //var roadmapDeletedEvent = new RoadmapDeletedEvent
                //{
                //    RoadmapCode = roadmap.RoadmapCode,
                //    ActionBy = roadmap.CreatedBy
                //};

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(roadmapDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<RoadmapExtended> GetCompleteInfoRoadmapByIdAsync(string roadmapsysid)
        {
            return await _roadmapRepository.GetCompleteInfoAsync(roadmapsysid);
        }




        public async Task<PagedResult<RoadmapExtended>> GetPagedRoadmapsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _roadmapRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }

        public async Task UpdateAsync(Roadmap incomingRoadmap, string transactionkey, string loggeduser)
        {
            //Load existing roadmap and all children from DB
            var existingRoadmap = await this.GetRoadmapByIdAsync(incomingRoadmap.RoadmapSysId);
            existingRoadmap.RoadmapName = incomingRoadmap.RoadmapName;
            existingRoadmap.RoadmapDescription = incomingRoadmap.RoadmapDescription;
            existingRoadmap.CategoryCode = incomingRoadmap.CategoryCode;
            existingRoadmap.ModifiedBy = loggeduser;
            existingRoadmap.TransactionKey = transactionkey;
            //Update roadmap properties
            var affectedrows = await this.UpdateRoadmapAsync(existingRoadmap);

        }

        public async Task ChangeStatusAsync(Roadmap incomingRoadmap, string transactionkey, string loggeduser)
        {
            //Load existing roadmap and all children from DB
            var existingRoadmap = await this.GetRoadmapByIdAsync(incomingRoadmap.RoadmapSysId);
            existingRoadmap.IsActive = incomingRoadmap.IsActive;
            existingRoadmap.ModifiedBy = loggeduser;
            existingRoadmap.TransactionKey = transactionkey;
            //Update roadmap properties
            var affectedrows = await this.UpdateRoadmapAsync(existingRoadmap);
        }



        public async Task<string> AddActivityPrerequisiteAsync(RoadmapActivityPrerequisite prerequisite)
        {
            return await _roadmapactivityprerequisiteRepository.AddAsync(prerequisite);

        }




        public async Task<dtoTreeResponse> GetTreeResponseAsync(string roadmapsysid)
        {
            var tree = new dtoTreeResponse();

            // Root forms
            var rootForms = await _formentitylinkRepository.GetRootNodeFormsAsync(roadmapsysid);
            tree.RootForms = rootForms
                .Select(Mapper.Map<dtoNodeForm>)
                .ToList();

            // Load all nodes once
            var nodes = (await _roadmapRepository.GetNodes(roadmapsysid)).ToList();

            // Build top-level nodes (children of roadmap)
            var rootNodeRows = nodes
                .Where(n => n.ParentType == "roadmap")
                .OrderByDescending(n => n.NodeType)
                .ThenBy(n => n.OrderIndex);

            foreach (var rootRow in rootNodeRows)
            {
                var rootNode = await BuildNodeRecursiveAsync(rootRow, nodes);
                tree.TreeData.Add(rootNode);
            }

            return tree;
        }

        /// <summary>
        /// Recursively builds a dtoNode tree from the given NodeRow.
        /// </summary>
        private async Task<dtoNode> BuildNodeRecursiveAsync(NodeRow nodeRow, List<NodeRow> allNodes)
        {
            // Map basic node
            var node = Mapper.Map<dtoNode>(nodeRow);

            // Ensure collections are not null 
            if (node.Children == null)
                node.Children = new List<dtoNode>();
            if (node.Forms == null)
                node.Forms = new List<dtoNodeForm>();
            if (node.Prerequisites == null)
                node.Prerequisites = new List<string>();

            // Load forms
            var forms = await _formentitylinkRepository.GetNodeFormsAsync(node.Id, node.Type);
            node.Forms = forms
                .Select(Mapper.Map<dtoNodeForm>)
                .ToList();

            // Load children (filter from the preloaded 'allNodes')
            var childRows = allNodes
                .Where(n => n.ParentSysId == nodeRow.NodeId &&
                            n.ParentType == nodeRow.NodeType)
                .OrderBy(n => n.NodeType)
                .ThenBy(n => n.OrderIndex);

            foreach (var childRow in childRows)
            {
                var childNode = await BuildNodeRecursiveAsync(childRow, allNodes);
                node.Children.Add(childNode);
            }

            // Load prerequisites for activities
            if (nodeRow.NodeType == "activity")
            {
                var prerequisites = await _roadmapactivityRepository
                    .GetPrerequisites(nodeRow.NodeId);

                foreach (var prerequisite in prerequisites)
                {
                    node.Prerequisites.Add(prerequisite.PrereqKey);
                }
            }

            return node;
        }
    }
}

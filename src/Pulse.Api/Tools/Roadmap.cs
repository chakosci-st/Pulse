using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pulse.Api.Tools
{
    public static class Roadmap
    {
        public static void Map(dtoStructRoadmapRoot src, string roadmapSysId, string user, ref dtoRoadmapExtended roadMap)
        {
            var now = DateTime.UtcNow;
            roadMap.RoadmapSysId = roadMap.RoadmapSysId ?? Guid.NewGuid().ToString(); //TEMP SYSID ONLY
            roadMap.CreatedBy = user;
            roadMap.CreatedDate = now;
            roadMap.ModifiedBy = user;
            roadMap.ModifiedDate = now;

            // 1) Map root-level forms (rootForms -> dtoRoadmap.Forms)
            int order = 0;
            foreach (var f in src.RootForms ?? Enumerable.Empty<dtoStructRoadmapForm>())
            {
                roadMap.Forms.Add(MapFormLink(
                    f,
                    entityType: "roadmap",
                    entitySysId: roadmapSysId,
                    orderIndex: order++,
                    user: user,
                    now: now
                ));
            }

            // 2) First pass: build all milestones/activities, keep lookup by node Id
            var nodeToActivity = new Dictionary<string, dtoRoadmapActivity>();
            var nodeToMilestone = new Dictionary<string, dtoRoadmapMilestone>();

            int milestoneOrder = 0;
            foreach (var node in src.TreeData ?? Enumerable.Empty<dtoStructRoadmapTreeNode>())
            {
                MapNodeRecursive(
                    node,
                    roadMap,
                    parentMilestone: null,
                    parentActivity: null,
                    milestoneOrderIndex: ref milestoneOrder,
                    activityOrderIndex: ref order,
                    user: user,
                    now: now,
                    nodeToActivity: nodeToActivity,
                    nodeToMilestone: nodeToMilestone 
                );
            }

            // 3) Second pass: build prerequisites from node.Prerequisites (they refer to node.Key or Id)
            BuildActivityPrerequisites(src.TreeData, nodeToActivity);


        }

        private static void MapNodeRecursive(
    dtoStructRoadmapTreeNode node,
    dtoRoadmap roadmap,
    dtoRoadmapMilestone parentMilestone,
    dtoRoadmapActivity parentActivity,
    ref int milestoneOrderIndex,
    ref int activityOrderIndex,
    string user,
    DateTime now,
    Dictionary<string, dtoRoadmapActivity> nodeToActivity,
    Dictionary<string, dtoRoadmapMilestone> nodeToMilestone)
        {
            if (node == null) return;

            if (node.Type == "milestone")
            {
                // Create milestone
                var ms = new dtoRoadmapMilestone
                {
                    RoadmapMilestoneSysId = node.Key ?? Guid.NewGuid().ToString(),
                    RoadmapSysId = roadmap.RoadmapSysId,
                    MaturityCode = node.Data?.Maturity,
                    ParentType = parentMilestone != null ? "milestone" : parentActivity != null ? "activity" : "roadmap",
                    ParentSysId = parentMilestone?.RoadmapMilestoneSysId ?? parentActivity?.RoadmapActivitySysId ?? roadmap.RoadmapSysId,
                    MilestoneAlias = node.Data?.Name,
                    MilestoneDescription = node.Data?.Desc,
                    IsRequired = node.Data.IsRequired,
                    OrderIndex = milestoneOrderIndex++,
                    CreatedBy = user,
                    CreatedDate = now,
                    ModifiedBy = user,
                    ModifiedDate = now
                };

                // Map forms under this milestone
                int formOrder = 0;
                foreach (var f in node.Forms ?? Enumerable.Empty<dtoStructRoadmapForm>())
                {
                    var _form = MapFormLink(
                        f,
                        entityType: "milestone",
                        entitySysId: ms.RoadmapMilestoneSysId,
                        orderIndex: formOrder++,
                        user: user,
                        now: now
                    );

                    ms.Forms.Add(_form);
                }



                // Attach to parent
                if (parentMilestone != null)
                    parentMilestone.SubMilestones.Add(ms);
                else if (parentActivity != null)
                    parentActivity.Milestones.Add(ms);
                else
                    roadmap.Milestones.Add(ms);



                // Remember in lookup (use node.Id as key; if you want node.Key instead, standardize it)
                nodeToMilestone[node.Id] = ms;

                // Recurse on children; for milestones, children can be other milestones or activities
                foreach (var child in node.Children ?? Enumerable.Empty<dtoStructRoadmapTreeNode>())
                {
                    MapNodeRecursive(
                        child,
                        roadmap,
                        parentMilestone: ms,
                        parentActivity: null,
                        milestoneOrderIndex: ref milestoneOrderIndex,
                        activityOrderIndex: ref activityOrderIndex,
                        user: user,
                        now: now,
                        nodeToActivity: nodeToActivity,
                        nodeToMilestone: nodeToMilestone
                    );
                }
            }
            else if (node.Type == "activity")
            {
                // Determine which milestone this activity belongs to
                var activity = new dtoRoadmapActivity
                {
                    RoadmapActivitySysId = node.Key ?? Guid.NewGuid().ToString(),
                    RoadmapSysId = roadmap.RoadmapSysId,
                    ParentType = parentMilestone != null ? "milestone" : parentActivity != null ? "activity" : "roadmap",
                    ParentSysId = parentMilestone?.RoadmapMilestoneSysId ?? parentActivity?.RoadmapActivitySysId ?? roadmap.RoadmapSysId,
                    ActivityName = node.Data?.Name,
                    ActivityDescription = node.Data?.Desc,
                    EstimatedManDays = ParseInt(node.Data?.Mandays),
                    OrderIndex = activityOrderIndex++,
                    IsRequired = (node.Data?.IsRequired ?? false),
                    IsActive = true,
                    CreatedBy = user,
                    CreatedDate = now,
                    ModifiedBy = user,
                    ModifiedDate = now
                };

                // Map forms under this milestone
                int formOrder = 0;
                foreach (var f in node.Forms ?? Enumerable.Empty<dtoStructRoadmapForm>())
                {
                    var _form = MapFormLink(
                        f,
                        entityType: "activity",
                        entitySysId: activity.RoadmapActivitySysId,
                        orderIndex: formOrder++,
                        user: user,
                        now: now
                    );

                    activity.Forms.Add(_form);
                }

                foreach (var prerequisite in node.Prerequisites ?? Enumerable.Empty<string>()) {
                    activity.Prerequisites.Add(new dtoRoadmapActivityPrerequisite
                    {
                        RoadMapActivitySysId = node.Key,
                        PrerequisiteSysId = prerequisite
                    });
                }



                // Attach to parent activity or roadmap.Activities
                if (parentMilestone != null)
                    parentMilestone.Activities.Add(activity);
                else if (parentActivity != null)
                    parentActivity.SubActivities.Add(activity);
                else
                    roadmap.Activities.Add(activity);



                nodeToActivity[node.Id] = activity;

                // Recurse on children: could be nested activities or milestones
                foreach (var child in node.Children ?? Enumerable.Empty<dtoStructRoadmapTreeNode>())
                {
                    MapNodeRecursive(
                        child,
                        roadmap,
                        parentMilestone: null,
                        parentActivity: activity,
                        milestoneOrderIndex: ref milestoneOrderIndex,
                        activityOrderIndex: ref activityOrderIndex,
                        user: user,
                        now: now,
                        nodeToActivity: nodeToActivity,
                        nodeToMilestone: nodeToMilestone
                    );
                }
            }
            else
            {
                // Unknown node type - ignore or throw
            }
        }

        private static dtoFormEntityLink MapFormLink(
    dtoStructRoadmapForm src,
    string entityType,
    string entitySysId,
    int orderIndex,
    string user,
    DateTime now)
        {
            return new dtoFormEntityLink
            {
                FormEntityLinkSysId = src.Key ?? Guid.NewGuid().ToString(),
                FormSysId = src.Sysid,
                EntityType = entityType,
                EntitySysId = entitySysId,
                OrderIndex = orderIndex,
                CreatedBy = user,
                CreatedDate = now,
                ModifiedBy = user,
                ModifiedDate = now
            };
        }

        private static void BuildActivityPrerequisites(
    IEnumerable<dtoStructRoadmapTreeNode> roots,
    Dictionary<string, dtoRoadmapActivity> nodeToActivity)
        {
            // Flatten tree for convenience
            foreach (var node in EnumerateNodes(roots))
            {
                if (node.Type != "activity") continue;

                if (!nodeToActivity.TryGetValue(node.Id, out var activity))
                    continue; // or throw

                foreach (var prereqNodeId in node.Prerequisites ?? Enumerable.Empty<string>())
                {
                    // The prereqNodeId refers to another activity node.Id or node.Key
                    if (!nodeToActivity.TryGetValue(prereqNodeId, out var prereqActivity))
                    {
                        // if your prerequisites reference Key instead of Id,
                        // you should build the lookup using Key instead of Id.
                        continue;
                    }

                    if (activity.Prerequisites.Where(p => p.PrerequisiteSysId == prereqActivity.RoadmapActivitySysId && p.RoadMapActivitySysId == activity.RoadmapActivitySysId).Count() == 0) {
                        activity.Prerequisites.Add(new dtoRoadmapActivityPrerequisite
                        {
                            //RoadmapActivityPrereqSysId = Guid.NewGuid().ToString(),
                            RoadMapActivitySysId = activity.RoadmapActivitySysId,
                            PrerequisiteSysId = prereqActivity.RoadmapActivitySysId
                        });
                    }


       
                }
            }
        }

        private static IEnumerable<dtoStructRoadmapTreeNode> EnumerateNodes(IEnumerable<dtoStructRoadmapTreeNode> roots)
        {
            if (roots == null) yield break;

            var stack = new Stack<dtoStructRoadmapTreeNode>(roots);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                if (node.Children != null)
                    foreach (var c in node.Children)
                        stack.Push(c);
            }
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }





    }
}
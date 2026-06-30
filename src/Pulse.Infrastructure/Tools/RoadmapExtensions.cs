using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Tools
{
    public static class RoadmapExtensions
    {
        public static IEnumerable<FormEntityLink> GetAllFormLinks(this Roadmap roadmap)
        {
            if (roadmap == null) yield break;

            // Roadmap-level forms
            foreach (var form in roadmap.Forms ?? Enumerable.Empty<FormEntityLink>())
                yield return form;

            var visitedMilestones = new HashSet<string>();
            var visitedActivities = new HashSet<string>();

            // Traverse milestones tree
            foreach (var milestone in roadmap.Milestones ?? Enumerable.Empty<RoadmapMilestone>())
            {
                foreach (var form in GetFormsFromMilestone(milestone, visitedMilestones, visitedActivities))
                    yield return form;
            }

            // Traverse top-level activities (in case some are not bound to milestones)
            foreach (var activity in roadmap.Activities ?? Enumerable.Empty<RoadmapActivity>())
            {
                foreach (var form in GetFormsFromActivity(activity, visitedActivities, visitedMilestones))
                    yield return form;
            }
        }

        private static IEnumerable<FormEntityLink> GetFormsFromMilestone(
            RoadmapMilestone milestone,
            HashSet<string> visitedMilestones,
            HashSet<string> visitedActivities)
        {
            if (milestone == null) yield break;

            if (!string.IsNullOrEmpty(milestone.RoadmapMilestoneSysId))
            {
                if (!visitedMilestones.Add(milestone.RoadmapMilestoneSysId))
                    yield break; // already visited (avoid cycles)
            }

            // Forms directly on this milestone
            foreach (var form in milestone.Forms ?? Enumerable.Empty<FormEntityLink>())
                yield return form;

            // Activities linked to this milestone
            foreach (var activity in milestone.Activities ?? Enumerable.Empty<RoadmapActivity>())
            {
                foreach (var form in GetFormsFromActivity(activity, visitedActivities, visitedMilestones))
                    yield return form;
            }

            // Sub-milestones
            foreach (var sub in milestone.SubMilestones ?? Enumerable.Empty<RoadmapMilestone>())
            {
                foreach (var form in GetFormsFromMilestone(sub, visitedMilestones, visitedActivities))
                    yield return form;
            }
        }

        private static IEnumerable<FormEntityLink> GetFormsFromActivity(
            RoadmapActivity activity,
            HashSet<string> visitedActivities,
            HashSet<string> visitedMilestones)
        {
            if (activity == null) yield break;

            if (!string.IsNullOrEmpty(activity.RoadmapActivitySysId))
            {
                if (!visitedActivities.Add(activity.RoadmapActivitySysId))
                    yield break; // already visited
            }

            // Forms directly on this activity
            foreach (var form in activity.Forms ?? Enumerable.Empty<FormEntityLink>())
                yield return form;

            // Sub-activities
            foreach (var sub in activity.SubActivities ?? Enumerable.Empty<RoadmapActivity>())
            {
                foreach (var form in GetFormsFromActivity(sub, visitedActivities, visitedMilestones))
                    yield return form;
            }

            // Milestones linked to this activity (if you want to follow this relationship too)
            foreach (var milestone in activity.Milestones ?? Enumerable.Empty<RoadmapMilestone>())
            {
                foreach (var form in GetFormsFromMilestone(milestone, visitedMilestones, visitedActivities))
                    yield return form;
            }
        }
    }
}

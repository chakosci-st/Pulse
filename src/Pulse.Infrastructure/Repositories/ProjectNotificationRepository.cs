using log4net; 
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Repositories
{
    public class ProjectNotificationRepository : BaseRepository<ProjectNotification, string>, IProjectNotificationRepository
    {
        private string sqlCommon = @"
with PRJMILESTONES as (select * from PROJECTMILESTONES where projectno = :projectno),
 PRJTASKS as (select * from PROJECTTASKS where projectno = :projectno),
METADATA AS (select milestonesysid entitysysid, 'milestone' entitytype, milestonealias context from PRJMILESTONES pm inner join PROJECTROADMAPMILESTONES prm on prm.projectno = pm.projectno and prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
union all
select projecttasksysid entitysysid, 'activity' entitytype, NVL(pt.alttaskname, ra.activityname) context from PRJTASKS pt left outer join ROADMAPACTIVITIES ra on ra.roadmapactivitysysid = pt.roadmapactivitysysid
)
SELECT pn.*, 
        JSON_OBJECT(
            'meta' VALUE (
                JSON_OBJECT(
                        'title' VALUE pn.title, 
                        'message' VALUE pn.message,  
                        'recipients' VALUE pn.recipients,  
                        'notificationDate' VALUE pn.notificationdate,  
                        'context' VALUE md.context,
                        'createdBy' VALUE u.firstname,
                        'createdDate' VALUE pn.createdDate
                        )
                    ) 
            )
         AS MetaJson 
FROM PROJECTNOTIFICATIONS pn 
LEFT OUTER JOIN METADATA md ON md.entitysysid = pn.entitysysid AND md.entitytype = pn.entitytype 
INNER JOIN USERS u ON u.userid = pn.createdby
";


        public ProjectNotificationRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectNotification Notification)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectNotification>(@"  
INSERT INTO projectnotifications (projectno,
                                  title,
                                  MESSAGE,
                                  recipients,
                                  notificationdate,
                                  entitytype,
                                  entitysysid,
                                  createdby)
     VALUES (:projectno,
             :title,
             :MESSAGE,
             :recipients,
             :notificationdate,
             :entitytype,
             :entitysysid,
             :createdby)
  RETURNING notificationsysid
       INTO :notificationsysid
", Notification, "notificationsysid");
        }



        public async Task<IEnumerable<ProjectNotification>> GetByEntityAsync(string projectno, string entitytype, string entitysysid)
        {
            return await _dataAccess.LoadDataAsync<ProjectNotification>($"{sqlCommon} WHERE PROJECTNO = :PROJECTNO AND ENTITYTYPE = :ENTITYTYPE AND ENTITYSYSID = :ENTITYSYSID",
         new ProjectNotification { ProjectNo = projectno, EntityType = entitytype, EntitySysId = entitysysid });
        }
 

        public async Task<IEnumerable<ProjectNotification>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<ProjectNotification>($"{sqlCommon} WHERE PROJECTNO = :PROJECTNO",
                   new ProjectNotification { ProjectNo = projectno });
        }

        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<ProjectNotification> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ProjectNotification>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<int> UpdateAsync(ProjectNotification entity)
        {
            throw new NotImplementedException();
        }
    }
}

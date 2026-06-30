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
    public class NotificationRepository : BaseRepository<Notification, string>, INotificationRepository
    {

        private string sqlCommon = @"
WITH
allowedentity as (
SELECT 'SYSTEM' entitysysid, 'system' entitytype FROM DUAL
UNION
SELECT plantcode entitysysid, 'plant' entitytype FROM PLANTMEMBERS WHERE :createdby IS NULL OR userid = :createdby
UNION
SELECT projectno entitysysid, 'project' entitytype FROM PROJECTMEMBERS WHERE :createdby IS NULL OR userid = :createdby
UNION
SELECT parentsysid entitysysid, parenttype entitytype FROM PROJECTOWNERS WHERE :createdby IS NULL OR userid = :createdby
),
isviewed as (
SELECT NOTIFICATIONSYSID, VIEWEDDATE FROM NOTIFICATIONVIEWED WHERE USERID = :createdby
),
metadata AS (SELECT 'SYSTEM' entitysysid, 'system' entitytype, 'PULSE' context FROM DUAL
                  UNION ALL
                  SELECT plantcode entitysysid, 'plant' entitytype, plantname context FROM plants
                  UNION ALL
                  SELECT projectno entitysysid, 'project' entitytype, projectname context FROM projects
                  UNION ALL
                  SELECT milestonesysid entitysysid, 'milestone' entitytype, milestonealias context
                                        FROM projectmilestones pm
                                        INNER JOIN projectroadmapmilestones prm
                                             ON prm.projectno = pm.projectno
                                            AND prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
                  UNION ALL
                                    SELECT projecttasksysid entitysysid, 'task' entitytype, NVL(pt.alttaskname, ra.activityname) context
                                        FROM projecttasks pt LEFT OUTER JOIN roadmapactivities ra ON ra.roadmapactivitysysid = pt.roadmapactivitysysid)
SELECT n.*, NVL2(ae.entitysysid,1,0) isnotify, NVL2(vw.NOTIFICATIONSYSID,1,0) isviewed,
        JSON_OBJECT(
            'meta' VALUE (
                JSON_OBJECT(
                        'title' VALUE n.title, 
                        'message' VALUE n.message,  
                        'recipients' VALUE n.recipients,  
                        'notificationDate' VALUE n.notificationdate,  
                        'expiryDate' VALUE n.expiryDate,                        
                        'context' VALUE md.context,
                        'createdBy' VALUE n.createdBy,
                        'createdFirstName' VALUE uc.firstname,                        
                        'createdLastName' VALUE uc.lastname,              
                        'createdDate' VALUE n.createdDate,
                        'modifiedBy' VALUE n.modifiedBy,                        
                        'modifiedFirstName' VALUE um.firstname,                        
                        'modifiedLastName' VALUE um.lastname,  
                        'modifiedDate' VALUE n.modifiedDate                        
                        )
                    ) 
            )
         AS MetaJson 
  FROM notifications n
       LEFT OUTER JOIN metadata md
          ON md.entitysysid = n.entitysysid AND md.entitytype = n.entitytype
       INNER JOIN users uc
          ON uc.userid = n.createdby
       LEFT OUTER JOIN users um
          ON um.userid = n.modifiedby
       LEFT OUTER JOIN allowedentity ae
          ON ae.entitysysid = n.entitysysid
          AND ae.entitytype = n.entitytype
       LEFT OUTER JOIN isviewed vw
          ON vw.NOTIFICATIONSYSID = n.NOTIFICATIONSYSID
";


        public NotificationRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }




        public async Task<IEnumerable<Notification>> GetListAsync(string entitytype, string entitysysid)
        {
            return await _dataAccess.LoadDataAsync<Notification>($"{sqlCommon} WHERE n.ENTITYTYPE = :ENTITYTYPE AND n.ENTITYSYSID = :ENTITYSYSID",
         new Notification { EntityType = entitytype, EntitySysId = entitysysid });
        }


        public async Task<IEnumerable<Notification>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<Notification>($@"{sqlCommon} 
WHERE EXISTS IN (select * FROM projectmilestones pm WHERE pm.projectno = :entitysysid AND pm.entitysysid = n.entitysysid AND pm.entitytype = n.entitytype )     
OR  EXISTS IN (select * FROM projecttasks pt WHERE pt.projectno = :entitysysid AND pt.entitysysid = n.entitysysid AND pt.entitytype = n.entitytype )
OR  (n.entitytype = 'project' AND n.entitysysid = :entitysysid)   ",
                   new Notification { EntitySysId = projectno });
        }

        public async override Task<IEnumerable<Notification>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Notification>($@"{sqlCommon}");
        }

        public async override Task<Notification> GetAsync(string id)
        {
            return await _dataAccess.FindDataAsync<Notification>($"{sqlCommon} WHERE NotificationSysId = :NotificationSysId",
         new Notification { NotificationSysId = id });
        }

        public override async Task<string> AddAsync(Notification Notification)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<Notification>(@"  
INSERT INTO notifications (entitytype,
                           entitysysid,
                           title,
                           MESSAGE,
                           recipients,
                           notificationdate,
                           expirydate,
                           createdby)
     VALUES (:entitytype,
             :entitysysid,
             :title,
             :MESSAGE,
             :recipients,
             :notificationdate,
             :expirydate,
             :createdby)
  RETURNING notificationsysid
       INTO :notificationsysid
", Notification, "notificationsysid");
        }


        public async override Task<int> UpdateAsync(Notification entity)
        {
            return await _dataAccess.SaveDataAsync<Notification>(@"  
UPDATE notifications SET  
    title = :title,
    MESSAGE = :MESSAGE,
    recipients = :recipients,
    notificationdate = :notificationdate,
    expirydate = :expirydate,
    modifiedby = :modifiedby,
    modifieddate = SYSTIMESTAMP
WHERE notificationsysid = :notificationsysid
", entity);
        }

        public async override Task<int> DeleteAsync(string id)
        {
            return await _dataAccess.SaveDataAsync<Notification>(@"DELETE notifications WHERE notificationsysid = :notificationsysid", new Notification { NotificationSysId = id });
        }

        public async Task<int> MarkAsReadAsync(string userid, string notificationsysid)
        {
            return await _dataAccess.SaveDataAsync<NotificationViewed>(@"INSERT INTO notificationviewed (notificationsysid, userid) VALUES  (:notificationsysid, :userid)", new NotificationViewed { NotificationSysId = notificationsysid, UserId = userid });
        }

        public async Task<int> MarkAsReadAsync(string userid, IEnumerable<string> notificationsysids)
        {
            var notificationIds = (notificationsysids ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!notificationIds.Any())
            {
                return 0;
            }

            var rowsAffected = 0;
            foreach (var notificationId in notificationIds)
            {
                try
                {
                    rowsAffected += await _dataAccess.SaveDataAsync<NotificationViewed>(@"INSERT INTO notificationviewed (notificationsysid, userid) VALUES (:notificationsysid, :userid)", new NotificationViewed
                    {
                        NotificationSysId = notificationId,
                        UserId = userid
                    });
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("ORA-00001"))
                    {
                        throw;
                    }
                }
            }

            return rowsAffected;
        }

        public async Task<IEnumerable<Notification>> GetActiveAsync(string userid)
        {
            return await _dataAccess.LoadDataAsync<Notification>($@"{sqlCommon} WHERE notificationdate <= sysdate and (expirydate is null or expirydate >= sysdate)  ", new Notification
            {
                CreatedBy = userid
            });
        }

        public async Task<IEnumerable<Notification>> GetActiveUnreadAsync(string userid)
        {
            return await _dataAccess.LoadDataAsync<Notification>($@"{sqlCommon} WHERE notificationdate <= sysdate and (expirydate is null or expirydate >= sysdate) and NVL2(vw.NOTIFICATIONSYSID,1,0) = 0  ", new Notification
            {
                CreatedBy = userid
            });
        }
    }
}

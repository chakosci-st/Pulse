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
    public class ProjectAttachmentRepository : BaseRepository<ProjectAttachment, string>, IProjectAttachmentRepository
    {
        private string sqlCommon = @"
with PRJMILESTONES as (select * from PROJECTMILESTONES where projectno = :PROJECTNO),
 PRJTASKS as (select * from PROJECTTASKS where projectno = :PROJECTNO),
METADATA AS (select milestonesysid entitysysid, 'milestone' entitytype, milestonealias context from PRJMILESTONES pm inner join PROJECTROADMAPMILESTONES prm on prm.projectno = pm.projectno and prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
union all
select projecttasksysid entitysysid, 'activity' entitytype, NVL(pt.alttaskname, ra.activityname) context from PRJTASKS pt left outer join ROADMAPACTIVITIES ra on ra.roadmapactivitysysid = pt.roadmapactivitysysid
)
SELECT pa.*, 
        JSON_OBJECT(
            'meta' VALUE (
                JSON_OBJECT(
                        'fileName' VALUE pa.filename,                
                        'safeName' VALUE pa.attachmentsysid || pa.altfilename,
                        'fileType' VALUE pa.filetype,
                        'fileSize' VALUE pa.filesize,
                        'fileExtension' VALUE pa.fileExtension,
                        'context' VALUE md.context,
                        'createdBy' VALUE u.firstname,
                        'createdDate' VALUE pa.createdDate
                        )
                    ) 
            )
         AS MetaJson 
FROM PROJECTATTACHMENTS pa 
LEFT OUTER JOIN METADATA md ON md.entitysysid = pa.entitysysid AND md.entitytype = pa.entitytype 
INNER JOIN USERS u ON u.userid = pa.createdby
";


        public ProjectAttachmentRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectAttachment attachment)
        {
            await _dataAccess.SaveDataAsync<ProjectAttachment>(@" 
INSERT INTO PROJECTATTACHMENTS (ATTACHMENTSYSID, PROJECTNO,
                                FILENAME,
                                ALTFILENAME,
                                FILETYPE,
                                FILESIZE,
                                FILEEXTENSION,
                                ENTITYTYPE,
                                ENTITYSYSID,
                                CREATEDBY)
     VALUES (:ATTACHMENTSYSID, :PROJECTNO,
             :FILENAME,
             :ALTFILENAME,
             :FILETYPE,
             :FILESIZE,
             :FILEEXTENSION,
             :ENTITYTYPE,
             :ENTITYSYSID,
             :CREATEDBY) 
", attachment);

            return attachment.AttachmentSysId;
        }



        public async Task<IEnumerable<ProjectAttachment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid)
        {
            return await _dataAccess.LoadDataAsync<ProjectAttachment>($@"{sqlCommon} WHERE pa.PROJECTNO = :PROJECTNO AND pa.ENTITYTYPE = :ENTITYTYPE AND pa.ENTITYSYSID = :ENTITYSYSID",
                new ProjectAttachment { ProjectNo = projectno, EntityType = entitytype, EntitySysId = entitysysid });
        }


        public async Task<IEnumerable<ProjectAttachment>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<ProjectAttachment>($@"{sqlCommon} WHERE PROJECTNO = :PROJECTNO",
                   new ProjectAttachment { ProjectNo = projectno });
        }

        public override Task<int> DeleteAsync(string id)
        {
            return _dataAccess.SaveDataAsync<ProjectAttachment>(
                @"DELETE FROM PROJECTATTACHMENTS WHERE ATTACHMENTSYSID = :ATTACHMENTSYSID",
                new ProjectAttachment { AttachmentSysId = id });
        }

        public override Task<ProjectAttachment> GetAsync(string id)
        {
            return _dataAccess.FindDataAsync<ProjectAttachment>(
                @"SELECT * FROM PROJECTATTACHMENTS WHERE ATTACHMENTSYSID = :ATTACHMENTSYSID",
                new ProjectAttachment { AttachmentSysId = id });
        }

        public override Task<IEnumerable<ProjectAttachment>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<int> UpdateAsync(ProjectAttachment entity)
        {
            throw new NotImplementedException();
        }
    }
}

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
    public class ProjectCommentRepository : BaseRepository<ProjectComment, string>, IProjectCommentRepository
    {

        private string sqlCommon = @"
WITH prjmilestones AS (SELECT *
                         FROM projectmilestones
                        WHERE projectno = :projectno),
     prjtasks AS (SELECT *
                    FROM projecttasks
                   WHERE projectno = :projectno),
     metadata AS (SELECT milestonesysid entitysysid, 'MILESTONE' entitytype, milestonealias context
                                        FROM prjmilestones pm INNER JOIN projectroadmapmilestones prm ON prm.projectno = pm.projectno AND prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
                  UNION ALL
                                    SELECT projecttasksysid entitysysid, 'TASK' entitytype, NVL(pt.alttaskname, ra.activityname) context
                                        FROM prjtasks pt LEFT OUTER JOIN roadmapactivities ra ON ra.roadmapactivitysysid = pt.roadmapactivitysysid)
SELECT pc.*, 
        JSON_OBJECT(
            'meta' VALUE (
                JSON_OBJECT(
                        'comments' VALUE pc.comments,    
                    'commentsRichText' VALUE pc.commentsrichtext,
                        'context' VALUE md.context,
                        'createdByUserId' VALUE pc.createdby,
                        'createdByFullName' VALUE TRIM(NVL(u.firstname, '') || ' ' || NVL(u.lastname, '')),
                        'createdBy' VALUE u.firstname,
                        'createdFirstName' VALUE u.firstname,
                        'createdLastName' VALUE u.lastname,
                        'createdDate' VALUE pc.createdDate
                        )
                    ) 
            )
         AS MetaJson 
  FROM projectcomments pc
       LEFT OUTER JOIN metadata md
          ON md.entitysysid = pc.entitysysid AND md.entitytype = pc.entitytype
       INNER JOIN users u
          ON u.userid = pc.createdby
";

        public ProjectCommentRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectComment Comment)
        {
            return await _dataAccess.SaveDataWithClobReturnParameterNameAsync<ProjectComment>(@"  INSERT INTO PROJECTCOMMENTS
      (
PROJECTNO
,COMMENTS
,COMMENTSRICHTEXT
,ENTITYTYPE
,ENTITYSYSID
,CREATEDBY  
      )
    VALUES
      (
:PROJECTNO
,:COMMENTS
,:COMMENTSRICHTEXT
,:ENTITYTYPE
,:ENTITYSYSID
,:CREATEDBY  
      )
RETURNING CommentSYSID INTO :CommentSYSID
", Comment, "CommentSYSID", "COMMENTSRICHTEXT");
        }



        public async Task<IEnumerable<ProjectComment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid)
        {
            return await _dataAccess.LoadDataAsync<ProjectComment>($"{sqlCommon} WHERE pc.PROJECTNO = :PROJECTNO AND pc.ENTITYTYPE = :ENTITYTYPE AND pc.ENTITYSYSID = :ENTITYSYSID",
         new ProjectComment { ProjectNo = projectno, EntityType = entitytype, EntitySysId = entitysysid });
        }
 

        public async Task<IEnumerable<ProjectComment>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<ProjectComment>($"{sqlCommon} WHERE pc.PROJECTNO = :PROJECTNO",
                   new ProjectComment { ProjectNo = projectno });
        }

        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<ProjectComment> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ProjectComment>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<int> UpdateAsync(ProjectComment entity)
        {
            throw new NotImplementedException();
        }
    }
}

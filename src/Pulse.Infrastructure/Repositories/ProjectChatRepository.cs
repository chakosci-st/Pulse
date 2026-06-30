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
    public class ProjectChatRepository : BaseRepository<ProjectChat, string>, IProjectChatRepository
    {

        private string sqlCommon = @"
SELECT pc.*, p.projectname, p.projecticon, p.projecticoncolor projectcolor,
        JSON_OBJECT(
            'meta' VALUE (
                JSON_OBJECT(
                        'message' VALUE pc.message,     
                        'createdBy' VALUE u.firstname,
                        'createdDate' VALUE pc.createdDate
                        )
                    ) 
            )
         AS MetaJson 
FROM PROJECTCHATS pc  
INNER JOIN USERS u ON u.userid = pc.createdby
INNER JOIN PROJECTS p ON p.projectno = pc.projectno
";

        public ProjectChatRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectChat Chat)
        {
            // MARK CHATS AS READ
            await _dataAccess.SaveDataAsync(@"  
INSERT INTO projectchatviewed (chatsysid, userid)
   SELECT chatsysid, :createdby userid FROM projectchats
   MINUS
   SELECT chatsysid,  userid
     FROM projectchatviewed
    WHERE userid = :createdby
", new ProjectChat { CreatedBy = Chat.CreatedBy });


            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectChat>(@"  
INSERT INTO PROJECTCHATS (PROJECTNO,
                          MESSAGE,
                          SENDERDISPLAYNAME,
                          CREATEDBY)
     VALUES (:PROJECTNO,
             :MESSAGE,
             :SENDERDISPLAYNAME,
             :CREATEDBY)
  RETURNING CHATSYSID
       INTO :CHATSYSID
", Chat, "CHATSYSID");
        }


        public async Task<IEnumerable<ProjectChat>> GetUnreadListByUserAsync(string user)
        {
            return await _dataAccess.LoadDataAsync<ProjectChat>(@"
WITH projectsregistered AS (SELECT projectno
                              FROM PROJECTOWNERS
                             WHERE userid = :CreatedBy
                            UNION
                            SELECT projectno
                              FROM PROJECTMEMBERS
                             WHERE userid = :CreatedBy),
     viewedchats AS (SELECT pcv.*
                       FROM PROJECTCHATVIEWED pcv
                      WHERE pcv.userid = :CreatedBy),
     chats AS (SELECT pc.chatsysid,
                      pc.projectno,
                      pc.MESSAGE,
                      pc.senderdisplayname,
                      pc.createdby,
                      pc.createddate
                 FROM PROJECTCHATS pc INNER JOIN projectsregistered pr ON pr.projectno = pc.projectno
                WHERE pc.createdby <> :CreatedBy)
SELECT c.*,
       p.projectname roomname,
       p.projecticon roomicon,
       p.projecticoncolor roomcolor
  FROM chats c INNER JOIN PROJECTS p ON p.projectno = c.projectno
 WHERE NOT EXISTS
          (SELECT 1
             FROM viewedchats vc
            WHERE vc.chatsysid = c.chatsysid)
",
                   new ProjectChat { CreatedBy = user });
        }


        public async Task<IEnumerable<ProjectChat>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<ProjectChat>($"{sqlCommon} WHERE pc.PROJECTNO = :PROJECTNO",
                   new ProjectChat { ProjectNo = projectno });
        }

        public async Task<IEnumerable<ProjectChat>> GetListAsync(string projectno, string user)
        {
            return await _dataAccess.LoadDataAsync<ProjectChat>($@"
                WITH viewed as (select* from projectchatviewed WHERE userid = :createdby)
SELECT pc.*, p.projecticon, p.projecticoncolor projectcolor, NVL2(v.chatsysid, 1, 0) viewed,
        JSON_OBJECT(
            'meta' VALUE(
                JSON_OBJECT(
                        'message' VALUE pc.message,
                        'createdBy' VALUE u.firstname,
                        'createdDate' VALUE pc.createdDate
                        )
                    )
            )
         AS MetaJson
FROM PROJECTCHATS pc
INNER JOIN USERS u ON u.userid = pc.createdby
LEFT OUTER JOIN viewed v ON v.chatsysid = pc.chatsysid
INNER JOIN PROJECTS p ON p.projectno = c.projectno
WHERE pc.projectno = :projectno
                ",
                   new ProjectChat { ProjectNo = projectno });
        }


        public async Task<RoomMeta> GetRoomMetaAsync(string room)
        {
            return await _dataAccess.FindDataAsync<RoomMeta>($@"
                SELECT projectno room,
       projectname roomname,
       projecticon roomicon,
       projecticoncolor roomcolor
  FROM projects
 WHERE projectno = :room
                ",
                   new RoomMeta { Room = room });
        }


        public async Task<IEnumerable<RoomMeta>> GetRoomsByUserIdAsync(string userid)
        {
            return await _dataAccess.LoadDataAsync<RoomMeta>($@"
WITH projectsregistered AS (SELECT projectno
                              FROM PROJECTOWNERS
                             WHERE userid = :loggeduser
                            UNION
                            SELECT projectno
                              FROM PROJECTMEMBERS
                             WHERE userid = :loggeduser
                            UNION
                            SELECT projectno
                              FROM PROJECTS prj
                             WHERE EXISTS
                                      (SELECT plantcode
                                         FROM plantmembers pm
                                        WHERE userid = :loggeduser AND prj.plantcode = pm.plantcode))
SELECT pr.projectno room,
       projectname roomname,
       projecticon roomicon,
       projecticoncolor roomcolor,
       (SELECT COUNT (1)
          FROM PROJECTCHATS pc
         WHERE     pc.projectno = pr.projectno
               AND createdby <> :loggeduser
               AND NOT EXISTS
                      (SELECT *
                         FROM PROJECTCHATVIEWED pcv
                        WHERE pcv.chatsysid = pc.chatsysid AND userid = :loggeduser))
          UnreadCount,
       (SELECT MAX (pc.createddate) LastMessagePreview
          FROM PROJECTCHATS pc
         WHERE     pc.projectno = pr.projectno
               AND createdby <> :loggeduser
               AND NOT EXISTS
                      (SELECT *
                         FROM PROJECTCHATVIEWED pcv
                        WHERE pcv.chatsysid = pc.chatsysid AND userid = :loggeduser))
          LastMessagePreview
  FROM projectsregistered pr INNER JOIN projects p ON p.projectno = pr.projectno
                ",
                   new RoomMeta { LoggedUser = userid });
        }

        public async Task<IEnumerable<string>> GetParticipantsByRoomAsync(string room)
        {
            return (await _dataAccess.LoadDataAsync<RoomMeta>($@"
SELECT userid LoggedUser
                              FROM PROJECTOWNERS
                             WHERE projectno = :Room
                            UNION
                            SELECT userid LoggedUser
                              FROM PROJECTMEMBERS
                             WHERE projectno = :Room 
                ",
                   new RoomMeta { Room = room })).Select(r => r.LoggedUser);
        }
        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<ProjectChat> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ProjectChat>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<int> UpdateAsync(ProjectChat entity)
        {
            throw new NotImplementedException();
        }
    }
}

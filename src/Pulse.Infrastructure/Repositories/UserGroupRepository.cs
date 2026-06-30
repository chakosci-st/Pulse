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
    public class UserGroupRepository : BaseRepository<UserGroup, int>, IUserGroupRepository
    {
        public UserGroupRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<int> AddAsync(UserGroup usergroup)
        {
            return await _dataAccess.SaveDataReturnIdAsync<UserGroup>(@"INSERT INTO USERGROUPS (USERGROUPNAME, USERGROUPDESCRIPTION, ISACTIVE, CREATEDBY) 
VALUES (:USERGROUPNAME, :USERGROUPDESCRIPTION, :ISACTIVE, :CREATEDBY) RETURNING USERGROUPID INTO :USERGROUPID", usergroup, "USERGROUPID");
        }

        public override async Task<int> UpdateAsync(UserGroup usergroup)
        {
            return await _dataAccess.SaveDataAsync<UserGroup>(@"UPDATE USERGROUPS 
                        SET USERGROUPNAME = :USERGROUPNAME, USERGROUPDESCRIPTION = :USERGROUPDESCRIPTION, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE USERGROUPID = :USERGROUPID AND TRANSACTIONKEY = :TRANSACTIONKEY", usergroup);
        }

        public override async Task<int> DeleteAsync(int usergroupid)
        {
            return await _dataAccess.SaveDataAsync<UserGroup>("DELETE FROM USERGROUPS WHERE USERGROUPID = :USERGROUPID", new UserGroup { UserGroupId = usergroupid });
        }

        public override async Task<IEnumerable<UserGroup>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<UserGroup>("SELECT * FROM USERGROUPS")
                .ContinueWith(t => (IEnumerable<UserGroup>)t.Result);
        }

        public async Task<PagedResult<UserGroup>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY {sortBy} {sortDirection}) rn
            FROM (SELECT ug.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname
                    FROM usergroups ug
                         INNER JOIN users cb
                            ON cb.userid = ug.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = ug.modifiedby
                   WHERE (LOWER (ug.usergroupname) LIKE
                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR ug.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn 
";
            string countQuery = @"
SELECT COUNT(1)
FROM usergroups ug
WHERE (LOWER (ug.usergroupname) LIKE LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR ug.ISACTIVE = :isactivestate)
";


            var parameters = new
            {
                searchvalue = searchValue,
                isactivestate = (isActive == null ? (char?)null : (isActive.Value ? '1' : '0')),
                offset = (pageNumber - 1) * pageSize,
                pagesize = pageSize
            };
            try
            {
                // Use Dapper's QueryAsync for mapping
                int totalRecords = await _dataAccess.ExecuteScalarAsync<int>(countQuery, parameters);

                var data = (await _dataAccess.QueryAsync<UserGroup>(pagedQuery, parameters)).ToList();

                return new PagedResult<UserGroup>
                {
                    TotalRecords = totalRecords,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public UserGroup Get(int usergroupid)
        {
            return _dataAccess.FindData<UserGroup>("SELECT * FROM USERGROUPS WHERE USERGROUPID = :USERGROUPID", new UserGroup { UserGroupId = usergroupid });
        }

        public override async Task<UserGroup> GetAsync(int usergroupid)
        {
            return await _dataAccess.FindDataAsync<UserGroup>("SELECT * FROM USERGROUPS WHERE USERGROUPID = :USERGROUPID",
                new UserGroup { UserGroupId = usergroupid });
        }

    }
}

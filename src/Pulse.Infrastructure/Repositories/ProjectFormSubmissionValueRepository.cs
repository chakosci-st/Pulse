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
    public class ProjectFormSubmissionValueRepository : BaseRepository<ProjectFormSubmissionValue, string>, IProjectFormSubmissionValueRepository
    {
        public ProjectFormSubmissionValueRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public async override Task<string> AddAsync(ProjectFormSubmissionValue value)
        {
            var hasClobValue = !string.IsNullOrWhiteSpace(value.FieldValueClob);
            value.FieldValue = hasClobValue ? null : value.FieldValue;
            value.FieldValueClob = hasClobValue ? value.FieldValueClob : null;

            if (!hasClobValue)
            {
                return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectFormSubmissionValue>(@" INSERT INTO PROJECTFORMSUBMISSIONVALUES
      (
        SUBMISSIONSYSID,
        PROJECTNO,
        FORMENTITYLINKSYSID,
        FORMSYSID,
        ENTITYSYSID,
        ENTITYTYPE,
        FORMFIELDSYSID,
                FIELDVALUE,
        CREATEDBY 
      )
    VALUES
      (
        :SUBMISSIONSYSID,
        :PROJECTNO,
        :FORMENTITYLINKSYSID,
        :FORMSYSID,
        :ENTITYSYSID,
        :ENTITYTYPE,
        :FORMFIELDSYSID,
                :FIELDVALUE,
        :CREATEDBY 
      )
RETURNING SUBMISSIONVALUESYSID INTO :SUBMISSIONVALUESYSID
", value, "SUBMISSIONVALUESYSID");
            }

            return await _dataAccess.SaveDataWithClobReturnParameterNameAsync<ProjectFormSubmissionValue>(@" INSERT INTO PROJECTFORMSUBMISSIONVALUES
            (
                SUBMISSIONSYSID,
                PROJECTNO,
                FORMENTITYLINKSYSID,
                FORMSYSID,
                ENTITYSYSID,
                ENTITYTYPE,
                FORMFIELDSYSID, 
                FIELDVALUECLOB,
                CREATEDBY 
            )
        VALUES
            (
                :SUBMISSIONSYSID,
                :PROJECTNO,
                :FORMENTITYLINKSYSID,
                :FORMSYSID,
                :ENTITYSYSID,
                :ENTITYTYPE,
                :FORMFIELDSYSID, 
                :FIELDVALUECLOB,
                :CREATEDBY 
            )
RETURNING SUBMISSIONVALUESYSID INTO :SUBMISSIONVALUESYSID
", value, "SUBMISSIONVALUESYSID", "FIELDVALUECLOB");


        }

        public override async Task<int> UpdateAsync(ProjectFormSubmissionValue value)
        {
            var hasClobValue = !string.IsNullOrWhiteSpace(value.FieldValueClob);
            value.FieldValue = hasClobValue ? null : value.FieldValue;
            value.FieldValueClob = hasClobValue ? value.FieldValueClob : null;

            // Client submission payload does not include IsActive; keep submission values active on update.
            value.IsActive = 1;

            const string sql = @"
                        UPDATE PROJECTFORMSUBMISSIONVALUES 
                        SET 
                            FIELDVALUE = :FIELDVALUE,
                            FIELDVALUECLOB = :FIELDVALUECLOB,
                            ISACTIVE = :ISACTIVE,
                            MODIFIEDBY = :MODIFIEDBY,
                            MODIFIEDDATE = SYSTIMESTAMP,
                            TRANSACTIONKEY = SYS_GUID() 
                        WHERE SUBMISSIONVALUESYSID = :SUBMISSIONVALUESYSID AND TRANSACTIONKEY = :TRANSACTIONKEY";
            try
            {
                if (hasClobValue)
                {
                    return await _dataAccess.SaveDataAsync(sql, value, "FIELDVALUECLOB");
                }

                return await _dataAccess.SaveDataAsync<ProjectFormSubmissionValue>(sql, value);
            }
            catch (Exception e)
            {
                throw;
            }





        }

        public async Task<List<ProjectFormSubmissionValue>> GetBySubmissionAsync(string id)
        {
            var sql = @" SELECT sv.submissionvaluesysid,
           sv.submissionsysid,
           sv.projectno,
           sv.formsysid,
           sv.formentitylinksysid,
           sv.entitysysid,
           sv.entitytype,
           sv.formfieldsysid,
           sv.fieldvalue,
         sv.fieldvalueclob,
           sv.transactionkey,
           s.transactionkey AS submissiontransactionkey
      FROM PROJECTFORMSUBMISSIONVALUES sv
      JOIN PROJECTFORMSUBMISSIONS s
        ON s.submissionsysid = sv.submissionsysid
     WHERE SUBMISSIONSYSID = :SUBMISSIONSYSID";
            return await _dataAccess.LoadDataAsync<ProjectFormSubmissionValue>(sql, new ProjectFormSubmissionValue { SubmissionSysId = id });
        }

        public async Task<IList<ProjectMonitoringDmsValue>> GetDmsValuesForMonitoringAsync(string loggedUser)
        {
            var sql = @"
WITH accessible_projects AS (
    SELECT projectno
      FROM PROJECTOWNERS
     WHERE :loggeduser IS NULL OR userid = :loggeduser
    UNION
    SELECT projectno
      FROM PROJECTMEMBERS
     WHERE :loggeduser IS NULL OR userid = :loggeduser
    UNION
    SELECT prj.projectno
      FROM PROJECTS prj
     WHERE EXISTS (
            SELECT 1
              FROM PLANTMEMBERS pm
             WHERE (:loggeduser IS NULL OR userid = :loggeduser)
               AND prj.plantcode = pm.plantcode
        )
),
ranked_values AS (
    SELECT
        v.projectno,
        v.entitysysid AS nodeid,
        TRIM(NVL(v.fieldvalue, DBMS_LOB.SUBSTR(v.fieldvalueclob, 4000, 1))) AS dmsvalue,
        ROW_NUMBER() OVER (
            PARTITION BY v.projectno, v.entitysysid
            ORDER BY NVL(v.modifieddate, v.createddate) DESC, v.createddate DESC, v.submissionvaluesysid DESC
        ) AS rn
    FROM PROJECTFORMSUBMISSIONVALUES v
    INNER JOIN FORMFIELDS ff
        ON ff.formfieldsysid = v.formfieldsysid
    INNER JOIN accessible_projects ap
        ON ap.projectno = v.projectno
    WHERE UPPER(TRIM(ff.fieldtitle)) = 'DMS'
      AND UPPER(TRIM(v.entitytype)) IN ('ACTIVITY', 'TASK')
      AND TRIM(NVL(v.fieldvalue, DBMS_LOB.SUBSTR(v.fieldvalueclob, 4000, 1))) IS NOT NULL
)
SELECT
    projectno,
    nodeid,
    dmsvalue
FROM ranked_values
WHERE rn = 1";

            return await _dataAccess.LoadDataAsync<ProjectMonitoringDmsValue>(sql, new ProjectMonitoringDmsValue { LoggedUser = loggedUser });
        }

        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }




        public async override Task<ProjectFormSubmissionValue> GetAsync(string id)
        {
            var sql = @" SELECT sv.submissionvaluesysid,
           sv.submissionsysid,
           sv.projectno,
           sv.formsysid,
           sv.formentitylinksysid,
           sv.entitysysid,
           sv.entitytype,
           sv.formfieldsysid,
           sv.fieldvalue,
         sv.fieldvalueclob,
           sv.transactionkey,
           s.transactionkey AS submissiontransactionkey
      FROM PROJECTFORMSUBMISSIONVALUES sv
      JOIN PROJECTFORMSUBMISSIONS s
        ON s.submissionsysid = sv.submissionsysid
     WHERE SUBMISSIONVALUESYSID = :SUBMISSIONVALUESYSID";
            return await _dataAccess.FindDataAsync<ProjectFormSubmissionValue>(sql, new ProjectFormSubmissionValue { SubmissionValueSysId = id });
        }

        public override Task<IEnumerable<ProjectFormSubmissionValue>> GetListAsync()
        {
            throw new NotImplementedException();
        }

    }
}

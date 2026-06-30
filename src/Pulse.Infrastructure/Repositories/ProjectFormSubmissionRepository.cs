
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
    public class ProjectFormSubmissionRepository : BaseRepository<ProjectFormSubmission, string>, IProjectFormSubmissionRepository
    {
        #region "SQL QUERY"
        const string sql_forms_layout = @"
WITH roadmap AS (SELECT projectno, roadmapsysid
                   FROM PROJECTS
                  WHERE projectno = :projectno),
     nodes AS (SELECT p.projectno, rm.roadmapmilestonesysid nodeid, 'milestone' nodetype
                 FROM PROJECTROADMAPMILESTONES rm INNER JOIN roadmap p ON rm.roadmapsysid = p.roadmapsysid
               UNION
               SELECT p.projectno, ra.roadmapactivitysysid nodeid, 'activity' nodetype
                 FROM PROJECTROADMAPACTIVITIES ra INNER JOIN roadmap p ON ra.roadmapsysid = p.roadmapsysid),
     nodevalues AS (SELECT sv.submissionvaluesysid,
                           sv.submissionsysid,
                           sv.projectno,
                           sv.formsysid,
                           sv.formentitylinksysid,
                           sv.entitysysid,
                           sv.entitytype,
                           sv.formfieldsysid,
                           sv.fieldvalue,
                           sv.fieldvalueclob, -- WILL BE RETREIVED LATER AS JSON CAN ONLY BE 4000Characters
                           sv.transactionkey,
                           sv.transactionkey submissiontransactionkey
                      FROM PROJECTFORMSUBMISSIONVALUES sv INNER JOIN PROJECTFORMSUBMISSIONS s ON s.submissionsysid = sv.submissionsysid
                     WHERE s.projectno = :projectno),
     formstoshow AS (SELECT n.projectno,
                            lnk.formentitylinksysid,
                            lnk.formsysid,
                            n.nodeid,
                            n.nodetype,
                            lnk.orderindex,
                            lnk.isactive
                       FROM FORMENTITYLINKS lnk JOIN nodes n ON lnk.entitysysid = n.nodeid AND lnk.entitytype = n.nodetype),
     forms_layout as (SELECT fs.projectno, f.formsysid, f.formname, f.formdescription, fs.formentitylinksysid, fs.nodeid, fs.nodetype, fs.orderindex, fs.isactive, 
JSON_OBJECT(
            'id'          VALUE f.formsysid,
            'name'        VALUE f.formname,
            'description' VALUE f.formdescription,
            'fields'      VALUE (
                SELECT JSON_ARRAYAGG(
                           JSON_OBJECT(
                               'id'                 VALUE ff.formfieldsysid,
                               'fieldSysId'         VALUE NVL(ff.fieldsysid, fld.fieldsysid),
                                                             'useFieldDefaults'   VALUE CASE
                                                                                                                    WHEN ff.fieldsysid IS NOT NULL AND ff.fieldname IS NULL AND ff.fieldtitle IS NULL AND ff.fieldtype IS NULL THEN 'true'
                                                                                                                    ELSE 'false'
                                                                                                                END,
                               'name'               VALUE COALESCE(ff.fieldname, fld.fieldname),
                               'title'              VALUE COALESCE(ff.fieldtitle, fld.fieldtitle),
                               'type'               VALUE COALESCE(ff.fieldtype, fld.fieldtype),
                                                             'isActive'           VALUE CASE
                                                                                                                     WHEN ff.isactive = 1 THEN 'true'
                                                                                                                     WHEN ff.isactive = 0 THEN 'false'
                                                                                                                     WHEN fld.isactive = 1 THEN 'true'
                                                                                                                     WHEN fld.isactive = 0 THEN 'false'
                                                                                                                     ELSE 'true'
                                                                                                                 END,
                               'placeholder'        VALUE COALESCE(ff.placeholder, fld.placeholder),
                               'tooltip'            VALUE COALESCE(ff.tooltip, fld.tooltip),

                               -- booleans as strings:
                               'isrequired'         VALUE CASE
                                                            WHEN ff.isrequired = 1 THEN 'true'
                                                                                                                        WHEN ff.isrequired = 0 THEN 'false'
                                                            ELSE 'false'
                                                          END,

                                                             'minLength'          VALUE COALESCE(ff.minlength, fld.minlength),
                                                             'maxLength'          VALUE COALESCE(ff.maxlength, fld.maxlength),
                                                             'caseOption'         VALUE COALESCE(ff.caseoption, fld.caseoption),
                                                             'fileTypes'          VALUE COALESCE(ff.filetype, fld.filetype),
                                                             'fileMaxSize'        VALUE COALESCE(ff.filemaxsize, fld.filemaxsize),
                                                             'readAccess'         VALUE NVL(ff.readaccess, '*'),
                                                             'writeAccess'        VALUE NVL(ff.writeaccess, '*'),
                                                             'validate'           VALUE COALESCE(ff.fieldvalidate, fld.fieldvalidate),
                                                             'datasource'         VALUE COALESCE(ff.datasource, fld.datasource),
                                                             'datasourceParamField' VALUE COALESCE(ff.datasourceparamfield, fld.datasourceparamfield),
                                                             'parentFieldId'      VALUE COALESCE(ff.parentfieldsysid, fld.parentfieldsysid),

                               'urlIsParameter'     VALUE CASE
                                                            WHEN ff.urlisparam = 1 THEN 'true'
                                                            WHEN ff.urlisparam = 0 THEN 'false'
                                                            WHEN fld.urlisparam = 1 THEN 'true'
                                                            WHEN fld.urlisparam = 0 THEN 'false'
                                                            ELSE 'false'
                                                          END,

                               'urlDefaultPattern'  VALUE COALESCE(ff.urldefaultpattern, fld.urldefaultpattern),
                               'defaultPattern'     VALUE COALESCE(ff.defaultpattern, fld.defaultpattern),
                               'defaultValue'       VALUE COALESCE(ff.defaultvalue, fld.defaultvalue),
                               'defaultClobValue'   VALUE COALESCE(ff.defaultclobvalue, fld.defaultclobvalue),
                               'formSysId'          VALUE ff.formSysId,
                               'formEntityLinkSysid' VALUE fs.formentitylinksysid,
                               'orderIndex'         VALUE ff.orderindex,

                               'options' VALUE (
                                   SELECT COALESCE(
                                       JSON_ARRAYAGG(
                                           JSON_OBJECT(
                                               'id'         VALUE opt.id,
                                               'value'      VALUE opt.optionvalue,
                                               'label'      VALUE opt.optionlabel,
                                               'orderIndex' VALUE opt.orderindex
                                           )
                                           RETURNING CLOB
                                       ),
                                       TO_CLOB('[]')
                                   )
                                   FROM (
                                       SELECT fo.formfieldoptionsysid id, fo.optionvalue, fo.optionlabel, fo.orderindex
                                       FROM FORMFIELDOPTIONS fo
                                       WHERE fo.formfieldsysid = ff.formfieldsysid
                                       UNION ALL
                                       SELECT bo.fieldoptionsysid id, bo.optionvalue, bo.optionlabel, bo.orderindex
                                       FROM FIELDOPTIONS bo
                                       WHERE bo.fieldsysid = NVL(ff.fieldsysid, fld.fieldsysid)
                                         AND NOT EXISTS (
                                            SELECT 1
                                            FROM FORMFIELDOPTIONS fo2
                                            WHERE fo2.formfieldsysid = ff.formfieldsysid
                                              AND fo2.optionvalue = bo.optionvalue
                                         )
                                   ) opt
                               ),

                               'rules' VALUE (
                                   SELECT COALESCE(
                                       JSON_ARRAYAGG(
                                           JSON_OBJECT(
                                               'id'          VALUE rul.id,
                                               'field'       VALUE rul.rulefield,
                                               'operator'    VALUE rul.ruleoperator,
                                               'value'       VALUE rul.rulevalue,
                                               'action'      VALUE rul.ruleaction,
                                               'actionValue' VALUE rul.ruleactionvalue
                                           )
                                           RETURNING CLOB
                                       ),
                                       TO_CLOB('[]')
                                   )
                                   FROM (
                                       SELECT fr.formfieldrulesysid id, fr.rulefield, fr.ruleoperator, fr.rulevalue, fr.ruleaction, fr.ruleactionvalue
                                       FROM FORMFIELDRULES fr
                                       WHERE fr.formfieldsysid = ff.formfieldsysid
                                       UNION ALL
                                       SELECT br.fieldrulesysid id, br.rulefield, br.ruleoperator, br.rulevalue, br.ruleaction, br.ruleactionvalue
                                       FROM FIELDRULES br
                                       WHERE br.fieldsysid = NVL(ff.fieldsysid, fld.fieldsysid)
                                         AND NOT EXISTS (
                                            SELECT 1
                                            FROM FORMFIELDRULES fr2
                                            WHERE fr2.formfieldsysid = ff.formfieldsysid
                                              AND fr2.rulefield = br.rulefield
                                              AND fr2.ruleoperator = br.ruleoperator
                                              AND fr2.rulevalue = br.rulevalue
                                              AND fr2.ruleaction = br.ruleaction
                                              AND NVL(fr2.ruleactionvalue, ' ') = NVL(br.ruleactionvalue, ' ')
                                         )
                                   ) rul
                               ),

                               'values' VALUE (
                                   SELECT COALESCE(
                                       JSON_ARRAYAGG(
                                           JSON_OBJECT(
                                               'id'                    VALUE nv.submissionvaluesysid,
                                               'value'                 VALUE nv.fieldvalue,
                                               'transactionkey'        VALUE nv.transactionkey,
                                               'submissionsysid'       VALUE nv.submissionsysid,
                                               'submissiontransactionkey' VALUE nv.submissiontransactionkey,
                                               'formsysid'             VALUE nv.formsysid,
                                               'formentitylinksysid'   VALUE nv.formentitylinksysid,
                                               'entitysysid'           VALUE nv.entitysysid,
                                               'entitytype'            VALUE nv.entitytype,
                                               'formfieldsysid'        VALUE nv.formfieldsysid
                                           )
                                           RETURNING CLOB
                                       ),
                                       TO_CLOB('[]')
                                   )
                                   FROM nodevalues nv
                                   WHERE nv.formentitylinksysid = fs.formentitylinksysid
                                     AND nv.entitysysid         = fs.nodeid
                                     AND nv.entitytype          = fs.nodetype
                               )
                           )
                           ORDER BY ff.orderindex
                           RETURNING CLOB
                       )
                FROM FORMFIELDS ff
                LEFT JOIN FIELDS fld ON fld.fieldsysid = ff.fieldsysid
                WHERE ff.formsysid = f.formsysid
            )
            RETURNING CLOB
        ) AS FormJson
            FROM forms f INNER JOIN formstoshow fs ON f.formsysid = fs.formsysid
    ) 
";
        #endregion

        public ProjectFormSubmissionRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }



        public async Task<IList<ProjectFormSubmissionExtended>> GetSubmittedForms(string projectNo)
        {
            var dataresult = await _dataAccess.LoadDataAsync<ProjectFormSubmissionExtended>(
                                                 sql_forms_layout + @" select * from forms_layout",
                                                 new ProjectFormSubmissionExtended
                                                 {
                                                     ProjectNo = projectNo
                                                 }
                                            );

            return dataresult;
        }

        public async Task<IList<ProjectFormSubmissionExtended>> GetSubmittedForms(string projectNo, string entityType, string entitySysId)
        {
            var dataresult = await _dataAccess.LoadDataAsync<ProjectFormSubmissionExtended>(
                                                 sql_forms_layout + @" select * from forms_layout WHERE NodeId = :NodeId AND NodeType = :NodeType",
                                                 new ProjectFormSubmissionExtended
                                                 {
                                                     ProjectNo = projectNo,
                                                     NodeId = entitySysId,
                                                     NodeType = entityType
                                                 }
                                            );

            return dataresult;
        }


        public async override Task<string> AddAsync(ProjectFormSubmission value)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectFormSubmission>(@"  
INSERT INTO PROJECTFORMSUBMISSIONS
      (
        PROJECTNO,
        FORMENTITYLINKSYSID,
        FORMSYSID, 
        CREATEDBY 
      )
    VALUES
      (
        :PROJECTNO,
        :FORMENTITYLINKSYSID,
        :FORMSYSID, 
        :CREATEDBY 
      )
RETURNING SUBMISSIONSYSID INTO :SUBMISSIONSYSID
", value, "SUBMISSIONSYSID");
        }

        public async override Task<int> UpdateAsync(ProjectFormSubmission value)
        {
            return await _dataAccess.SaveDataAsync<ProjectFormSubmission>(@"UPDATE PROJECTFORMSUBMISSIONS 
                        SET MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE SUBMISSIONSYSID = :SUBMISSIONSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", value);
        }


        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<ProjectFormSubmission> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ProjectFormSubmission>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        
    }
}

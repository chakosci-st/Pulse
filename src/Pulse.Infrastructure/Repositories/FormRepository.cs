
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
    public class FormRepository : BaseRepository<Form, string>, IFormRepository
    {

        public FormRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Form form)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync(@"INSERT INTO FORMS (FORMNAME, FORMDESCRIPTION, CREATEDBY) 
VALUES (:FORMNAME, :FORMDESCRIPTION, :CREATEDBY) RETURNING FORMSYSID INTO :FORMSYSID", form, "FORMSYSID");
        }

        public override async Task<int> UpdateAsync(Form form)
        {
            return await _dataAccess.SaveDataAsync<Form>(@"UPDATE FORMS 
                        SET FORMNAME = :FORMNAME, FORMDESCRIPTION = :FORMDESCRIPTION, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE FORMSYSID = :FORMSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", form);
        }

        public override async Task<int> DeleteAsync(string formsysid)
        {
            return await _dataAccess.SaveDataAsync<Form>("DELETE FROM FORMS WHERE FORMSYSID = :FORMSYSID", new Form { FormSysId = formsysid });
        }


        public override async Task<IEnumerable<Form>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Form>("SELECT * FROM FORMS")
                .ContinueWith(t => (IEnumerable<Form>)t.Result);
        }


        public async Task<PagedResult<FormExtended>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY  {sortBy} {sortDirection}) rn
            FROM (SELECT f.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname,
 JSON_OBJECT(
    'id' VALUE f.formsysid,
    'name' VALUE f.formname,
    'description' VALUE f.formdescription,
    'fields' VALUE (
        SELECT JSON_ARRAYAGG(
            JSON_OBJECT(
                'id' VALUE ff.formfieldsysid,
                'fieldSysId' VALUE NVL(ff.fieldsysid, fld.fieldsysid),
                'useFieldDefaults' VALUE CASE
                    WHEN ff.fieldsysid IS NOT NULL AND ff.fieldname IS NULL AND ff.fieldtitle IS NULL AND ff.fieldtype IS NULL THEN 'true'
                    ELSE 'false'
                END,
                'name' VALUE COALESCE(ff.fieldname, fld.fieldname),
                'title' VALUE COALESCE(ff.fieldtitle, fld.fieldtitle),
                'type' VALUE COALESCE(ff.fieldtype, fld.fieldtype),
                'isActive' VALUE NVL2(ff.isactive, DECODE(ff.isactive, 1, 'true', 'false'), NVL2(fld.isactive, DECODE(fld.isactive, 1, 'true', 'false'), 'true')),
                'placeholder' VALUE COALESCE(ff.placeholder, fld.placeholder),
                'tooltip' VALUE COALESCE(ff.tooltip, fld.tooltip),
                'isrequired' VALUE NVL2(ff.isrequired,DECODE(ff.isrequired,1,'true','false'), 'false'), 
                'minLength' VALUE COALESCE(ff.minlength, fld.minlength),
                'maxLength' VALUE COALESCE(ff.maxlength, fld.maxlength),
                'caseOption' VALUE COALESCE(ff.caseoption, fld.caseoption),
                'fileTypes' VALUE COALESCE(ff.filetype, fld.filetype),
                'fileMaxSize' VALUE COALESCE(ff.filemaxsize, fld.filemaxsize),
                'urlIsParam' VALUE NVL2(ff.urlIsParam, DECODE(ff.urlIsParam,1,'true','false'), NVL2(fld.urlIsParam, DECODE(fld.urlIsParam,1,'true','false'), null)),
                'urlDefaultPattern' VALUE COALESCE(ff.urlDefaultPattern, fld.urlDefaultPattern),
                'defaultValue' VALUE COALESCE(ff.defaultValue, fld.defaultValue),
                'defaultClobValue' VALUE COALESCE(ff.defaultClobValue, fld.defaultClobValue),
                'readAccess' VALUE NVL(ff.readaccess, '*'),
                'writeAccess' VALUE NVL(ff.writeaccess, '*'),
                'validate' VALUE COALESCE(ff.fieldvalidate, fld.fieldvalidate),
                'datasource' VALUE COALESCE(ff.datasource, fld.datasource),
                'datasourceParamField' VALUE COALESCE(ff.datasourceparamfield, fld.datasourceparamfield),
                'parentFieldId' VALUE COALESCE(ff.parentfieldsysid, fld.parentfieldsysid),
                'orderIndex' VALUE ff.orderindex,
                'options' VALUE (
                    SELECT COALESCE(JSON_ARRAYAGG(
                        JSON_OBJECT(
                            'id' VALUE opt.id,
                            'value' VALUE opt.optionvalue,
                            'label' VALUE opt.optionlabel,
                            'orderIndex' VALUE opt.orderindex
                        )
                    ), JSON_ARRAY())
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
                    SELECT COALESCE(JSON_ARRAYAGG(
                        JSON_OBJECT(
                            'id' VALUE rul.id,
                            'field' VALUE rul.rulefield,
                            'operator' VALUE rul.ruleoperator,
                            'value' VALUE rul.rulevalue,
                            'action' VALUE rul.ruleaction,
                            'actionValue' VALUE rul.ruleactionvalue
                        )
                    ), JSON_ARRAY())
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
                )
            )
            ORDER BY ff.orderindex
        )
        FROM FORMFIELDS ff
        LEFT JOIN FIELDS fld ON fld.fieldsysid = ff.fieldsysid
        WHERE ff.formsysid = f.formsysid
    )
) AS FormJson
                    FROM forms f
                         INNER JOIN users cb
                            ON cb.userid = f.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = f.modifiedby
                   WHERE (LOWER (f.formname) LIKE LOWER (:searchvalue) || '%'
                          OR LOWER (f.formdescription) LIKE LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR f.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(1)
FROM forms frm
WHERE (LOWER (frm.formname) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (frm.formdescription) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR frm.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<FormExtended>(pagedQuery, parameters)).ToList();

                return new PagedResult<FormExtended>
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

        public override async Task<Form> GetAsync(string formsysid)
        {
            return await GetInternalAsync(formsysid);
        }
        public Form Get(string formsysid)
        {
            return GetInternalAsync(formsysid).GetAwaiter().GetResult();
        }


        private async Task<Form> GetInternalAsync(string formsysid)
        {
            return await _dataAccess.FindDataAsync<Form>("SELECT * FROM FORMS WHERE FORMSYSID = :FORMSYSID",
                new Form { FormSysId = formsysid });
        }

        private async Task<FormExtended> GetCompleteInfoInternalAsync(string formsysid)
        {
            var sql = @"SELECT
f.*,
       cb.firstname || ' ' || cb.lastname createdbyname,
       mb.firstname || ' ' || mb.lastname modifiedbyname,
  JSON_OBJECT(
    'id' VALUE f.formsysid,
    'name' VALUE f.formname,
    'description' VALUE f.formdescription,
    'fields' VALUE (
        SELECT JSON_ARRAYAGG(
            JSON_OBJECT(
                'id' VALUE ff.formfieldsysid,
                'fieldSysId' VALUE NVL(ff.fieldsysid, fld.fieldsysid),
                'useFieldDefaults' VALUE CASE
                    WHEN ff.fieldsysid IS NOT NULL AND ff.fieldname IS NULL AND ff.fieldtitle IS NULL AND ff.fieldtype IS NULL THEN 'true'
                    ELSE 'false'
                END,
                'name' VALUE COALESCE(ff.fieldname, fld.fieldname),
                'title' VALUE COALESCE(ff.fieldtitle, fld.fieldtitle),
                'type' VALUE COALESCE(ff.fieldtype, fld.fieldtype),
                'isActive' VALUE NVL2(ff.isactive, DECODE(ff.isactive, 1, 'true', 'false'), NVL2(fld.isactive, DECODE(fld.isactive, 1, 'true', 'false'), 'true')),
                'placeholder' VALUE COALESCE(ff.placeholder, fld.placeholder),
                'tooltip' VALUE COALESCE(ff.tooltip, fld.tooltip),
                'isrequired' VALUE NVL2(ff.isrequired,DECODE(ff.isrequired,1,'true','false'), 'false'), 
                'minLength' VALUE COALESCE(ff.minlength, fld.minlength),
                'maxLength' VALUE COALESCE(ff.maxlength, fld.maxlength),
                'caseOption' VALUE COALESCE(ff.caseoption, fld.caseoption),
                'fileTypes' VALUE COALESCE(ff.filetype, fld.filetype),
                'fileMaxSize' VALUE COALESCE(ff.filemaxsize, fld.filemaxsize),
                'urlIsParam' VALUE  NVL2(ff.urlIsParam, DECODE(ff.urlIsParam,1,'true','false'), NVL2(fld.urlIsParam, DECODE(fld.urlIsParam,1,'true','false'), null)),
                'urlDefaultPattern' VALUE COALESCE(ff.urlDefaultPattern, fld.urlDefaultPattern),
                'defaultValue' VALUE COALESCE(ff.defaultValue, fld.defaultValue),
                'defaultClobValue' VALUE COALESCE(ff.defaultClobValue, fld.defaultClobValue),
                'readAccess' VALUE NVL(ff.readaccess, '*'),
                'writeAccess' VALUE NVL(ff.writeaccess, '*'),
                'validate' VALUE COALESCE(ff.fieldvalidate, fld.fieldvalidate),
                'datasource' VALUE COALESCE(ff.datasource, fld.datasource),
                'datasourceParamField' VALUE COALESCE(ff.datasourceparamfield, fld.datasourceparamfield),
                'parentFieldId' VALUE COALESCE(ff.parentfieldsysid, fld.parentfieldsysid),
                'orderIndex' VALUE ff.orderindex,
                'options' VALUE (
                    SELECT COALESCE(JSON_ARRAYAGG(
                        JSON_OBJECT(
                            'id' VALUE opt.id,
                            'value' VALUE opt.optionvalue,
                            'label' VALUE opt.optionlabel,
                            'orderIndex' VALUE opt.orderindex
                        )
                    ), JSON_ARRAY())
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
                    SELECT COALESCE(JSON_ARRAYAGG(
                        JSON_OBJECT(
                            'id' VALUE rul.id,
                            'field' VALUE rul.rulefield,
                            'operator' VALUE rul.ruleoperator,
                            'value' VALUE rul.rulevalue,
                            'action' VALUE rul.ruleaction,
                            'actionValue' VALUE rul.ruleactionvalue
                        )
                    ), JSON_ARRAY())
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
                )
            )
            ORDER BY ff.orderindex
        )
        FROM FORMFIELDS ff
        LEFT JOIN FIELDS fld ON fld.fieldsysid = ff.fieldsysid
        WHERE ff.formsysid = f.formsysid
    )
) AS FormJson
FROM forms f        
INNER JOIN users cb
     ON cb.userid = f.createdby
LEFT OUTER JOIN users mb
          ON mb.userid = f.modifiedby
WHERE f.formsysid = :formsysid";


            return await _dataAccess.FindDataAsync<FormExtended>(sql, new FormExtended { FormSysId = formsysid });
        }


        public FormExtended GetCompleteInfo(string formsysid)
        {
            return GetCompleteInfoInternalAsync(formsysid).GetAwaiter().GetResult();
        }

        public async Task<FormExtended> GetCompleteInfoAsync(string formsysid)
        {
            return await GetCompleteInfoInternalAsync(formsysid);
        }


        
  

    }
}

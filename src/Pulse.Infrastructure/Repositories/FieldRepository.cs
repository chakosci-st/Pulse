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
    public class FieldRepository : BaseRepository<Field, string>, IFieldRepository
    {

        public FieldRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Field field)
        {
            //return await _dataAccess.SaveDataReturnParameterNameAsync<Field>(, field, "FieldSYSID");


            var sql = @"
INSERT INTO FieldS (
   FIELDSYSID, FIELDNAME, 
   FIELDTITLE, FIELDTYPE, PLACEHOLDER, 
   TOOLTIP,   MINLENGTH, 
   MAXLENGTH, CASEOPTION, FILETYPE, 
   FILEMAXSIZE,
   FIELDVALIDATE, DATASOURCE, DATASOURCEPARAMFIELD, 
   PARENTFIELDSYSID, DEFAULTPATTERN, URLISPARAM, URLDEFAULTPATTERN, 
    DEFAULTVALUE, DEFAULTCLOBVALUE, ISACTIVE,
  CREATEDBY) 
VALUES (:FIELDSYSID, :FIELDNAME, 
   :FIELDTITLE, :FIELDTYPE, :PLACEHOLDER, 
   :TOOLTIP,   :MINLENGTH, 
   :MAXLENGTH, :CASEOPTION, :FILETYPE, 
   :FILEMAXSIZE,   
   :FIELDVALIDATE, :DATASOURCE, :DATASOURCEPARAMFIELD, 
   :PARENTFIELDSYSID, :DEFAULTPATTERN, :URLISPARAM, :URLDEFAULTPATTERN, 
    :DEFAULTVALUE, :DEFAULTCLOBVALUE, :ISACTIVE,  :CREATEDBY) ";

            try
            {
                var rowsaffected = await _dataAccess.SaveDataAsync(sql, new
                {
                    FieldSysId = field.FieldSysId,
                    FieldName = field.FieldName,
                    FieldTitle = field.FieldTitle,
                    FieldType = field.FieldType,
                    Placeholder = field.Placeholder,
                    Tooltip = field.Tooltip, 
                    MinLength = field.MinLength,
                    MaxLength = field.MaxLength,
                    CaseOption = field.CaseOption,
                    FileType = field.FileType,
                    FileMaxSize = field.FileMaxSize,
                    FieldValidate = field.FieldValidate,
                    DataSource = field.DataSource,
                    DataSourceParamField = field.DataSourceParamField,
                    ParentFieldSysId = field.ParentFieldSysId,
                    UrlIsParam = field.UrlIsParam,
                    DefaultPattern = field.DefaultPattern,
                    UrlDefaultPattern = field.UrlDefaultPattern,
                    DefaultValue = (field.FieldType == "richtext" ? null : field.FieldType == "textarea" ? field.DefaultClobValue : field.DefaultValue),
                    DefaultClobValue = field.FieldType == "richtext" ? string.IsNullOrEmpty(field.DefaultClobValue) ? "" : field.DefaultClobValue as object : "",
                    IsActive = field.IsActive, 
                    CreatedBy = field.CreatedBy
                });

                return field.FieldSysId;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }


        }

        public override async Task<int> UpdateAsync(Field field)
        {
            var sql = @"
UPDATE FieldS 
SET FIELDNAME=:FIELDNAME, FIELDTITLE=:FIELDTITLE, FIELDTYPE=:FIELDTYPE, PLACEHOLDER=:PLACEHOLDER, 
   TOOLTIP=:TOOLTIP,  MINLENGTH=:MINLENGTH, 
   MAXLENGTH=:MAXLENGTH, CASEOPTION=:CASEOPTION, FILETYPE=:FILETYPE, 
   FILEMAXSIZE=:FILEMAXSIZE,  
   FIELDVALIDATE=:FIELDVALIDATE, DATASOURCE=:DATASOURCE, DATASOURCEPARAMFIELD=:DATASOURCEPARAMFIELD, 
   PARENTFIELDSYSID=:PARENTFIELDSYSID, DEFAULTPATTERN=:DEFAULTPATTERN, URLISPARAM=:URLISPARAM, URLDEFAULTPATTERN=:URLDEFAULTPATTERN, 
    DEFAULTVALUE=:DEFAULTVALUE, DEFAULTCLOBVALUE=:DEFAULTCLOBVALUE, ISACTIVE=:ISACTIVE,
   MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = :ModifiedDate, TRANSACTIONKEY = SYS_GUID() 
WHERE FIELDSYSID = :FIELDSYSID";


            return await _dataAccess.SaveDataAsync(sql, new
            {
                FieldSysId = field.FieldSysId,
                FieldName = field.FieldName,
                FieldTitle = field.FieldTitle,
                FieldType = field.FieldType,
                Placeholder = field.Placeholder,
                Tooltip = field.Tooltip, 
                MinLength = field.MinLength,
                MaxLength = field.MaxLength,
                CaseOption = field.CaseOption,
                FileType = field.FileType,
                FileMaxSize = field.FileMaxSize,
                FieldValidate = field.FieldValidate,
                DataSource = field.DataSource,
                DataSourceParamField = field.DataSourceParamField,
                ParentFieldSysId = field.ParentFieldSysId,
                DefaultPattern = field.DefaultPattern,
                UrlIsParam = field.UrlIsParam,
                UrlDefaultPattern = field.UrlDefaultPattern,
                DefaultValue = (field.FieldType == "richtext" ? null : field.FieldType == "textarea" ? field.DefaultClobValue : field.DefaultValue),
                DefaultClobValue = field.FieldType == "richtext" ? string.IsNullOrEmpty(field.DefaultClobValue) ? "" : field.DefaultClobValue as object : "",
                IsActive = field.IsActive, 
                ModifiedBy = field.ModifiedBy,
                ModifiedDate = field.ModifiedDate ?? DateTime.UtcNow
            });
        }

        public async Task<bool> IsReferencedAsync(string FieldSysId)
        {
            var sql = @"SELECT COUNT(1) FROM PROJECTFORMSUBMISSIONVALUES WHERE FIELDSYSID = :FIELDSYSID";
            var count = await _dataAccess.ExecuteScalarAsync<int>(sql, new { FieldSysId = FieldSysId });
            return count > 0;
        }

        public async Task<int> ChangeStatusAsync(string FieldSysId, int isActive, string modifiedBy)
        {
            var sql = @"
UPDATE FIELDS
SET ISACTIVE = :ISACTIVE,
    MODIFIEDBY = :MODIFIEDBY,
    MODIFIEDDATE = SYSTIMESTAMP,
    TRANSACTIONKEY = SYS_GUID()
WHERE FieldSYSID = :FieldSYSID";

            return await _dataAccess.SaveDataAsync(sql, new
            {
                FieldSysId = FieldSysId,
                IsActive = isActive,
                ModifiedBy = modifiedBy
            });
        }

        public async Task<ISet<string>> GetActiveFieldIdsByFormAsync(string formSysId)
        {
            var sql = @"SELECT FORMFIELDSYSID FROM FORMFIELDS lnk INNER JOIN FIELDS fld ON lnk.FIELDSYSID = fld.FIELDSYSID WHERE lnk.FORMSYSID = :FORMSYSID AND NVL(lnk.ISACTIVE, 1) = 1";
            var rows = await _dataAccess.LoadDataAsync<FormField>(sql, new FormField { FormSysId = formSysId });
            return new HashSet<string>(rows.Select(r => r.FormFieldSysId), StringComparer.OrdinalIgnoreCase);
        }

        public async override Task<int> DeleteAsync(string Fieldsysid)
        {
            return await _dataAccess.SaveDataAsync<Field>("DELETE FROM FIELDS WHERE FIELDSYSID = :FIELDSYSID", new Field { FieldSysId = Fieldsysid });
        }

        public async override Task<Field> GetAsync(string Fieldsysid)
        {
            return await _dataAccess.FindDataAsync<Field>("SELECT * FROM FIELDS  WHERE FIELDSYSID = :FIELDSYSID ",
                new Field { FieldSysId = Fieldsysid });
        }
        public async override Task<IEnumerable<Field>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Field>(@"SELECT * FROM FIELDS ")
              .ContinueWith(t => (IEnumerable<Field>)t.Result);
        }


        public async Task<PagedResult<FieldWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY {sortBy} {sortDirection}
    ) rn
            FROM (SELECT fld.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname,
                         (SELECT COUNT (1)
                            FROM formfields frm
                           WHERE frm.fieldsysid = fld.fieldsysid)
                            formlinkedcount 
                    FROM fields fld
                         INNER JOIN users cb
                            ON cb.userid = fld.createdby
                         LEFT OUTER JOIN users mb
                            ON mb.userid = fld.modifiedby
                   WHERE (LOWER (fld.fieldtitle) LIKE
                             LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR fld.ISACTIVE = :isactivestate)) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn 
";

            string countQuery = @"
SELECT COUNT(1)
FROM fields fld 
WHERE (LOWER (fld.fieldtitle) LIKE LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR fld.ISACTIVE = :isactivestate)
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

                var data = (await _dataAccess.QueryAsync<FieldWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<FieldWithStats>
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
        public Field Get(string fieldSysId)
        {
            return _dataAccess.FindData<Field>("SELECT * FROM FIELDS  WHERE FIELDSYSID = :FIELDSYSID ",
                new Field { FieldSysId = fieldSysId });
        }
    }
}

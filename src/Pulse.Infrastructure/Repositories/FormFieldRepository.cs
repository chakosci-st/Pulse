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
    public class FormFieldRepository : BaseRepository<FormField, string>, IFormFieldRepository
    {

        public FormFieldRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(FormField field)
        {
            //return await _dataAccess.SaveDataReturnParameterNameAsync<FormField>(, field, "FORMFIELDSYSID");


            var sql = @"
INSERT INTO FORMFIELDS (
    FORMSYSID, FIELDSYSID, FIELDNAME, 
   FIELDTITLE, FIELDTYPE, PLACEHOLDER, 
   TOOLTIP, ISREQUIRED, MINLENGTH, 
   MAXLENGTH, CASEOPTION, FILETYPE, 
   FILEMAXSIZE, READACCESS, WRITEACCESS, 
   FIELDVALIDATE, DATASOURCE, DATASOURCEPARAMFIELD, 
   PARENTFIELDSYSID, DEFAULTPATTERN, URLISPARAM, URLDEFAULTPATTERN, 
    DEFAULTVALUE, DEFAULTCLOBVALUE, ISACTIVE,
ORDERINDEX, CREATEDBY) 
VALUES (:FORMSYSID, :FIELDSYSID, :FIELDNAME, 
   :FIELDTITLE, :FIELDTYPE, :PLACEHOLDER, 
   :TOOLTIP, :ISREQUIRED, :MINLENGTH, 
   :MAXLENGTH, :CASEOPTION, :FILETYPE, 
   :FILEMAXSIZE, :READACCESS, :WRITEACCESS, 
   :FIELDVALIDATE, :DATASOURCE, :DATASOURCEPARAMFIELD, 
   :PARENTFIELDSYSID, :DEFAULTPATTERN, :URLISPARAM, :URLDEFAULTPATTERN, 
    :DEFAULTVALUE, :DEFAULTCLOBVALUE, :ISACTIVE, :ORDERINDEX, :CREATEDBY)
RETURNING FORMFIELDSYSID INTO :FORMFIELDSYSID";

            try {
                return await _dataAccess.SaveDataWithClobReturnParameterNameAsync(sql, new
                {
                    FormSysId = field.FormSysId,
                    FieldSysId = string.IsNullOrWhiteSpace(field.FieldSysId) ? null : field.FieldSysId,
                    FieldName = field.UseFieldDefaults ? null : field.FieldName,
                    FieldTitle = field.UseFieldDefaults ? null : field.FieldTitle,
                    FieldType = field.UseFieldDefaults ? null : field.FieldType,
                    Placeholder = field.UseFieldDefaults ? null : field.Placeholder,
                    Tooltip = field.UseFieldDefaults ? null : field.Tooltip,
                    IsRequired = field.IsRequired,
                    MinLength = field.UseFieldDefaults ? (int?)null : field.MinLength,
                    MaxLength = field.UseFieldDefaults ? (int?)null : field.MaxLength,
                    CaseOption = field.UseFieldDefaults ? null : field.CaseOption,
                    FileType = field.UseFieldDefaults ? null : field.FileType,
                    FileMaxSize = field.UseFieldDefaults ? (double?)null : field.FileMaxSize,
                    ReadAccess = string.IsNullOrWhiteSpace(field.ReadAccess) ? "*" : field.ReadAccess,
                    WriteAccess = string.IsNullOrWhiteSpace(field.WriteAccess) ? "*" : field.WriteAccess,
                    FieldValidate = field.UseFieldDefaults ? null : field.FieldValidate,
                    DataSource = field.UseFieldDefaults ? null : field.DataSource,
                    DataSourceParamField = field.UseFieldDefaults ? null : field.DataSourceParamField,
                    ParentFieldSysId = field.UseFieldDefaults ? null : field.ParentFieldSysId,
                    UrlIsParam = field.UseFieldDefaults ? (int?)null : field.UrlIsParam,
                    DefaultPattern = field.UseFieldDefaults ? null : field.DefaultPattern,
                    UrlDefaultPattern = field.UseFieldDefaults ? null : field.UrlDefaultPattern,
                    DefaultValue = field.UseFieldDefaults ? null : (field.FieldType == "richtext" ? null : field.FieldType == "textarea" ? field.DefaultClobValue : field.DefaultValue),
                    DefaultClobValue = field.UseFieldDefaults ? null : (field.FieldType == "richtext" ? (string.IsNullOrEmpty(field.DefaultClobValue) ? "" : field.DefaultClobValue) : ""),
                    IsActive = field.IsActive,
                    OrderIndex = field.OrderIndex,
                    CreatedBy = field.CreatedBy
                }, "FORMFIELDSYSID", "DEFAULTCLOBVALUE");
            } catch (Exception e) {
                throw new Exception(e.Message);
            }




        }

        public override async Task<int> UpdateAsync(FormField field)
        {
            var sql = @"
UPDATE FORMFIELDS 
SET FIELDSYSID=:FIELDSYSID, FIELDNAME=:FIELDNAME, FIELDTITLE=:FIELDTITLE, FIELDTYPE=:FIELDTYPE, PLACEHOLDER=:PLACEHOLDER, 
   TOOLTIP=:TOOLTIP, ISREQUIRED=:ISREQUIRED, MINLENGTH=:MINLENGTH, 
   MAXLENGTH=:MAXLENGTH, CASEOPTION=:CASEOPTION, FILETYPE=:FILETYPE, 
   FILEMAXSIZE=:FILEMAXSIZE, READACCESS=:READACCESS, WRITEACCESS=:WRITEACCESS, 
   FIELDVALIDATE=:FIELDVALIDATE, DATASOURCE=:DATASOURCE, DATASOURCEPARAMFIELD=:DATASOURCEPARAMFIELD, ORDERINDEX=:ORDERINDEX,
   PARENTFIELDSYSID=:PARENTFIELDSYSID, DEFAULTPATTERN=:DEFAULTPATTERN, URLISPARAM=:URLISPARAM, URLDEFAULTPATTERN=:URLDEFAULTPATTERN, 
    DEFAULTVALUE=:DEFAULTVALUE, DEFAULTCLOBVALUE=:DEFAULTCLOBVALUE, ISACTIVE=:ISACTIVE,
   MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE FORMFIELDSYSID = :FORMFIELDSYSID";


              return await _dataAccess.SaveDataAsync(sql, new
              {
                  FormFieldSysId = field.FormFieldSysId,
                  FormSysId = field.FormSysId,
                  FieldSysId = string.IsNullOrWhiteSpace(field.FieldSysId) ? null : field.FieldSysId,
                  FieldName = field.UseFieldDefaults ? null : field.FieldName,
                  FieldTitle = field.UseFieldDefaults ? null : field.FieldTitle,
                  FieldType = field.UseFieldDefaults ? null : field.FieldType,
                  Placeholder = field.UseFieldDefaults ? null : field.Placeholder,
                  Tooltip = field.UseFieldDefaults ? null : field.Tooltip,
                  IsRequired = field.IsRequired,
                  MinLength = field.UseFieldDefaults ? (int?)null : field.MinLength,
                  MaxLength = field.UseFieldDefaults ? (int?)null : field.MaxLength,
                  CaseOption = field.UseFieldDefaults ? null : field.CaseOption,
                  FileType = field.UseFieldDefaults ? null : field.FileType,
                  FileMaxSize = field.UseFieldDefaults ? (double?)null : field.FileMaxSize,
                  ReadAccess = string.IsNullOrWhiteSpace(field.ReadAccess) ? "*" : field.ReadAccess,
                  WriteAccess = string.IsNullOrWhiteSpace(field.WriteAccess) ? "*" : field.WriteAccess,
                  FieldValidate = field.UseFieldDefaults ? null : field.FieldValidate,
                  DataSource = field.UseFieldDefaults ? null : field.DataSource,
                  DataSourceParamField = field.UseFieldDefaults ? null : field.DataSourceParamField,
                  ParentFieldSysId = field.UseFieldDefaults ? null : field.ParentFieldSysId,
                  DefaultPattern = field.UseFieldDefaults ? null : field.DefaultPattern,
                  UrlIsParam = field.UseFieldDefaults ? (int?)null : field.UrlIsParam,
                  UrlDefaultPattern = field.UseFieldDefaults ? null : field.UrlDefaultPattern,
                  DefaultValue = field.UseFieldDefaults ? null : (field.FieldType == "richtext" ? null : field.FieldType == "textarea" ? field.DefaultClobValue : field.DefaultValue),
                  DefaultClobValue = field.UseFieldDefaults ? null : (field.FieldType == "richtext" ? (string.IsNullOrEmpty(field.DefaultClobValue) ? "" : field.DefaultClobValue) : ""),
                  IsActive = field.IsActive,
                  OrderIndex = field.OrderIndex,
                  ModifiedBy = field.ModifiedBy,
                  ModifiedDate = field.ModifiedDate
              }, "DEFAULTCLOBVALUE");
        }

        public async Task<bool> IsReferencedAsync(string formFieldSysId)
        {
            var sql = @"SELECT COUNT(1) FROM PROJECTFORMSUBMISSIONVALUES WHERE FORMFIELDSYSID = :FORMFIELDSYSID";
            var count = await _dataAccess.ExecuteScalarAsync<int>(sql, new { FormFieldSysId = formFieldSysId });
            return count > 0;
        }

        public async Task<int> ChangeStatusAsync(string formFieldSysId, int isActive, string modifiedBy)
        {
            var sql = @"
UPDATE FORMFIELDS
SET ISACTIVE = :ISACTIVE,
    MODIFIEDBY = :MODIFIEDBY,
    MODIFIEDDATE = SYSTIMESTAMP,
    TRANSACTIONKEY = SYS_GUID()
WHERE FORMFIELDSYSID = :FORMFIELDSYSID";

            return await _dataAccess.SaveDataAsync(sql, new
            {
                FormFieldSysId = formFieldSysId,
                IsActive = isActive,
                ModifiedBy = modifiedBy
            });
        }

        public async Task<ISet<string>> GetActiveFieldIdsByFormAsync(string formSysId)
        {
            var sql = @"SELECT FORMFIELDSYSID FROM FORMFIELDS WHERE FORMSYSID = :FORMSYSID AND NVL(ISACTIVE, 1) = 1";
            var rows = await _dataAccess.LoadDataAsync<FormField>(sql, new FormField { FormSysId = formSysId });
            return new HashSet<string>(rows.Select(r => r.FormFieldSysId), StringComparer.OrdinalIgnoreCase);
        }

        public async override Task<int> DeleteAsync(string formfieldsysid)
        {
            return await _dataAccess.SaveDataAsync<FormField>("DELETE FROM FORMFIELDS WHERE FORMFIELDSYSID = :FORMFIELDSYSID", new FormField { FormFieldSysId = formfieldsysid });
        }

        public async override Task<FormField> GetAsync(string formfieldsysid)
        {
            return await _dataAccess.FindDataAsync<FormField>("SELECT * FROM FORMFIELDS  WHERE FORMFIELDSYSID = :FORMFIELDSYSID ",
                new FormField { FormFieldSysId = formfieldsysid });
        }
        public async override Task<IEnumerable<FormField>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<FormField>(@"SELECT * FROM FORMFIELDS ")
              .ContinueWith(t => (IEnumerable<FormField>)t.Result);
        }


    }
}

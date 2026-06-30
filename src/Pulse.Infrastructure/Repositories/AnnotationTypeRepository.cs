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
    public class AnnotationTypeRepository : BaseRepository<AnnotationType, int>, IAnnotationTypeRepository
    {

        public AnnotationTypeRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<int> AddAsync(AnnotationType annotationtype)
        {
            return  await _dataAccess.SaveDataReturnIdAsync <AnnotationType>(@"INSERT INTO ANNOTATIONTYPES (ANNOTATIONTYPENAME, ANNOTATIONTYPEDESC, ANNOTATIONTYPEOPTIONS, ISPRIVATE, ISACTIVE, CREATEDBY) 
                        VALUES (:ANNOTATIONTYPENAME, :ANNOTATIONTYPEDESC, :ANNOTATIONTYPEOPTIONS, :ISPRIVATE, 1, :CREATEDBY) RETURNING ANNOTATIONTYPEID INTO :ANNOTATIONTYPEID ", annotationtype, "ANNOTATIONTYPEID");
        }

        public override async Task<int> UpdateAsync(AnnotationType annotationtype)
        {
            return await _dataAccess.SaveDataAsync<AnnotationType>(@"UPDATE ANNOTATIONTYPES SET ANNOTATIONTYPENAME = :ANNOTATIONTYPENAME, ANNOTATIONTYPEDESC = :ANNOTATIONTYPEDESC, 
                        ANNOTATIONTYPEOPTIONS = :ANNOTATIONTYPEOPTIONS, ISPRIVATE = :ISPRIVATE, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE ANNOTATIONTYPEID = :ANNOTATIONTYPEID AND TRANSACTIONKEY = :TRANSACTIONKEY", annotationtype);
        }

        public override async Task<int> DeleteAsync(int annotationtypeid)
        {

            return await _dataAccess.SaveDataAsync<AnnotationType>("DELETE FROM ANNOTATIONTYPES WHERE ANNOTATIONTYPEID = :ANNOTATIONTYPEID", new AnnotationType { AnnotationTypeId = annotationtypeid });
        }

        public override async Task<IEnumerable<AnnotationType>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<AnnotationType>("SELECT * FROM ANNOTATIONTYPES")
                .ContinueWith(t => (IEnumerable<AnnotationType>)t.Result);
        }

        public override async Task<AnnotationType> GetAsync(int annotationtypeid)
        {
            return await _dataAccess.FindDataAsync<AnnotationType>("SELECT * FROM ANNOTATIONTYPES WHERE ANNOTATIONTYPEID = :ANNOTATIONTYPEID", new AnnotationType { AnnotationTypeId = annotationtypeid });
        }


    }
}

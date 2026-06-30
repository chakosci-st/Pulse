using log4net;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Repositories
{
    public class ProductionCalendarRepository : BaseRepository<ProductionCalendar, DateTime>, IProductionCalendarRepository
    {

        public ProductionCalendarRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }



        public async override Task<DateTime> AddAsync(ProductionCalendar entity)
        {
            var rowsaffected = await _dataAccess.SaveDataAsync<ProductionCalendar>(@"INSERT INTO PRODUCTIONCALENDARS (FISCALDATE, CALENDARYEAR, CALENDARQUARTER, CALENDARMONTH, CALENDARWORKWEEK, CREATEDBY) 
VALUES (:FISCALDATE, :CALENDARYEAR, :CALENDARQUARTER, :CALENDARMONTH, :CALENDARWORKWEEK, :CREATEDBY)", entity);


            return entity.FiscalDate;
        }

        public async override Task<int> DeleteAsync(DateTime year)
        {
            var calendaryear = year.Year.ToString();
            return await _dataAccess.SaveDataAsync<ProductionCalendar>("DELETE FROM PRODUCTIONCALENDARS WHERE CALENDARYEAR = :CALENDARYEAR", new ProductionCalendar { CalendarYear = calendaryear });
        }

        public async Task<IEnumerable<ProductionCalendar>> GetByYearAsync(int year)
        {
            var calendaryear = year.ToString();
            return await _dataAccess.LoadDataAsync<ProductionCalendar>("SELECT * FROM PRODUCTIONCALENDARS WHERE CALENDARYEAR = :CALENDARYEAR", new ProductionCalendar { CalendarYear = calendaryear });
        }

        public async Task<IEnumerable<ProductionCalendar>> GetByYearMonthAsync(int year, int month)
        {
            var calendaryear = year.ToString();
            var calendarmonth = NumberHelper.ZeroFill(month, 2);
            return await _dataAccess.LoadDataAsync<ProductionCalendar>("SELECT * FROM PRODUCTIONCALENDARS WHERE CALENDARYEAR = :CALENDARYEAR AND CALENDARMONTH = :CALENDARMONTH", new ProductionCalendar { CalendarYear = calendaryear, CalendarMonth = calendarmonth });
        }

        public async Task<IEnumerable<ProductionCalendar>> GetByYearQuarterAsync(int year, int quarter)
        {
            var calendaryear = year.ToString();
            var calendarquarter = NumberHelper.ZeroFill(quarter, 2);
            return await _dataAccess.LoadDataAsync<ProductionCalendar>("SELECT * FROM PRODUCTIONCALENDARS WHERE CALENDARYEAR = :CALENDARYEAR AND CALENDARQUARTER = :CALENDARQUARTER", new ProductionCalendar { CalendarYear = calendaryear, CalendarQuarter = calendarquarter });
        }

        public async Task<IEnumerable<ProductionCalendar>> GetByYearWorkWeekAsync(int year, int workweek)
        {
            var calendaryear = year.ToString();
            var calendarworkweek = NumberHelper.ZeroFill(workweek, 2);
            return await _dataAccess.LoadDataAsync<ProductionCalendar>("SELECT * FROM PRODUCTIONCALENDARS WHERE CALENDARYEAR = :CALENDARYEAR AND CALENDARWORKWEEK = :CALENDARWORKWEEK", new ProductionCalendar { CalendarYear = calendaryear, CalendarWorkWeek = calendarworkweek });
        }

        public async override Task<int> UpdateAsync(ProductionCalendar entity)
        {
            return await _dataAccess.SaveDataAsync<ProductionCalendar>("UPDATE PRODUCTIONCALENDARS SET CALENDARYEAR = :CALENDARYEAR, CALENDARQUARTER = :CALENDARQUARTER, CALENDARMONTH = :CALENDARMONTH, CALENDARWORKWEEK = :CALENDARWORKWEEK, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE FISCALDATE = :FISCALDATE AND TRANSACTIONKEY = :TRANSACTIONKEY ", entity);
        }
        public async override Task<ProductionCalendar> GetAsync(DateTime fiscaldate)
        {
            return await _dataAccess.FindDataAsync<ProductionCalendar>("SELECT * FROM PRODUCTIONCALENDARS WHERE TRUNC(FISCALDATE) = TRUNC(:FISCALDATE)", new ProductionCalendar { FiscalDate = fiscaldate });
        }

        public async override Task<IEnumerable<ProductionCalendar>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ProductionCalendar>(@"SELECT * FROM PRODUCTIONCALENDARS ")
              .ContinueWith(t => (IEnumerable<ProductionCalendar>)t.Result);
        }


        public async Task<PagedResult<ProductionCalendarWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
    FROM (SELECT l.*, ROW_NUMBER () OVER (ORDER BY  {sortBy} {sortDirection}) rn
            FROM (SELECT DISTINCT pc.calendaryear,
                  MIN (pc.createddate) createddate,
                  MAX (pc.modifieddate) modifieddate,
                  cb.firstname || ' ' || cb.lastname createdbyname,
                  mb.firstname || ' ' || mb.lastname modifiedbyname
    FROM productioncalendars pc
         INNER JOIN users cb
            ON cb.userid = pc.createdby
         LEFT OUTER JOIN users mb
            ON mb.userid = pc.modifiedby
                   WHERE (pc.calendaryear LIKE :searchvalue || '%')
GROUP BY pc.calendaryear, cb.firstname || ' ' || cb.lastname, mb.firstname || ' ' || mb.lastname
) l)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(DISTINCT pc.calendaryear)
FROM productioncalendars pc
WHERE (pc.calendaryear LIKE :searchvalue || '%')
";


            var parameters = new
            {
                searchvalue = searchValue,
                offset = (pageNumber - 1) * pageSize,
                pagesize = pageSize
            };
            try
            {
                // Use Dapper's QueryAsync for mapping
                int totalRecords = await _dataAccess.ExecuteScalarAsync<int>(countQuery, parameters);

                var data = (await _dataAccess.QueryAsync<ProductionCalendarWithStats>(pagedQuery, parameters)).ToList();

                return new PagedResult<ProductionCalendarWithStats>
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

        public async Task<IEnumerable<WorkWeekCalendar>> GetWorkWeekAsync()
        {
            return await _dataAccess.LoadDataAsync<WorkWeekCalendar>(@"SELECT calendaryear || calendarworkweek workweek,
         MIN (fiscaldate) fiscaldatestart,
         MAX (fiscaldate) fiscaldateend
    FROM PRODUCTIONCALENDARS
GROUP BY calendaryear || calendarworkweek
ORDER BY calendaryear || calendarworkweek ")
                .ContinueWith(t => (IEnumerable<WorkWeekCalendar>)t.Result);
        }
    }
}

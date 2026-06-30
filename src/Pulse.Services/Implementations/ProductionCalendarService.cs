using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class ProductionCalendarService : IProductionCalendarService
    {
        private readonly IProductionCalendarRepository _productioncalendarRepository;

        public ProductionCalendarService(IProductionCalendarRepository productioncalendarRepository)
        {
            _productioncalendarRepository = productioncalendarRepository;
        }

        public async Task<int> DeleteProductionCalendarAsync(int year, string userid)
        {

            var calendar = DateTime.Parse("01-01-" + year);
            try
            {

                //DELETE RECORD
                var rowsaffected = await _productioncalendarRepository.DeleteAsync(calendar);


                // Publish the ProductUpdatedEvent
                //var plantDeletedEvent = new PlantDeletedEvent
                //{
                //    PlantCode = plant.PlantCode,
                //    ActionBy = plant.CreatedBy
                //};


                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(plantDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<int> GenerateProductionCalendarAsync(int year, DateTime week1end, int januaryWorkWeeks, string userid)
        {

            var calendar = DateTime.Parse("01-01-" + year);
            try
            {
                //CLEAR CALENDAR TABLE
                await _productioncalendarRepository.DeleteAsync(calendar);

                //SET INITIAL PARAMETERS
                DateTime fiscaldate = DateTime.Parse($"{year}-01-01");
                DateTime fiscaldateend = DateTime.Parse($"{year}-12-31");
                int workweek = 1;
                int workweekend = 52;

                int monthdate = 1;
                int[] noofweeksmovement = januaryWorkWeeks == 5
                    ? new[] { 5, 4, 4, 4, 4, 5, 4, 4, 5, 4, 4, 5 }
                    : new[] { 4, 4, 5, 4, 4, 5, 4, 4, 5, 4, 4, 5 };
                int currentMonthWeekCount = 1;

                do
                {

                    // Start a new workweek every Sunday after the selected week 1 end date.
                    if (fiscaldate.DayOfWeek == DayOfWeek.Sunday && (fiscaldate > week1end))
                    {
                        if (workweek < 51)
                        {
                            workweek++;
                        }
                        else
                        {
                            workweek = 52;
                        }

                        if (currentMonthWeekCount >= noofweeksmovement[Math.Min(monthdate - 1, noofweeksmovement.Length - 1)])
                        {
                            monthdate = Math.Min(monthdate + 1, 12);
                            currentMonthWeekCount = 1;
                        }
                        else
                        {
                            currentMonthWeekCount++;
                        }
                    }

                    int quarterdate = ((monthdate - 1) / 3) + 1;

                    // Create a ProductionCalendar object for the current day 
                    var calendardate = new ProductionCalendar
                    {
                        CalendarYear = year.ToString(),
                        CalendarQuarter = NumberHelper.ZeroFill(quarterdate, 2),
                        CalendarMonth = NumberHelper.ZeroFill(monthdate, 2),
                        CalendarWorkWeek = NumberHelper.ZeroFill(workweek, 2),
                        FiscalDate = fiscaldate,
                        CreatedBy = userid
                    };

                    await _productioncalendarRepository.AddAsync(calendardate);


                    fiscaldate = fiscaldate.AddDays(1);


                } while (fiscaldate <= fiscaldateend);




                return 1;
            }
            catch (Exception ex)
            {



                return 0;
            }
        }

        public async Task<ProductionCalendar> GetProductionCalendarByFiscalDateAsync(DateTime fiscaldate)
        {
            return await _productioncalendarRepository.GetAsync(fiscaldate);
        }

        public async Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearAsync(int year)
        {
            return await _productioncalendarRepository.GetByYearAsync(year);
        }

        public async Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearMonthAsync(int year, int month)
        {
            return await _productioncalendarRepository.GetByYearMonthAsync(year, month);
        }

        public async Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearQuarterAsync(int year, int quarter)
        {
            return await _productioncalendarRepository.GetByYearQuarterAsync(year, quarter);
        }

        public async Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearWorkWeekAsync(int year, int workweek)
        {
            return await _productioncalendarRepository.GetByYearWorkWeekAsync(year, workweek);
        }

        public async Task<int> UpdateFiscalDateAsync(ProductionCalendar calendardate)
        {
            return await _productioncalendarRepository.UpdateAsync(calendardate);
        }

        public async Task<PagedResult<ProductionCalendarWithStats>> GetPagedProductionCalendarsAsync(string searchValue, string sortBy, string sortDirection, int pageNumber, int pageSize)
        {
            return await _productioncalendarRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, pageNumber, pageSize);
        }

        public async Task<IEnumerable<WorkWeekCalendar>> GetWorkWeekAsync()
        {
            return await _productioncalendarRepository.GetWorkWeekAsync();
        }
    }
}

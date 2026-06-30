using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for Production Calendar-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProductionCalendarService
    {
        /// <summary>
        /// Retrieves all Production Calendar by year from the system.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <returns>A list of all production calendar per year.</returns>
        Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearAsync(int year);

        /// <summary>
        /// Retrieves all Production Calendar by year - quarter from the system.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <param name="quarter">The unique identifier of the production calendar.</param>
        /// <returns>A list of all production calendar per year - quarter.</returns>
        Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearQuarterAsync(int year, int quarter);

        /// <summary>
        /// Retrieves all Production Calendar by year - month from the system.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <param name="month">The unique identifier of the production calendar.</param>
        /// <returns>A list of all production calendar per year - month.</returns>
        Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearMonthAsync(int year, int month);

        /// <summary>
        /// Retrieves all Production Calendar by year - workweek from the system.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <param name="workweek">The unique identifier of the production calendar.</param>
        /// <returns>A list of all production calendar per year - workweek.</returns>
        Task<IEnumerable<ProductionCalendar>> GetProductionCalendarByYearWorkWeekAsync(int year, int workweek);

        /// <summary>
        /// Retrieves Production Calendar by fiscal date from the system.
        /// </summary>
        /// <param name="fiscaldate">The unique identifier of the production calendar.</param>
        /// <returns>A list of all production calendar per year.</returns>
        Task<ProductionCalendar> GetProductionCalendarByFiscalDateAsync(DateTime fiscaldate);

        /// <summary>
        /// Adds a new production calendar to the system.
        /// </summary>
        /// <param name="year">The year to add.</param>
        Task<int> GenerateProductionCalendarAsync(int year, DateTime week1end, int januaryWorkWeeks, string userid);

        /// <summary>
        /// Updates an existing production calendar date in the system.
        /// </summary>
        /// <param name="calendardate">The calendardate to update.</param>
        Task<int> UpdateFiscalDateAsync(ProductionCalendar calendardate);

        /// <summary>
        /// Deletes a production calendar by year from the system by its unique identifier.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar to delete.</param>
        /// <param name="userid">The user who deleted the production calendar.</param>
        Task<int> DeleteProductionCalendarAsync(int year, string userid);


        /// <summary>
        /// Retrieves all ProductionCalendars from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The plant status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all ProductionCalendars.</returns>
        Task<PagedResult<ProductionCalendarWithStats>> GetPagedProductionCalendarsAsync(string searchValue, string sortBy, string sortDirection, int pageNumber, int pageSize);

        Task<IEnumerable<WorkWeekCalendar>> GetWorkWeekAsync();
    }
}

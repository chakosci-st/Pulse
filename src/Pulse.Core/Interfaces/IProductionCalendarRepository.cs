using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing production calendar in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProductionCalendarRepository : IBaseRepository<ProductionCalendar, DateTime>
    {
        /// <summary>
        /// Retrieves all production calendar by year from the repository.
        /// </summary>
        /// <param name="year">The unique identifier of the category.</param>
        /// <returns>A list of all plants.</returns>
        Task<IEnumerable<ProductionCalendar>> GetByYearAsync(int year);

        /// <summary>
        /// Retrieves production calendar by year-quarter from the repository.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <param name="quarter">The unique identifier of the production calendar.</param>
        /// <returns>A list of production calendar.</returns>
        Task<IEnumerable<ProductionCalendar>> GetByYearQuarterAsync(int year, int quarter);

        /// <summary>
        /// Retrieves production calendar by year-month from the repository.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <param name="month">The unique identifier of the production calendar.</param>
        /// <returns>A list of production calendar.</returns>
        Task<IEnumerable<ProductionCalendar>> GetByYearMonthAsync(int year, int month);

        /// <summary>
        /// Retrieves production calendar by year-workweek from the repository.
        /// </summary>
        /// <param name="year">The unique identifier of the production calendar.</param>
        /// <param name="workweek">The unique identifier of the production calendar.</param>
        /// <returns>A list of production calendar.</returns>
        Task<IEnumerable<ProductionCalendar>> GetByYearWorkWeekAsync(int year, int workweek);


        Task<IEnumerable<WorkWeekCalendar>> GetWorkWeekAsync();

        Task<PagedResult<ProductionCalendarWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection,  int pageNumber, int pageSize);

    }
}

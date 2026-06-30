using AutoMapper;
using Pulse.Api.Filters;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using Pulse.SharedUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    //[Authorize]
    [RoutePrefix("api/productioncalendars")]
    public class ProductionCalendarsController : ApiController
    {
        private readonly IProductionCalendarService _productioncalendarService;

        public ProductionCalendarsController(IProductionCalendarService productioncalendarService)
        {
            _productioncalendarService = productioncalendarService;
        }



        /// <summary>
        /// Gets a paged list of productioncalendars with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for productioncalendar code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "CALNDRVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetProductionCalendarsForDataTables([FromBody] DataTablesRequest request)
        {

            int pageNumber = 1;
            int pageSize = request.length;

            // If length == -1, fetch all rows
            if (request.length == -1)
            {
                pageSize = int.MaxValue; // or a large number, or remove paging logic
            }
            else
            {
                pageNumber = (request.start / request.length) + 1;
            }

            string searchValue = request.search?.value ?? "";
            bool? isActive = request.isActive;

            // Whitelist of allowed columns and directions
            var allowedColumns = new HashSet<string> { "CALENDARYEAR" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "CALENDARYEAR";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "CALENDARYEAR"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction

            var pagedResult = await _productioncalendarService.GetPagedProductionCalendarsAsync(searchValue, sortBy, sortDir, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoProductionCalendarWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoProductionCalendarWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a productioncalendar by year.
        ///// </summary>
        [HttpGet]
        [Authorize]
       // [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "CALNDRVIEW")]
        [Route("{year}")]
        public async Task<IHttpActionResult> GetByYear(int year)
        {
            var productioncalendar = (await _productioncalendarService.GetProductionCalendarByYearAsync(year)).Select(Mapper.Map<dtoProductionCalendar>);
            if (productioncalendar == null)
                return NotFound();

            var response = new DataTablesResponse<dtoProductionCalendar>
            { 
                data = productioncalendar.ToList() 
            };
            return Ok(response);

        }

        /// <summary>
        /// Creates a new production calendar.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "CALNDRGEN")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the productioncalendar data (assuming the input name is 'productioncalendar')
            var productioncalendarContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "productioncalendar");
            dtoProductionCalendarCreate productioncalendar = null;
            if (productioncalendarContent != null)
            {
                var productioncalendarJson = await productioncalendarContent.ReadAsStringAsync();
                productioncalendar = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoProductionCalendarCreate>(productioncalendarJson);
            }

            if (productioncalendar == null)
                return BadRequest("Product Division data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            productioncalendar.CreatedBy = User.Identity.GetClaim("employeeid");


            await _productioncalendarService.GenerateProductionCalendarAsync(int.Parse(productioncalendar.CalendarYear), productioncalendar.Week1End, productioncalendar.JanuaryWorkWeeks, productioncalendar.CreatedBy);

            return Created($"api/productioncalendars/{productioncalendar.CalendarYear}", productioncalendar);
        }

        ///// <summary>
        ///// Gets a productioncalendar by year.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "CALNDRVIEW")]
        [Route("workweek")]
        public async Task<IHttpActionResult> GetWorkWeeks(int year)
        {
            var obj = (await _productioncalendarService.GetWorkWeekAsync());
            if (obj == null)
                return NotFound();

            var response = new DataTablesResponse<WorkWeekCalendar>
            {
                data = obj.ToList()
            };
            return Ok(response);

        }
    }

}

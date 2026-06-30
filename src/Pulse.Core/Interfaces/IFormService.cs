using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for Form-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IFormService
    {
        /// <summary>
        /// Retrieves all forms from the system.
        /// </summary>
        /// <returns>A list of all forms.</returns>
        Task<IEnumerable<Form>> GetAllFormsAsync();

        /// <summary>
        /// Retrieves a form by its unique identifier.
        /// </summary>
        /// <param name="formsysid">The unique identifier of the form.</param>
        /// <returns>The form with the specified formsysid, or null if not found.</returns>
        Task<Form> GetFormByIdAsync(string formsysid);

        /// <summary>
        /// Retrieves a form by its unique identifier.
        /// </summary>
        /// <param name="formsysid">The unique identifier of the form.</param>
        /// <returns>The form with the specified formsysid, or null if not found.</returns>
        Form GetFormById(string formsysid);


        /// <summary>
        /// Retrieves a form by its unique identifier.
        /// </summary>
        /// <param name="formsysid">The unique identifier of the form.</param>
        /// <returns>The form with the specified formsysid, or null if not found.</returns>
        Task<FormExtended> GetCompleteInfoFormByIdAsync(string formsysid);





        /// <summary>
        /// Retrieves all form from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The form status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all productdivisions.</returns>
        Task<PagedResult<FormExtended>> GetPagedFormsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);



        /// <summary>
        /// Adds a new form to the system.
        /// </summary>
        /// <param name="form">The form to add.</param>
        /// <returns>Rows affected.</returns>
        Task<string> BuildFormAsync(Form form, string loggeduser);

        /// <summary>
        /// Updates an existing form in the system.
        /// </summary>
        /// <param name="form">The form to update.</param> 
        System.Threading.Tasks.Task RebuildFormAsync(Form form, string transactionkey, string loggeduser);


        /// <summary>
        /// Updates an existing form (basic info only) in the system.
        /// </summary>
        /// <param name="form">The form to update.</param> 
        System.Threading.Tasks.Task UpdateAsync(Form form, string transactionkey, string loggeduser);


        /// <summary>
        /// Change status of an existing form (basic info only) in the system.
        /// </summary>
        /// <param name="form">The form to update.</param> 
        System.Threading.Tasks.Task ChangeStatusAsync(Form form, string transactionkey, string loggeduser);

        /// <summary>
        /// Change status of an existing form field in the system.
        /// </summary>
        /// <param name="formFieldSysId">The field id to update.</param>
        /// <param name="isActive">Target status.</param>
        /// <param name="loggeduser">User making the change.</param>
        System.Threading.Tasks.Task ChangeFieldStatusAsync(string formFieldSysId, bool isActive, string loggeduser);

        /// <summary>
        /// Delete an existing form (basic info only) in the system.
        /// </summary>
        /// <param name="formsysid">The form to delete.</param> 
        /// <param name="userid">The user deletes the form.</param> 
        Task<int> DeleteFormAsync(string formsysid, string userid);
    }
}

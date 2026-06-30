using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IAnnotationTypeService
    {
        /// <summary>
        /// Retrieves all annotation types from the system.
        /// </summary>
        /// <returns>A list of all annotation types.</returns>
        Task<IEnumerable<AnnotationType>> GetAllAnnotationTypesAsync();

        /// <summary>
        /// Retrieves a annotation type by its unique identifier.
        /// </summary>
        /// <param name="annotationtypeid">The unique identifier of the annotation type.</param>
        /// <returns>The annotation type with the specified ANNOTATIONTYPEID, or null if not found.</returns>
        Task<AnnotationType> GetAnnotationTypeByIdAsync(int annotationtypeid);

        /// <summary>
        /// Adds a new annotation type to the system.
        /// </summary>
        /// <param name="annotationtype">The annotation type to add.</param>
       Task<int> AddAnnotationTypeAsync(AnnotationType annotationtype);

        /// <summary>
        /// Updates an existing annotation type in the system.
        /// </summary>
        /// <param name="annotationtype">The annotation type to update.</param>
       Task<int> UpdateAnnotationTypeAsync(AnnotationType annotationtype);

        /// <summary>
        /// Deletes annotation type from the system by its unique identifier.
        /// </summary>
        /// <param name="annotationtypeid">The unique identifier of the annotation type to delete.</param>
        /// <param name="userid">The user who deleted the annotation type.</param>
        Task<int> DeleteAnnotationTypeAsync(int annotationtypeid, string userid);
    }
}

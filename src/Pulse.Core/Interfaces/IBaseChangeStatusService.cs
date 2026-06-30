using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Interfaces
{
    public interface IBaseChangeStatusService<TEntity>
    {
        Task InitializeAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task StartAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task HoldAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task UnholdAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task CancelAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task ArchiveAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task CompleteAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        Task ForceCompleteAsync(TEntity obj, string reason, bool? begintransaction = false, bool? includechild = false, bool? notify = true);
        /*
         
        /// <summary>
        /// Add statuschange to NotStarted in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToNotStartedAsync(Project project, string reason, bool notify = true);

        /// <summary>
        /// Add statuschange to Started (Ongoing) in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToStartedAsync(Project project, string reason, bool notify = true);

        /// <summary>
        /// Add statuschange to continue (Ongoing) in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToResumedAsync(Project project, string reason);


        /// <summary>
        /// Add statuschange to Completed in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToCompletedAsync(Project project, string reason);

        /// <summary>
        /// Add statuschange to Completed in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToForcedCompletedAsync(Project project, string reason);

        /// <summary>
        /// Add statuschange to Canceled in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToCanceledAsync(Project project, string reason);

        /// <summary>
        /// Add statuschange to Hold in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToHoldAsync(Project project, string reason);

        /// <summary>
        /// Add statuschange to Failed in the system.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToFailedAsync(Project project, string reason);
         */
    }
}

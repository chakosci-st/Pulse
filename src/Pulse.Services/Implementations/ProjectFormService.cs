using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
namespace Pulse.Services.Implementations
{
    public class ProjectFormService : IProjectFormService
    {
        private readonly IProjectFormSubmissionRepository _projectformsubmissionRepository;
        private readonly IProjectFormSubmissionValueRepository _projectformsubmissionvalueRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IEventPublisher _eventBus;

        private DateTime transactiondatetime = DateTime.UtcNow;
        public ProjectFormService(OracleDataAccessLayer dataAccess, IEventPublisher eventBus, IProjectFormSubmissionRepository projectformsubmissionRepository, IProjectFormSubmissionValueRepository projectformsubmissionvalueRepository, IFormFieldRepository formFieldRepository)
        {
            _dataAccess = dataAccess;
            _eventBus = eventBus;
            _projectformsubmissionRepository = projectformsubmissionRepository;
            _projectformsubmissionvalueRepository = projectformsubmissionvalueRepository;
            _formFieldRepository = formFieldRepository;

        }

        private async Task<HashSet<string>> GetActiveFieldIdsAsync(IEnumerable<ProjectFormSubmissionValue> values)
        {
            var activeFieldIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var formIds = (values ?? Enumerable.Empty<ProjectFormSubmissionValue>())
                .Where(v => !string.IsNullOrWhiteSpace(v.FormSysId))
                .Select(v => v.FormSysId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var formId in formIds)
            {
                var ids = await _formFieldRepository.GetActiveFieldIdsByFormAsync(formId);
                foreach (var id in ids)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        activeFieldIds.Add(id);
                    }
                }
            }

            return activeFieldIds;
        }

        public async Task SubmitFormAsync(ProjectFormSubmission form, string loggeduserid)
        {
            _dataAccess.BeginTransaction();
            try
            {
                var submissionSysId = await _projectformsubmissionRepository.AddAsync(form);
                var activeFieldIds = await GetActiveFieldIdsAsync(form.SubmissionValues);

                var valuesCollection = form.SubmissionValues
                    .Where(sv => !string.IsNullOrWhiteSpace(sv.FormFieldSysId) && activeFieldIds.Contains(sv.FormFieldSysId))
                    .Select(sv => { sv.CreatedBy = loggeduserid; sv.CreatedDate = transactiondatetime; sv.ProjectNo = form.ProjectNo; sv.SubmissionSysId = submissionSysId; return sv; });

                var addTaskCollection = valuesCollection.Select(v => _projectformsubmissionvalueRepository.AddAsync(v));
                await Task.WhenAll(addTaskCollection);
                ////forms = forms.Select(pf => { pf.CreatedBy = loggeduserid; pf.CreatedDate = transactiondatetime; return pf; });

                ////var addprojectFormCollection = forms.Select<ProjectFormSubmission, Task>(async f =>
                ////            {
                ////                var id = await _projectformsubmissionRepository.AddAsync(f);
                ////                f.SubmissionSysId = id.ToString();
                ////            });

                ////await Task.WhenAll(addprojectFormCollection);



                _dataAccess.CommitTransaction();

                //_eventBus.Publish(new ProjectFieldCreatedEventArgs(loggeduserid, transactiondatetime));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw;

            }
        }

        //var tasks = formList.Select<ProjectFormSubmission, Task>(f => InsertFormAsync(f));
        private async Task InsertFormAsync(ProjectFormSubmission f)
        {
            var id = await _projectformsubmissionRepository.AddAsync(f);
            f.SubmissionSysId = id.ToString();
        }


        public async Task UpdateFormAsync(ProjectFormSubmission form, string loggeduserid)
        {
            _dataAccess.BeginTransaction();
            try
            {
                var count = await _projectformsubmissionRepository.UpdateAsync(form);
                if (count > 0)
                {
                    var activeFieldIds = await GetActiveFieldIdsAsync(form.SubmissionValues);

                    var valuesCollection = form.SubmissionValues
                        .Where(sv => !string.IsNullOrWhiteSpace(sv.FormFieldSysId) && activeFieldIds.Contains(sv.FormFieldSysId))
                        .Select(sv => { sv.ModifiedBy = loggeduserid; sv.ModifiedDate = transactiondatetime; return sv; });

                    var updateTaskCollection = valuesCollection.Select(v => _projectformsubmissionvalueRepository.UpdateAsync(v));
                    await Task.WhenAll(updateTaskCollection);
                    _dataAccess.CommitTransaction();
                }
                else {
                    _dataAccess.RollbackTransaction();
                }


                //_eventBus.Publish(new ProjectFieldCreatedEventArgs(loggeduserid, transactiondatetime));
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw;

            }
        }

        public async Task<List<ProjectFormSubmissionValue>> GetFormValuesBySubmissionAsync(string id) {
            return await _projectformsubmissionvalueRepository.GetBySubmissionAsync(id);
        }

 

        public async Task<ProjectFormSubmissionValue> GetFormValueAsync(string id)
        {
            return await _projectformsubmissionvalueRepository.GetAsync(id);
        }
    }
}

using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class AnnotationTypeService : IAnnotationTypeService 
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IAnnotationTypeRepository _annotationtypeRepository;

        public AnnotationTypeService(OracleDataAccessLayer dataAccess, IAnnotationTypeRepository annotationtypeRepositorysitory)
        {
            _dataAccess = dataAccess;
            _annotationtypeRepository = annotationtypeRepositorysitory;
        }

        public async Task<IEnumerable<AnnotationType>> GetAllAnnotationTypesAsync()
        {
            return await _annotationtypeRepository.GetListAsync();
        }

        public async Task<AnnotationType> GetAnnotationTypeByIdAsync(int annotationtypeid)
        {
            return await _annotationtypeRepository.GetAsync(annotationtypeid);
        }

        public async Task<int> AddAnnotationTypeAsync(AnnotationType annotationtype)
        {
            return await _annotationtypeRepository.AddAsync(annotationtype);
        }

        public async Task<int> UpdateAnnotationTypeAsync(AnnotationType annotationtype)
        {

            var rowsaffected = await _annotationtypeRepository.UpdateAsync(annotationtype);

            // Publish the ProductUpdatedEvent
            //var plantUpdatedEvent = new PlantUpdatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(plantUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public async Task<int> DeleteAnnotationTypeAsync(int annotationtypeid, string userid)
        {
            _dataAccess.BeginTransaction();

            try
            {
                //GET INFO
                var obj = await _annotationtypeRepository.GetAsync(annotationtypeid); ;


                //SET USER WHO DELETES THE AnnotationType
                obj.ModifiedBy = userid;
                await _annotationtypeRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _annotationtypeRepository.DeleteAsync(annotationtypeid);


                // Publish the ProductUpdatedEvent
                //var plantDeletedEvent = new PlantDeletedEvent
                //{
                //    PlantCode = plant.PlantCode,
                //    ActionBy = plant.CreatedBy
                //};

                _dataAccess.CommitTransaction();

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(plantDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);

            }



            return 0;
        }
    }
}

using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
namespace Pulse.Services.Implementations
{
    public class PlantService : IPlantService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IEmailSender _emailSender;
        private readonly IEventPublisher _eventBus;
        private readonly IPlantRepository _plantRepository;
        private readonly IPlantMemberRepository _plantmemberRepository;
        private readonly IPlantRoadmapLinkRepository _plantroadmaplinkRepository;
        private readonly IUserRepository _userRepository;
        public PlantService(OracleDataAccessLayer dataAccess, IEmailSender emailSender, IEventPublisher eventBus,
            IPlantRepository plantRepositorysitory, IPlantMemberRepository plantmemberRepository, IUserRepository userRepository,
            IPlantRoadmapLinkRepository plantroadmaplinkRepository)
        {
            _plantRepository = plantRepositorysitory;
            _plantmemberRepository = plantmemberRepository;
            _userRepository = userRepository;
            _dataAccess = dataAccess;
            _emailSender = emailSender;
            _eventBus = eventBus;
            _plantroadmaplinkRepository = plantroadmaplinkRepository;
        }



        public async Task<int> DeletePlantAsync(string plantcode, string userid)
        {


            try
            {
                //GET PLANT INFO
                var obj = await _plantRepository.GetAsync(plantcode);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = userid;
                await _plantRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _plantRepository.DeleteAsync(plantcode);


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

        public async Task<IEnumerable<Plant>> GetAllPlantsAsync()
        {
            return await _plantRepository.GetListAsync();
        }

        public async Task<Plant> GetPlantByCodeAsync(string plantcode)
        {
            return await _plantRepository.GetAsync(plantcode);
        }

        public async Task<IEnumerable<Plant>> GetAllPlantsByUserAsync(string userid) {
            return await _plantRepository.GetListByUserAsync(userid);
        }

        public async Task<string> AddPlantAsync(Plant plant)
        {
            return await _plantRepository.AddAsync(plant);
        }

        public async Task<string> AddPlantAsync(Plant plant, byte[] fileBytes, string fileName)
        {
            var code = await _plantRepository.AddAsync(plant);


            if (fileBytes != null && !string.IsNullOrEmpty(fileName))
            {
                // Define the folder path (e.g., App_Data/PlantFiles)
                var folderPath = HostingEnvironment.MapPath("~/content/uploads/PlantFiles/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                // Combine folder and file name
                var filePath = Path.Combine(folderPath, fileName);

                // Save the file asynchronously
                File.WriteAllBytes(filePath, fileBytes);
            }

            // Publish the ProductUpdatedEvent
            //var plantUpdatedEvent = new PlantUpdatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};



            return code;
        }



        public async Task<int> UpdatePlantAsync(Plant plant)
        {
            var rowsaffected = await _plantRepository.UpdateAsync(plant);

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

        public async Task<int> UpdatePlantAsync(Plant plant, byte[] fileBytes, string fileName)
        {
            var rowsaffected = await _plantRepository.UpdateAsync(plant);


            if (fileBytes != null && !string.IsNullOrEmpty(fileName))
            {
                // Define the folder path (e.g., App_Data/PlantFiles)
                var folderPath = HostingEnvironment.MapPath("~/content/uploads/PlantFiles/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                // Combine folder and file name
                var filePath = Path.Combine(folderPath, fileName);

                // Save the file asynchronously
                File.WriteAllBytes(filePath, fileBytes);
            }

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
        public Plant GetPlantByCode(string plantcode)
        {
            return _plantRepository.Get(plantcode);
        }
        public async Task<PagedResult<PlantWithStats>> GetPagedPlantsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _plantRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }

        public async Task<IEnumerable<PlantMember>> GetMembersByCode(string plantCode)
        {
            return await _plantmemberRepository.GetListAsync(plantCode);
        }

        public async Task<string> AddMemberAsync(PlantMember plantmember)
        {
            try
            {
                _dataAccess.BeginTransaction();
                plantmember.CreatedDate = DateTime.UtcNow;

                var user = await _userRepository.GetAsync(plantmember.UserId);

                if (user == null)
                {
                    plantmember.UserInfo.CreatedBy = plantmember.CreatedBy;
                    //Create user
                    await _userRepository.AddAsync(plantmember.UserInfo);

                }

                var plantmembersysid = await _plantmemberRepository.AddAsync(plantmember);


                _dataAccess.CommitTransaction();

                return plantmembersysid;
            }
            catch
            {
                _dataAccess.RollbackTransaction();
                throw;
            }


        }

        public async Task<int> UpdateMemberAsync(PlantMember plantmember)
        {
            return await _plantmemberRepository.UpdateAsync(plantmember);
        }



        #region "ROADMAP LINK"

        public async Task<string> SelectRoadmapAsync(PlantRoadmapLink link)
        {
            var id = link.PlantRoadmapLinkSysId;
            link.IsActive = 1;
            if (await _plantroadmaplinkRepository.UpdateAsync(link) == 0)
            {
                id = await _plantroadmaplinkRepository.AddAsync(link);
            } 

            return id;
        }

        public async Task<int> UnselectRoadmapAsync(PlantRoadmapLink link)
        {
            link.IsActive = 0;
            return await _plantroadmaplinkRepository.UpdateAsync(link);
        }

        public async Task<IEnumerable<PlantRoadmapLinkExtended>> GetRoadmapListAsync(string plantcode = null, string roadmapsysid = null)
        {
            return await _plantroadmaplinkRepository.GetLinkListAsync(plantcode, roadmapsysid);
        }
        #endregion


    }
}

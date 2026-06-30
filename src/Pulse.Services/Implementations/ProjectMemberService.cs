using Pulse.Core.Entities;
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
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IProjectMemberRepository _projectmemberRepository;
        private readonly IUserRepository _userRepository;

        public ProjectMemberService(OracleDataAccessLayer dataAccess, IUserRepository userRepository, IProjectMemberRepository projectmemberRepository)
        {
            _dataAccess = dataAccess;
            _projectmemberRepository = projectmemberRepository;
            _userRepository = userRepository;
        }

        public async Task<string> EnrollMemberAsync(ProjectMember projectmember)
        {

            _dataAccess.BeginTransaction();
            try
            {
                projectmember.IsOwner = projectmember.IsOwner == 1 ? 1 : 0;

                projectmember.CreatedDate = DateTime.UtcNow;

                var user = await _userRepository.GetAsync(projectmember.UserId);

                if (user == null)
                {
                    projectmember.User.CreatedBy = projectmember.CreatedBy;
                    //Create user
                    await _userRepository.AddAsync(projectmember.User);

                }

                var membersysId = await _projectmemberRepository.AddAsync(projectmember);


                _dataAccess.CommitTransaction();

                return membersysId;
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public async Task EnrollMembersAsync(IEnumerable<ProjectMember> projectmembers)
        {
            var memberAddTasks = projectmembers.Select(m => this.EnrollMemberAsync(m));
            await Task.WhenAll(memberAddTasks);
        } 
        public async Task<int> UpdateMemberAsync(ProjectMember projectmember)
        {
            projectmember.IsOwner = projectmember.IsOwner == 1 ? 1 : 0;
            return await _projectmemberRepository.UpdateAsync(projectmember);
        }

        public async Task UpdateMembersAsync(IEnumerable<ProjectMember> projectmembers)
        {
            var memberUpdateTasks = projectmembers.Select(m => _projectmemberRepository.UpdateAsync(m));
            await Task.WhenAll(memberUpdateTasks);
        }

        public async Task<int> RemoveMemberAsync(string projectmemberno, string loggeduserid)
        {
        

            try
            {
                //GET INFO
                var obj = await _projectmemberRepository.GetAsync(projectmemberno);


                //SET USER WHO DELETES THE OBJECT
                obj.ModifiedBy = loggeduserid;
                await _projectmemberRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _projectmemberRepository.DeleteAsync(projectmemberno);


                // Publish the ProductUpdatedEvent
                //var productgroupDeletedEvent = new ProductGroupDeletedEvent
                //{
                //    ProductGroupCode = productgroup.ProductGroupCode,
                //    ActionBy = productgroup.CreatedBy
                //};


                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(productgroupDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }



            return 0;

        }
 

        public async Task<ProjectMember> GetProjectMemberByIdAsync(string projectmemberno)
        {
            return await _projectmemberRepository.GetAsync(projectmemberno);
        }

        public async Task<IEnumerable<ProjectMember>> GetAllProjectMembersAsync()
        {
            return await _projectmemberRepository.GetListAsync();
        }

        public async Task<IEnumerable<ProjectMember>> GetAllProjectMembersAsync(string projectno)
        {
            return await _projectmemberRepository.GetListAsync(projectno);
        }
    }
}

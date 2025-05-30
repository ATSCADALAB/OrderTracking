using AutoMapper;
using Contracts;
using Entities.Exceptions.RolePermission;
using Entities.Models;
using QuickStart.Shared.DataTransferObjects.RolePermission;
using Service.Contracts;
using Shared.DataTransferObjects.RolePermission;

namespace Service
{
    internal sealed class RolePermissionService : IRolePermissionService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public RolePermissionService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }



        public async Task<RolePermissionDto> CreateRolePermissionAsync(RolePermissionForCreationDto RolePermission)
        {
            var RolePermissionEntity = _mapper.Map<RolePermission>(RolePermission);

            _repository.RolePermission.CreateRolePermission(RolePermissionEntity);
            await _repository.SaveAsync();

            var RolePermissionToReturn = _mapper.Map<RolePermissionDto>(RolePermissionEntity);
            return RolePermissionToReturn;
        }

        public async Task DeleteRolePermissionAsync(Guid RolePermissionId, bool trackChanges)
        {
            var RolePermission = await GetRolePermissionAndCheckIfItExists(RolePermissionId, trackChanges);

            _repository.RolePermission.DeleteRolePermission(RolePermission);
            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<RolePermissionDto>> GetAllRolePermissionsAsync(bool trackChanges)
        {
            var RolePermissions = await _repository.RolePermission.GetRolePermissionsAsync(trackChanges);

            var RolePermissionsDto = _mapper.Map<IEnumerable<RolePermissionDto>>(RolePermissions);

            return RolePermissionsDto;
        }

        public async Task<RolePermissionDto> GetRolePermissionAsync(Guid RolePermissionId, bool trackChanges)
        {
            var RolePermission = await GetRolePermissionAndCheckIfItExists(RolePermissionId, trackChanges);

            var RolePermissionDto = _mapper.Map<RolePermissionDto>(RolePermission);
            return RolePermissionDto;
        }

        public async Task UpdateRolePermissionAsync(Guid RolePermissionId, RolePermissionForUpdateDto RolePermissionForUpdate, bool trackChanges)
        {
            var RolePermission = await GetRolePermissionAndCheckIfItExists(RolePermissionId, trackChanges);

            _mapper.Map(RolePermissionForUpdate, RolePermission);
            await _repository.SaveAsync();
        }


        private async Task<RolePermission> GetRolePermissionAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var RolePermission = await _repository.RolePermission.GetRolePermissionAsync(id, trackChanges);
            if (RolePermission is null)
                throw new RolePermissionNotFoundException(id);

            return RolePermission;
        }
        public async Task<IEnumerable<RoleMapPermissionDto>> GetRolePermissionsByRoleNameAsync(string roleId, bool trackChanges)
        {
            var rolePermissions = await _repository.RolePermission.GetRolePermissionsByRoleNameAsync(roleId, trackChanges);
            return _mapper.Map<IEnumerable<RoleMapPermissionDto>>(rolePermissions);
        }
        public async Task<IEnumerable<RolePermissionDto>> GetRolePermissionsByRoleIdAsync(string roleId, bool trackChanges)
        {
            var rolePermissions = await _repository.RolePermission.GetRolePermissionsByRoleIdAsync(roleId, trackChanges);
            return _mapper.Map<IEnumerable<RolePermissionDto>>(rolePermissions);
        }
        public async Task AssignPermissionsToRoleAsync(RolePermissionForAssignmentDto assignmentDto)
        {
            try
            {
                await _repository.RolePermission.DeleteRolePermissionsByRoleIdAsync(assignmentDto.RoleId);

                foreach (var categoryAssignment in assignmentDto.CategoryAssignments)
                {
                    foreach (var permissionId in categoryAssignment.PermissionIds)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = assignmentDto.RoleId,
                            CategoryId = categoryAssignment.CategoryId,
                            PermissionId = permissionId
                        };
                        _repository.RolePermission.CreateRolePermission(rolePermission);
                    }
                }

                await _repository.SaveAsync();
            }
            catch(Exception e)
            {
                var a = 0;
            }
            
        }
    }
}

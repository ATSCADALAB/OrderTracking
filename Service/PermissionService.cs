using AutoMapper;
using Contracts;
using Entities.Exceptions.Permission;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Permission;

namespace Service
{
    internal sealed class PermissionService : IPermissionService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public PermissionService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }



        public async Task<PermissionDto> CreatePermissionAsync(PermissionForCreationDto Permission)
        {
            var PermissionEntity = _mapper.Map<Permission>(Permission);

            _repository.Permission.CreatePermission(PermissionEntity);
            await _repository.SaveAsync();

            var PermissionToReturn = _mapper.Map<PermissionDto>(PermissionEntity);
            return PermissionToReturn;
        }

        public async Task DeletePermissionAsync(Guid PermissionId, bool trackChanges)
        {
            var Permission = await GetPermissionAndCheckIfItExists(PermissionId, trackChanges);

            _repository.Permission.DeletePermission(Permission);
            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(bool trackChanges)
        {
            var Permissions = await _repository.Permission.GetPermissionsAsync(trackChanges);

            var PermissionsDto = _mapper.Map<IEnumerable<PermissionDto>>(Permissions);

            return PermissionsDto;
        }

        public async Task<PermissionDto> GetPermissionAsync(Guid PermissionId, bool trackChanges)
        {
            var Permission = await GetPermissionAndCheckIfItExists(PermissionId, trackChanges);

            var PermissionDto = _mapper.Map<PermissionDto>(Permission);
            return PermissionDto;
        }

        public async Task UpdatePermissionAsync(Guid PermissionId, PermissionForUpdateDto PermissionForUpdate, bool trackChanges)
        {
            var Permission = await GetPermissionAndCheckIfItExists(PermissionId, trackChanges);

            _mapper.Map(PermissionForUpdate, Permission);
            await _repository.SaveAsync();
        }


        private async Task<Permission> GetPermissionAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var Permission = await _repository.Permission.GetPermissionAsync(id, trackChanges);
            if (Permission is null)
                throw new PermissionNotFoundException(id);

            return Permission;
        }
    }
}

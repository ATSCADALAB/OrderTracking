using AutoMapper;
using Contracts;
using Entities.Exceptions.Category;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Category;

namespace Service
{
    internal sealed class CategoryService : ICategoryService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CategoryService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }



        public async Task<CategoryDto> CreateCategoryAsync(CategoryForCreationDto Category)
        {
            var CategoryEntity = _mapper.Map<Category>(Category);

            _repository.Category.CreateCategory(CategoryEntity);
            await _repository.SaveAsync();

            var CategoryToReturn = _mapper.Map<CategoryDto>(CategoryEntity);
            return CategoryToReturn;
        }

        public async Task DeleteCategoryAsync(Guid CategoryId, bool trackChanges)
        {
            var Category = await GetCategoryAndCheckIfItExists(CategoryId, trackChanges);

            _repository.Category.DeleteCategory(Category);
            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategorysAsync(bool trackChanges)
        {
            var Categorys = await _repository.Category.GetCategorysAsync(trackChanges);

            var CategorysDto = _mapper.Map<IEnumerable<CategoryDto>>(Categorys);

            return CategorysDto;
        }

        public async Task<CategoryDto> GetCategoryAsync(Guid CategoryId, bool trackChanges)
        {
            var Category = await GetCategoryAndCheckIfItExists(CategoryId, trackChanges);

            var CategoryDto = _mapper.Map<CategoryDto>(Category);
            return CategoryDto;
        }

        public async Task UpdateCategoryAsync(Guid CategoryId, CategoryForUpdateDto CategoryForUpdate, bool trackChanges)
        {
            var Category = await GetCategoryAndCheckIfItExists(CategoryId, trackChanges);

            _mapper.Map(CategoryForUpdate, Category);
            await _repository.SaveAsync();
        }


        private async Task<Category> GetCategoryAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var Category = await _repository.Category.GetCategoryAsync(id, trackChanges);
            if (Category is null)
                throw new CategoryNotFoundException(id);

            return Category;
        }
    }
}

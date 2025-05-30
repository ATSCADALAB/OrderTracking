namespace Entities.Exceptions.Category
{
    public sealed class CategoryNotFoundException : NotFoundException
    {
        public CategoryNotFoundException(Guid CategoryId)
            : base($"The Category with id: {CategoryId} doesn't exist in the database.")
        {
        }
    }
}

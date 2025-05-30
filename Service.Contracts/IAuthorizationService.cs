namespace Service.Contracts
{
    public interface IAuthorizationServiceLocal
    {
        Task<bool> HasPermission(string userId, string resource, string action);
    }
}

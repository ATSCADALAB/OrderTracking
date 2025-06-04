namespace Service.Contracts
{
    public interface IServiceManager
    {
        IAuditService AuditService { get; }
        IAuthenticationService AuthenticationService { get; }
        IAuthorizationServiceLocal AuthorizationService { get; }
        ICategoryService CategoryService { get; }
        IPermissionService PermissionService { get; }
        IRolePermissionService RolePermissionService { get; }
        IUserService UserService { get; }
        IUserCalendarService UserCalendarService { get; }
        IRoleService RoleService { get; }
        IWcfService WcfService { get; }
        ICalendarReportService CalendarReportService { get; }
        ISheetOrderService SheetOrderService { get; }
        ICalendarEventService CalendarEventService { get; }
        IKpiConfigurationService KpiConfigurationService { get; }
        IEventExclusionKeywordService EventExclusionKeywordService { get; }
        ISendMailService SendMailService { get; }
    }
}

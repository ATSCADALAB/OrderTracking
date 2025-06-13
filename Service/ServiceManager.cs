using AutoMapper;
using Contracts;
using EmailService;
using Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR; // Thêm để dùng IHubContext
using Microsoft.Extensions.Configuration;
using QuickStart.Hubs; // Thêm để dùng DataHub
using QuickStart.Service;
using Service.Contracts;
using Service.JwtFeatures;
using System.Net.Http;

namespace Service
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<ICategoryService> _categoryRepository;
        private readonly Lazy<IPermissionService> _permissionService;
        private readonly Lazy<IRolePermissionService> _rolePermissionService;
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<IAuthorizationServiceLocal> _authorization1Service;
        private readonly Lazy<IUserService> _userService;
        private readonly Lazy<IRoleService> _roleService;
        private readonly Lazy<IAuditService> _auditService;
        private readonly Lazy<IWcfService> _wcfService;
        private readonly Lazy<IUserCalendarService> _userCalendarService;
        private readonly Lazy<ICalendarReportService> _calendarReportService;
        private readonly Lazy<ISheetOrderService> _sheetOrderService;
        private readonly Lazy<ICalendarEventService> _calendarEventService;
        private readonly Lazy<ISendMailService> _sendMailService;
        private readonly Lazy<IKpiConfigurationService> _kpiConfigurationService;
        private readonly Lazy<IEventExclusionKeywordService> _eventExclusionKeywordService;
        private readonly Lazy<IEmailCcConfigurationService> _emailCcConfigurationService;
        public ServiceManager(
            IRepositoryManager repositoryManager,
            ILoggerManager logger,
            IMapper mapper,
            UserManager<User> userManager,
            IConfiguration configuration,
            RoleManager<UserRole> roleManager,
            JwtHandler jwtHandler,
            IEmailSender emailSender,
            IHubContext<DataHub> hubContext, IWebHostEnvironment evn, HttpClient httpClient) // Thêm IHubContext<DataHub>
        {
            _authorization1Service = new Lazy<IAuthorizationServiceLocal>(() => new AuthorizationService(userManager, repositoryManager, logger));
            _categoryRepository = new Lazy<ICategoryService>(() => new CategoryService(repositoryManager, logger, mapper));
            _permissionService = new Lazy<IPermissionService>(() => new PermissionService(repositoryManager, logger, mapper));
            _rolePermissionService = new Lazy<IRolePermissionService>(() => new RolePermissionService(repositoryManager, logger, mapper));
            _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(
                logger, mapper, userManager, configuration, jwtHandler, emailSender));
            _userService = new Lazy<IUserService>(() => new UserService(logger, mapper, userManager));
            _roleService = new Lazy<IRoleService>(() => new RoleService(logger, mapper, roleManager));
            _auditService = new Lazy<IAuditService>(() => new AuditService(repositoryManager, logger, mapper));
            _userCalendarService = new Lazy<IUserCalendarService>(() => new UserCalendarService(repositoryManager, logger, mapper));
            _wcfService = new Lazy<IWcfService>(() => new WcfService(configuration, hubContext));
            _calendarReportService = new Lazy<ICalendarReportService>(() => new CalendarReportService(repositoryManager, logger, mapper,evn));
            _sheetOrderService = new Lazy<ISheetOrderService>(() => new SheetOrderService(repositoryManager, mapper));
            _calendarEventService = new Lazy<ICalendarEventService>(() => new CalendarEventService(repositoryManager, mapper));
            _sendMailService = new Lazy<ISendMailService>(() => new SendMailService(repositoryManager, logger, emailSender, httpClient));
            _kpiConfigurationService= new Lazy<IKpiConfigurationService>(() => new KpiConfigurationService(repositoryManager, logger, mapper));
            _eventExclusionKeywordService = new Lazy<IEventExclusionKeywordService>(() => new EventExclusionKeywordService(repositoryManager, logger, mapper));
            _emailCcConfigurationService = new Lazy<IEmailCcConfigurationService>(() => new EmailCcConfigurationService(repositoryManager, mapper));
        }

        public IWcfService WcfService => _wcfService.Value;
        public ICategoryService CategoryService => _categoryRepository.Value;
        public IPermissionService PermissionService => _permissionService.Value;
        public IRolePermissionService RolePermissionService => _rolePermissionService.Value;
        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public IAuthorizationServiceLocal AuthorizationService => _authorization1Service.Value;
        public IUserService UserService => _userService.Value;
        public IRoleService RoleService => _roleService.Value;
        public IAuditService AuditService => _auditService.Value;
        public IUserCalendarService UserCalendarService => _userCalendarService.Value;
        public ICalendarReportService CalendarReportService => _calendarReportService.Value;
        public ISheetOrderService SheetOrderService => _sheetOrderService.Value;
        public ICalendarEventService CalendarEventService => _calendarEventService.Value;
        public ISendMailService SendMailService => _sendMailService.Value;
        public IKpiConfigurationService KpiConfigurationService => _kpiConfigurationService.Value;
        public IEventExclusionKeywordService EventExclusionKeywordService => _eventExclusionKeywordService.Value;
        public IEmailCcConfigurationService EmailCcConfigurationService => _emailCcConfigurationService.Value;
    }
}
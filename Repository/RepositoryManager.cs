using Contracts;
using Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Repository
{
    public sealed class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repositoryContext;
        private readonly Lazy<ICategoryRepository> _categoryRepository;
        private readonly Lazy<IPermissionRepository> _permissionRepository;
        private readonly Lazy<IRolePermissionRepository> _rolePermissionRepository;
        private readonly Lazy<IAuditRepository> _auditRepository;
        private readonly Lazy<IUserCalendarRepository> _userCalendarRepository;
        private readonly Lazy<ISheetOrderRepository> _sheetOrderRepository;
        private readonly Lazy<ICalendarEventRepository> _calendarEventRepository;
        private readonly Lazy<ISendMailRepository> _sendMailRepository;
        private readonly Lazy<IKpiConfigurationRepository> _kpiConfigurationRepository;
        private readonly Lazy<IEventExclusionKeywordRepository> _eventExclusionKeywordRepository;
        private readonly Lazy<IEmailCcConfigurationRepository> _emailCcConfigurationRepository;
        private readonly Lazy<IEmployeeRepository> _employeeRepository;
        public RepositoryManager(RepositoryContext repositoryContext, RoleManager<UserRole> roleManager)
        {
            _repositoryContext = repositoryContext;
            _categoryRepository = new Lazy<ICategoryRepository>(() => new CategoryRepository(repositoryContext));
            _permissionRepository = new Lazy<IPermissionRepository>(() => new PermissionRepository(repositoryContext));
            _rolePermissionRepository = new Lazy<IRolePermissionRepository>(() => new RolePermissionRepository(repositoryContext));
            _auditRepository = new Lazy<IAuditRepository>(() => new AuditRepository(repositoryContext));
            _userCalendarRepository = new Lazy<IUserCalendarRepository>(() => new UserCalendarRepository(repositoryContext));
            _sheetOrderRepository = new Lazy<ISheetOrderRepository>(() => new SheetOrderRepository(repositoryContext));
            _calendarEventRepository = new Lazy<ICalendarEventRepository>(() => new CalendarEventRepository(repositoryContext));
            _sendMailRepository = new Lazy<ISendMailRepository>(() => new SendMailRepository(repositoryContext));
            _kpiConfigurationRepository = new Lazy<IKpiConfigurationRepository>(() => new KpiConfigurationRepository(repositoryContext));
            _eventExclusionKeywordRepository = new Lazy<IEventExclusionKeywordRepository>(() => new EventExclusionKeywordRepository(repositoryContext));
            _emailCcConfigurationRepository = new Lazy<IEmailCcConfigurationRepository>(() => new EmailCcConfigurationRepository(_repositoryContext));
            _employeeRepository = new Lazy<IEmployeeRepository>(() => new EmployeeRepository(repositoryContext));
        }
        public IEmailCcConfigurationRepository EmailCcConfiguration => _emailCcConfigurationRepository.Value;
        public IUserCalendarRepository UserCalendar => _userCalendarRepository.Value;
        public ICategoryRepository Category => _categoryRepository.Value;
        public IPermissionRepository Permission => _permissionRepository.Value;
        public IRolePermissionRepository RolePermission => _rolePermissionRepository.Value;
        public IAuditRepository Audit => _auditRepository.Value;
        public ISheetOrderRepository SheetOrder => _sheetOrderRepository.Value;
        public ICalendarEventRepository CalendarEvent => _calendarEventRepository.Value;
        public ISendMailRepository SendMail => _sendMailRepository.Value;
        public IKpiConfigurationRepository KpiConfiguration => _kpiConfigurationRepository.Value;
        public IEventExclusionKeywordRepository EventExclusionKeyword => _eventExclusionKeywordRepository.Value;
        public IEmployeeRepository Employee => _employeeRepository.Value;
        public async Task SaveAsync() => await _repositoryContext.SaveChangesAsync();
    }
}

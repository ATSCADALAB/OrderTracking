﻿namespace Contracts
{
    public interface IRepositoryManager
    {
        IAuditRepository Audit { get; }
        ICategoryRepository Category { get; }
        IPermissionRepository Permission { get; }
        IRolePermissionRepository RolePermission { get; }
        IUserCalendarRepository UserCalendar { get; }
        ISheetOrderRepository SheetOrder { get; }
        ICalendarEventRepository CalendarEvent { get; }
        ISendMailRepository SendMail { get; }
        IKpiConfigurationRepository KpiConfiguration { get; }
        IEventExclusionKeywordRepository EventExclusionKeyword { get; }
        IEmailCcConfigurationRepository EmailCcConfiguration { get; }
        IEmployeeRepository Employee { get; }
        Task SaveAsync();
    }
}

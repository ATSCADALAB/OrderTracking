using AutoMapper;
using Entities.Identity;
using Entities.Models;
using Shared.DataTransferObjects.AuditLog;
using Shared.DataTransferObjects.Authentication;
using Shared.DataTransferObjects.Calendar;
using Shared.DataTransferObjects.Category;
using Shared.DataTransferObjects.Permission;
using Shared.DataTransferObjects.RolePermission;
using Shared.DataTransferObjects.User;
using Shared.DataTransferObjects.UserCalendar;
using Shared.DataTransferObjects.UserRole;
using Shared.DataTransferObjects.SheetOrder;
using SheetOrderDto = Shared.DataTransferObjects.SheetOrder.SheetOrderDto;
using Shared.DataTransferObjects.CalendarEvent;
using Shared.DataTransferObjects.KpiConfiguration;
using Shared.DataTransferObjects.EventExclusion;
namespace QuickStart
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserCalendarForCreationDto, UserCalendar>();
            CreateMap<CalendarEvent, CalendarEventDto>();
            CreateMap<CalendarEventDto, CalendarEvent>();
            CreateMap<SheetOrderDto, SheetOrder>();
            CreateMap<SheetOrder, SheetOrderDto>();
            CreateMap<UserCalendar, UserCalendarDto>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName +" "+src.User.LastName));
            CreateMap<RolePermission, RolePermissionDto>();
            CreateMap<RolePermission, RolePermissionForCreationDto>();
            CreateMap<RolePermissionForCreationDto, RolePermission>();
            CreateMap<RolePermissionForUpdateDto, RolePermission>();
            CreateMap<PermissionForCreationDto, Permission>();
            CreateMap<Permission, PermissionDto>();
            CreateMap<PermissionForCreationDto, Permission>();
            CreateMap<PermissionForUpdateDto, Permission>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryForCreationDto, Category>();
            CreateMap<CategoryForUpdateDto, Category>();
            CreateMap<UserForRegistrationDto, User>();
            CreateMap<User, UserDto>();
            CreateMap<UserRole, UserRoleDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<UserRoleForCreationDto, UserRole>();
            CreateMap<UserRoleForUpdateDto, UserRole>();
            CreateMap<AuditLog, AuditLogDto>();
            CreateMap<RolePermission, RoleMapPermissionDto>()
                .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            // KPI Configuration mappings
            CreateMap<KpiConfiguration, KpiConfigurationDto>();
            CreateMap<KpiConfigurationForCreationDto, KpiConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<KpiConfigurationForUpdateDto, KpiConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            // Entity to DTO
            CreateMap<EventExclusionKeyword, EventExclusionKeywordDto>();

            // Creation DTO to Entity
            CreateMap<EventExclusionKeywordForCreationDto, EventExclusionKeyword>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Update DTO to Entity  
            CreateMap<EventExclusionKeywordForUpdateDto, EventExclusionKeyword>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}

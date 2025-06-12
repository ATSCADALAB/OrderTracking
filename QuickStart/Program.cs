using Contracts;
using EmailService;
using Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLog;
using QuickStart;
using QuickStart.Extensions;
using QuickStart.Hubs;
using QuickStart.Presentation.ActionFilters;
using QuickStart.Service;
using Repository;
using Service;
using Service.Contracts;
using Service.JwtFeatures;
// Thêm Hangfire imports
using Hangfire;
using Hangfire.MySql;
using QuickStart.Services;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options; // Namespace cho HangfireSyncService

var builder = WebApplication.CreateBuilder(args);

LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSignalR();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// ✅ EMAIL CONFIGURATION - SỬA LẠI ĐÚNG THỨ TỰ
var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

// ✅ THÊM EMAIL SETTINGS
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// ✅ CHỈ ĐĂNG KÝ 1 LẦN IEmailSender
builder.Services.AddSingleton<IEmailSender>(provider =>
{
    var emailConfig = provider.GetRequiredService<EmailConfiguration>();
    var emailSettings = provider.GetRequiredService<IOptions<EmailSettings>>().Value;
    return new EmailSender(emailConfig, emailSettings);
});

// ✅ HTTP CLIENT CHO SEND MAIL SERVICE
builder.Services.AddHttpClient<ISendMailService, SendMailService>();

// ✅ BACKGROUND SERVICE VỚI EMAIL SETTINGS
builder.Services.AddHostedService<SendMailBackgroundService>();

builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.AddScoped<JwtHandler>();

builder.Services.AddScoped<AuthorizePermissionAttribute>(provider =>
    new AuthorizePermissionAttribute(
        "",
        "",
        provider.GetRequiredService<IServiceManager>()
    ));
// === THÊM HANGFIRE CONFIGURATION ===
// Đăng ký HangfireSyncService
builder.Services.AddScoped<HangfireSyncService>();

// Cấu hình Hangfire cho MySQL
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("sqlConnection"),
        new MySqlStorageOptions
        {
            QueuePollInterval = TimeSpan.FromSeconds(15),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true,
            DashboardJobListLimit = 50000,
            TransactionTimeout = TimeSpan.FromMinutes(1),
            TablesPrefix = "Hangfire"
        })));

// Thêm Hangfire server
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
});
// === KẾT THÚC HANGFIRE CONFIGURATION ===

builder.Services.AddControllers()
    .AddApplicationPart(typeof(QuickStart.Presentation.AssemblyReference).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Middleware
app.UseCors("AllowAll");
app.UseExceptionHandler(opt => { });

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseRouting();

// === THÊM HANGFIRE DASHBOARD ===
// Thêm Hangfire Dashboard (đặt trước UseAuthentication để có thể truy cập)
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "QuickStart Job Dashboard",
    StatsPollingInterval = 2000 // Refresh mỗi 2 giây
});
// === KẾT THÚC HANGFIRE DASHBOARD ===

// Seeding users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        await SeedingUsers.SeedUsers(userManager);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred during user seeding");
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseSwagger();
app.UseSwaggerUI(s =>
{
    s.SwaggerEndpoint("/swagger/v1/swagger.json", "Matech Coding API v1");
});

app.Use(next => context =>
{
    context.Request.EnableBuffering();
    return next(context);
});

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// === THIẾT LẬP HANGFIRE JOBS ===
// Tự động tạo các recurring jobs khi ứng dụng khởi động
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Job chính: Đồng bộ hàng tháng vào ngày 1 lúc 2:00 AM (GMT+7)
        RecurringJob.AddOrUpdate<HangfireSyncService>(
            "monthly-full-sync",
            service => service.ExecuteMonthlySyncAsync(),
            Cron.Monthly(1, 2), // Ngày 1 hàng tháng lúc 2:00 AM
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time") // Múi giờ Việt Nam
        );

        // Job riêng lẻ (không tự động chạy, chỉ chạy thủ công)
        RecurringJob.AddOrUpdate<HangfireSyncService>(
            "sync-sheet-only",
            service => service.ExecuteSyncSheetAsync(),
            Cron.Never()
        );

        RecurringJob.AddOrUpdate<HangfireSyncService>(
            "sync-calendar-only",
            service => service.ExecuteSyncCalendarAsync(),
            Cron.Never()
        );

        logger.LogInformation("✅ Đã thiết lập Hangfire jobs thành công!");
        logger.LogInformation("📊 Monthly Sync sẽ chạy vào ngày 1 hàng tháng lúc 2:00 AM (GMT+7)");
        logger.LogInformation("🌐 Hangfire Dashboard: /hangfire");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Lỗi khi thiết lập Hangfire jobs");
    }
}
// === KẾT THÚC THIẾT LẬP HANGFIRE JOBS ===

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

app.Run();

// === HANGFIRE AUTHORIZATION FILTER ===
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Trong development, cho phép truy cập tự do
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        // Trong production, kiểm tra authentication
        // Bạn có thể customize logic này theo nhu cầu
        return httpContext.User.Identity?.IsAuthenticated == true;

        // Hoặc kiểm tra role cụ thể:
        // return httpContext.User.IsInRole("Admin");
    }
}
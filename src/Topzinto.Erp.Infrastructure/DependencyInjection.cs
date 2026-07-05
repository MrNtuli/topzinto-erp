using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Infrastructure.Caching;
using Topzinto.Erp.Infrastructure.Identity;
using Topzinto.Erp.Infrastructure.Persistence;
using Topzinto.Erp.Infrastructure.Services;

namespace Topzinto.Erp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useSqlite = configuration.GetValue("UseSqlite", false);
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<IAppCacheInvalidator, AppCacheInvalidator>();
        services.AddScoped<DashboardCacheInvalidationInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            if (useSqlite)
                options.UseSqlite(connectionString);
            else
                options.UseNpgsql(connectionString);

            options.AddInterceptors(sp.GetRequiredService<DashboardCacheInvalidationInterceptor>());
        });

        AddCaching(services, configuration);

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
            .AddUserManager<UserManager<ApplicationUser>>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(1));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<ITenderService, TenderService>();
        services.AddScoped<ISiteReportService, SiteReportService>();
        services.AddScoped<IProjectTaskService, ProjectTaskService>();
        services.AddScoped<IProjectMilestoneService, ProjectMilestoneService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IFleetService, FleetService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProcurementService, ProcurementService>();
        services.AddScoped<IStoresService, StoresService>();
        services.AddScoped<IBoqService, BoqService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<IDashboardService, CachedDashboardService>();
        services.AddScoped<ReportsService>();
        services.AddScoped<IReportsService, CachedReportsService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICompanySettingsService, CompanySettingsService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITimesheetService, TimesheetService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISafetyService, SafetyService>();
        services.AddScoped<IComplianceService, ComplianceService>();

        var storageProvider = configuration["FileStorage:Provider"] ?? "Local";
        if (storageProvider.Equals("Minio", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        else
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddHostedService<ScheduledBackupService>();
        services.AddHostedService<ScheduledSystemAlertService>();

        return services;
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisEnabled = configuration.GetValue("Redis:Enabled", false);
        if (redisEnabled)
        {
            var redisConnection = configuration.GetConnectionString("Redis")
                ?? configuration["Redis:ConnectionString"]
                ?? "localhost:6379";

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection.Contains("abortConnect", StringComparison.OrdinalIgnoreCase)
                    ? redisConnection
                    : $"{redisConnection},abortConnect=false";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }
    }
}

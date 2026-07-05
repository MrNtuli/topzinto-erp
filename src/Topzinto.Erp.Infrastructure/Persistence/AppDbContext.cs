using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Topzinto.Erp.Domain.Common;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Infrastructure.Identity;

namespace Topzinto.Erp.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientContact> ClientContacts => Set<ClientContact>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<SiteReport> SiteReports => Set<SiteReport>();
    public DbSet<SiteReportPhoto> SiteReportPhotos => Set<SiteReportPhoto>();
    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleMaintenance> VehicleMaintenance => Set<VehicleMaintenance>();
    public DbSet<FuelLog> FuelLogs => Set<FuelLog>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<EquipmentBooking> EquipmentBookings => Set<EquipmentBooking>();
    public DbSet<EquipmentInspection> EquipmentInspections => Set<EquipmentInspection>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<BoqItem> BoqItems => Set<BoqItem>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<DocumentRecord> Documents => Set<DocumentRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CompanySetting> CompanySettings => Set<CompanySetting>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<ChatChannel> ChatChannels => Set<ChatChannel>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatChannelRead> ChatChannelReads => Set<ChatChannelRead>();
    public DbSet<ChatChannelMember> ChatChannelMembers => Set<ChatChannelMember>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SafetyIncident> SafetyIncidents => Set<SafetyIncident>();
    public DbSet<ComplianceRecord> ComplianceRecords => Set<ComplianceRecord>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => new { x.EntityType, x.EntityId });
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100);
        });

        builder.Entity<Client>(e =>
        {
            e.HasIndex(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ClientContact>(e =>
        {
            e.HasOne(x => x.Client).WithMany(c => c.Contacts).HasForeignKey(x => x.ClientId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Project>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(300);
            e.Property(x => x.ContractValue).HasPrecision(18, 2);
            e.Property(x => x.Budget).HasPrecision(18, 2);
            e.HasOne(x => x.Client).WithMany(c => c.Projects).HasForeignKey(x => x.ClientId);
            e.HasOne(x => x.Tender).WithMany().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Contract).WithOne(c => c.Project).HasForeignKey<Contract>(c => c.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Contract>(e =>
        {
            e.HasIndex(x => x.ContractNumber);
            e.Property(x => x.ContractNumber).HasMaxLength(100);
            e.Property(x => x.Value).HasPrecision(18, 2);
            e.Property(x => x.RetentionPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Tender>(e =>
        {
            e.HasIndex(x => x.ClosingDate);
            e.Property(x => x.ReferenceNumber).HasMaxLength(100);
            e.Property(x => x.EstimatedValue).HasPrecision(18, 2);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<SiteReport>(e =>
        {
            e.HasIndex(x => new { x.ProjectId, x.ReportDate });
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<SiteReportPhoto>(e =>
        {
            e.HasOne(x => x.SiteReport).WithMany(x => x.Photos).HasForeignKey(x => x.SiteReportId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ProjectMilestone>(e =>
        {
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ProjectTask>(e =>
        {
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasOne(x => x.Milestone).WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Vehicle>(e =>
        {
            e.HasIndex(x => x.RegistrationNumber).IsUnique();
            e.Property(x => x.RegistrationNumber).HasMaxLength(20);
            e.HasOne(x => x.AssignedProject).WithMany().HasForeignKey(x => x.AssignedProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<VehicleMaintenance>(e =>
        {
            e.Property(x => x.Cost).HasPrecision(18, 2);
            e.HasOne(x => x.Vehicle).WithMany(v => v.MaintenanceRecords).HasForeignKey(x => x.VehicleId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<FuelLog>(e =>
        {
            e.Property(x => x.Cost).HasPrecision(18, 2);
            e.Property(x => x.Litres).HasPrecision(10, 2);
            e.HasOne(x => x.Vehicle).WithMany(v => v.FuelLogs).HasForeignKey(x => x.VehicleId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Equipment>(e =>
        {
            e.HasIndex(x => x.AssetTag).IsUnique();
            e.HasOne(x => x.AssignedProject).WithMany().HasForeignKey(x => x.AssignedProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<EquipmentBooking>(e =>
        {
            e.HasOne(x => x.Equipment).WithMany(eq => eq.Bookings).HasForeignKey(x => x.EquipmentId);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<EquipmentInspection>(e =>
        {
            e.HasOne(x => x.Equipment).WithMany(eq => eq.Inspections).HasForeignKey(x => x.EquipmentId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Supplier>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<PurchaseOrder>(e =>
        {
            e.HasIndex(x => x.PoNumber).IsUnique();
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(x => x.SupplierId);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<PurchaseOrderLine>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.LineTotal).HasPrecision(18, 2);
            e.HasOne(x => x.PurchaseOrder).WithMany(po => po.Lines).HasForeignKey(x => x.PurchaseOrderId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<InventoryItem>(e =>
        {
            e.HasIndex(x => x.ItemCode).IsUnique();
            e.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            e.Property(x => x.ReorderLevel).HasPrecision(18, 3);
            e.Property(x => x.UnitCost).HasPrecision(18, 2);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<InventoryTransaction>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.HasOne(x => x.InventoryItem).WithMany(i => i.Transactions).HasForeignKey(x => x.InventoryItemId);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.TransactionDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<BoqItem>(e =>
        {
            e.HasIndex(x => new { x.ProjectId, x.ItemCode });
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.Property(x => x.Rate).HasPrecision(18, 2);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Claim>(e =>
        {
            e.HasIndex(x => x.ClaimNumber).IsUnique();
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Invoice>(e =>
        {
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<DocumentRecord>(e =>
        {
            e.ToTable("Documents");
            e.HasIndex(x => x.ExpiryDate);
            e.HasIndex(x => new { x.ParentType, x.ParentId });
            e.Property(x => x.Title).HasMaxLength(300);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<Notification>(e =>
        {
            e.HasIndex(x => x.ReferenceKey).IsUnique();
            e.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<CompanySetting>(e =>
        {
            e.HasKey(x => x.Id);
        });

        builder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.EmployeeNumber).IsUnique();
            e.Property(x => x.EmployeeNumber).HasMaxLength(50);
            e.Property(x => x.FirstName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100);
            e.Property(x => x.HourlyRate).HasPrecision(18, 2);
            e.HasOne(x => x.AssignedProject).WithMany().HasForeignKey(x => x.AssignedProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<TimesheetEntry>(e =>
        {
            e.HasIndex(x => new { x.ProjectId, x.WorkDate });
            e.HasIndex(x => new { x.EmployeeId, x.WorkDate });
            e.Property(x => x.Hours).HasPrecision(6, 2);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ChatChannel>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ChatMessage>(e =>
        {
            e.Property(x => x.Content).HasMaxLength(4000);
            e.Property(x => x.SenderName).HasMaxLength(200);
            e.Property(x => x.AttachmentFileName).HasMaxLength(255);
            e.Property(x => x.AttachmentStoragePath).HasMaxLength(500);
            e.Property(x => x.AttachmentContentType).HasMaxLength(100);
            e.HasOne(x => x.Channel).WithMany(c => c.Messages).HasForeignKey(x => x.ChannelId);
            e.HasIndex(x => new { x.ChannelId, x.CreatedAt });
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ChatChannelRead>(e =>
        {
            e.HasOne(x => x.Channel).WithMany().HasForeignKey(x => x.ChannelId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.ChannelId }).IsUnique();
        });

        builder.Entity<ChatChannelMember>(e =>
        {
            e.HasOne(x => x.Channel).WithMany(c => c.Members).HasForeignKey(x => x.ChannelId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ChannelId, x.UserId }).IsUnique();
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => new { x.UserId, x.RevokedAt });
            e.Property(x => x.TokenHash).HasMaxLength(128);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SafetyIncident>(e =>
        {
            e.HasIndex(x => new { x.ProjectId, x.IncidentDate });
            e.Property(x => x.Title).HasMaxLength(300);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ComplianceRecord>(e =>
        {
            e.HasIndex(x => x.ExpiryDate);
            e.HasIndex(x => new { x.ProjectId, x.Type });
            e.Property(x => x.Title).HasMaxLength(300);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<AttendanceRecord>(e =>
        {
            e.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();
            e.HasIndex(x => new { x.ProjectId, x.WorkDate });
            e.Property(x => x.HoursWorked).HasPrecision(6, 2);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

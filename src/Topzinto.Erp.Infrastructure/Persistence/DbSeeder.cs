using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Identity;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await EnsureSchemaAsync(context);

        foreach (var role in SystemRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        const string adminEmail = "admin@topzinto.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Asiphe",
                LastName = "Gwala",
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(admin, "Topzinto@2024");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, SystemRoles.Director);
        }

        await SeedDemoUsersAsync(userManager);

        if (!await context.Clients.AnyAsync())
            await SeedBusinessDataAsync(context);

        if (!await context.SiteReports.AnyAsync())
            await SeedOperationsDataAsync(context);

        if (!await context.Vehicles.AnyAsync())
            await SeedAssetsDataAsync(context);

        if (!await context.Suppliers.AnyAsync())
            await SeedProcurementDataAsync(context);

        if (!await context.BoqItems.AnyAsync())
            await SeedFinancialDataAsync(context);

        if (!await context.Documents.AnyAsync())
            await SeedDocumentsDataAsync(context);

        if (!await context.Employees.AnyAsync())
            await SeedEmployeesDataAsync(context);

        if (!await context.TimesheetEntries.AnyAsync())
            await SeedTimesheetsDataAsync(context);

        if (!await context.ChatChannels.AnyAsync())
            await SeedChatDataAsync(context);

        await EnsureCompanySettingsAsync(context);

        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notifier.GenerateSystemAlertsAsync();
    }

    private static async Task EnsureCompanySettingsAsync(AppDbContext context)
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        if (await context.CompanySettings.AnyAsync()) return;
        context.CompanySettings.Add(new CompanySetting
        {
            Id = id,
            Address = "Durban, KwaZulu-Natal",
            City = "Durban",
            Province = "KZN",
            Phone = "+27 31 000 0000",
            Email = "info@topzinto.com",
            VatNumber = "4123456789",
            CidbNumber = "7CE",
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager)
    {
        const string defaultPassword = "Topzinto@2024";
        var demoUsers = new[]
        {
            ("pm@topzinto.com", "Thabo", "Mokoena", SystemRoles.ProjectManager),
            ("foreman@topzinto.com", "Sipho", "Nkosi", SystemRoles.Foreman),
            ("procurement@topzinto.com", "Lerato", "Dlamini", SystemRoles.Procurement),
        };

        foreach (var (email, first, last, role) in demoUsers)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FirstName = first,
                LastName = last,
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(user, defaultPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }

  private static async Task EnsureSchemaAsync(AppDbContext context)
    {
        // Legacy EnsureCreated DBs lack migration history — recreate so schema stays in sync.
        var applied = await context.Database.GetAppliedMigrationsAsync();
        if (!applied.Any() && await context.Database.CanConnectAsync())
        {
            try
            {
                if (await context.Clients.AnyAsync())
                    await context.Database.EnsureDeletedAsync();
            }
            catch
            {
                await context.Database.EnsureDeletedAsync();
            }
        }

        await context.Database.MigrateAsync();
    }

    private static async Task SeedProcurementDataAsync(AppDbContext context)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001");

        var suppliers = new[]
        {
            new Supplier { Code = "SUP-001", Name = "BuildMart Supplies", Category = SupplierCategory.Materials, Status = SupplierStatus.Active, ContactPerson = "David Naidoo", Phone = "031 555 1234", Email = "orders@buildmart.co.za", City = "Durban", Province = "KZN", VatNumber = "4123456789" },
            new Supplier { Code = "SUP-002", Name = "KZN Plant Hire", Category = SupplierCategory.PlantHire, Status = SupplierStatus.Active, ContactPerson = "Lungile Dube", Phone = "031 444 5678", Email = "hire@kznplant.co.za", City = "Pinetown", Province = "KZN" },
            new Supplier { Code = "SUP-003", Name = "Steel & Mesh Co", Category = SupplierCategory.Materials, Status = SupplierStatus.Active, ContactPerson = "Raj Patel", Phone = "031 333 9012", Email = "sales@steelandmesh.co.za", City = "Durban", Province = "KZN" },
            new Supplier { Code = "SUP-004", Name = "SafetyFirst PPE", Category = SupplierCategory.Services, Status = SupplierStatus.Active, ContactPerson = "Nomsa Khumalo", Phone = "031 222 3456", Email = "info@safetyfirst.co.za", City = "Durban", Province = "KZN" },
            new Supplier { Code = "SUP-005", Name = "Durban Concrete", Category = SupplierCategory.Materials, Status = SupplierStatus.Inactive, ContactPerson = "Mike Govender", Phone = "031 111 7890", City = "Durban", Province = "KZN" },
        };
        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();

        var pos = new[]
        {
            new PurchaseOrder
            {
                PoNumber = "PO-2024-001",
                Title = "Blockwork materials - Ridgeview Mall",
                SupplierId = suppliers[0].Id,
                ProjectId = project?.Id,
                Status = PoStatus.Delivered,
                OrderDate = DateTime.UtcNow.AddDays(-45),
                RequiredDate = DateTime.UtcNow.AddDays(-30),
                TotalAmount = 185_400,
                RequestedByName = "Site Supervisor",
                ApprovedByName = "Project Manager",
            },
            new PurchaseOrder
            {
                PoNumber = "PO-2024-002",
                Title = "Reinforcement steel - Level 2 slab",
                SupplierId = suppliers[2].Id,
                ProjectId = project?.Id,
                Status = PoStatus.Approved,
                OrderDate = DateTime.UtcNow.AddDays(-5),
                RequiredDate = DateTime.UtcNow.AddDays(7),
                TotalAmount = 342_800,
                RequestedByName = "QS",
                ApprovedByName = "Director",
            },
            new PurchaseOrder
            {
                PoNumber = "PO-2024-003",
                Title = "TLB hire - Impendle Road",
                SupplierId = suppliers[1].Id,
                Status = PoStatus.PendingApproval,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                RequiredDate = DateTime.UtcNow.AddDays(14),
                TotalAmount = 96_000,
                RequestedByName = "Project Manager",
            },
            new PurchaseOrder
            {
                PoNumber = "PO-2024-004",
                Title = "PPE replenishment - all sites",
                SupplierId = suppliers[3].Id,
                Status = PoStatus.Ordered,
                OrderDate = DateTime.UtcNow.AddDays(-10),
                RequiredDate = DateTime.UtcNow.AddDays(3),
                TotalAmount = 28_500,
                RequestedByName = "Safety Officer",
                ApprovedByName = "Fleet Manager",
            },
        };
        context.PurchaseOrders.AddRange(pos);
        await context.SaveChangesAsync();

        context.PurchaseOrderLines.AddRange(
            new PurchaseOrderLine { PurchaseOrderId = pos[0].Id, Description = "Stock bricks 222x106x73", Quantity = 12000, Unit = "ea", UnitPrice = 4.50m, LineTotal = 54_000 },
            new PurchaseOrderLine { PurchaseOrderId = pos[0].Id, Description = "Building sand", Quantity = 45, Unit = "m³", UnitPrice = 420, LineTotal = 18_900 },
            new PurchaseOrderLine { PurchaseOrderId = pos[0].Id, Description = "Cement 50kg bags", Quantity = 800, Unit = "bag", UnitPrice = 89, LineTotal = 71_200 },
            new PurchaseOrderLine { PurchaseOrderId = pos[1].Id, Description = "Y12 rebar 12m", Quantity = 450, Unit = "bar", UnitPrice = 285, LineTotal = 128_250 },
            new PurchaseOrderLine { PurchaseOrderId = pos[1].Id, Description = "Y16 rebar 12m", Quantity = 320, Unit = "bar", UnitPrice = 420, LineTotal = 134_400 },
            new PurchaseOrderLine { PurchaseOrderId = pos[2].Id, Description = "JCB 3CX TLB - monthly hire", Quantity = 1, Unit = "month", UnitPrice = 96_000, LineTotal = 96_000 },
            new PurchaseOrderLine { PurchaseOrderId = pos[3].Id, Description = "Hard hats", Quantity = 50, Unit = "ea", UnitPrice = 120, LineTotal = 6_000 },
            new PurchaseOrderLine { PurchaseOrderId = pos[3].Id, Description = "Safety boots", Quantity = 30, Unit = "pair", UnitPrice = 450, LineTotal = 13_500 },
            new PurchaseOrderLine { PurchaseOrderId = pos[3].Id, Description = "Reflective vests", Quantity = 60, Unit = "ea", UnitPrice = 85, LineTotal = 5_100 }
        );

        var items = new[]
        {
            new InventoryItem { ItemCode = "INV-001", Name = "Cement 50kg", Category = "Building Materials", Unit = "bag", QuantityOnHand = 240, ReorderLevel = 100, Location = "Main Store", UnitCost = 89 },
            new InventoryItem { ItemCode = "INV-002", Name = "Y12 Rebar 12m", Category = "Steel", Unit = "bar", QuantityOnHand = 85, ReorderLevel = 50, Location = "Steel Yard", UnitCost = 285 },
            new InventoryItem { ItemCode = "INV-003", Name = "Hard Hat - White", Category = "PPE", Unit = "ea", QuantityOnHand = 12, ReorderLevel = 25, Location = "PPE Store", UnitCost = 120 },
            new InventoryItem { ItemCode = "INV-004", Name = "Diesel 50ppm", Category = "Fuel", Unit = "litre", QuantityOnHand = 3500, ReorderLevel = 1000, Location = "Fuel Tank", UnitCost = 22.50m },
            new InventoryItem { ItemCode = "INV-005", Name = "Formwork plywood 18mm", Category = "Formwork", Unit = "sheet", QuantityOnHand = 45, ReorderLevel = 30, Location = "Yard B", UnitCost = 680 },
        };
        context.InventoryItems.AddRange(items);
        await context.SaveChangesAsync();

        context.InventoryTransactions.AddRange(
            new InventoryTransaction { InventoryItemId = items[0].Id, TransactionType = InventoryTransactionType.StockIn, Quantity = 800, TransactionDate = DateTime.UtcNow.AddDays(-40), Reference = "PO-2024-001", ProjectId = project?.Id, RecordedByName = "Store Controller" },
            new InventoryTransaction { InventoryItemId = items[0].Id, TransactionType = InventoryTransactionType.StockOut, Quantity = 560, TransactionDate = DateTime.UtcNow.AddDays(-15), Reference = "Issue Ridgeview", ProjectId = project?.Id, RecordedByName = "Store Controller" },
            new InventoryTransaction { InventoryItemId = items[2].Id, TransactionType = InventoryTransactionType.StockOut, Quantity = 38, TransactionDate = DateTime.UtcNow.AddDays(-7), Reference = "Site issue", ProjectId = project?.Id, RecordedByName = "Store Controller" },
            new InventoryTransaction { InventoryItemId = items[3].Id, TransactionType = InventoryTransactionType.StockIn, Quantity = 2000, TransactionDate = DateTime.UtcNow.AddDays(-3), Reference = "Fuel delivery", RecordedByName = "Fleet Manager" }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedFinancialDataAsync(AppDbContext context)
    {
        var projects = await context.Projects.ToListAsync();
        var ridgeview = projects.FirstOrDefault(p => p.Code == "PRJ-001") ?? projects.First();
        var impendle = projects.FirstOrDefault(p => p.Code == "PRJ-003") ?? ridgeview;

        context.BoqItems.AddRange(
            new BoqItem { ProjectId = ridgeview.Id, ItemCode = "BOQ-001", Description = "Excavation and earthworks", Category = "Earthworks", Unit = "m³", Quantity = 2500, Rate = 185, Amount = 462_500 },
            new BoqItem { ProjectId = ridgeview.Id, ItemCode = "BOQ-002", Description = "Concrete works - foundations", Category = "Concrete", Unit = "m³", Quantity = 680, Rate = 3200, Amount = 2_176_000 },
            new BoqItem { ProjectId = ridgeview.Id, ItemCode = "BOQ-003", Description = "Structural steel supply & erect", Category = "Steel", Unit = "ton", Quantity = 145, Rate = 28_500, Amount = 4_132_500 },
            new BoqItem { ProjectId = ridgeview.Id, ItemCode = "BOQ-004", Description = "Blockwork - external walls", Category = "Masonry", Unit = "m²", Quantity = 3200, Rate = 420, Amount = 1_344_000 },
            new BoqItem { ProjectId = ridgeview.Id, ItemCode = "BOQ-005", Description = "Roofing - IBR sheeting", Category = "Roofing", Unit = "m²", Quantity = 1800, Rate = 680, Amount = 1_224_000 },
            new BoqItem { ProjectId = impendle.Id, ItemCode = "BOQ-101", Description = "Road base layer", Category = "Earthworks", Unit = "m³", Quantity = 4200, Rate = 220, Amount = 924_000 },
            new BoqItem { ProjectId = impendle.Id, ItemCode = "BOQ-102", Description = "Asphalt surfacing", Category = "Paving", Unit = "m²", Quantity = 8500, Rate = 195, Amount = 1_657_500 }
        );

        context.Claims.AddRange(
            new Claim { ProjectId = ridgeview.Id, ClaimNumber = "CLM-2024-001", Title = "Interim Claim No. 8 - Superstructure", ClaimDate = DateTime.UtcNow.AddDays(-14), PeriodFrom = DateTime.UtcNow.AddMonths(-1), PeriodTo = DateTime.UtcNow.AddDays(-15), Amount = 4_850_000, Status = ClaimStatus.Approved, SubmittedByName = "QS" },
            new Claim { ProjectId = ridgeview.Id, ClaimNumber = "CLM-2024-002", Title = "Interim Claim No. 9 - Blockwork & roofing", ClaimDate = DateTime.UtcNow.AddDays(-3), PeriodFrom = DateTime.UtcNow.AddDays(-14), PeriodTo = DateTime.UtcNow.AddDays(-1), Amount = 3_200_000, Status = ClaimStatus.Submitted, SubmittedByName = "QS" },
            new Claim { ProjectId = impendle.Id, ClaimNumber = "CLM-2024-003", Title = "Monthly Claim - Earthworks phase", ClaimDate = DateTime.UtcNow.AddDays(-7), Amount = 1_150_000, Status = ClaimStatus.Submitted, SubmittedByName = "PM" },
            new Claim { ProjectId = ridgeview.Id, ClaimNumber = "CLM-2023-012", Title = "Final Claim - Foundations", ClaimDate = DateTime.UtcNow.AddMonths(-3), Amount = 2_400_000, Status = ClaimStatus.Paid, SubmittedByName = "QS" }
        );

        context.Invoices.AddRange(
            new Invoice { ProjectId = ridgeview.Id, InvoiceNumber = "INV-2024-045", InvoiceDate = DateTime.UtcNow.AddDays(-20), DueDate = DateTime.UtcNow.AddDays(10), Amount = 4_850_000, Status = InvoiceStatus.Sent },
            new Invoice { ProjectId = ridgeview.Id, InvoiceNumber = "INV-2024-038", InvoiceDate = DateTime.UtcNow.AddDays(-45), DueDate = DateTime.UtcNow.AddDays(-15), Amount = 3_600_000, Status = InvoiceStatus.Overdue },
            new Invoice { ProjectId = impendle.Id, InvoiceNumber = "INV-2024-041", InvoiceDate = DateTime.UtcNow.AddDays(-30), DueDate = DateTime.UtcNow.AddDays(-5), Amount = 980_000, Status = InvoiceStatus.Paid },
            new Invoice { ProjectId = ridgeview.Id, InvoiceNumber = "INV-2024-032", InvoiceDate = DateTime.UtcNow.AddMonths(-2), DueDate = DateTime.UtcNow.AddMonths(-1), Amount = 2_400_000, Status = InvoiceStatus.Paid }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedDocumentsDataAsync(AppDbContext context)
    {
        var ridgeview = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001");
        var contract = await context.Contracts.FirstOrDefaultAsync(c => c.ContractNumber == "CON-2024-001");
        var tender = await context.Tenders.FirstOrDefaultAsync(t => t.ReferenceNumber == "TEN-2024-001");

        context.Documents.AddRange(
            new DocumentRecord { Title = "Main Contract Agreement", Category = "Contract", ParentType = DocumentParentType.Contract, ParentId = contract?.Id, ParentName = contract?.Title ?? "Ridgeview Contract", FileName = "CON-2024-001-signed.pdf", IssueDate = new DateTime(2023, 6, 1), ExpiryDate = new DateTime(2024, 12, 15), Status = DocumentStatus.Approved, Version = 1 },
            new DocumentRecord { Title = "Professional Indemnity Insurance", Category = "Insurance", ParentType = DocumentParentType.Company, ParentName = "TopZinto CC", FileName = "PI-Insurance-2024.pdf", IssueDate = DateTime.UtcNow.AddMonths(-6), ExpiryDate = DateTime.UtcNow.AddDays(18), Status = DocumentStatus.Approved, Version = 1 },
            new DocumentRecord { Title = "CIDB Registration Certificate", Category = "Compliance", ParentType = DocumentParentType.Company, ParentName = "TopZinto CC", FileName = "CIDB-7CE.pdf", IssueDate = DateTime.UtcNow.AddYears(-1), ExpiryDate = DateTime.UtcNow.AddMonths(4), Status = DocumentStatus.Approved, Version = 2 },
            new DocumentRecord { Title = "Site Safety File", Category = "H&S", ParentType = DocumentParentType.Project, ParentId = ridgeview?.Id, ParentName = ridgeview?.Name, FileName = "Ridgeview-Safety-File.pdf", IssueDate = DateTime.UtcNow.AddMonths(-3), ExpiryDate = DateTime.UtcNow.AddDays(45), Status = DocumentStatus.Approved },
            new DocumentRecord { Title = "Tender Compliance Pack", Category = "Tender", ParentType = DocumentParentType.Tender, ParentId = tender?.Id, ParentName = tender?.Title, FileName = "TEN-2024-001-compliance.zip", IssueDate = DateTime.UtcNow.AddDays(-20), Status = DocumentStatus.PendingApproval },
            new DocumentRecord { Title = "Structural Drawings Rev C", Category = "Drawings", ParentType = DocumentParentType.Project, ParentId = ridgeview?.Id, ParentName = ridgeview?.Name, FileName = "Ridgeview-Struct-RevC.dwg", IssueDate = DateTime.UtcNow.AddMonths(-2), Status = DocumentStatus.Approved, Version = 3 },
            new DocumentRecord { Title = "Tax Clearance Certificate", Category = "Compliance", ParentType = DocumentParentType.Company, ParentName = "TopZinto CC", FileName = "Tax-Clearance-2023.pdf", IssueDate = new DateTime(2023, 1, 15), ExpiryDate = new DateTime(2023, 12, 31), Status = DocumentStatus.Expired }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedAssetsDataAsync(AppDbContext context)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001");

        var vehicles = new[]
        {
            new Vehicle { RegistrationNumber = "ND 123 GP", MakeModel = "Toyota Hilux 2.8 GD-6", Type = VehicleType.Bakkie, Status = VehicleStatus.InUse, DriverName = "Sipho Nkosi", LicenseExpiryDate = DateTime.UtcNow.AddMonths(8), InsuranceExpiryDate = DateTime.UtcNow.AddMonths(5), CurrentLocation = "Ridgeview Mall Site", AssignedProjectId = project?.Id },
            new Vehicle { RegistrationNumber = "ND 456 GP", MakeModel = "Isuzu FVZ 1400 Tipper", Type = VehicleType.TipperTruck, Status = VehicleStatus.InUse, DriverName = "Thabo Mthembu", LicenseExpiryDate = DateTime.UtcNow.AddDays(20), InsuranceExpiryDate = DateTime.UtcNow.AddMonths(3), CurrentLocation = "Impendle Road Site", AssignedProjectId = project?.Id },
            new Vehicle { RegistrationNumber = "ND 789 GP", MakeModel = "Ford Ranger 3.2 TDCi", Type = VehicleType.LDV, Status = VehicleStatus.Available, DriverName = "Unassigned", LicenseExpiryDate = DateTime.UtcNow.AddMonths(10), InsuranceExpiryDate = DateTime.UtcNow.AddMonths(7), CurrentLocation = "Head Office" },
            new Vehicle { RegistrationNumber = "ND 321 GP", MakeModel = "Mercedes-Benz Actros", Type = VehicleType.HeavyVehicle, Status = VehicleStatus.Maintenance, DriverName = "—", LicenseExpiryDate = DateTime.UtcNow.AddMonths(4), InsuranceExpiryDate = DateTime.UtcNow.AddMonths(2), CurrentLocation = "Workshop" },
            new Vehicle { RegistrationNumber = "ND 654 GP", MakeModel = "Water Tanker 15000L", Type = VehicleType.WaterTanker, Status = VehicleStatus.InUse, DriverName = "Bongani Zulu", LicenseExpiryDate = DateTime.UtcNow.AddMonths(6), InsuranceExpiryDate = DateTime.UtcNow.AddDays(15), CurrentLocation = "Mau Mau Housing", AssignedProjectId = project?.Id },
        };
        context.Vehicles.AddRange(vehicles);
        await context.SaveChangesAsync();

        context.VehicleMaintenance.Add(new VehicleMaintenance
        {
            VehicleId = vehicles[3].Id,
            ServiceDate = DateTime.UtcNow.AddDays(-5),
            Description = "Engine overhaul and brake replacement",
            Cost = 45_000,
            NextServiceDue = DateTime.UtcNow.AddMonths(6),
            ServiceProvider = "Mercedes-Benz Durban",
        });

        var equipment = new[]
        {
            new Equipment { AssetTag = "EQ-001", Name = "CAT 320 Excavator", Category = EquipmentCategory.Excavator, Status = EquipmentStatus.InUse, OperatorName = "John Dlamini", NextServiceDue = DateTime.UtcNow.AddMonths(2), NextInspectionDue = DateTime.UtcNow.AddDays(25), AssignedProjectId = project?.Id },
            new Equipment { AssetTag = "EQ-002", Name = "JCB 3CX TLB", Category = EquipmentCategory.TLB, Status = EquipmentStatus.InUse, OperatorName = "Peter Khumalo", NextServiceDue = DateTime.UtcNow.AddMonths(4), AssignedProjectId = project?.Id },
            new Equipment { AssetTag = "EQ-003", Name = "Bomag BW 120 Roller", Category = EquipmentCategory.Roller, Status = EquipmentStatus.Available, NextServiceDue = DateTime.UtcNow.AddMonths(6) },
            new Equipment { AssetTag = "EQ-004", Name = "Caterpillar 140G Grader", Category = EquipmentCategory.Grader, Status = EquipmentStatus.Maintenance, NextInspectionDue = DateTime.UtcNow.AddDays(10) },
            new Equipment { AssetTag = "EQ-005", Name = "Atlas Copco Generator 100kVA", Category = EquipmentCategory.Generator, Status = EquipmentStatus.InUse, AssignedProjectId = project?.Id },
        };
        context.Equipment.AddRange(equipment);
        await context.SaveChangesAsync();

        if (project is not null)
        {
            context.EquipmentBookings.Add(new EquipmentBooking
            {
                EquipmentId = equipment[0].Id,
                ProjectId = project.Id,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BookedByName = "Project Manager",
            });
        }
    }

    private static async Task SeedOperationsDataAsync(AppDbContext context)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001")
            ?? await context.Projects.FirstAsync();

        context.SiteReports.Add(new SiteReport
        {
            ProjectId = project.Id,
            ReportDate = DateTime.UtcNow.Date,
            Weather = "Sunny",
            Temperature = "28°C",
            WindSpeed = "12 km/h",
            PersonnelCount = 24,
            WorkCompleted = "Completed formwork for Level 2 slab. Installed reinforcement for columns C4-C8.",
            WorkPlanned = "Pour concrete for Level 2 slab. Continue blockwork on east elevation.",
            DelaysIssues = "Minor delay: concrete truck arrived 45 minutes late.",
            Status = SiteReportStatus.Submitted,
            SubmittedByName = "Site Supervisor",
            SubmittedAt = DateTime.UtcNow,
        });

        var milestones = new[]
        {
            new ProjectMilestone { ProjectId = project.Id, Name = "Site Establishment", StartDate = new DateTime(2023, 6, 1), DueDate = new DateTime(2023, 7, 15), Status = MilestoneStatus.Completed, Progress = 100, SortOrder = 1 },
            new ProjectMilestone { ProjectId = project.Id, Name = "Foundation Works", StartDate = new DateTime(2023, 7, 16), DueDate = new DateTime(2023, 10, 30), Status = MilestoneStatus.Completed, Progress = 100, SortOrder = 2 },
            new ProjectMilestone { ProjectId = project.Id, Name = "Superstructure", StartDate = new DateTime(2023, 11, 1), DueDate = new DateTime(2024, 8, 31), Status = MilestoneStatus.InProgress, Progress = 68, SortOrder = 3 },
            new ProjectMilestone { ProjectId = project.Id, Name = "Roofing & Finishes", StartDate = new DateTime(2024, 9, 1), DueDate = new DateTime(2024, 12, 15), Status = MilestoneStatus.Planned, Progress = 0, SortOrder = 4 },
        };
        context.ProjectMilestones.AddRange(milestones);

        context.ProjectTasks.AddRange(
            new ProjectTask { ProjectId = project.Id, Title = "Submit RFI for steel specification", DueDate = DateTime.UtcNow.AddDays(-2), Priority = TaskPriority.High, Status = ProjectTaskStatus.Overdue, AssignedToName = "PM" },
            new ProjectTask { ProjectId = project.Id, Title = "Order blockwork materials", DueDate = DateTime.UtcNow.AddDays(3), Priority = TaskPriority.Medium, Status = ProjectTaskStatus.InProgress, AssignedToName = "Procurement" },
            new ProjectTask { ProjectId = project.Id, Title = "Safety inspection - scaffolding", DueDate = DateTime.UtcNow.AddDays(7), Priority = TaskPriority.Critical, Status = ProjectTaskStatus.NotStarted, AssignedToName = "Safety Officer" }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedBusinessDataAsync(AppDbContext context)
    {
        var clients = new Dictionary<string, Client>
        {
            ["private"] = new() { Name = "Private", Type = ClientType.Private, City = "Durban" },
            ["siyagroup"] = new() { Name = "Siyagroup", Type = ClientType.Private, City = "Johannesburg" },
            ["impendle"] = new() { Name = "Impendle LM", Type = ClientType.Municipal, Province = "KZN" },
            ["dpw"] = new() { Name = "DPW KZN", Type = ClientType.Government, Province = "KZN" },
            ["ethekwini"] = new() { Name = "Ethekwini M", Type = ClientType.Municipal, City = "Durban" },
        };

        context.Clients.AddRange(clients.Values);
        await context.SaveChangesAsync();

        var projects = new[]
        {
            new Project
            {
                Code = "PRJ-001",
                Name = "Ridgeview Mall Construction",
                ClientId = clients["private"].Id,
                Status = ProjectStatus.Active,
                Progress = 68,
                EndDate = new DateTime(2024, 12, 15),
                ContractValue = 45_600_000,
                Budget = 42_000_000,
                SiteLocation = "Ridgeview, Durban",
            },
            new Project
            {
                Code = "PRJ-002",
                Name = "Mau Mau Housing Project",
                ClientId = clients["siyagroup"].Id,
                Status = ProjectStatus.Active,
                Progress = 55,
                EndDate = new DateTime(2025, 3, 20),
                ContractValue = 12_300_000,
                Budget = 11_500_000,
            },
            new Project
            {
                Code = "PRJ-003",
                Name = "Impendle Road Upgrade",
                ClientId = clients["impendle"].Id,
                Status = ProjectStatus.Active,
                Progress = 42,
                EndDate = new DateTime(2025, 6, 30),
                ContractValue = 8_750_000,
                Budget = 8_200_000,
            },
            new Project
            {
                Code = "PRJ-004",
                Name = "DPW KZN Office Block",
                ClientId = clients["dpw"].Id,
                Status = ProjectStatus.Active,
                Progress = 35,
                EndDate = new DateTime(2025, 9, 10),
                ContractValue = 28_400_000,
                Budget = 26_000_000,
            },
            new Project
            {
                Code = "PRJ-005",
                Name = "Ethekwini Water Pipeline",
                ClientId = clients["ethekwini"].Id,
                Status = ProjectStatus.Completed,
                Progress = 100,
                EndDate = new DateTime(2023, 7, 10),
                ContractValue = 444_440,
                Budget = 420_000,
            },
        };

        context.Projects.AddRange(projects);

        context.Tenders.AddRange(
            new Tender
            {
                ReferenceNumber = "TEN-2024-001",
                Title = "Municipal Roads Maintenance Framework",
                ClientId = clients["ethekwini"].Id,
                ClosingDate = DateTime.UtcNow.AddDays(14),
                Status = TenderStatus.Preparing,
                EstimatedValue = 15_000_000,
            },
            new Tender
            {
                ReferenceNumber = "TEN-2024-002",
                Title = "School Renovation Programme",
                ClientId = clients["dpw"].Id,
                ClosingDate = DateTime.UtcNow.AddDays(30),
                Status = TenderStatus.Identified,
                EstimatedValue = 8_500_000,
            }
        );

        context.Contracts.AddRange(
            new Contract
            {
                ContractNumber = "CON-2024-001",
                Title = "Ridgeview Mall Main Contract",
                ClientId = clients["private"].Id,
                ProjectId = projects[0].Id,
                Value = 45_600_000,
                StartDate = new DateTime(2023, 6, 1),
                EndDate = new DateTime(2024, 12, 15),
                RetentionPercent = 5,
                Status = ContractStatus.Active,
            },
            new Contract
            {
                ContractNumber = "CON-2023-015",
                Title = "Ethekwini Water Pipeline Contract",
                ClientId = clients["ethekwini"].Id,
                ProjectId = projects[4].Id,
                Value = 444_440,
                Status = ContractStatus.Completed,
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedEmployeesDataAsync(AppDbContext context)
    {
        var ridgeview = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001");
        var impendle = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-003");

        context.Employees.AddRange(
            new Employee
            {
                EmployeeNumber = "EMP-001",
                FirstName = "Sipho",
                LastName = "Mthembu",
                JobTitle = "Site Foreman",
                Department = EmployeeDepartment.Site,
                Trade = EmployeeTrade.Foreman,
                Status = EmploymentStatus.Active,
                Phone = "082 555 1001",
                HireDate = new DateTime(2021, 3, 15),
                AssignedProjectId = ridgeview?.Id,
                HourlyRate = 185,
            },
            new Employee
            {
                EmployeeNumber = "EMP-002",
                FirstName = "Thabo",
                LastName = "Nkosi",
                JobTitle = "Bricklayer",
                Department = EmployeeDepartment.Site,
                Trade = EmployeeTrade.Bricklayer,
                Status = EmploymentStatus.Active,
                Phone = "083 555 1002",
                HireDate = new DateTime(2022, 1, 10),
                AssignedProjectId = ridgeview?.Id,
                HourlyRate = 95,
            },
            new Employee
            {
                EmployeeNumber = "EMP-003",
                FirstName = "Lerato",
                LastName = "Mokoena",
                JobTitle = "Steel Fixer",
                Department = EmployeeDepartment.Site,
                Trade = EmployeeTrade.SteelFixer,
                Status = EmploymentStatus.Active,
                Phone = "084 555 1003",
                HireDate = new DateTime(2020, 8, 20),
                AssignedProjectId = ridgeview?.Id,
                HourlyRate = 110,
            },
            new Employee
            {
                EmployeeNumber = "EMP-004",
                FirstName = "Bongani",
                LastName = "Zulu",
                JobTitle = "TLB Operator",
                Department = EmployeeDepartment.Site,
                Trade = EmployeeTrade.Operator,
                Status = EmploymentStatus.OnLeave,
                Phone = "081 555 1004",
                HireDate = new DateTime(2019, 5, 1),
                AssignedProjectId = impendle?.Id,
                HourlyRate = 120,
            },
            new Employee
            {
                EmployeeNumber = "EMP-005",
                FirstName = "Nokuthula",
                LastName = "Dlamini",
                JobTitle = "HR Administrator",
                Department = EmployeeDepartment.Administration,
                Trade = EmployeeTrade.General,
                Status = EmploymentStatus.Active,
                Email = "hr@topzinto.com",
                Phone = "031 555 2001",
                HireDate = new DateTime(2018, 2, 1),
                HourlyRate = 150,
            },
            new Employee
            {
                EmployeeNumber = "EMP-006",
                FirstName = "Mandla",
                LastName = "Gumede",
                JobTitle = "Fleet Driver",
                Department = EmployeeDepartment.Fleet,
                Trade = EmployeeTrade.Driver,
                Status = EmploymentStatus.Active,
                Phone = "082 555 3001",
                HireDate = new DateTime(2021, 11, 1),
                HourlyRate = 85,
            },
            new Employee
            {
                EmployeeNumber = "EMP-007",
                FirstName = "Zanele",
                LastName = "Khumalo",
                JobTitle = "Safety Officer",
                Department = EmployeeDepartment.Safety,
                Trade = EmployeeTrade.Supervisor,
                Status = EmploymentStatus.Active,
                Phone = "083 555 4001",
                HireDate = new DateTime(2020, 4, 15),
                HourlyRate = 175,
            },
            new Employee
            {
                EmployeeNumber = "EMP-008",
                FirstName = "Peter",
                LastName = "van Wyk",
                JobTitle = "Electrician",
                Department = EmployeeDepartment.Site,
                Trade = EmployeeTrade.Electrician,
                Status = EmploymentStatus.Terminated,
                Phone = "084 555 5001",
                HireDate = new DateTime(2017, 6, 1),
                TerminationDate = new DateTime(2024, 1, 31),
                HourlyRate = 130,
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedTimesheetsDataAsync(AppDbContext context)
    {
        var ridgeview = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001");
        var impendle = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-003");
        var employees = await context.Employees.Where(e => e.Department == EmployeeDepartment.Site).Take(4).ToListAsync();
        if (employees.Count == 0 || ridgeview is null) return;

        var e0 = employees[0];
        var e1 = employees.Count > 1 ? employees[1] : e0;
        var e2 = employees.Count > 2 ? employees[2] : e0;

        context.TimesheetEntries.AddRange(
            new TimesheetEntry { EmployeeId = e0.Id, ProjectId = ridgeview.Id, WorkDate = DateTime.UtcNow.AddDays(-5), Hours = 9, Status = TimesheetStatus.Approved, Description = "Blockwork supervision" },
            new TimesheetEntry { EmployeeId = e1.Id, ProjectId = ridgeview.Id, WorkDate = DateTime.UtcNow.AddDays(-5), Hours = 8, Status = TimesheetStatus.Approved, Description = "Bricklaying - north wing" },
            new TimesheetEntry { EmployeeId = e2.Id, ProjectId = ridgeview.Id, WorkDate = DateTime.UtcNow.AddDays(-4), Hours = 8.5m, Status = TimesheetStatus.Approved, Description = "Steel fixing level 2" },
            new TimesheetEntry { EmployeeId = e1.Id, ProjectId = ridgeview.Id, WorkDate = DateTime.UtcNow.AddDays(-3), Hours = 8, Status = TimesheetStatus.Submitted, Description = "Bricklaying - east elevation" },
            new TimesheetEntry { EmployeeId = e0.Id, ProjectId = ridgeview.Id, WorkDate = DateTime.UtcNow.AddDays(-2), Hours = 9, Status = TimesheetStatus.Submitted, Description = "Site coordination" }
        );

        if (impendle is not null && employees.Count > 3)
        {
            context.TimesheetEntries.Add(
                new TimesheetEntry { EmployeeId = employees[3].Id, ProjectId = impendle.Id, WorkDate = DateTime.UtcNow.AddDays(-4), Hours = 10, Status = TimesheetStatus.Approved, Description = "TLB operation - road base" });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedChatDataAsync(AppDbContext context)
    {
        var ridgeview = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-001");
        var impendle = await context.Projects.FirstOrDefaultAsync(p => p.Code == "PRJ-003");
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@topzinto.com");
        var senderId = admin?.Id ?? Guid.Empty;
        var senderName = admin is not null ? $"{admin.FirstName} {admin.LastName}" : "TopZinto Admin";

        var general = new ChatChannel { Name = "General", Slug = "general", Type = ChatChannelType.General, Description = "Company-wide updates" };
        var siteOps = new ChatChannel { Name = "Site Operations", Slug = "site-ops", Type = ChatChannelType.Department, Department = EmployeeDepartment.Site, Description = "Site foremen and crew coordination" };
        var procurement = new ChatChannel { Name = "Procurement", Slug = "procurement", Type = ChatChannelType.Department, Department = EmployeeDepartment.Procurement, Description = "POs, suppliers, deliveries" };
        var fleet = new ChatChannel { Name = "Fleet", Slug = "fleet", Type = ChatChannelType.Department, Department = EmployeeDepartment.Fleet, Description = "Vehicles, plant, fuel" };
        var hr = new ChatChannel { Name = "HR & Admin", Slug = "hr", Type = ChatChannelType.Department, Department = EmployeeDepartment.Administration, Description = "HR, leave, admin" };

        context.ChatChannels.AddRange(general, siteOps, procurement, fleet, hr);

        if (ridgeview is not null)
        {
            context.ChatChannels.Add(new ChatChannel
            {
                Name = $"Project: {ridgeview.Name}",
                Slug = "prj-ridgeview",
                Type = ChatChannelType.Project,
                ProjectId = ridgeview.Id,
                Description = "Ridgeview Mall site channel",
            });
        }

        if (impendle is not null)
        {
            context.ChatChannels.Add(new ChatChannel
            {
                Name = $"Project: {impendle.Name}",
                Slug = "prj-impendle",
                Type = ChatChannelType.Project,
                ProjectId = impendle.Id,
                Description = "Impendle Road project channel",
            });
        }

        await context.SaveChangesAsync();

        var channels = await context.ChatChannels.ToListAsync();
        var generalCh = channels.First(c => c.Slug == "general");
        var siteCh = channels.First(c => c.Slug == "site-ops");
        var procCh = channels.First(c => c.Slug == "procurement");
        var projCh = channels.FirstOrDefault(c => c.Slug == "prj-ridgeview");

        context.ChatMessages.AddRange(
            new ChatMessage { ChannelId = generalCh.Id, SenderUserId = senderId, SenderName = senderName, Content = "Welcome to TopZinto Chat — use this for work communication instead of WhatsApp." },
            new ChatMessage { ChannelId = generalCh.Id, SenderUserId = senderId, SenderName = senderName, Content = "All project and department channels are on the left. Messages are stored for audit." },
            new ChatMessage { ChannelId = siteCh.Id, SenderUserId = senderId, SenderName = senderName, Content = "Blockwork on north wing starts Monday — confirm crew allocation." },
            new ChatMessage { ChannelId = siteCh.Id, SenderUserId = senderId, SenderName = "Site Foreman", Content = "Copy. Sipho and team will be on site from 07:00." },
            new ChatMessage { ChannelId = procCh.Id, SenderUserId = senderId, SenderName = senderName, Content = "PO-2024-002 steel delivery expected Tuesday — store to confirm receipt." }
        );

        if (projCh is not null)
        {
            context.ChatMessages.Add(new ChatMessage
            {
                ChannelId = projCh.Id,
                SenderUserId = senderId,
                SenderName = senderName,
                Content = "Ridgeview level 2 slab pour scheduled for next week. QS to confirm BOQ quantities.",
            });
        }

        await context.SaveChangesAsync();
    }
}

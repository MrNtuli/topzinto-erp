using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Topzinto.Erp.Application.DTOs.Documents;
using Topzinto.Erp.Application.Interfaces;
using Topzinto.Erp.Domain.Entities;
using Topzinto.Erp.Domain.Enums;
using Topzinto.Erp.Infrastructure.Common;
using Topzinto.Erp.Infrastructure.Persistence;

namespace Topzinto.Erp.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IFileStorageService _files;

    public DocumentService(AppDbContext db, IAuditService audit, IFileStorageService files)
    {
        _db = db;
        _audit = audit;
        _files = files;
    }

    public async Task<DocumentSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var docs = await _db.Documents.ToListAsync(ct);
        var today = DateTime.UtcNow.Date;
        var soon = today.AddDays(30);
        return new DocumentSummaryDto(
            docs.Count,
            docs.Count(d => d.ExpiryDate is not null && d.ExpiryDate.Value.Date <= soon && d.ExpiryDate.Value.Date >= today),
            docs.Count(d => d.ExpiryDate is not null && d.ExpiryDate.Value.Date < today)
        );
    }

    public async Task<IReadOnlyList<DocumentDto>> GetAllAsync(string? search = null, string? parentType = null, Guid? parentId = null, bool? expiringOnly = null, CancellationToken ct = default)
    {
        var query = _db.Documents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Title.Contains(search) || d.Category.Contains(search) || (d.FileName != null && d.FileName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(parentType))
        {
            var pt = DocumentDisplay.ParseParentType(parentType);
            query = query.Where(d => d.ParentType == pt);
        }

        if (parentId.HasValue)
            query = query.Where(d => d.ParentId == parentId.Value);

        var list = await query.OrderBy(d => d.Title).ToListAsync(ct);

        if (expiringOnly == true)
            list = list.Where(d => DocumentDisplay.IsExpiringSoon(d.ExpiryDate)).ToList();

        return list.Select(Map).ToList();
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentRequest request, Guid? userId, CancellationToken ct = default)
    {
        var doc = new DocumentRecord
        {
            Title = request.Title,
            Category = request.Category,
            ParentType = DocumentDisplay.ParseParentType(request.ParentType),
            ParentId = request.ParentId,
            ParentName = request.ParentName,
            FileName = request.FileName,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            Status = DocumentDisplay.ParseStatus(request.Status),
            Notes = request.Notes,
            CreatedBy = userId,
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Create", "Documents", "Document", doc.Id.ToString(), newValues: doc.Title, ct: ct);
        return Map(doc);
    }

    public async Task<DocumentDto?> AttachFileAsync(Guid id, Stream fileStream, string fileName, string contentType, Guid? userId, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FindAsync([id], ct);
        if (doc is null) return null;

        if (!string.IsNullOrEmpty(doc.StoragePath))
            _files.Delete(doc.StoragePath);

        var saved = await _files.SaveAsync(fileStream, fileName, contentType, ct);
        doc.FileName = saved.FileName;
        doc.StoragePath = saved.StoragePath;
        doc.ContentType = saved.ContentType;
        doc.FileSizeBytes = saved.SizeBytes;
        doc.Version += 1;
        doc.UpdatedAt = DateTime.UtcNow;
        doc.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "", "Upload", "Documents", "Document", doc.Id.ToString(), newValues: doc.FileName, ct: ct);
        return Map(doc);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetFileAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FindAsync([id], ct);
        if (doc?.StoragePath is null) return null;
        var file = await _files.OpenReadAsync(doc.StoragePath, ct);
        if (file is null) return null;
        return (file.Value.Stream, doc.ContentType ?? file.Value.ContentType, doc.FileName ?? file.Value.FileName);
    }

    private static DocumentDto Map(DocumentRecord d) => new(
        d.Id, d.Title, d.Category,
        DocumentDisplay.FormatParentType(d.ParentType),
        d.ParentName, d.FileName, d.Version,
        DocumentDisplay.FormatStatus(d.Status),
        DocumentDisplay.FormatDate(d.IssueDate),
        DocumentDisplay.FormatDate(d.ExpiryDate),
        DocumentDisplay.IsExpiringSoon(d.ExpiryDate),
        !string.IsNullOrEmpty(d.StoragePath),
        d.FileSizeBytes
    );
}

public class ExportService : IExportService
{
    private readonly AppDbContext _db;

    public ExportService(AppDbContext db) => _db = db;

    public async Task<byte[]> ExportProjectsCsvAsync(CancellationToken ct = default)
    {
        var projects = await _db.Projects.Include(p => p.Client).OrderBy(p => p.Code).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Client,Status,Progress,Contract Value,Budget,End Date");
        foreach (var p in projects)
        {
            sb.AppendLine(string.Join(",",
                Csv(p.Code), Csv(p.Name), Csv(p.Client.Name),
                Csv(EnumDisplay.FormatProjectStatus(p.Status)),
                p.Progress.ToString(CultureInfo.InvariantCulture),
                p.ContractValue.ToString(CultureInfo.InvariantCulture),
                p.Budget.ToString(CultureInfo.InvariantCulture),
                Csv(p.EndDate?.ToString("yyyy-MM-dd") ?? "")));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportBoqCsvAsync(CancellationToken ct = default)
    {
        var items = await _db.BoqItems.Include(i => i.Project).OrderBy(i => i.ItemCode).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Item Code,Description,Project,Category,Quantity,Unit,Rate,Amount");
        foreach (var i in items)
        {
            sb.AppendLine(string.Join(",",
                Csv(i.ItemCode), Csv(i.Description), Csv(i.Project.Name), Csv(i.Category),
                i.Quantity.ToString(CultureInfo.InvariantCulture), Csv(i.Unit),
                i.Rate.ToString(CultureInfo.InvariantCulture),
                i.Amount.ToString(CultureInfo.InvariantCulture)));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportClaimsCsvAsync(CancellationToken ct = default)
    {
        var claims = await _db.Claims.Include(c => c.Project).OrderByDescending(c => c.ClaimDate).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Claim Number,Title,Project,Status,Amount,Claim Date");
        foreach (var c in claims)
        {
            sb.AppendLine(string.Join(",",
                Csv(c.ClaimNumber), Csv(c.Title), Csv(c.Project.Name),
                Csv(FinancialDisplay.FormatClaimStatus(c.Status)),
                c.Amount.ToString(CultureInfo.InvariantCulture),
                Csv(c.ClaimDate.ToString("yyyy-MM-dd"))));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportSuppliersCsvAsync(CancellationToken ct = default)
    {
        var suppliers = await _db.Suppliers.OrderBy(s => s.Code).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Category,Status,Contact,Phone,Email,City,Province,VAT Number");
        foreach (var s in suppliers)
        {
            sb.AppendLine(string.Join(",",
                Csv(s.Code), Csv(s.Name), Csv(s.Category.ToString()), Csv(s.Status.ToString()),
                Csv(s.ContactPerson), Csv(s.Phone), Csv(s.Email), Csv(s.City), Csv(s.Province), Csv(s.VatNumber)));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportProcurementCsvAsync(CancellationToken ct = default)
    {
        var pos = await _db.PurchaseOrders.Include(p => p.Supplier).Include(p => p.Project).OrderByDescending(p => p.OrderDate).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("PO Number,Title,Supplier,Project,Status,Order Date,Required Date,Total Amount");
        foreach (var p in pos)
        {
            sb.AppendLine(string.Join(",",
                Csv(p.PoNumber), Csv(p.Title), Csv(p.Supplier.Name), Csv(p.Project?.Name),
                Csv(p.Status.ToString()), Csv(p.OrderDate.ToString("yyyy-MM-dd")),
                Csv(p.RequiredDate?.ToString("yyyy-MM-dd") ?? ""),
                p.TotalAmount.ToString(CultureInfo.InvariantCulture)));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportInvoicesCsvAsync(CancellationToken ct = default)
    {
        var invoices = await _db.Invoices.Include(i => i.Project).ThenInclude(p => p.Client).OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Invoice Number,Project,Client,Status,Amount,Invoice Date,Due Date");
        foreach (var i in invoices)
        {
            sb.AppendLine(string.Join(",",
                Csv(i.InvoiceNumber), Csv(i.Project.Name), Csv(i.Project.Client.Name),
                Csv(FinancialDisplay.FormatInvoiceStatus(i.Status)),
                i.Amount.ToString(CultureInfo.InvariantCulture),
                Csv(i.InvoiceDate.ToString("yyyy-MM-dd")),
                Csv(i.DueDate?.ToString("yyyy-MM-dd") ?? "")));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportFleetCsvAsync(CancellationToken ct = default)
    {
        var vehicles = await _db.Vehicles.Include(v => v.AssignedProject).OrderBy(v => v.RegistrationNumber).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Registration,Make/Model,Type,Status,Driver,Assigned Project,License Expiry,Insurance Expiry");
        foreach (var v in vehicles)
        {
            sb.AppendLine(string.Join(",",
                Csv(v.RegistrationNumber), Csv(v.MakeModel), Csv(v.Type.ToString()),
                Csv(v.Status.ToString()), Csv(v.DriverName), Csv(v.AssignedProject?.Name),
                Csv(v.LicenseExpiryDate?.ToString("yyyy-MM-dd") ?? ""),
                Csv(v.InsuranceExpiryDate?.ToString("yyyy-MM-dd") ?? "")));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportDocumentsCsvAsync(CancellationToken ct = default)
    {
        var docs = await _db.Documents.OrderBy(d => d.Title).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Title,Category,Parent Type,Parent Name,File Name,Has File,Status,Issue Date,Expiry Date,Version");
        foreach (var d in docs)
        {
            sb.AppendLine(string.Join(",",
                Csv(d.Title), Csv(d.Category), Csv(d.ParentType.ToString()), Csv(d.ParentName),
                Csv(d.FileName), d.StoragePath is not null ? "Yes" : "No",
                Csv(DocumentDisplay.FormatStatus(d.Status)),
                Csv(d.IssueDate?.ToString("yyyy-MM-dd") ?? ""),
                Csv(d.ExpiryDate?.ToString("yyyy-MM-dd") ?? ""),
                d.Version.ToString(CultureInfo.InvariantCulture)));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportEmployeesCsvAsync(CancellationToken ct = default)
    {
        var employees = await _db.Employees.Include(e => e.AssignedProject).OrderBy(e => e.EmployeeNumber).ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Employee Number,First Name,Last Name,Job Title,Department,Trade,Status,Phone,Email,Hire Date,Hourly Rate,Assigned Project");
        foreach (var e in employees)
        {
            sb.AppendLine(string.Join(",",
                Csv(e.EmployeeNumber), Csv(e.FirstName), Csv(e.LastName), Csv(e.JobTitle),
                Csv(HrDisplay.FormatDepartment(e.Department)), Csv(HrDisplay.FormatTrade(e.Trade)),
                Csv(HrDisplay.FormatStatus(e.Status)), Csv(e.Phone), Csv(e.Email),
                Csv(e.HireDate.ToString("yyyy-MM-dd")),
                e.HourlyRate?.ToString(CultureInfo.InvariantCulture) ?? "",
                Csv(e.AssignedProject?.Name)));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportTimesheetsCsvAsync(CancellationToken ct = default)
    {
        var entries = await _db.TimesheetEntries
            .Include(t => t.Employee)
            .Include(t => t.Project)
            .OrderByDescending(t => t.WorkDate)
            .ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Work Date,Employee,Project,Hours,Status,Description,Labour Cost");
        foreach (var t in entries)
        {
            var cost = t.Employee.HourlyRate.HasValue ? t.Hours * t.Employee.HourlyRate.Value : (decimal?)null;
            sb.AppendLine(string.Join(",",
                Csv(t.WorkDate.ToString("yyyy-MM-dd")),
                Csv($"{t.Employee.FirstName} {t.Employee.LastName}"),
                Csv(t.Project.Name),
                t.Hours.ToString(CultureInfo.InvariantCulture),
                Csv(HrDisplay.FormatTimesheetStatus(t.Status)),
                Csv(t.Description),
                cost?.ToString(CultureInfo.InvariantCulture) ?? ""));
        }
        return ToCsvBytes(sb);
    }

    public async Task<byte[]> ExportProjectsExcelAsync(CancellationToken ct = default)
    {
        var projects = await _db.Projects.Include(p => p.Client).OrderBy(p => p.Code).ToListAsync(ct);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Projects");
        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Client";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "Progress";
        ws.Cell(1, 6).Value = "Contract Value";
        ws.Cell(1, 7).Value = "Budget";
        ws.Cell(1, 8).Value = "End Date";
        ws.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var p in projects)
        {
            ws.Cell(row, 1).Value = p.Code;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.Client.Name;
            ws.Cell(row, 4).Value = EnumDisplay.FormatProjectStatus(p.Status);
            ws.Cell(row, 5).Value = p.Progress;
            ws.Cell(row, 6).Value = p.ContractValue;
            ws.Cell(row, 7).Value = p.Budget;
            ws.Cell(row, 8).Value = p.EndDate?.ToString("yyyy-MM-dd") ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportSuppliersExcelAsync(CancellationToken ct = default)
    {
        var suppliers = await _db.Suppliers.OrderBy(s => s.Code).ToListAsync(ct);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Suppliers");
        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Category";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "Contact";
        ws.Cell(1, 6).Value = "Phone";
        ws.Cell(1, 7).Value = "Email";
        ws.Cell(1, 8).Value = "City";
        ws.Cell(1, 9).Value = "Province";
        ws.Cell(1, 10).Value = "VAT Number";
        ws.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var s in suppliers)
        {
            ws.Cell(row, 1).Value = s.Code;
            ws.Cell(row, 2).Value = s.Name;
            ws.Cell(row, 3).Value = s.Category.ToString();
            ws.Cell(row, 4).Value = s.Status.ToString();
            ws.Cell(row, 5).Value = s.ContactPerson ?? "";
            ws.Cell(row, 6).Value = s.Phone ?? "";
            ws.Cell(row, 7).Value = s.Email ?? "";
            ws.Cell(row, 8).Value = s.City ?? "";
            ws.Cell(row, 9).Value = s.Province ?? "";
            ws.Cell(row, 10).Value = s.VatNumber ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportProcurementExcelAsync(CancellationToken ct = default)
    {
        var pos = await _db.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Project)
            .Include(p => p.Lines)
            .OrderByDescending(p => p.OrderDate)
            .ToListAsync(ct);

        using var wb = new XLWorkbook();
        var poSheet = wb.Worksheets.Add("Purchase Orders");
        poSheet.Cell(1, 1).Value = "PO Number";
        poSheet.Cell(1, 2).Value = "Title";
        poSheet.Cell(1, 3).Value = "Supplier";
        poSheet.Cell(1, 4).Value = "Project";
        poSheet.Cell(1, 5).Value = "Status";
        poSheet.Cell(1, 6).Value = "Order Date";
        poSheet.Cell(1, 7).Value = "Required Date";
        poSheet.Cell(1, 8).Value = "Total Amount";
        poSheet.Cell(1, 9).Value = "Requested By";
        poSheet.Cell(1, 10).Value = "Approved By";
        poSheet.Row(1).Style.Font.Bold = true;

        var poRow = 2;
        foreach (var p in pos)
        {
            poSheet.Cell(poRow, 1).Value = p.PoNumber;
            poSheet.Cell(poRow, 2).Value = p.Title;
            poSheet.Cell(poRow, 3).Value = p.Supplier.Name;
            poSheet.Cell(poRow, 4).Value = p.Project?.Name ?? "";
            poSheet.Cell(poRow, 5).Value = p.Status.ToString();
            poSheet.Cell(poRow, 6).Value = p.OrderDate.ToString("yyyy-MM-dd");
            poSheet.Cell(poRow, 7).Value = p.RequiredDate?.ToString("yyyy-MM-dd") ?? "";
            poSheet.Cell(poRow, 8).Value = p.TotalAmount;
            poSheet.Cell(poRow, 9).Value = p.RequestedByName ?? "";
            poSheet.Cell(poRow, 10).Value = p.ApprovedByName ?? "";
            poRow++;
        }
        poSheet.Columns().AdjustToContents();

        var lineSheet = wb.Worksheets.Add("Line Items");
        lineSheet.Cell(1, 1).Value = "PO Number";
        lineSheet.Cell(1, 2).Value = "Description";
        lineSheet.Cell(1, 3).Value = "Quantity";
        lineSheet.Cell(1, 4).Value = "Unit";
        lineSheet.Cell(1, 5).Value = "Unit Price";
        lineSheet.Cell(1, 6).Value = "Line Total";
        lineSheet.Row(1).Style.Font.Bold = true;

        var lineRow = 2;
        foreach (var p in pos)
        {
            foreach (var line in p.Lines.OrderBy(l => l.Description))
            {
                lineSheet.Cell(lineRow, 1).Value = p.PoNumber;
                lineSheet.Cell(lineRow, 2).Value = line.Description;
                lineSheet.Cell(lineRow, 3).Value = line.Quantity;
                lineSheet.Cell(lineRow, 4).Value = line.Unit;
                lineSheet.Cell(lineRow, 5).Value = line.UnitPrice;
                lineSheet.Cell(lineRow, 6).Value = line.LineTotal;
                lineRow++;
            }
        }
        lineSheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportEmployeesExcelAsync(CancellationToken ct = default)
    {
        var employees = await _db.Employees.Include(e => e.AssignedProject).OrderBy(e => e.EmployeeNumber).ToListAsync(ct);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Employees");
        ws.Cell(1, 1).Value = "Employee Number";
        ws.Cell(1, 2).Value = "First Name";
        ws.Cell(1, 3).Value = "Last Name";
        ws.Cell(1, 4).Value = "Job Title";
        ws.Cell(1, 5).Value = "Department";
        ws.Cell(1, 6).Value = "Trade";
        ws.Cell(1, 7).Value = "Status";
        ws.Cell(1, 8).Value = "Phone";
        ws.Cell(1, 9).Value = "Email";
        ws.Cell(1, 10).Value = "Hire Date";
        ws.Cell(1, 11).Value = "Hourly Rate";
        ws.Cell(1, 12).Value = "Assigned Project";
        ws.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var e in employees)
        {
            ws.Cell(row, 1).Value = e.EmployeeNumber;
            ws.Cell(row, 2).Value = e.FirstName;
            ws.Cell(row, 3).Value = e.LastName;
            ws.Cell(row, 4).Value = e.JobTitle;
            ws.Cell(row, 5).Value = HrDisplay.FormatDepartment(e.Department);
            ws.Cell(row, 6).Value = HrDisplay.FormatTrade(e.Trade);
            ws.Cell(row, 7).Value = HrDisplay.FormatStatus(e.Status);
            ws.Cell(row, 8).Value = e.Phone ?? "";
            ws.Cell(row, 9).Value = e.Email ?? "";
            ws.Cell(row, 10).Value = e.HireDate.ToString("yyyy-MM-dd");
            if (e.HourlyRate.HasValue) ws.Cell(row, 11).Value = e.HourlyRate.Value;
            ws.Cell(row, 12).Value = e.AssignedProject?.Name ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] ToCsvBytes(StringBuilder sb)
    {
        // UTF-8 BOM helps Excel open CSV with correct encoding on Windows.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

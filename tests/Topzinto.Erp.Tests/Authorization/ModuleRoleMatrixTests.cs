using Topzinto.Erp.Api.Authorization;
using Topzinto.Erp.Domain.Enums;

namespace Topzinto.Erp.Tests.Authorization;

public class ModuleRoleMatrixTests
{
    [Theory]
    [InlineData(ErpModules.Fleet)]
    [InlineData(ErpModules.Boq)]
    [InlineData(ErpModules.Reports)]
    [InlineData(ErpModules.Procurement)]
    public void Director_HasAccessToAllModules(string module)
    {
        Assert.True(ModuleRoleMatrix.CanAccess(module, [SystemRoles.Director]));
    }

    [Theory]
    [InlineData(ErpModules.Dashboard)]
    [InlineData(ErpModules.SiteReports)]
    [InlineData(ErpModules.Schedule)]
    [InlineData(ErpModules.Timesheets)]
    public void Foreman_CanAccessFieldModules(string module)
    {
        Assert.True(ModuleRoleMatrix.CanAccess(module, [SystemRoles.Foreman]));
    }

    [Theory]
    [InlineData(ErpModules.Projects)]
    [InlineData(ErpModules.Fleet)]
    [InlineData(ErpModules.Boq)]
    [InlineData(ErpModules.Tenders)]
    public void Foreman_CannotAccessOfficeModules(string module)
    {
        Assert.False(ModuleRoleMatrix.CanAccess(module, [SystemRoles.Foreman]));
    }

    [Fact]
    public void FleetManager_CanAccessFleet_NotBoq()
    {
        Assert.True(ModuleRoleMatrix.CanAccess(ErpModules.Fleet, [SystemRoles.FleetManager]));
        Assert.False(ModuleRoleMatrix.CanAccess(ErpModules.Boq, [SystemRoles.FleetManager]));
    }

    [Fact]
    public void Finance_CanAccessBoqAndReports()
    {
        Assert.True(ModuleRoleMatrix.CanAccess(ErpModules.Boq, [SystemRoles.Finance]));
        Assert.True(ModuleRoleMatrix.CanAccess(ErpModules.Reports, [SystemRoles.Finance]));
    }

    [Fact]
    public void ProjectManager_CanAccessProjectsAndUnrestrictedModules()
    {
        Assert.True(ModuleRoleMatrix.CanAccess(ErpModules.Projects, [SystemRoles.ProjectManager]));
        Assert.True(ModuleRoleMatrix.CanAccess(ErpModules.Clients, [SystemRoles.ProjectManager]));
    }

    [Fact]
    public void EmptyRoles_Denied()
    {
        Assert.False(ModuleRoleMatrix.CanAccess(ErpModules.Dashboard, []));
    }
}

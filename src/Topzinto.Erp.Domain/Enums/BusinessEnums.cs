namespace Topzinto.Erp.Domain.Enums;

public enum ProjectStatus
{
    Planned = 0,
    Active = 1,
    OnHold = 2,
    Completed = 3,
}

public enum TenderStatus
{
    Identified = 0,
    Preparing = 1,
    Submitted = 2,
    Won = 3,
    Lost = 4,
}

public enum ContractStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Terminated = 3,
}

public enum ClientType
{
    Government = 0,
    Municipal = 1,
    Private = 2,
    SOE = 3,
}

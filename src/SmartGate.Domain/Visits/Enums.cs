namespace SmartGate.Domain.Visits;

public enum VisitStatus
{
    PreRegistered = 0,
    AtGate = 1,
    OnSite = 2,
    Completed = 3
}

public enum ActivityType
{
    Delivery = 0,
    Collection = 1
}
namespace ATRIUM.Domain.Constants;

public static class OrderStatuses
{
    public const string Pending = "Pendiente";
    public const string Approved = "Aprobado";
    public const string Rejected = "Rechazado";

    public static readonly string[] All = [Pending, Approved, Rejected];
}

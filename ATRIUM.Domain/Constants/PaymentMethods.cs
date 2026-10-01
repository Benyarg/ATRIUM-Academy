namespace ATRIUM.Domain.Constants;

public static class PaymentMethods
{
    public const string Yape = "Yape";
    public const string BankTransfer = "Transferencia";

    public static readonly string[] All = [Yape, BankTransfer];
}

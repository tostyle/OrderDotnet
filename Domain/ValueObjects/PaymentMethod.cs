namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a payment method
/// </summary>
public record PaymentMethod
{
    public PaymentMethodType Type { get; private init; }
    public string? CardLast4 { get; private init; }
    public string? CardBrand { get; private init; }
    public string? BankName { get; private init; }
    public string? WalletProvider { get; private init; }

    private PaymentMethod(PaymentMethodType type)
    {
        Type = type;
    }

    /// <summary>
    /// Creates a credit card payment method
    /// </summary>
    public static PaymentMethod CreditCard(string cardLast4, string cardBrand)
    {
        if (string.IsNullOrEmpty(cardLast4) || cardLast4.Length != 4)
            throw new ArgumentException("Card last 4 digits must be exactly 4 characters", nameof(cardLast4));

        return new PaymentMethod(PaymentMethodType.CreditCard)
        {
            CardLast4 = cardLast4,
            CardBrand = cardBrand
        };
    }

    /// <summary>
    /// Creates a debit card payment method
    /// </summary>
    public static PaymentMethod DebitCard(string cardLast4, string cardBrand)
    {
        if (string.IsNullOrEmpty(cardLast4) || cardLast4.Length != 4)
            throw new ArgumentException("Card last 4 digits must be exactly 4 characters", nameof(cardLast4));

        return new PaymentMethod(PaymentMethodType.DebitCard)
        {
            CardLast4 = cardLast4,
            CardBrand = cardBrand
        };
    }

    /// <summary>
    /// Creates a bank transfer payment method
    /// </summary>
    public static PaymentMethod BankTransfer(string bankName)
    {
        return new PaymentMethod(PaymentMethodType.BankTransfer)
        {
            BankName = bankName
        };
    }

    /// <summary>
    /// Creates a digital wallet payment method
    /// </summary>
    public static PaymentMethod DigitalWallet(string walletProvider)
    {
        return new PaymentMethod(PaymentMethodType.DigitalWallet)
        {
            WalletProvider = walletProvider
        };
    }

    /// <summary>
    /// Creates a cash payment method
    /// </summary>
    public static PaymentMethod Cash() => new(PaymentMethodType.Cash);

    /// <summary>
    /// Creates PaymentMethod from string representation
    /// Business logic encapsulated in domain layer
    /// </summary>
    public static PaymentMethod FromString(string paymentMethodType)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodType))
        {
            throw new ArgumentException("Payment method type cannot be null or empty", nameof(paymentMethodType));
        }

        return paymentMethodType.ToLowerInvariant().Trim() switch
        {
            "creditcard" or "credit_card" or "credit-card" => CreditCard("0000", "Unknown"),
            "debitcard" or "debit_card" or "debit-card" => DebitCard("0000", "Unknown"),
            "banktransfer" or "bank_transfer" or "bank-transfer" => BankTransfer("Unknown Bank"),
            "digitalwallet" or "digital_wallet" or "digital-wallet" or "wallet" => DigitalWallet("Unknown Wallet"),
            "cash" => Cash(),
            _ => throw new ArgumentException($"Unsupported payment method type: {paymentMethodType}", nameof(paymentMethodType))
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            PaymentMethodType.CreditCard => $"Credit Card (*{CardLast4})",
            PaymentMethodType.DebitCard => $"Debit Card (*{CardLast4})",
            PaymentMethodType.BankTransfer => $"Bank Transfer ({BankName})",
            PaymentMethodType.DigitalWallet => $"Digital Wallet ({WalletProvider})",
            PaymentMethodType.Cash => "Cash",
            _ => Type.ToString()
        };
    }
}

/// <summary>
/// Types of payment methods
/// </summary>
public enum PaymentMethodType
{
    CreditCard,
    DebitCard,
    BankTransfer,
    DigitalWallet,
    Cash
}

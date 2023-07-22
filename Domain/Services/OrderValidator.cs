namespace Domain.Services;

public class OrderValidator : IValidateOrders
{
    public bool Validate(string firstName,
                         string lastName,
                         string address,
                         ICollection<SetLineItemInput> lineItems,
                         out IDictionary<string, string> errors)
    {
        errors  = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add(nameof(firstName), "First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add(nameof(lastName), "Last name is required");

        if (string.IsNullOrWhiteSpace(address))
            errors.Add(nameof(address), "Address is required");

        if (!lineItems.Any())
            errors.Add(nameof(lineItems), "Line Items are required");

        return !errors.Any();
    }
}
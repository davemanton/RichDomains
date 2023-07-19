using Client.Dtos;

namespace Application.Validation;

internal class OrderRequestValidator : IValidateOrderRequests
{
    public bool IsValidRequest(CreateOrderRequestDto request, out IDictionary<string, string> errors)
    {
        errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add(nameof(request.FirstName), "First name is required");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add(nameof(request.LastName), "Last name is required");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add(nameof(request.Address), "Address is required");

        if (!request.LineItems.Any())
            errors.Add(nameof(request.LineItems), "Line Items are required");

        return !errors.Any();
    }
}
using Client.Dtos;

namespace Application.Validation;

public interface IValidateOrderRequests
{
    bool IsValidRequest(CreateOrderRequestDto request, out IDictionary<string, string> errors);
}
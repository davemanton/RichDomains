namespace Domain.Services;

public interface IValidateOrders
{
    bool Validate(string firstName,
                  string lastName,
                  string address,
                  ICollection<SetLineItemInput> lineItems,
                  out IDictionary<string, string> errors);
}
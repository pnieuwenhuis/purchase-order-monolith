public record SuccessResponse<TData>(TData Data, IDictionary<string, string> Properties);
public record PostProductResponse(int Id);
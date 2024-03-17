namespace Nice.Core.Exceptions;

public class ServiceException(string message) : Exception(message);

public class RepositoryException(string message) : Exception(message);

public class ValidationException(string message, IEnumerable<string> errors) : Exception($"{message}\n{string.Join("\n", errors)}");

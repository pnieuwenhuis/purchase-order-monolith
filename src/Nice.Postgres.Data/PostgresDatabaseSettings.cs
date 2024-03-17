using FluentValidation;
using Nice.Core.Validation;

namespace Nice.Postgres.Data;

[ValidateSettings(typeof(PostgresDatabaseSettingsValidator))]
public class PostgresDatabaseSettings : ISettings
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public DatabaseCredentials SuperUserCredentials { get; set; } = new();
    public DatabaseCredentials ServiceUserCredentials { get; set; } = new();

    public class DatabaseCredentials
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

public class PostgresDatabaseSettingsValidator : AbstractValidator<PostgresDatabaseSettings>
{
    public PostgresDatabaseSettingsValidator()
    {
        RuleFor(x => x.HostName).NotEmpty();
        RuleFor(x => x.Port).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DatabaseName).NotEmpty();
        RuleFor(x => x.SuperUserCredentials.UserName).NotEmpty();
        RuleFor(x => x.SuperUserCredentials.Password).NotEmpty();
        RuleFor(x => x.ServiceUserCredentials.UserName).NotEmpty();
        RuleFor(x => x.ServiceUserCredentials.Password).NotEmpty();
    }
}

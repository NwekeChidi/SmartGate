namespace SmartGate.Application.Abstractions;

public interface IPiiPolicy
{
    string SanitizeFirstName(string value);
    string SanitizeLastName(string value);
}

public sealed class PassthroughPiiPolicy : IPiiPolicy
{
    public string SanitizeFirstName(string value) => value;
    public string SanitizeLastName(string value)  => value;
}
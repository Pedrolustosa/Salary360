namespace Salary360.Application.Interfaces.Email;

public interface IEmailTemplateRenderer
{
    string Render(string templateName, IReadOnlyDictionary<string, string> values);
}
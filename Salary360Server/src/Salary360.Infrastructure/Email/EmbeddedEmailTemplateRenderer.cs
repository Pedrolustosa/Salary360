using System.Net;
using System.Reflection;
using Salary360.Application.Interfaces.Email;

namespace Salary360.Infrastructure.Email;

public sealed class EmbeddedEmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly Assembly TemplatesAssembly = typeof(EmbeddedEmailTemplateRenderer).Assembly;

    public string Render(string templateName, IReadOnlyDictionary<string, string> values)
    {
        var resourceName = TemplatesAssembly.GetManifestResourceNames()
                               .SingleOrDefault(name => name.EndsWith($".{templateName}.html", StringComparison.OrdinalIgnoreCase))
                           ?? throw new InvalidOperationException($"E-mail template '{templateName}' was not found.");

        using var stream = TemplatesAssembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        foreach (var (key, value) in values)
            html = html.Replace("{{" + key + "}}", WebUtility.HtmlEncode(value));

        return html;
    }
}

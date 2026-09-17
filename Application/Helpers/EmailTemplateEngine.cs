using System.Reflection;

namespace FVN_REGISTER.Application.Helpers
{
    /// <summary>
    /// Renders simple notification templates using public object properties.
    /// Transport and persistence remain outside Application.
    /// </summary>
    public static class EmailTemplateEngine
    {
        public static string Render(string template, object data)
        {
            if (string.IsNullOrEmpty(template) || data == null)
                return template;

            var properties = data.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var placeholder = "{{" + property.Name + "}}";
                if (template.Contains(placeholder, StringComparison.Ordinal))
                {
                    var value = property.GetValue(data)?.ToString() ?? string.Empty;
                    template = template.Replace(placeholder, value, StringComparison.Ordinal);
                }
            }

            return template;
        }
    }
}

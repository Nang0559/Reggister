
using System.Reflection;


namespace FVN_REGISTER.Core.Utils
{
    public static class EmailTemplateEngine
    {
        public static string Render(string template, object data)
        {
            if (string.IsNullOrEmpty(template) || data == null) return template;

            // Lấy tất cả thuộc tính của object data
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var placeholder = "{{" + prop.Name + "}}";
                if (template.Contains(placeholder))
                {
                    var value = prop.GetValue(data)?.ToString() ?? string.Empty;
                    template = template.Replace(placeholder, value);
                }
            }

            return template;
        }
    }
}

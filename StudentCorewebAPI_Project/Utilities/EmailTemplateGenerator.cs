using System;
using System.IO;
using StudentCorewebAPI_Project.Enums;

public static class EmailTemplateGenerator
{
    public static string GetEmailBody(EmailTemplateType type, Dictionary<string, string> placeholders)
    {
        string baseDirectory = AppContext.BaseDirectory;

        //string templatePath = type switch
        //{
        //    EmailTemplateType.RegisterUser => Path.Combine(baseDirectory, "Templates", "RegisterTemplates.html"),
        //    EmailTemplateType.AddUser => Path.Combine(baseDirectory, "Templates", "AddUserTemplates.html"),
        //    _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported email template type")
        //};
        string templatePath = type switch
        {
            EmailTemplateType.RegisterUser => Path.Combine(Directory.GetCurrentDirectory(), "Templates", "RegisterTemplates.html"),
            EmailTemplateType.AddUser => Path.Combine(Directory.GetCurrentDirectory(), "Templates", "AddUserTemplates.html"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported email template type")
        };
        //string templatePath = type switch
        //{
        //    EmailTemplateType.RegisterUser => Path.Combine(AppContext.BaseDirectory, "Templates", "RegisterTemplates.html"),
        //    EmailTemplateType.AddUser => Path.Combine(AppContext.BaseDirectory, "Templates", "AddUserTemplates.html"),
        //    _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported email template type")
        //};

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template file not found", templatePath);
        string body = File.ReadAllText(templatePath);
        //var placeholders = new Dictionary<string, string>
        //{
        //    {"{{FirstName}}",firstName },
        //    {"{{Email}}",email },
        //    {"{{RoleName}}",roleName },
        //    {"{{Year}}",DateTime.Now.ToString() },
        //};
        foreach(var placeholder in placeholders)
        {
            if(body.Contains(placeholder.Key))
            {
                body = body.Replace(placeholder.Key, placeholder.Value);
            }
        }

        return body;
    }
}


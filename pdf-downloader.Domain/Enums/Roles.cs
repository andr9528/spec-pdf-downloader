namespace PdfDownloader.Domain.Enums;

public enum Role{
    ADMIN,
    USER
}

public static class RoleExtensions
{
    public static string ToRoleString(this Role role) => role.ToString();
}
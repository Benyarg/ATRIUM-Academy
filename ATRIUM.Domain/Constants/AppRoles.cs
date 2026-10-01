namespace ATRIUM.Domain.Constants;

public static class AppRoles
{
    public const string Administrator = "Administrador";
    public const string Student = "Estudiante";
    public const string StudentOrAdministrator = Student + "," + Administrator;

    public static readonly string[] All = [Administrator, Student];
}

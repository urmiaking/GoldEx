namespace GoldEx.Shared.Routings;

public static class ApiRoutes
{
    public static class Account
    {
        public const string Base = "/Account";

        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string PerformExternalLogin = "PerformExternalLogin";

        public static class Manage
        {
            public const string Base = "Manage";

            public const string LinkExternalLogin = "LinkExternalLogin";

        }
    }
}
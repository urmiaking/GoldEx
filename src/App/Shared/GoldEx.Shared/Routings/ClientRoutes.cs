namespace GoldEx.Shared.Routings;

public static class ClientRoutes
{
    public static class Home
    {
        public const string Index = "/";
    }

    public static class Accounts
    {
        private const string AccountPrefix = "/Account";
        

        public static class Manage
        {
            private const string ManagePrefix = $"{AccountPrefix}/Manage";

            public const string Index = $"{ManagePrefix}";
            public const string ChangePassword = $"{ManagePrefix}/ChangePassword";
            public const string Disable2Fa = $"{ManagePrefix}/Disable2Fa";
            public const string Email = $"{ManagePrefix}/Email";
            public const string ExternalLogins = $"{ManagePrefix}/ExternalLogins";
            public const string EnableAuthenticator = $"{ManagePrefix}/EnableAuthenticator";
            public const string GenerateRecoveryCodes = $"{ManagePrefix}/GenerateRecoveryCodes";
            public const string ResetAuthenticator = $"{ManagePrefix}/ResetAuthenticator";
            public const string SetPassword = $"{ManagePrefix}/SetPassword";
            public const string TwoFactorAuthentication = $"{ManagePrefix}/TwoFactorAuthentication";
            public const string UserList = $"{ManagePrefix}/UserList";
            public const string EditUser = $"{ManagePrefix}/EditUser/{{id:guid}}";
            public const string LockUser = $"{ManagePrefix}/LockUser/{{id:guid}}";
            public const string NewUser = $"{ManagePrefix}/NewUser";
        }
        
        //public const string Register = $"{AccountPrefix}/Register";
        public const string Login = $"{AccountPrefix}/Login";
        public const string LoginWithPhoneNumber = $"{AccountPrefix}/LoginWithPhoneNumber";
        public const string VerifyPhoneNumber = $"{AccountPrefix}/VerifyPhoneNumber";
        public const string ForgotPassword = $"{AccountPrefix}/ForgotPassword";
        public const string ForgotPasswordConfirmation = $"{AccountPrefix}/ForgotPasswordConfirmation";
        public const string ConfirmEmail = $"{AccountPrefix}/ConfirmEmail";
        public const string AccessDenied = $"{AccountPrefix}/AccessDenied";
        public const string ConfirmEmailChange = $"{AccountPrefix}/ConfirmEmailChange";
        public const string ExternalLogin = $"{AccountPrefix}/ExternalLogin";
        public const string InvalidPasswordReset = $"{AccountPrefix}/InvalidPasswordReset";
        public const string InvalidUser = $"{AccountPrefix}/InvalidUser";
        public const string Lockout = $"{AccountPrefix}/Lockout";
        public const string LoginWithRecoveryCode = $"{AccountPrefix}/LoginWithRecoveryCode";
        public const string LoginWith2Fa = $"{AccountPrefix}/LoginWith2Fa";
        public const string RegisterConfirmation = $"{AccountPrefix}/RegisterConfirmation";
        public const string ResendEmailConfirmation = $"{AccountPrefix}/ResendEmailConfirmation";
        public const string ResetPassword = $"{AccountPrefix}/ResetPassword";
        public const string ResetPasswordConfirmation = $"{AccountPrefix}/ResetPasswordConfirmation";
    }

    public static class Health
    {
        public const string Base = "/HealthCheck";
    }
}
namespace SmartGate.Api.Common;

public static partial class AppConstants
{
    public const string ServiceName = "SmartGate.Api";
    public const string ApiVersion = "v1";
    public static class RFCErrors
    {
        public const string DefaultErrorType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
        public const string ValidationErrorTitle = "One or more validation errors occurred.";
    }

    public static class Policies
    {
        public static class Visits
        {
            public const string Read = "VisitsRead";
            public const string Write = "VisitsWrite";
        }
    }
}
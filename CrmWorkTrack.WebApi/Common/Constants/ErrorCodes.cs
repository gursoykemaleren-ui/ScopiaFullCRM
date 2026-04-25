namespace CrmWorkTrack.WebApi.Common.Constants;

public static class ErrorCodes
{
    public static class General
    {
        public const string BusinessRuleViolation = "business.rule.violation";
        public const string RequestInvalid = "request.invalid";
        public const string RequestInvalidOperation = "request.invalid_operation";
        public const string ServerError = "server.error";
    }

    public static class Auth
    {
        public const string Unauthorized = "auth.unauthorized";
        public const string Forbidden = "auth.forbidden";
    }

    public static class Resource
    {
        public const string NotFound = "resource.not_found";
    }
}


namespace CrmWorkTrack.WebApi.Common.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string message)
        : this("business.rule.violation", message)
    {
    }

    public BusinessException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
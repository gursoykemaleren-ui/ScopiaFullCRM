namespace CrmWorkTrack.WebApi.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime AsUtc(this DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public static DateTime? AsUtc(this DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return value.Value.Kind == DateTimeKind.Utc
            ? value.Value
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }
}

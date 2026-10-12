using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Application.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HttpsUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString())) return true;
        return Uri.TryCreate(value.ToString(), UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps
            && !string.IsNullOrWhiteSpace(uri.Host);
    }
}

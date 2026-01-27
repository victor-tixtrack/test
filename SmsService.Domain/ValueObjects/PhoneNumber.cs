using System.Text.RegularExpressions;

namespace SmsService.Domain.ValueObjects;

/// <summary>
/// Value object representing a phone number in E.164 format
/// </summary>
public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    private static readonly Regex E164Regex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new PhoneNumber from a string value
    /// </summary>
    /// <param name="value">Phone number in E.164 format (e.g., +1234567890)</param>
    /// <returns>PhoneNumber value object</returns>
    /// <exception cref="ArgumentException">Thrown when the phone number is not in valid E.164 format</exception>
    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone number cannot be empty", nameof(value));
        }

        if (!E164Regex.IsMatch(value))
        {
            throw new ArgumentException(
                $"Phone number must be in E.164 format (e.g., +1234567890). Provided: {value}",
                nameof(value)
            );
        }

        return new PhoneNumber(value);
    }

    /// <summary>
    /// Tries to create a PhoneNumber from a string value
    /// </summary>
    /// <param name="value">Phone number string</param>
    /// <param name="phoneNumber">Output PhoneNumber if successful</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool TryCreate(string value, out PhoneNumber? phoneNumber)
    {
        phoneNumber = null;

        if (string.IsNullOrWhiteSpace(value) || !E164Regex.IsMatch(value))
        {
            return false;
        }

        phoneNumber = new PhoneNumber(value);
        return true;
    }

    /// <summary>
    /// Validates whether a string is a valid E.164 phone number
    /// </summary>
    public static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && E164Regex.IsMatch(value);
    }

    // Implicit conversion to string
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    // Explicit conversion from string
    public static explicit operator PhoneNumber(string value) => Create(value);

    public override string ToString() => Value;

    public bool Equals(PhoneNumber? other)
    {
        if (other is null)
            return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !(left == right);
}

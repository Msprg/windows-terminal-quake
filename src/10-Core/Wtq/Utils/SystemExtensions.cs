using System.Globalization;
using System.Text.Json;

namespace Wtq.Utils;

public static class SystemExtensions
{
	/// <summary>
	/// "" => null<br/>
	/// " " => null<br/>
	/// "\t" => null<br/>
	/// "\n" => null<br/>
	/// "not-null" => "not-null".<br/>
	/// </summary>
	public static string? EmptyOrWhiteSpaceToNull(this string? input) => string.IsNullOrWhiteSpace(input) ? null : input;

	public static string GetFileNameWithoutExtension(this string fileName)
	{
		Guard.Against.NullOrWhiteSpace(fileName);

		return Path.GetFileNameWithoutExtension(fileName)?.EmptyOrWhiteSpaceToNull() ?? fileName;
	}

	/// <summary>
	/// Returns whether the 2 specified paths point to the same location (case-insensitively on Windows).
	/// </summary>
	public static bool IsSamePathAs(this string path1, string path2)
	{
		Guard.Against.NullOrWhiteSpace(path1);
		Guard.Against.NullOrWhiteSpace(path2);

		var comparison = OperatingSystem.IsWindows()
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

		return string.Equals(Path.GetFullPath(path1), Path.GetFullPath(path2), comparison);
	}

	public static TValue JsonDeepClone<TValue>(this TValue value)
	{
		var json = JsonSerializer.Serialize(value);

		return JsonSerializer.Deserialize<TValue>(json)!;
	}

	public static string ToIso8601(this DateTimeOffset dateTimeOffset)
	{
		return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
	}

	public static IEnumerable<ValidationResult> Validate(this IValidatableObject validatable)
	{
		Guard.Against.Null(validatable);

		return validatable.Validate(new ValidationContext(new object()));
	}

	/// <summary>
	/// "ToSnakeCase" => "to_snake_case".
	/// </summary>
	public static string ToSnakeCase(this string source)
	{
		Guard.Against.Null(source);

		if (source.Length <= 1)
		{
			return source.ToLowerInvariant();
		}

		var sb = new StringBuilder();
		sb.Append(char.ToLowerInvariant(source[0]));
		for (var i = 1; i < source.Length; ++i)
		{
			var c = source[i];
			if (char.IsUpper(c))
			{
				sb.Append('_');
				sb.Append(char.ToLowerInvariant(c));
			}
			else
			{
				sb.Append(c);
			}
		}

		return sb.ToString();
	}
}
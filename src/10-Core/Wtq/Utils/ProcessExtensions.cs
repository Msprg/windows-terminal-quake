namespace Wtq.Utils;

public static class ProcessExtensions
{
	/// <summary>
	/// Returns the path to the executable of the specified <paramref name="process"/>, or null if it cannot be determined
	/// (e.g. because the process runs with higher privileges than we do, or has exited in the meantime).
	/// </summary>
	public static string? GetPathOrNull(this Process process)
	{
		Guard.Against.Null(process);

		try
		{
			return process.MainModule?.FileName;
		}
		catch (Exception)
		{
			return null;
		}
	}
}

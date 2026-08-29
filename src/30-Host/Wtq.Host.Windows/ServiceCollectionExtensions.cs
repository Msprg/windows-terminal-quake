using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wtq.Configuration;
using Wtq.Services.SharpHook;
using Wtq.Services.WinForms;
using Wtq.Utils;

namespace Wtq.Host.Windows;

public static class ServiceCollectionExtensions
{
	private static readonly ILogger _log = Log.For<WtqWin32>();

	public static IServiceCollection AddHotkeyService(
		this IServiceCollection services,
		WtqOptions opts)
	{
		Guard.Against.Null(services);
		Guard.Against.Null(opts);

		var backend = opts.HotkeyBackend ?? HotkeyBackend.SharpHook;

		if (backend == HotkeyBackend.RegisterHotKey)
		{
			_log.LogInformation("Using 'RegisterHotKey' hotkey backend (WinForms)");

			services.AddWinFormsHotkeyService();
		}
		else
		{
			_log.LogInformation("Using 'SharpHook' hotkey backend");

			services.AddSharpHookHotkeyService();
		}

		return services;
	}
}
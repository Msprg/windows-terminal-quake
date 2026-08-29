using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Wtq.Configuration;
using Wtq.Input;
using Wtq.Services.WinForms.Native;
using KeyModifiers = Wtq.Services.WinForms.Native.KeyModifiers;
using Keys = System.Windows.Forms.Keys;

namespace Wtq.Services.WinForms;

/// <summary>
/// Registers hotkeys through <b>RegisterHotKey</b> (using an invisible WinForms window to receive them).
/// </summary>
public sealed class WinFormsHotkeyService : WtqHostedService
{
	private readonly ILogger _log = Log.For<WinFormsHotkeyService>();

	private readonly IOptionsMonitor<WtqOptions> _opts;
	private readonly IWtqBus _bus;

	private readonly Lock _lock = new();

	/// <summary>
	/// Ids of the hotkeys we registered, so we can unregister them again when the settings change.
	/// </summary>
	private readonly List<int> _registeredIds = [];

	/// <summary>
	/// Maps registered (key, modifiers) combinations back to the sequence as configured in the settings.<br/>
	/// Necessary as we may register a different combination than configured (e.g. when a key character
	/// requires "shift" to be typed), and the hotkey routing matches on the configured sequence.
	/// </summary>
	private readonly Dictionary<(Keys Key, KeyModifiers Modifiers), KeySequence> _registeredSequences = [];

	public WinFormsHotkeyService(
		IOptionsMonitor<WtqOptions> opts,
		IWtqBus bus)
	{
		_opts = Guard.Against.Null(opts);
		_bus = Guard.Against.Null(bus);

		// Update registrations every time the settings file is reloaded.
		opts.OnChange((_, _) => RegisterAll());

		HotkeyManager.HotkeyPressed += (s, a) =>
		{
			KeySequence? keySeq;

			lock (_lock)
			{
				keySeq = _registeredSequences.GetValueOrDefault((a.Key, a.Modifiers));
			}

			// Fall back to the raw key combination, should we not have a configured sequence for it.
			keySeq ??= new KeySequence(a.Modifiers.ToWtqKeyModifiers(), null, a.Key.ToWtqKeys());

			bus.Publish(new WtqHotkeyPressedEvent(keySeq.Value));
		};
	}

	protected override Task OnStartAsync(CancellationToken cancellationToken)
	{
		RegisterAll();

		return Task.CompletedTask;
	}

	protected override ValueTask OnDisposeAsync()
	{
		HotkeyManager.Exit();

		return ValueTask.CompletedTask;
	}

	private void RegisterAll()
	{
		lock (_lock)
		{
			// Drop previous registrations first, otherwise we'd accumulate stale ones on every settings change.
			foreach (var id in _registeredIds)
			{
				HotkeyManager.UnregisterHotkey(id);
			}

			_registeredIds.Clear();
			_registeredSequences.Clear();

			// Apps
			foreach (var app in _opts.CurrentValue.Apps)
			{
				foreach (var hk in app.Hotkeys)
				{
					_log.LogInformation("Registering hotkey '{Hotkey}' for app '{App}'", hk.Sequence, app);
					Register(hk.Sequence);
				}
			}

			// Global
			foreach (var hk in _opts.CurrentValue.Hotkeys)
			{
				_log.LogInformation("Registering global hotkey '{Hotkey}'", hk.Sequence);
				Register(hk.Sequence);
			}
		}
	}

	private void Register(KeySequence sequence)
	{
		var mods = (KeyModifiers)sequence.Modifiers;
		Keys key;

		if (sequence.HasKeyCode)
		{
			key = (Keys)sequence.KeyCode;
		}
		else if (sequence.HasKeyChar && TryResolveKeyChar(sequence.KeyChar, out key, out var extraMods))
		{
			// Some characters need a modifier to be typed (e.g. "+" requires shift on a US layout), add those.
			mods |= extraMods;
		}
		else
		{
			_log.LogWarning("Could not register hotkey '{Sequence}': no key code, and the key character could not be mapped to a key on the current keyboard layout", sequence);
			return;
		}

		_log.LogInformation("Registering hotkey '{Sequence}' as key '{Key}' with modifiers '{Modifiers}'", sequence, key, mods);

		_registeredIds.Add(HotkeyManager.RegisterHotkey(key, mods));
		_registeredSequences[(key, mods)] = sequence;
	}

	/// <summary>
	/// Resolves a key character (as configured through "KeyChar") to a virtual key, taking the current keyboard layout into account.<br/>
	/// Single characters are looked up on the keyboard layout; longer names ("Tab", "F13", etc.) are matched against <see cref="KeyCode"/>.
	/// </summary>
	private static bool TryResolveKeyChar(string keyChar, out Keys key, out KeyModifiers modifiers)
	{
		key = Keys.None;
		modifiers = KeyModifiers.None;

		// Single character: ask the keyboard layout.
		if (keyChar.Length == 1)
		{
			var res = User32.VkKeyScanW(keyChar[0]);
			if (res == -1)
			{
				return false;
			}

			key = (Keys)(res & 0xFF);

			// Shift state (note that these flags differ from the RegisterHotKey modifier flags).
			var shiftState = (res >> 8) & 0xFF;
			if ((shiftState & 1) != 0)
			{
				modifiers |= KeyModifiers.Shift;
			}

			if ((shiftState & 2) != 0)
			{
				modifiers |= KeyModifiers.Control;
			}

			if ((shiftState & 4) != 0)
			{
				modifiers |= KeyModifiers.Alt;
			}

			return true;
		}

		// Longer names: look for a key code with a matching (display) name, e.g. "Tab", "F13", "Space".
		foreach (var field in typeof(KeyCode).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			var displayName = field.GetCustomAttribute<DisplayAttribute>()?.Name;

			if (field.Name.Equals(keyChar, StringComparison.OrdinalIgnoreCase) ||
				(displayName?.Equals(keyChar, StringComparison.OrdinalIgnoreCase) ?? false))
			{
				key = (Keys)(KeyCode)field.GetValue(null)!;
				return key != Keys.None;
			}
		}

		return false;
	}
}

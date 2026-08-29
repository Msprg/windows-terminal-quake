namespace Wtq.Configuration;

/// <summary>
/// (Windows only) How hotkeys are registered with the OS.
/// </summary>
public enum HotkeyBackend
{
	/// <summary>
	/// Used to detect serialization issues.
	/// </summary>
	[DisplayFlags(IsVisible = false)] // Don't show this in the GUI, it's purely for internal use.
	None = 0,

	/// <summary>
	/// Uses a low-level keyboard hook (through <a href="https://github.com/TolikPylypchuk/SharpHook">SharpHook</a>).<br/>
	/// <br/>
	/// Supports the most keys (including the "Windows", or "Super" modifier), and hotkeys can be suspended while editing them in the GUI.<br/>
	/// Does <b>not</b> receive key presses while a window that runs as administrator (elevated) has focus.
	/// </summary>
	SharpHook,

	/// <summary>
	/// Registers hotkeys with Windows (<b>RegisterHotKey</b>).<br/>
	/// <br/>
	/// Hotkeys are matched by Windows itself, and keep working while an elevated window has focus.<br/>
	/// The "Windows" modifier is not available with this backend.
	/// </summary>
	[Display(Name = "RegisterHotKey")]
	RegisterHotKey,
}

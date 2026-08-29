namespace Wtq.Configuration;

/// <summary>
/// How WTQ should try to attach to an app.
/// </summary>
public enum AttachMode
{
	/// <summary>
	/// Used to detect serialization issues.
	/// </summary>
	[DisplayFlags(IsVisible = false)] // Don't show this in the GUI, it's purely for internal use.
	None = 0,

	/// <summary>
	/// Only look for <b>existing</b> app instances (but don't create one).
	/// </summary>
	Find,

	/// <summary>
	/// Look for an <b>existing</b> app instance, <b>create one</b> if one does not exist yet.
	/// </summary>
	[Display(Name = "Find or start")]
	FindOrStart,

	/// <summary>
	/// Attach to <b>whatever app is in the foreground</b> when pressing an assigned hotkey.
	/// </summary>
	Manual,

	/// <summary>
	/// <b>Start</b> a new app instance, and only attach to <b>that</b> (never to windows WTQ did not start).<br/>
	/// Useful for apps where you also want regular windows, that WTQ should leave alone (like Windows Terminal).
	/// </summary>
	[Display(Name = "Start")]
	Start,
}
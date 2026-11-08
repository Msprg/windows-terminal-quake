namespace Wtq.Services.Win32;

public class Win32Window(Func<bool> hasExited)
{
	private readonly Func<bool> _hasExited = Guard.Against.Null(hasExited);

	public bool HasExited => _hasExited();

	public bool IsMainWindow { get; set; }

	/// <summary>
	/// Whether the window never takes focus (WS_EX_NOACTIVATE), which is typical for helper windows.<br/>
	/// Qt 6 for example creates an invisible one of these ("_q_titlebar") for every dialog it ever showed.
	/// </summary>
	public bool IsNoActivate { get; set; }

	/// <summary>
	/// Whether the window is a "tool window" (WS_EX_TOOLWINDOW): a helper window that doesn't show up in the taskbar or alt-tab.
	/// </summary>
	public bool IsToolWindow { get; set; }

	/// <summary>
	/// Whether the window looks like an internal helper window of its process, rather than something a user interacts with:
	/// it is hidden, and either never takes focus, is a tool window, or doesn't even have a title.<br/>
	/// Such windows still match on process name, but should never be picked as the window to toggle.
	/// </summary>
	public bool IsHelperWindow =>
		!IsVisible && (IsNoActivate || IsToolWindow || string.IsNullOrWhiteSpace(WindowCaption));

	/// <summary>
	/// Whether the window is currently visible (as in <c>IsWindowVisible()</c>), as opposed to hidden (e.g. minimized to tray, or never shown).
	/// </summary>
	public bool IsVisible { get; set; }

	public nint MainWindowHandle { get; set; }

	/// <summary>
	/// Handle of the window that owns this one (e.g. the main window of a dialog), or 0 if it has no owner.
	/// </summary>
	public nint OwnerHandle { get; set; }

	public uint ProcessId { get; set; }

	public string? ProcessName { get; set; }

	public Rectangle Rect { get; set; }

	public nint Style { get; set; }

	public uint ThreadId { get; set; }

	public string? WindowCaption { get; set; }

	public string? WindowClass { get; set; }

	public nint WindowHandle { get; set; }

	public override string ToString() => $"ProcessId:{ProcessId} ProcessName:{ProcessName} MainWindow:{MainWindowHandle:X} WindowHandle:{WindowHandle:X}";
}
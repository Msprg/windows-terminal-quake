namespace Wtq.Services.Win32.UnitTest;

[TestClass]
public class Win32WtqWindowTest
{
	private readonly Mock<IWin32> _win32 = new();
	private Win32Window _wnd = new(() => false);
	private WtqAppOptions _opts = new();

	private Win32WtqWindow _window = null!;

	[TestInitialize]
	public void Setup()
	{
		_window = new Win32WtqWindow(_win32.Object, _wnd);
	}

	/// <summary>
	/// Hidden windows that can't take focus, are tool windows, or have no title, are helper windows. Visible ones never are.
	/// </summary>
	[TestMethod]
	[DataRow(false, true, false, "_q_titlebar", true)] // Qt 6 title bar helper.
	[DataRow(false, false, false, "", true)] // Qt 6 theme/screen observer.
	[DataRow(false, false, true, "Floating toolbar", true)]
	[DataRow(false, false, false, "My database - KeePassXC", false)] // Main window, minimized to tray.
	[DataRow(true, true, true, "", false)] // Visible windows are never filtered out.
	public void IsHelperWindow(bool isVisible, bool isNoActivate, bool isToolWindow, string caption, bool expected)
	{
		var wnd = new Win32Window(() => false) { IsVisible = isVisible, IsNoActivate = isNoActivate, IsToolWindow = isToolWindow, WindowCaption = caption };

		Assert.AreEqual(expected, wnd.IsHelperWindow);
	}

	/// <summary>
	/// A window is owned by another when that other is somewhere up its owner chain.
	/// </summary>
	[TestMethod]
	public void IsOwnedBy()
	{
		// Arrange
		var main = new Win32WtqWindow(_win32.Object, new Win32Window(() => false) { WindowHandle = 100 });
		var dialog = new Win32WtqWindow(_win32.Object, new Win32Window(() => false) { WindowHandle = 200, OwnerHandle = 100 });
		var subDialog = new Win32WtqWindow(_win32.Object, new Win32Window(() => false) { WindowHandle = 300, OwnerHandle = 200 });
		var unrelated = new Win32WtqWindow(_win32.Object, new Win32Window(() => false) { WindowHandle = 400, OwnerHandle = 999 });

		_win32.Setup(w => w.GetWindowOwner(200)).Returns(100);
		_win32.Setup(w => w.GetWindowOwner(999)).Returns(0);

		// Act + Assert
		Assert.IsTrue(dialog.IsOwnedBy(main), "direct owner");
		Assert.IsTrue(subDialog.IsOwnedBy(main), "indirect owner");
		Assert.IsFalse(main.IsOwnedBy(dialog), "owner is not owned by its dialog");
		Assert.IsFalse(unrelated.IsOwnedBy(main), "unrelated window");
		Assert.IsFalse(main.IsOwnedBy(main), "window does not own itself");
	}

	/// <summary>
	/// Only a file name is specified, which is then used to match on process name.
	/// </summary>
	[TestMethod]

	// Exact
	[DataRow("ProcessExplorer", "ProcessExplorer", true)]
	[DataRow("ProcessExplorer", "processexplorer", true)]
	[DataRow("processexplorer", "ProcessExplorer", true)]

	// No match
	[DataRow("procexp", "ProcessExplorer", false)]
	public void ByFileName_ProcessName(string opt, string wnd, bool isMatch)
	{
		// Arrange
		_opts.FileName = opt;
		_wnd.ProcessName = wnd;

		// Act + Assert
		Assert.AreEqual(isMatch, _window.Matches(_opts));
	}

	[TestMethod]

	// Either
	[DataRow(MainWindowState.Either, true, true)]
	[DataRow(MainWindowState.Either, false, true)]

	// Main window only
	[DataRow(MainWindowState.MainWindowOnly, true, true)]
	[DataRow(MainWindowState.MainWindowOnly, false, false)]

	// Non-main window only
	[DataRow(MainWindowState.NonMainWindowOnly, true, false)]
	[DataRow(MainWindowState.NonMainWindowOnly, false, true)]
	public void ByMainWindow(MainWindowState opt, bool isMainWindow, bool isMatch)
	{
		// Arrange
		_opts.MainWindow = opt;
		_wnd.IsMainWindow = isMainWindow;

		// Act + Assert
		Assert.AreEqual(isMatch, _window.Matches(_opts));
	}

	/// <summary>
	/// An explicit process name is specified, so we can use that to match.
	/// </summary>
	[TestMethod]

	// Exact
	[DataRow("ProcessExplorer", "ProcessExplorer", true)]
	[DataRow("ProcessExplorer", "processexplorer", true)]
	[DataRow("processexplorer", "ProcessExplorer", true)]

	// No match
	[DataRow("procexp", "ProcessExplorer", false)]

	// Regex - Match
	[DataRow("ProcessExp", "ProcessExplorer", true)] // Without explicit regex symbols, acts as "contains".
	[DataRow("ProcessExp.*", "ProcessExplorer", true)]
	[DataRow(".*cessExp.*", "ProcessExplorer", true)]
	[DataRow(".rocessExplorer", "ProcessExplorer", true)]

	// Regex - No match
	[DataRow("TheProcessName", null, false)]
	[DataRow("TheProcessName", "", false)]
	[DataRow("TheProcessName", " ", false)]
	[DataRow(".*ExpProc.*", "ProcessExplorer", false)] // No match
	public void ByProcessName(string opt, string wnd, bool isMatch)
	{
		// Arrange
		_opts.FileName = "the-file-name"; // Should not be used in matching.
		_opts.ProcessName = opt;
		_wnd.ProcessName = wnd;

		// Act + Assert
		Assert.AreEqual(isMatch, _window.Matches(_opts));
	}

	[TestMethod]

	// Exact
	[DataRow("TheWindowClass", "TheWindowClass", true)]
	[DataRow("TheWindowClass", "thewindowclass", true)]
	[DataRow("thewindowclass", "TheWindowClass", true)]

	// No match
	[DataRow("thewndclass", "TheWindowClass", false)]

	// Regex - Match
	[DataRow("TheWindowC", "TheWindowClass", true)] // Without explicit regex symbols, acts as "contains".
	[DataRow("TheWindowC.*", "TheWindowClass", true)]
	[DataRow(".*WindowCl.*", "TheWindowClass", true)]
	[DataRow(".WindowClass", "TheWindowClass", true)]

	// Regex - No match
	[DataRow("TheWindowClass", null, false)]
	[DataRow("TheWindowClass", "", false)]
	[DataRow("TheWindowClass", " ", false)]
	[DataRow(".*ClassWindow.*", "TheWindowClass", false)]
	public void ByWindowClass(string opt, string wnd, bool isMatch)
	{
		// Arrange
		_opts.WindowClass = opt;
		_wnd.WindowClass = wnd;

		// Act + Assert
		Assert.AreEqual(isMatch, _window.Matches(_opts));
	}

	[TestMethod]

	// Exact
	[DataRow("TheWindowTitle", "TheWindowTitle", true)]
	[DataRow("TheWindowTitle", "thewindowtitle", true)]
	[DataRow("thewindowtitle", "TheWindowTitle", true)]

	// No match
	[DataRow("thewndtitle", "TheWindowTitle", false)]

	// Regex - Match
	[DataRow("TheWindowT", "TheWindowTitle", true)] // Without explicit regex symbols, acts as "contains".
	[DataRow("TheWindowT.*", "TheWindowTitle", true)]
	[DataRow(".*WindowT.*", "TheWindowTitle", true)]
	[DataRow(".WindowTitle", "TheWindowTitle", true)]

	// Regex - No match
	[DataRow("TheWindowTitle", null, false)]
	[DataRow("TheWindowTitle", "", false)]
	[DataRow("TheWindowTitle", " ", false)]
	[DataRow(".*TitleWindow.*", "TheWindowTitle", false)]
	public void ByWindowTitle(string opt, string wnd, bool isMatch)
	{
		// Arrange
		_opts.WindowTitle = opt;
		_wnd.WindowCaption = wnd;

		// Act + Assert
		Assert.AreEqual(isMatch, _window.Matches(_opts));
	}
}
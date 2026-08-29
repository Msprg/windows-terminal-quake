using System.Runtime.InteropServices;

namespace Wtq.Services.WinForms.Native;

internal static class User32
{
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool UnregisterHotKey(nint hWnd, int id);

	/// <summary>
	/// Translates a character to the corresponding virtual-key code and shift state, for the current keyboard layout.<br/>
	/// Returns -1 if the character cannot be typed with the current layout. Otherwise, the low byte contains the
	/// virtual-key code, the high byte the shift state (1: shift, 2: control, 4: alt).
	/// </summary>
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern short VkKeyScanW(char ch);
}
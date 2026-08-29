using Gn = Wtq.WtqConstants.Settings.GroupNames;

namespace Wtq.Configuration;

/// <summary>
/// Defines WTQ-wide options, including the list of configured apps.
/// </summary>
[Display(Name = ":material-earth: Global options")]
public sealed class WtqOptions : WtqSharedOptions
{
	/// <summary>
	/// Path to wtq.schema.json.<br/>
	/// <br/>
	/// Used for adding intellisense-like features to editors that support JSON schema (such as VSCode).<br/>
	/// <br/>
	/// Fixed value, changes to this property in the wtq.jsonc will be ignored/overwritten.
	/// </summary>
	[DisplayFlags(IsVisible = false)]
	[JsonPropertyName("$schema")]
	[JsonPropertyOrder(0)]
	public string Schema { get; } = "wtq.schema.json";

	/// <summary>
	/// Applications to enable Quake-style dropdown for.<br/>
	/// <br/>
	/// See the <b>Examples</b> page or the docs for some examples.
	/// </summary>
	[Display(GroupName = Gn.General)]
	[JsonPropertyOrder(101)]
	[Required]
	public ICollection<WtqAppOptions> Apps { get; set; }
		= [];

	/// <summary>
	/// Global hotkeys, that toggle either the first, or the most recently toggled app.
	/// </summary>
	[Display(GroupName = Gn.General)]
	[JsonPropertyOrder(102)]
	public ICollection<HotkeyOptions> Hotkeys { get; set; }
		= [];


	/// <summary>
	/// How many frames per second the animation should be.<br/>
	/// <br/>
	/// Note that this may not be hit if moving windows takes too long, hence "target" FPS.<br/>
	/// <br/>
	/// Must be between 5 and 120, to prevent issues that can arise with values that are too low or too high.
	/// </summary>
	[DefaultValue(40)]
	[Display(GroupName = Gn.Animation, Name = "Animation target FPS")]
	[JsonPropertyOrder(103)]
	[Range(5, 120)]
	public int? AnimationTargetFps { get; set; }

	/// <summary>
	/// WTQ comes with an HTTP API (<b>disabled</b> by default), that can be used to control WTQ programmatically.<br/>
	/// <br/>
	/// See the docs for more information and usage examples.
	/// </summary>
	[Display(GroupName = Gn.General, Name = "API")]
	[JsonPropertyOrder(104)]
	public WtqApiOptions? Api { get; set; }

	/// <summary>
	/// (Windows only) How hotkeys are registered with the OS.<br/>
	/// <br/>
	/// <b>SharpHook</b> (default) uses a low-level keyboard hook, supports the most keys (including the "Windows" modifier),
	/// but does not receive key presses while a window that runs as administrator (elevated) has focus.<br/>
	/// <b>RegisterHotKey</b> lets Windows match the hotkeys, which also works while an elevated window has focus,
	/// but does not support the "Windows" modifier.<br/>
	/// <br/>
	/// Requires a restart of WTQ to take effect.
	/// </summary>
	[DefaultValue(Wtq.Configuration.HotkeyBackend.SharpHook)]
	[Display(GroupName = Gn.General, Name = "Hotkey backend", Prompt = "Windows only, requires restart")]
	[JsonPropertyOrder(105)]
	public HotkeyBackend? HotkeyBackend { get; set; }

	/// <summary>
	/// Whether to show the GUI when WTQ is started.
	/// </summary>
	[DefaultValue(false)]
	[Display(GroupName = Gn.General, Name = "Show UI on start")]
	[JsonPropertyOrder(106)]
	public bool? ShowUiOnStart { get; set; }

	/// <summary>
	/// The tray icon (color) style (dark/light).
	/// </summary>
	[DefaultValue(Wtq.Configuration.TrayIconStyle.Auto)]
	[Display(GroupName = Gn.General, Name = "Tray icon (color) style")]
	[JsonPropertyOrder(107)]
	public TrayIconStyle? TrayIconStyle { get; set; }

	/// <summary>
	/// Called right after the options are loaded from file.
	/// </summary>
	public void OnPostConfigure()
	{
		foreach (var app in Apps ?? [])
		{
			app.OnPostConfigure(this);
		}
	}

	/// <summary>
	/// Called right before the options are saved to file.
	/// </summary>
	public void PrepareForSave()
	{
		foreach (var app in Apps.ToList())
		{
			app.PrepareForSave();
		}

		// Explicit ToList() since we're modifying it from within the loop.
		foreach (var hk in Hotkeys.Where(hk => hk.Sequence.IsEmpty).ToList())
		{
			Hotkeys.Remove(hk);
		}
	}

	protected override IEnumerable<ValidationResult> OnValidate(ValidationContext context)
	{
		// TODO
		return [];
	}
}
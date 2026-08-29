namespace Wtq.Core.UnitTest.Services;

[TestClass]
public class WtqOptionsSaveServiceTest
{
	private WtqOptions _opts = new();

	private WtqOptionsSaveService _svc = new(new Mock<IPlatformService>(MockBehavior.Strict).Object);

	[TestMethod]
	public void Empty()
	{
		// Act
		var act = _svc.Write(new());

		// Assert
		var exp =
			"""
			{
				"$schema": "wtq.schema.json"
			}
			""";

		Assert.Inconclusive("TODO");
	}

	[TestMethod]
	public void HotkeyBackend_RegisterHotKey_IsWritten()
	{
		// Arrange
		_opts.HotkeyBackend = HotkeyBackend.RegisterHotKey;

		// Act
		var act = _svc.Write(_opts);

		// Assert
		StringAssert.Contains(act, "\"HotkeyBackend\": \"RegisterHotKey\"");
	}

	[TestMethod]
	public void HotkeyBackend_SharpHook_IsWritten()
	{
		// Arrange (explicitly set, so it should be kept even though it's the default)
		_opts.HotkeyBackend = HotkeyBackend.SharpHook;

		// Act
		var act = _svc.Write(_opts);

		// Assert
		StringAssert.Contains(act, "\"HotkeyBackend\": \"SharpHook\"");
	}

	[TestMethod]
	public void HotkeyBackend_NotSet_IsOmitted()
	{
		// Act
		var act = _svc.Write(_opts);

		// Assert
		Assert.IsFalse(act.Contains("HotkeyBackend", StringComparison.Ordinal), $"Expected no 'HotkeyBackend' in:\n{act}");
	}
}
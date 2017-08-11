namespace PiFace.Net.CAD {
	public partial class Controller {
		// mcp23s17 GpIoB to HD44780 pin map
		internal static class Pin {
			internal const uint D4 = 0;
			internal const uint D5 = 1;
			internal const uint D6 = 2;
			internal const uint D7 = 3;
			internal const uint Enable = 4;
			internal const uint Rw = 5;
			internal const uint Rs = 6;

			internal const uint Backlight = 7;
		}
	}
}
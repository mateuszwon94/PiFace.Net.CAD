namespace PiFace.Net.CAD {
	public partial class Controller {
		public partial class Lcd {
			// commands
			internal static class Command {
				internal const uint ClearDisplay = 0x01;
				internal const uint ReturnHome = 0x02;
				internal const uint EntryModeSet = 0x04;
				internal const uint DisplayControl = 0x08;
				internal const uint CursorShift = 0x10;
				internal const uint FunctionSet = 0x20;
				internal const uint SetCgRamAddr = 0x40;
				internal const uint SetDdRmaAddr = 0x80;
				internal const uint NewLine = 0xC0;
			}
		}
	}
}
namespace PiFace.Net.CAD {
	public partial class Controller {
		public partial class Lcd {
			internal static class Flag {
				// flags for display entry mode
				internal const uint EntryRight = 0x00;
				internal const uint EntryLeft = 0x02;
				internal const uint EntryShiftIncrement = 0x01;
				internal const uint EntryShiftDecrement = 0x00;

				// flags for display on/off control
				internal const uint DisplayOn = 0x04;
				internal const uint DisplayOff = 0x00;
				internal const uint CursorOn = 0x02;
				internal const uint CursorOff = 0x00;
				internal const uint BlinkOn = 0x01;
				internal const uint BlinkOff = 0x00;

				// flags for display/cursor shift
				internal const uint DisplayMove = 0x08;
				internal const uint CursorMove = 0x00;
				internal const uint MoveRight = 0x04;
				internal const uint MoveLeft = 0x00;

				// flags for function set
				internal const uint EightBitMode = 0x10;
				internal const uint FoutBitMode = 0x00;
				internal const uint TwoLine = 0x08;
				internal const uint OneLine = 0x00;
				internal const uint FiveXTenDots = 0x04;
				internal const uint FiveXEightDots = 0x00;
				internal const uint MaxLines = 2;
				internal const uint Width = 16;
				internal const uint RamWidth = 80; // RAM is 80 wide, split over two lines
				internal static uint[] RowOffsets = { 0, 0x40 };
			}
		}
	}
}
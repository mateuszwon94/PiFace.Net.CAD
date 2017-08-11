using PiFace.Net.CAD.Exceptions;
using System;

namespace PiFace.Net.CAD {
	/// <inheritdoc />
	/// <summary>
	/// Controller to manage text on device
	/// </summary>
	public partial class Controller : IDisposable {
		/// <summary>
		/// Opens the display to messages.
		/// </summary>
		public Controller() {
			// All PiFace Digital are connected to the same SPI bus, only need 1 fd.
			if ( (mcp23S17Fd = Libmcp23s17Wrapper.mcp23s17_open(bus, chipSelect)) < 0 )
				throw new OpenException("Cannot open connection to PiFace Digital");

			//Set IO config
			uint ioconfig = Mcp23s17Data.BANK_OFF |
			                Mcp23s17Data.INT_MIRROR_OFF |
			                Mcp23s17Data.SEQOP_OFF |
			                Mcp23s17Data.DISSLW_OFF |
			                Mcp23s17Data.HAEN_ON |
			                Mcp23s17Data.ODR_OFF |
			                Mcp23s17Data.INTPOL_LOW;

			Libmcp23s17Wrapper.mcp23s17_write_reg(ioconfig, Mcp23s17Data.IOCON, hardwareAddr, mcp23S17Fd);

			// Set GPIO Port A as inputs (switches)
			Libmcp23s17Wrapper.mcp23s17_write_reg(0xff, Mcp23s17Data.IODIRA, hardwareAddr, mcp23S17Fd);
			Libmcp23s17Wrapper.mcp23s17_write_reg(0xff, Mcp23s17Data.GPPUA, hardwareAddr, mcp23S17Fd);

			// Set GPIO Port B as outputs (connected to HD44780)
			Libmcp23s17Wrapper.mcp23s17_write_reg(0x00, Mcp23s17Data.IODIRB, hardwareAddr, mcp23S17Fd);

			// enable interrupts
			Libmcp23s17Wrapper.mcp23s17_write_reg(0xFF, Mcp23s17Data.GPINTENA, hardwareAddr, mcp23S17Fd);

			Screen = new LCD(this);
		}

		public LCD Screen { get; private set; }

		public uint ReadSwitches()
			=> Libmcp23s17Wrapper.mcp23s17_read_reg(switchPort, hardwareAddr, mcp23S17Fd);

		public uint ReadSwitch(int switchNum)
			=> (uint)((int)Libmcp23s17Wrapper.mcp23s17_read_reg(switchPort, hardwareAddr, mcp23S17Fd) >> switchNum) & 1;

		#region PrivateHelperVariables

		private int bus = 0;
		private int chipSelect = 1;
		private uint hardwareAddr = 0;
		private int mcp23S17Fd = 0; // MCP23S17 SPI file descriptor

		private uint switchPort = Mcp23s17Data.GPIOA;
		private uint lcdPort = Mcp23s17Data.GPIOB;

		#endregion // PrivateHelperVariables

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if ( !disposedValue ) {
				if ( disposing ) {
					Screen.Dispose();

					uint intenb = Libmcp23s17Wrapper.mcp23s17_read_reg(Mcp23s17Data.GPINTENA, hardwareAddr, mcp23S17Fd);
					if ( intenb == 0 ) {
						Libmcp23s17Wrapper.mcp23s17_write_reg(0, Mcp23s17Data.GPINTENA, hardwareAddr, mcp23S17Fd);
					}
				}

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}

		#endregion
	}
}
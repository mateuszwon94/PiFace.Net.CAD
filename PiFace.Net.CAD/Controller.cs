using PiFace.Net.CAD.Exceptions;
using System;
using System.Threading;

namespace PiFace.Net.CAD {
	/// <inheritdoc />
	/// <summary>
	/// Controller to manage text on device
	/// </summary>
	public class Controller : IDisposable {
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

		public class LCD : IDisposable {
			/// <summary>
			/// Initializes this instance.
			/// </summary>
			internal LCD(Controller parent) {
				parent_ = parent;

				Thread.Sleep(pifacecadData.DELAY_SETUP_0_NS);
				Libmcp23s17Wrapper.mcp23s17_write_reg(0x3, parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				PulseEnable();
				Thread.Sleep(pifacecadData.DELAY_SETUP_1_NS);
				Libmcp23s17Wrapper.mcp23s17_write_reg(0x3, parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				PulseEnable();
				Thread.Sleep(pifacecadData.DELAY_SETUP_2_NS);
				Libmcp23s17Wrapper.mcp23s17_write_reg(0x3, parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				PulseEnable();
				Libmcp23s17Wrapper.mcp23s17_write_reg(0x2, parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				PulseEnable();
				curFunctionSet |= pifacecadData.LCD_4BITMODE | pifacecadData.LCD_2LINE | pifacecadData.LCD_5X8DOTS;
				SendCommand(pifacecadData.LCD_FUNCTIONSET | curFunctionSet);
				curDisplayControl |= pifacecadData.LCD_DISPLAYOFF | pifacecadData.LCD_CURSOROFF | pifacecadData.LCD_BLINKOFF;
				SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
				Clear();
				curEntryMode |= pifacecadData.LCD_ENTRYLEFT | pifacecadData.LCD_ENTRYSHIFTDECREMENT;
				SendCommand(pifacecadData.LCD_ENTRYMODESET | curEntryMode);
				curDisplayControl |= pifacecadData.LCD_DISPLAYON | pifacecadData.LCD_CURSORON | pifacecadData.LCD_BLINKON;
				SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
			}

			/// <summary>
			/// Pulses the enable for the display.
			/// </summary>
			public void PulseEnable() {
				SetEnable(1);
				Thread.Sleep(pifacecadData.DELAY_PULSE_NS);
				SetEnable(0);
				Thread.Sleep(pifacecadData.DELAY_PULSE_NS);
			}

			/// <summary>
			/// Clears the display.
			/// </summary>
			public void Clear() {
				SendCommand(pifacecadData.LCD_CLEARDISPLAY);
				Thread.Sleep(pifacecadData.DELAY_CLEAR_NS);
				curAddress = 0;
			}

			/// <summary>
			/// Sends the command to device.
			/// </summary>
			/// <param name="command">The command.</param>
			public void SendCommand(uint command) {
				SetRs(0);
				SendByte(command);
				Thread.Sleep(pifacecadData.DELAY_SETTLE_NS);
			}

			/// <summary>
			/// Writes the specified message.
			/// </summary>
			/// <param name="message">The message.</param>
			/// <returns>Returns the current address</returns>
			public uint Write(string message) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | curAddress);

				foreach ( char c in message ) {
					if ( c == '\n' ) {
						SetCursor(0, 1);
					} else {
						SendData(c);
						curAddress++;
					}
				}

				return curAddress;
			}

			/// <summary>
			/// Sets the cursor.
			/// </summary>
			/// <param name="col">The coloumn.</param>
			/// <param name="row">The row.</param>
			/// <returns>Returns the cursor at new setting</returns>
			public uint SetCursor(uint col, uint row) {
				col = Math.Max(0, Math.Min(col, (pifacecadData.LCD_RAM_WIDTH / 2) - 1));
				row = Math.Max(0, Math.Min(row, pifacecadData.LCD_MAX_LINES - 1));
				CursorAddress = ColRowToAddress(col, row);
				Thread.Sleep(pifacecadData.DELAY_SETUP_0_NS);

				return curAddress;
			}

			/// <summary>
			/// Gets and sets the cursor address.
			/// </summary>
			public uint CursorAddress {
				get => curAddress;
				set {
					curAddress = value % pifacecadData.LCD_RAM_WIDTH;
					SendCommand(pifacecadData.LCD_SETDDRAMADDR | curAddress);
				}
			}

			public void Home() {
				SendCommand(pifacecadData.LCD_RETURNHOME);
				Thread.Sleep(pifacecadData.DELAY_CLEAR_NS); /* 2.6 ms  - added JW 2014/06/26 */
				CursorAddress = 0;
			}

			public Mode Display {
				get => (Mode)((curDisplayControl & pifacecadData.LCD_DISPLAYON) >> 2);
				set {
					switch ( value ) {
						case Mode.Off:
							curDisplayControl &= 0xff ^ pifacecadData.LCD_DISPLAYON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
							break;
						case Mode.On:
							curDisplayControl |= pifacecadData.LCD_DISPLAYON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
							break;
					}
				}
			}

			public Mode Blink {
				get => (Mode)((curDisplayControl & pifacecadData.LCD_BLINKON) >> 0);
				set {
					switch ( value ) {
						case Mode.Off:
							curDisplayControl &= 0xff ^ pifacecadData.LCD_BLINKON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
							break;
						case Mode.On:
							curDisplayControl |= pifacecadData.LCD_BLINKON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
							break;
					}
				}
			}

			public Mode Cursor {
				get => (Mode)((curDisplayControl & pifacecadData.LCD_DISPLAYCONTROL) >> 1);
				set {
					if ( value == Mode.Off ) {
						curDisplayControl &= 0xff ^ pifacecadData.LCD_CURSORON;
						SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
					} else if ( value == Mode.On ) {
						curDisplayControl |= pifacecadData.LCD_CURSORON;
						SendCommand(pifacecadData.LCD_DISPLAYCONTROL | curDisplayControl);
					}
				}
			}

			public Mode Autoscroll {
				get => (Mode)((curEntryMode & pifacecadData.LCD_ENTRYSHIFTINCREMENT) >> 0);
				set {
					if ( value == Mode.Off ) {
						curEntryMode &= 0xff ^ pifacecadData.LCD_ENTRYSHIFTINCREMENT;
						SendCommand(pifacecadData.LCD_ENTRYMODESET | curDisplayControl);
					} else if ( value == Mode.On ) {
						curEntryMode |= pifacecadData.LCD_ENTRYSHIFTINCREMENT;
						SendCommand(pifacecadData.LCD_ENTRYMODESET | curDisplayControl);
					}
				}
			}

			public Mode Backlight {
				get => (Mode)curBacklight;
				set {
					curBacklight = (uint)value;
					if ( value == Mode.Off )
						Libmcp23s17Wrapper.mcp23s17_write_bit(0, pifacecadData.PIN_BACKLIGHT, parent_.lcdPort, parent_.hardwareAddr,
						                                      parent_.mcp23S17Fd);
					else if ( value == Mode.On ) {
						Libmcp23s17Wrapper.mcp23s17_write_bit(1, pifacecadData.PIN_BACKLIGHT, parent_.lcdPort, parent_.hardwareAddr,
						                                      parent_.mcp23S17Fd);
					}
				}
			}

			public void MoveLeft() {
				SendCommand(pifacecadData.LCD_CURSORSHIFT |
				            pifacecadData.LCD_DISPLAYMOVE |
				            pifacecadData.LCD_MOVELEFT);
			}

			public void MoveRight() {
				SendCommand(pifacecadData.LCD_CURSORSHIFT |
				            pifacecadData.LCD_DISPLAYMOVE |
				            pifacecadData.LCD_MOVERIGHT);
			}

			public void LeftToRight() {
				curEntryMode |= pifacecadData.LCD_ENTRYLEFT;
				SendCommand(pifacecadData.LCD_ENTRYMODESET | curEntryMode);
			}

			public void RightToLeft() {
				curEntryMode &= 0xff ^ pifacecadData.LCD_ENTRYLEFT;
				SendCommand(pifacecadData.LCD_ENTRYMODESET | curEntryMode);
			}

			public void WriteCustomBitmap(uint location) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | curAddress);
				SendData(location);
				curAddress++;
			}

			public void StoreCustomBitmap(uint location, uint[] bitmap) {
				location &= 0x7; // we only have 8 locations 0-7
				SendCommand(pifacecadData.LCD_SETCGRAMADDR | (location << 3));
				int i;
				for ( i = 0 ; i < 8 ; i++ ) {
					SendData(bitmap[i]);
				}
			}

			/// <summary>
			/// Writes a scrolling text.
			/// </summary>
			/// <param name="message">The message.</param>
			public void ScrollingText(string message) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | curAddress);

				foreach ( char c in message ) {
					if ( c == '\n' ) {
						SetCursor(0, 1);
					} else {
						SendData(Convert.ToChar(" ")); //Laver mellemrum mellem tegnene
						SendData(c); //Skriver data
					}
				}
			}

			/// <summary>
			/// Sends the byte.
			/// </summary>
			/// <param name="byteNumber">The byte number.</param>
			private void SendByte(uint byteNumber) {
				uint currentState = Libmcp23s17Wrapper.mcp23s17_read_reg(parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				currentState &= 0xF0; // clear the data bits
				uint newByte = currentState | ((byteNumber >> 4) & 0xF); // set nibble
				Libmcp23s17Wrapper.mcp23s17_write_reg(newByte, parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				PulseEnable();
				newByte = currentState | (byteNumber & 0xF); // set nibble
				Libmcp23s17Wrapper.mcp23s17_write_reg(newByte, parent_.lcdPort, parent_.hardwareAddr, parent_.mcp23S17Fd);
				PulseEnable();
			}

			/// <summary>
			/// Sends the data.
			/// </summary>
			/// <param name="data">The data.</param>
			private void SendData(uint data) {
				SetRs(1);
				SendByte(data);

				Thread.Sleep(pifacecadData.DELAY_SETTLE_NS);
			}

			/// <summary>
			/// Sets the rs.
			/// </summary>
			/// <param name="state">The state.</param>
			private void SetRs(uint state) {
				Libmcp23s17Wrapper.mcp23s17_write_bit(state, pifacecadData.PIN_RS, parent_.lcdPort, parent_.hardwareAddr,
				                                      parent_.mcp23S17Fd);
			}

			/// <summary>
			/// Sets the enable.
			/// </summary>
			/// <param name="state">The state of LCD Display.</param>
			private void SetEnable(uint state) {
				Libmcp23s17Wrapper.mcp23s17_write_bit(state, pifacecadData.PIN_ENABLE, parent_.lcdPort, parent_.hardwareAddr,
				                                      parent_.mcp23S17Fd);
			}

			/// <summary>
			/// Coloumns the row to address.
			/// </summary>
			/// <param name="col">The coloumn.</param>
			/// <param name="row">The row.</param>
			/// <returns>Returns the address</returns>
			private uint ColRowToAddress(uint col, uint row) => col + pifacecadData.ROW_OFFSETS[row];

			private uint address2col(uint address) => address % pifacecadData.ROW_OFFSETS[1];

			private uint address2row(uint address) => address > pifacecadData.ROW_OFFSETS[1] ? 1u : 0u;

			public uint curAddress = 0;
			public uint curDisplayControl = 0;
			public uint curBacklight = 0;
			public uint curEntryMode = 0;
			public uint curFunctionSet = 0;

			private Controller parent_;

			public void Dispose() {
				Clear();
				Cursor = Mode.Off;
				Blink = Mode.Off;
				Display = Mode.Off;
				Backlight = Mode.Off;
			}
		}

		private int bus = 0;
		private int chipSelect = 1;
		private uint hardwareAddr = 0;
		private int mcp23S17Fd = 0; // MCP23S17 SPI file descriptor

		private uint switchPort = Mcp23s17Data.GPIOA;
		private uint lcdPort = Mcp23s17Data.GPIOB;

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
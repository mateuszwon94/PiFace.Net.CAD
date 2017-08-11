using System;
using System.Threading;

namespace PiFace.Net.CAD {
	public partial class Controller {
		public class LCD : IDisposable {
			/// <summary>
			/// Initializes this instance.
			/// </summary>
			/// <param name="parent">Controller to which display id conected</param>
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
				currentFunctionSet_ |= pifacecadData.LCD_4BITMODE | pifacecadData.LCD_2LINE | pifacecadData.LCD_5X8DOTS;
				SendCommand(pifacecadData.LCD_FUNCTIONSET | currentFunctionSet_);
				currentDisplayControl_ |= pifacecadData.LCD_DISPLAYOFF | pifacecadData.LCD_CURSOROFF | pifacecadData.LCD_BLINKOFF;
				SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
				Clear();
				currentEntryMode_ |= pifacecadData.LCD_ENTRYLEFT | pifacecadData.LCD_ENTRYSHIFTDECREMENT;
				SendCommand(pifacecadData.LCD_ENTRYMODESET | currentEntryMode_);
				currentDisplayControl_ |= pifacecadData.LCD_DISPLAYON | pifacecadData.LCD_CURSORON | pifacecadData.LCD_BLINKON;
				SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
			}

			/// <summary>
			/// Clears the display.
			/// </summary>
			public void Clear() {
				SendCommand(pifacecadData.LCD_CLEARDISPLAY);
				Thread.Sleep(pifacecadData.DELAY_CLEAR_NS);
				CurrentAddress = 0;
			}

			#region WriteFunctions

			/// <summary>
			/// Writes the specified message.
			/// </summary>
			/// <param name="message">The message.</param>
			/// <returns>Returns the current address</returns>
			public uint Write(string message) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | CurrentAddress);

				foreach ( char c in message ) {
					if ( c == '\n' ) {
						SetCursor(0, 1);
					} else {
						SendData(c);
						CurrentAddress++;
					}
				}

				return CurrentAddress;
			}

			/// <summary>
			/// Writes a scrolling text.
			/// </summary>
			/// <param name="message">The message.</param>
			public void WriteScrollingText(string message) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | CurrentAddress);

				foreach ( char c in message ) {
					if ( c == '\n' ) {
						SetCursor(0, 1);
					} else {
						SendData(Convert.ToChar(" ")); //Laver mellemrum mellem tegnene
						SendData(c); //Skriver data
					}
				}
			}

			#endregion // WriteFunctions

			#region CursorFunction

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

				return CurrentAddress;
			}

			/// <summary>
			/// Gets and sets the cursor address.
			/// </summary>
			public uint CursorAddress {
				get => CurrentAddress;
				set {
					CurrentAddress = value % pifacecadData.LCD_RAM_WIDTH;
					SendCommand(pifacecadData.LCD_SETDDRAMADDR | CurrentAddress);
				}
			}

			public void Home() {
				SendCommand(pifacecadData.LCD_RETURNHOME);
				Thread.Sleep(pifacecadData.DELAY_CLEAR_NS); /* 2.6 ms  - added JW 2014/06/26 */
				CursorAddress = 0;
			}

			#endregion // CursorFunction

			#region DisplayProperties

			public Mode Display {
				get => (Mode)((currentDisplayControl_ & pifacecadData.LCD_DISPLAYON) >> 2);
				set {
					switch ( value ) {
						case Mode.Off:
							currentDisplayControl_ &= 0xff ^ pifacecadData.LCD_DISPLAYON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
							break;
						case Mode.On:
							currentDisplayControl_ |= pifacecadData.LCD_DISPLAYON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
							break;
					}
				}
			}

			public Mode Blink {
				get => (Mode)((currentDisplayControl_ & pifacecadData.LCD_BLINKON) >> 0);
				set {
					switch ( value ) {
						case Mode.Off:
							currentDisplayControl_ &= 0xff ^ pifacecadData.LCD_BLINKON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
							break;
						case Mode.On:
							currentDisplayControl_ |= pifacecadData.LCD_BLINKON;
							SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
							break;
					}
				}
			}

			public Mode Cursor {
				get => (Mode)((currentDisplayControl_ & pifacecadData.LCD_DISPLAYCONTROL) >> 1);
				set {
					if ( value == Mode.Off ) {
						currentDisplayControl_ &= 0xff ^ pifacecadData.LCD_CURSORON;
						SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
					} else if ( value == Mode.On ) {
						currentDisplayControl_ |= pifacecadData.LCD_CURSORON;
						SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
					}
				}
			}

			public Mode Backlight {
				get => (Mode)currrentBacklight_;
				set {
					currrentBacklight_ = (uint)value;
					if ( value == Mode.Off )
						Libmcp23s17Wrapper.mcp23s17_write_bit(0, pifacecadData.PIN_BACKLIGHT, parent_.lcdPort, parent_.hardwareAddr,
						                                      parent_.mcp23S17Fd);
					else if ( value == Mode.On ) {
						Libmcp23s17Wrapper.mcp23s17_write_bit(1, pifacecadData.PIN_BACKLIGHT, parent_.lcdPort, parent_.hardwareAddr,
						                                      parent_.mcp23S17Fd);
					}
				}
			}

			#endregion // DisplayProperties

			#region EntryModeProperties

			public Mode Autoscroll {
				get => (Mode)((currentEntryMode_ & pifacecadData.LCD_ENTRYSHIFTINCREMENT) >> 0);
				set {
					if ( value == Mode.Off ) {
						currentEntryMode_ &= 0xff ^ pifacecadData.LCD_ENTRYSHIFTINCREMENT;
						SendCommand(pifacecadData.LCD_ENTRYMODESET | currentDisplayControl_);
					} else if ( value == Mode.On ) {
						currentEntryMode_ |= pifacecadData.LCD_ENTRYSHIFTINCREMENT;
						SendCommand(pifacecadData.LCD_ENTRYMODESET | currentDisplayControl_);
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
				currentEntryMode_ |= pifacecadData.LCD_ENTRYLEFT;
				SendCommand(pifacecadData.LCD_ENTRYMODESET | currentEntryMode_);
			}

			public void RightToLeft() {
				currentEntryMode_ &= 0xff ^ pifacecadData.LCD_ENTRYLEFT;
				SendCommand(pifacecadData.LCD_ENTRYMODESET | currentEntryMode_);
			}

			#endregion // EntryModeProperties

			#region CustomBitmap

			public void WriteCustomBitmap(uint location) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | CurrentAddress);
				SendData(location);
				CurrentAddress++;
			}

			public void StoreCustomBitmap(uint location, uint[] bitmap) {
				location &= 0x7; // we only have 8 locations 0-7
				SendCommand(pifacecadData.LCD_SETCGRAMADDR | (location << 3));
				int i;
				for ( i = 0 ; i < 8 ; i++ ) {
					SendData(bitmap[i]);
				}
			}

			#endregion // CustomBitmap

			#region PrivateHelperFunctions

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
			private void SetRs(uint state) =>
				Libmcp23s17Wrapper.mcp23s17_write_bit(state, pifacecadData.PIN_RS,
				                                      parent_.lcdPort,
				                                      parent_.hardwareAddr,
				                                      parent_.mcp23S17Fd);

			/// <summary>
			/// Sets the enable.
			/// </summary>
			/// <param name="state">The state of LCD Display.</param>
			private void SetEnable(uint state)
				=> Libmcp23s17Wrapper.mcp23s17_write_bit(state, pifacecadData.PIN_ENABLE,
				                                         parent_.lcdPort,
				                                         parent_.hardwareAddr,
				                                         parent_.mcp23S17Fd);

			/// <summary>
			/// Pulses the enable for the display.
			/// </summary>
			private void PulseEnable() {
				SetEnable(1);
				Thread.Sleep(pifacecadData.DELAY_PULSE_NS);
				SetEnable(0);
				Thread.Sleep(pifacecadData.DELAY_PULSE_NS);
			}

			/// <summary>
			/// Sends the command to device.
			/// </summary>
			/// <param name="command">The command.</param>
			private void SendCommand(uint command) {
				SetRs(0);
				SendByte(command);
				Thread.Sleep(pifacecadData.DELAY_SETTLE_NS);
			}

			/// <summary>
			/// Coloumns the row to address.
			/// </summary>
			/// <param name="col">The coloumn.</param>
			/// <param name="row">The row.</param>
			/// <returns>Returns the address</returns>
			private uint ColRowToAddress(uint col, uint row) => col + pifacecadData.ROW_OFFSETS[row];

			private uint AddressToCol(uint address) => address % pifacecadData.ROW_OFFSETS[1];

			private uint AddressToRow(uint address) => address > pifacecadData.ROW_OFFSETS[1] ? 1u : 0u;

			#endregion PrivateHelperFunctions

			public uint CurrentAddress { get; private set; } = 0;
			private uint currentDisplayControl_ = 0;
			private uint currrentBacklight_ = 0;
			private uint currentEntryMode_ = 0;
			private uint currentFunctionSet_ = 0;

			private readonly Controller parent_;

			public void Dispose() {
				Clear();
				Cursor = Mode.Off;
				Blink = Mode.Off;
				Display = Mode.Off;
				Backlight = Mode.Off;
			}
		}
	}
}
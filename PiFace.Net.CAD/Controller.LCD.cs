using System;
using System.Threading;

namespace PiFace.Net.CAD {
	public partial class Controller {
		public class Lcd : IDisposable {
			/// <summary>
			/// Initializes this instance.
			/// </summary>
			/// <param name="parent">Controller to which display id conected</param>
			/// <param name="enableCursor">If <value>true</value> sets ON Cursor and Blink.</param>
			internal Lcd(Controller parent, bool enableCursor = true) {
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

				Display = false;
				Cursor = false;
				Blink = false;

				Clear();

				EntryMode = Mode.LeftToRight;
				Autoscroll = false;

				Display = true;
				if ( enableCursor ) {
					Cursor = true;
					Blink = true;
				}
			}

			#region WriteFunctions

			/// <summary>
			/// Writes the specified message.
			/// </summary>
			/// <param name="message">The message.</param>
			public void Write(string message) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | currentCursorAddress_);

				foreach ( char c in message ) {
					if ( c == '\n' ) {
						CursorPosition = (0, 1);
					} else {
						SendData(c);
						currentCursorAddress_++;
					}
				}
			}

			/// <summary>
			/// Writes a scrolling text.
			/// </summary>
			/// <param name="message">The message.</param>
			public void WriteScrollingText(string message) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | currentCursorAddress_);

				foreach ( char c in message ) {
					if ( c == '\n' ) {
						CursorPosition = (0, 1);
					} else {
						SendData(Convert.ToChar(" ")); //Laver mellemrum mellem tegnene
						SendData(c); //Skriver data
					}
				}
			}

			/// <summary>
			/// Clears text on the display.
			/// </summary>
			public void Clear() {
				SendCommand(pifacecadData.LCD_CLEARDISPLAY);
				Thread.Sleep(pifacecadData.DELAY_CLEAR_NS);
				CursorAddress = 0;
			}

			#endregion // WriteFunctions

			#region CursorFunction

			/// <summary>
			/// Gets and sets the cursor position
			/// </summary>
			public (uint Col, uint Row) CursorPosition {
				get => (AddressToCol(CursorAddress), AddressToRow(CursorAddress));
				set {
					uint col = Math.Max(0, Math.Min(value.Col, (pifacecadData.LCD_RAM_WIDTH / 2) - 1));
					uint row = Math.Max(0, Math.Min(value.Row, pifacecadData.LCD_MAX_LINES - 1));
					CursorAddress = ColRowToAddress(col, row);
					Thread.Sleep(pifacecadData.DELAY_SETUP_0_NS);
				}
			}

			/// <summary>
			/// Gets and sets the cursor address.
			/// </summary>
			public uint CursorAddress {
				get => currentCursorAddress_;
				set {
					currentCursorAddress_ = value % pifacecadData.LCD_RAM_WIDTH;
					SendCommand(pifacecadData.LCD_SETDDRAMADDR | currentCursorAddress_);
				}
			}

			/// <summary>
			/// Moves corsor to home position (0, 0)
			/// </summary>
			public void Home() {
				SendCommand(pifacecadData.LCD_RETURNHOME);
				Thread.Sleep(pifacecadData.DELAY_CLEAR_NS);
				CursorAddress = 0;
			}

			#endregion // CursorFunction

			#region DisplayProperties

			/// <summary>
			/// Gets and sets Display ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Display {
				get => (currentDisplayControl_ & pifacecadData.LCD_DISPLAYON) >> 2 == 1;
				set {
					if ( value )
						currentDisplayControl_ |= pifacecadData.LCD_DISPLAYON;
					else
						currentDisplayControl_ &= 0xff ^ pifacecadData.LCD_DISPLAYON;
					SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets Blink Cursor ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Blink {
				get => (currentDisplayControl_ & pifacecadData.LCD_BLINKON) >> 0 == 1;
				set {
					if ( value )
						currentDisplayControl_ &= 0xff ^ pifacecadData.LCD_BLINKON;
					else
						currentDisplayControl_ |= pifacecadData.LCD_BLINKON;
					SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets Cursor ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Cursor {
				get => (currentDisplayControl_ & pifacecadData.LCD_DISPLAYCONTROL) >> 1 == 1;
				set {
					if ( value )
						currentDisplayControl_ |= pifacecadData.LCD_CURSORON;
					else
						currentDisplayControl_ &= 0xff ^ pifacecadData.LCD_CURSORON;
					SendCommand(pifacecadData.LCD_DISPLAYCONTROL | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets Backlight ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Backlight {
				get => currrentBacklight_;
				set => Libmcp23s17Wrapper.mcp23s17_write_bit(value ? 1u : 0u, pifacecadData.PIN_BACKLIGHT, parent_.lcdPort,
				                                             parent_.hardwareAddr,
				                                             parent_.mcp23S17Fd);
			}

			#endregion // DisplayProperties

			#region EntryModeProperties

			/// <summary>
			/// Gets and sets Autoscroll ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Autoscroll {
				get => (currentEntryMode_ & pifacecadData.LCD_ENTRYSHIFTINCREMENT) >> 0 == 1;
				set {
					if ( value )
						currentEntryMode_ |= pifacecadData.LCD_ENTRYSHIFTINCREMENT;
					else
						currentEntryMode_ &= 0xff ^ pifacecadData.LCD_ENTRYSHIFTINCREMENT;
					SendCommand(pifacecadData.LCD_ENTRYMODESET | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets EntryMode. Cursor moves accordingly.
			/// </summary>
			public Mode EntryMode {
				get => (Mode)((currentEntryMode_ & pifacecadData.LCD_ENTRYLEFT) >> 1);
				set {
					if ( value == Mode.LeftToRight )
						currentEntryMode_ |= pifacecadData.LCD_ENTRYLEFT;
					else
						currentEntryMode_ &= 0xff ^ pifacecadData.LCD_ENTRYLEFT;
					SendCommand(pifacecadData.LCD_ENTRYMODESET | currentEntryMode_);
				}
			}

			/// <summary>
			/// Move cursor left
			/// </summary>
			public void MoveLeft() {
				SendCommand(pifacecadData.LCD_CURSORSHIFT |
				            pifacecadData.LCD_DISPLAYMOVE |
				            pifacecadData.LCD_MOVELEFT);
			}

			/// <summary>
			/// Move cursor right
			/// </summary>
			public void MoveRight() {
				SendCommand(pifacecadData.LCD_CURSORSHIFT |
				            pifacecadData.LCD_DISPLAYMOVE |
				            pifacecadData.LCD_MOVERIGHT);
			}

			#endregion // EntryModeProperties

			#region CustomBitmap

			/// <summary>
			/// Write custom bitmap on PiFace Display
			/// </summary>
			/// <param name="i">Number of bitmap, which should be writet</param>
			public void WriteCustomBitmap(uint i) {
				SendCommand(pifacecadData.LCD_SETDDRAMADDR | currentCursorAddress_);
				SendData(i);
				currentCursorAddress_++;
			}

			/// <summary>
			/// Store custom bitmap in PiFace Memory.
			/// Only 8 slots avaliable.
			/// </summary>
			/// <param name="i">Number of bitmap slot, in which it should be storet</param>
			/// <param name="bitmap">Bitmp to store</param>
			/// <exception cref="IndexOutOfRangeException">Throwed if <paramref name="i"/> is grater than number of avaliable slots</exception>
			public void StoreCustomBitmap(uint i, uint[] bitmap) {
				if ( i > 7 )
					throw new IndexOutOfRangeException($"Avaliable 8 slots, but given is {i}");

				SendCommand(pifacecadData.LCD_SETCGRAMADDR | (i << 3));
				for ( int j = 0 ; j < 8 ; j++ )
					SendData(bitmap[j]);
			}

			#endregion // CustomBitmap

			#region PrivateMembers

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

			private uint currentCursorAddress_ = 0;
			private uint currentDisplayControl_ = 0;
			private bool currrentBacklight_ = false;
			private uint currentEntryMode_ = 0;
			private uint currentFunctionSet_ = 0;

			private readonly Controller parent_;

			#endregion PrivateMembers

			#region IDisposable

			/// <inheritdoc />
			/// <summary>
			/// Clears the display, disable all cursors, screen and backlight.
			/// </summary>
			public void Dispose() {
				Clear();
				Cursor = false;
				Blink = false;
				Display = false;
				Backlight = false;
			}

			#endregion //IDisposable
		}
	}
}
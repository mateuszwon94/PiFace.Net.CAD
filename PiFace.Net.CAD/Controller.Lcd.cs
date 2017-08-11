using System;
using System.Threading;

namespace PiFace.Net.CAD {
	public partial class Controller {
		public partial class Lcd : IDisposable {
			/// <summary>
			/// Initializes this instance.
			/// </summary>
			/// <param name="parent">Controller to which display id conected</param>
			/// <param name="enableCursor">If <value>true</value> sets ON Cursor and Blink.</param>
			internal Lcd(Controller parent, bool enableCursor = true) {
				parent_ = parent;

				Thread.Sleep(Delay.Setup0Ns);
				Libmcp23s17.mcp23s17_write_reg(0x3, parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				PulseEnable();
				Thread.Sleep(Delay.Setup1Ns);
				Libmcp23s17.mcp23s17_write_reg(0x3, parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				PulseEnable();
				Thread.Sleep(Delay.Setup2Ns);
				Libmcp23s17.mcp23s17_write_reg(0x3, parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				PulseEnable();
				Libmcp23s17.mcp23s17_write_reg(0x2, parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				PulseEnable();
				currentFunctionSet_ |= Flag.FoutBitMode | Flag.TwoLine | Flag.FiveXEightDots;
				SendCommand(Command.FunctionSet | currentFunctionSet_);

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
				SendCommand(Command.SetDdRmaAddr | currentCursorAddress_);

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
				SendCommand(Command.SetDdRmaAddr | currentCursorAddress_);

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
				SendCommand(Command.ClearDisplay);
				Thread.Sleep(Delay.ClearNs);
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
					uint col = Math.Max(0, Math.Min(value.Col, (Flag.RamWidth / 2) - 1));
					uint row = Math.Max(0, Math.Min(value.Row, Flag.MaxLines - 1));
					CursorAddress = ColRowToAddress(col, row);
					Thread.Sleep(Delay.Setup0Ns);
				}
			}

			/// <summary>
			/// Gets and sets the cursor address.
			/// </summary>
			public uint CursorAddress {
				get => currentCursorAddress_;
				set {
					currentCursorAddress_ = value % Flag.RamWidth;
					SendCommand(Command.SetDdRmaAddr | currentCursorAddress_);
				}
			}

			/// <summary>
			/// Moves corsor to home position (0, 0)
			/// </summary>
			public void Home() {
				SendCommand(Command.ReturnHome);
				Thread.Sleep(Delay.ClearNs);
				CursorAddress = 0;
			}

			#endregion // CursorFunction

			#region DisplayProperties

			/// <summary>
			/// Gets and sets Display ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Display {
				get => (currentDisplayControl_ & Flag.DisplayOn) >> 2 == 1;
				set {
					if ( value )
						currentDisplayControl_ |= Flag.DisplayOn;
					else
						currentDisplayControl_ &= 0xff ^ Flag.DisplayOn;
					SendCommand(Command.DisplayControl | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets Blink Cursor ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Blink {
				get => (currentDisplayControl_ & Flag.BlinkOn) >> 0 == 1;
				set {
					if ( value )
						currentDisplayControl_ &= 0xff ^ Flag.BlinkOn;
					else
						currentDisplayControl_ |= Flag.BlinkOn;
					SendCommand(Command.DisplayControl | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets Cursor ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Cursor {
				get => (currentDisplayControl_ & Flag.CursorOn) >> 1 == 1;
				set {
					if ( value )
						currentDisplayControl_ |= Flag.CursorOn;
					else
						currentDisplayControl_ &= 0xff ^ Flag.CursorOn;
					SendCommand(Command.DisplayControl | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets Backlight ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Backlight {
				get => currrentBacklight_;
				set => Libmcp23s17.mcp23s17_write_bit(value ? 1u : 0u, Pin.Backlight, parent_.lcdPort_,
				                                      parent_.hardwareAddr_,
				                                      parent_.mcp23s17Fd_);
			}

			#endregion // DisplayProperties

			#region EntryModeProperties

			/// <summary>
			/// Gets and sets Autoscroll ON(<value>true</value>)/OFF(<value>false</value>)
			/// </summary>
			public bool Autoscroll {
				get => (currentEntryMode_ & Flag.EntryShiftIncrement) >> 0 == 1;
				set {
					if ( value )
						currentEntryMode_ |= Flag.EntryShiftIncrement;
					else
						currentEntryMode_ &= 0xff ^ Flag.EntryShiftIncrement;
					SendCommand(Command.EntryModeSet | currentDisplayControl_);
				}
			}

			/// <summary>
			/// Gets and sets EntryMode. Cursor moves accordingly.
			/// </summary>
			public Mode EntryMode {
				get => (Mode)((currentEntryMode_ & Flag.EntryLeft) >> 1);
				set {
					if ( value == Mode.LeftToRight )
						currentEntryMode_ |= Flag.EntryLeft;
					else
						currentEntryMode_ &= 0xff ^ Flag.EntryLeft;
					SendCommand(Command.EntryModeSet | currentEntryMode_);
				}
			}

			/// <summary>
			/// Move cursor left
			/// </summary>
			public void MoveLeft() {
				SendCommand(Command.CursorShift |
				            Flag.DisplayMove |
				            Flag.MoveLeft);
			}

			/// <summary>
			/// Move cursor right
			/// </summary>
			public void MoveRight() {
				SendCommand(Command.CursorShift |
				            Flag.DisplayMove |
				            Flag.MoveRight);
			}

			#endregion // EntryModeProperties

			#region CustomBitmap

			/// <summary>
			/// Write custom bitmap on PiFace Display
			/// </summary>
			/// <param name="i">Number of bitmap, which should be writet</param>
			public void WriteCustomBitmap(uint i) {
				SendCommand(Command.SetDdRmaAddr | currentCursorAddress_);
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

				SendCommand(Command.SetCgRamAddr | (i << 3));
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
				uint currentState = Libmcp23s17.mcp23s17_read_reg(parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				currentState &= 0xF0; // clear the data bits
				uint newByte = currentState | ((byteNumber >> 4) & 0xF); // set nibble
				Libmcp23s17.mcp23s17_write_reg(newByte, parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				PulseEnable();
				newByte = currentState | (byteNumber & 0xF); // set nibble
				Libmcp23s17.mcp23s17_write_reg(newByte, parent_.lcdPort_, parent_.hardwareAddr_, parent_.mcp23s17Fd_);
				PulseEnable();
			}

			/// <summary>
			/// Sends the data.
			/// </summary>
			/// <param name="data">The data.</param>
			private void SendData(uint data) {
				SetRs(1);
				SendByte(data);

				Thread.Sleep(Delay.SettleNs);
			}

			/// <summary>
			/// Sets the rs.
			/// </summary>
			/// <param name="state">The state.</param>
			private void SetRs(uint state) =>
				Libmcp23s17.mcp23s17_write_bit(state, Pin.Rs,
				                               parent_.lcdPort_,
				                               parent_.hardwareAddr_,
				                               parent_.mcp23s17Fd_);

			/// <summary>
			/// Sets the enable.
			/// </summary>
			/// <param name="state">The state of LCD Display.</param>
			private void SetEnable(uint state)
				=> Libmcp23s17.mcp23s17_write_bit(state, Pin.Enable,
				                                  parent_.lcdPort_,
				                                  parent_.hardwareAddr_,
				                                  parent_.mcp23s17Fd_);

			/// <summary>
			/// Pulses the enable for the display.
			/// </summary>
			private void PulseEnable() {
				SetEnable(1);
				Thread.Sleep(Delay.PulseNs);
				SetEnable(0);
				Thread.Sleep(Delay.PulseNs);
			}

			/// <summary>
			/// Sends the command to device.
			/// </summary>
			/// <param name="command">The command.</param>
			private void SendCommand(uint command) {
				SetRs(0);
				SendByte(command);
				Thread.Sleep(Delay.SettleNs);
			}

			/// <summary>
			/// Coloumns the row to address.
			/// </summary>
			/// <param name="col">The coloumn.</param>
			/// <param name="row">The row.</param>
			/// <returns>Returns the address</returns>
			private uint ColRowToAddress(uint col, uint row) => col + Flag.RowOffsets[row];

			private uint AddressToCol(uint address) => address % Flag.RowOffsets[1];

			private uint AddressToRow(uint address) => address > Flag.RowOffsets[1] ? 1u : 0u;

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
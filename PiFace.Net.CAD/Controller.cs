using PiFace.Net.CAD.Exceptions;
using System;
using System.Timers;

namespace PiFace.Net.CAD {
	/// <inheritdoc />
	/// <summary>
	/// Controller to manage text on device
	/// </summary>
	public partial class Controller : IDisposable {
		/// <summary>
		/// Opens the display to messages.
		/// </summary>
		/// <param name="interval">Interval for checking if buttons state changed</param>
		/// <param name="enableCursor">If <value>true</value> sets ON Cursor and Blink on Screen.</param>
		public Controller(double interval = 100, bool enableCursor = true) {
			// All PiFace Digital are connected to the same SPI bus, only need 1 fd.
			if ( (mcp23s17Fd_ = Libmcp23s17.mcp23s17_open(bus_, chipSelect_)) < 0 )
				throw new OpenException("Cannot open connection to PiFace Digital");

			//Set IO config
			uint ioconfig = Libmcp23s17.Data.BankOff |
			                Libmcp23s17.Data.IntMirrorOff |
			                Libmcp23s17.Data.SeqopOff |
			                Libmcp23s17.Data.DisslwOff |
			                Libmcp23s17.Data.HaenOn |
			                Libmcp23s17.Data.OdrOff |
			                Libmcp23s17.Data.IntpolLow;

			Libmcp23s17.mcp23s17_write_reg(ioconfig, Libmcp23s17.Data.IoCon, hardwareAddr_, mcp23s17Fd_);

			// Set GPIO Port A as inputs (switches)
			Libmcp23s17.mcp23s17_write_reg(0xff, Libmcp23s17.Data.IoDirA, hardwareAddr_, mcp23s17Fd_);
			Libmcp23s17.mcp23s17_write_reg(0xff, Libmcp23s17.Data.GpPuA, hardwareAddr_, mcp23s17Fd_);

			// Set GPIO Port B as outputs (connected to HD44780)
			Libmcp23s17.mcp23s17_write_reg(0x00, Libmcp23s17.Data.IoDirB, hardwareAddr_, mcp23s17Fd_);

			// enable interrupts
			Libmcp23s17.mcp23s17_write_reg(0xFF, Libmcp23s17.Data.GpIntEnA, hardwareAddr_, mcp23s17Fd_);

			Screen = new Lcd(this, enableCursor);

			HoldMinTime = 2000;

			timer_ = new Timer(interval);

			ButtonsArray = new Button[4];
			for ( int i = 0 ; i < 4 ; ++i ) {
				ButtonsArray[i] = new Button(this, i);
				timer_.Elapsed += ButtonsArray[i].TimerOnElapsed;
			}

			SingleButton = new Button(this, 4);
			timer_.Elapsed += SingleButton.TimerOnElapsed;

			Switch = new Switch(this, 5, 6, 7);
			timer_.Elapsed += Switch.TimerOnElapsed;

			timer_.Start();
		}

		/// <summary>
		/// Gets and sets time interval (in milisecond) for checking if buttons state changed
		/// </summary>
		public double Interval {
			get => timer_.Interval;
			set {
				timer_.Stop();
				timer_.Interval = value;
				timer_.Start();
			}
		}

		/// <summary>
		/// Gets and sets time time (in milisecond) during which button should be hold to rise Hold event
		/// </summary>
		public double HoldMinTime { get; set; }

		/// <summary>
		/// Gets screen controll
		/// </summary>
		public Lcd Screen { get; private set; }

		/// <summary>
		/// Gets array of fours buttons located together
		/// </summary>
		public Button[] ButtonsArray { get; private set; }

		/// <summary>
		/// Getsone button located at the right
		/// </summary>
		public Button SingleButton { get; private set; }

		/// <summary>
		/// Gets switch controller
		/// </summary>
		public Switch Switch { get; private set; }

		#region PrivateHelper

		internal uint ReadSwitches()
			=> Libmcp23s17.mcp23s17_read_reg(switchPort_, hardwareAddr_, mcp23s17Fd_);

		internal uint ReadSwitch(int switchNum)
			=> (uint)((int)Libmcp23s17.mcp23s17_read_reg(switchPort_, hardwareAddr_, mcp23s17Fd_) >> switchNum) & 1;

		private int bus_ = 0;
		private int chipSelect_ = 1;
		private uint hardwareAddr_ = 0;
		private readonly int mcp23s17Fd_ = 0; // MCP23S17 SPI file descriptor

		private uint switchPort_ = Libmcp23s17.Data.GpIoA;
		private uint lcdPort_ = Libmcp23s17.Data.GpIoB;

		private readonly Timer timer_;

		#endregion // PrivateHelper

		#region IDisposable Support

		private bool disposedValue_ = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if ( !disposedValue_ ) {
				if ( disposing ) {
					Screen.Dispose();

					uint intenb = Libmcp23s17.mcp23s17_read_reg(Libmcp23s17.Data.GpIntEnA, hardwareAddr_, mcp23s17Fd_);
					if ( intenb == 0 ) {
						Libmcp23s17.mcp23s17_write_reg(0, Libmcp23s17.Data.GpIntEnA, hardwareAddr_, mcp23s17Fd_);
					}
				}

				disposedValue_ = true;
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
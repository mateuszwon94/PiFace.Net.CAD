using System;
using System.Diagnostics;
using System.Timers;
using PiFace.Net.CAD.EventArg;

namespace PiFace.Net.CAD {
	public class Button {
		/// <summary>
		/// For internal use only
		/// </summary>
		/// <param name="parent">Parent controller</param>
		/// <param name="buttonNum">Switch number which represents button</param>
		internal Button(Controller parent, int buttonNum) {
			parent_ = parent;
			buttonNum_ = buttonNum;

			lastButtonState_ = false;
			buttonTimer_ = new Stopwatch();
		}

		/// <summary>
		/// Returns <value>true</value> if button is pressed
		/// </summary>
		public bool IsButtonPressed => parent_.ReadSwitch(buttonNum_) == 0;

		/// <summary>
		/// Event invoked when button was released
		/// </summary>
		public event EventHandler<ButtonReleasedEventArgs> ButtonReleased;

		/// <summary>
		/// Event invoked when button was pressed
		/// </summary>
		public event EventHandler<EventArgs> ButtonPressed;

		/// <summary>
		/// Event listner for timer in controller
		/// </summary>
		/// <param name="sender">Event invoker</param>
		/// <param name="e">Event arguments</param>
		internal void TimerOnElapsed(object sender, ElapsedEventArgs e) {
			if ( IsButtonPressed && !lastButtonState_ ) {
				buttonTimer_.Reset();
				buttonTimer_.Start();
				ButtonPressed?.Invoke(this, EventArgs.Empty);
			} else if ( !IsButtonPressed && lastButtonState_ ) {
				buttonTimer_.Stop();

				ButtonReleased?.Invoke(this,
				                       new ButtonReleasedEventArgs(buttonTimer_.Elapsed.TotalMilliseconds < parent_.HoldMinTime
					                                                  ? ButtonState.Clicked
					                                                  : ButtonState.Holded));
			}

			lastButtonState_ = IsButtonPressed;
		}

		#region Private Variables

		private readonly int buttonNum_;
		private bool lastButtonState_;
		private readonly Stopwatch buttonTimer_;

		private readonly Controller parent_;

		#endregion
	}
}
using System;
using System.Diagnostics;
using System.Timers;
using PiFace.Net.CAD.EventArg;

namespace PiFace.Net.CAD {
	public class Switch {
		/// <summary>
		/// For internal use only
		/// </summary>
		/// <param name="parent">Parrent controller</param>
		/// <param name="down">Switch number which represents down position</param>
		/// <param name="left">Switch number which represents left position</param>
		/// <param name="right">Switch number which represents right position</param>
		internal Switch(Controller parent, int down, int left, int right) {
			parent_ = parent;
			down_ = down;
			left_ = left;
			right_ = right;

			lastSwitchState_ = SwitchState.Up;
			switchTimer_ = new Stopwatch();
		}

		/// <summary>
		/// Returns current switch position
		/// </summary>
		public SwitchState CurrentSwitchState {
			get {
				if ( parent_.ReadSwitch(down_) == 0 )
					return SwitchState.Down;
				if ( parent_.ReadSwitch(left_) == 0 )
					return SwitchState.Left;
				if ( parent_.ReadSwitch(right_) == 0 )
					return SwitchState.Right;
				return SwitchState.Up;
			}
		}

		/// <summary>
		/// Invoked when switch was released (back to up position)
		/// </summary>
		public event EventHandler<SwitchReleasedEventArgs> SwitchReleased;

		/// <summary>
		/// Invoked when switch position has changed
		/// </summary>
		public event EventHandler<SwitchPressedEventArgs> SwitchPressed;

		/// <summary>
		/// Event listner for timer in controller
		/// </summary>
		/// <param name="sender">Event invoker</param>
		/// <param name="e">Event arguments</param>
		internal void TimerOnElapsed(object sender, ElapsedEventArgs e) {
			if ( CurrentSwitchState != SwitchState.Up && lastSwitchState_ == SwitchState.Up ) {
				switchTimer_.Reset();
				switchTimer_.Start();
				SwitchPressed?.Invoke(this, new SwitchPressedEventArgs(CurrentSwitchState));
			} else if ( CurrentSwitchState == SwitchState.Up && lastSwitchState_ != SwitchState.Up ) {
				switchTimer_.Stop();

				SwitchReleased?.Invoke(this,
				                     new SwitchReleasedEventArgs(lastSwitchState_,
				                                                     switchTimer_.Elapsed.TotalMilliseconds < parent_.HoldMinTime
					                                                     ? ButtonState.Clicked
					                                                     : ButtonState.Holded));
			}

			lastSwitchState_ = CurrentSwitchState;
		}

		#region Private Variables

		private readonly int down_, left_, right_;
		private SwitchState lastSwitchState_;
		private readonly Stopwatch switchTimer_;

		private readonly Controller parent_; 

		#endregion
	}

}
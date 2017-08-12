using System;

namespace PiFace.Net.CAD.EventArg {
	public class SwitchReleasedEventArgs : EventArgs {
		public SwitchReleasedEventArgs(SwitchState state, ButtonState mode) {
			State = state;
			Mode = mode;
		}

		public SwitchState State { get; }
		public ButtonState Mode { get; }
	}
}
using System;

namespace PiFace.Net.CAD.EventArg {
	public class SwitchPressedEventArgs : EventArgs {
		public SwitchPressedEventArgs(SwitchState state) => State = state;

		public SwitchState State { get; }
	}
}
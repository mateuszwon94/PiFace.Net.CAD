using System;

namespace PiFace.Net.CAD.EventArg {
	public class ButtonReleasedEventArgs : EventArgs {
		public ButtonReleasedEventArgs(ButtonState mode) => PressMode = mode;

		public ButtonState PressMode { get; }
	}
}
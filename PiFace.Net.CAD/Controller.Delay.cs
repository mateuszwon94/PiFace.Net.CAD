namespace PiFace.Net.CAD {
	public partial class Controller {
		internal static class Delay {
			internal const int PulseNs = 1; //1ms
			internal const int SettleNs = 1; //1ms
			internal const int ClearNs = 3; // 3ms
			internal const int Setup0Ns = 15; // 15ms
			internal const int Setup1Ns = 5; // 5ms
			internal const int Setup2Ns = 1; // 1ms
		}
	}
}
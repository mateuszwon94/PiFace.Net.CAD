namespace PiFace.Net.CAD {
	internal partial class Libmcp23s17 {
		internal static class Data {
			internal const uint WriteCmd = 0;
			internal const uint ReadCmd = 1;

			// Register addresses
			internal const uint IoDirA = 0x00; // I/O direction A

			internal const uint IoDirB = 0x01; // I/O direction B
			internal const uint IoPolA = 0x02; // I/O polarity A
			internal const uint IoPolB = 0x03; // I/O polarity B
			internal const uint GpIntEnA = 0x04; // interupt enable A
			internal const uint GpIntEnB = 0x05; // interupt enable B
			internal const uint DefValA = 0x06; // register default value A (interupts)
			internal const uint DefValB = 0x07; // register default value B (interupts)
			internal const uint IntConA = 0x08; // interupt control A
			internal const uint IntConB = 0x09; // interupt control B
			internal const uint IoCon = 0x0A; // I/O config (also 0x0B)
			internal const uint GpPuA = 0x0C; // port A pullups
			internal const uint GpPuB = 0x0D; // port B pullups
			internal const uint IntFA = 0x0E; // interupt flag A (where the interupt came from)
			internal const uint IntFb = 0x0F; // interupt flag B
			internal const uint IntCapA = 0x10; // interupt capture A (value at interupt is saved here)
			internal const uint IntCapB = 0x11; // interupt capture B
			internal const uint GpIoA = 0x12; // port A
			internal const uint GpIoB = 0x13; // port B
			internal const uint OLatA = 0x14; // output latch A
			internal const uint OLatB = 0x15; // output latch B

			// I/O config
			internal const uint BankOff = 0x00; // addressing mode

			internal const uint BankOn = 0x80;
			internal const uint IntMirrorOn = 0x40; // interupt mirror (INTa|INTb)
			internal const uint IntMirrorOff = 0x00;
			internal const uint SeqopOff = 0x20; // incrementing address pointer
			internal const uint SeqopOn = 0x00;
			internal const uint DisslwOn = 0x10; // slew rate
			internal const uint DisslwOff = 0x00;
			internal const uint HaenOn = 0x08; // hardware addressing
			internal const uint HaenOff = 0x00;
			internal const uint OdrOn = 0x04; // open drain for interupts
			internal const uint OdrOff = 0x00;
			internal const uint IntpolHigh = 0x02; // interupt polarity
			internal const uint IntpolLow = 0x00;
			internal const uint GpioInterruptPin = 25;
		}
	}
}
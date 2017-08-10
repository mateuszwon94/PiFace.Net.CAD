using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiFace.Net.CAD {

	public static class Mcp23s17Data {
		public const uint WRITE_CMD = 0;

		public const uint READ_CMD = 1;

		// Register addresses
		public const uint IODIRA = 0x00; // I/O direction A

		public const uint IODIRB = 0x01; // I/O direction B
		public const uint IPOLA = 0x02; // I/O polarity A
		public const uint IPOLB = 0x03; // I/O polarity B
		public const uint GPINTENA = 0x04; // interupt enable A
		public const uint GPINTENB = 0x05; // interupt enable B
		public const uint DEFVALA = 0x06; // register default value A (interupts)
		public const uint DEFVALB = 0x07; // register default value B (interupts)
		public const uint INTCONA = 0x08; // interupt control A
		public const uint INTCONB = 0x09; // interupt control B
		public const uint IOCON = 0x0A; // I/O config (also 0x0B)
		public const uint GPPUA = 0x0C; // port A pullups
		public const uint GPPUB = 0x0D; // port B pullups
		public const uint INTFA = 0x0E; // interupt flag A (where the interupt came from)
		public const uint INTFB = 0x0F; // interupt flag B
		public const uint INTCAPA = 0x10; // interupt capture A (value at interupt is saved here)
		public const uint INTCAPB = 0x11; // interupt capture B
		public const uint GPIOA = 0x12; // port A
		public const uint GPIOB = 0x13; // port B
		public const uint OLATA = 0x14; // output latch A

		public const uint OLATB = 0x15; // output latch B

		// I/O config
		public const uint BANK_OFF = 0x00; // addressing mode

		public const uint BANK_ON = 0x80;
		public const uint INT_MIRROR_ON = 0x40; // interupt mirror (INTa|INTb)
		public const uint INT_MIRROR_OFF = 0x00;
		public const uint SEQOP_OFF = 0x20; // incrementing address pointer
		public const uint SEQOP_ON = 0x00;
		public const uint DISSLW_ON = 0x10; // slew rate
		public const uint DISSLW_OFF = 0x00;
		public const uint HAEN_ON = 0x08; // hardware addressing
		public const uint HAEN_OFF = 0x00;
		public const uint ODR_ON = 0x04; // open drain for interupts
		public const uint ODR_OFF = 0x00;
		public const uint INTPOL_HIGH = 0x02; // interupt polarity
		public const uint INTPOL_LOW = 0x00;
		public const uint GPIO_INTERRUPT_PIN = 25;
	}
}
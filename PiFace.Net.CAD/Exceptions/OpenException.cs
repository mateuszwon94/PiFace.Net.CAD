using System;
using System.Runtime.Serialization;

namespace PiFace.Net.CAD.Exceptions {
	public class OpenException : Exception {
		public OpenException() { }

		public OpenException(string message) : base(message) { }

		public OpenException(string message, Exception innerException) : base(message, innerException) { }

		protected OpenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
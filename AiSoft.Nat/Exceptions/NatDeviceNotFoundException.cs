using System;
using System.Runtime.Serialization;

namespace AiSoft.Nat.Exceptions
{
    [Serializable]
	public class NatDeviceNotFoundException : Exception
	{
        public NatDeviceNotFoundException()
		{
		}

		public NatDeviceNotFoundException(string message)
			: base(message)
		{
		}

		public NatDeviceNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected NatDeviceNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
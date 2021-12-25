using System;
using AiSoft.Nat.Base;

namespace AiSoft.Nat.EventArgs
{
	internal class DeviceEventArgs : System.EventArgs
	{
		public DeviceEventArgs(NatDevice device)
		{
			Device = device;
		}

		public NatDevice Device { get; private set; }
	}
}
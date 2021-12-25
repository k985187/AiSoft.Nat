using  System;

namespace AiSoft.Nat.Pmp
{
	internal static class PmpConstants
	{
		public const byte Version = 0;

		public const byte OperationExternalAddressRequest = 0;
		public const byte OperationCodeUdp = 1;
		public const byte OperationCodeTcp = 2;
		public const byte ServerNoop = 128;

		public const int ClientPort = 5350;
		public const int ServerPort = 5351;

		public const int RetryDelay = 250;
		public const int RetryAttempts = 9;

		public const int RecommendedLeaseTime = 60*60;
		public const int DefaultLeaseTime = RecommendedLeaseTime;

		public const short ResultCodeSuccess = 0; // Success
		public const short ResultCodeUnsupportedVersion = 1; // Unsupported Version

		public const short ResultCodeNotAuthorized = 2; // Not Authorized/Refused (e.g. box supports mapping, but user has turned feature off)

		public const short ResultCodeNetworkFailure = 3; // Network Failure (e.g. NAT box itself has not obtained a DHCP lease)

		public const short ResultCodeOutOfResources = 4; // Out of resources (NAT box cannot create any more mappings at this time)

		public const short ResultCodeUnsupportedOperationCode = 5; // Unsupported opcode
	}
}
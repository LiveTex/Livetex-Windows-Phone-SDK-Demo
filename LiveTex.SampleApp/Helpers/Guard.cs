using System.Runtime.CompilerServices;

namespace System
{
	public static class Guard
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotNull<T>(T value, string paramName = "", string message = "")
			where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(paramName, message);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotNullAndEmpty(string value, string paramName = "", string message = "")
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException(paramName, message);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotNullAndWhiteSpace(string value, string paramName = "", string message = "")
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException(paramName, message);
			}
		}
	}
}

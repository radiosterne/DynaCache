using Functional.Maybe;

namespace DynaCache.MultilevelCache.Internals
{
	internal static class StringExtensions
	{
		public static Maybe<T> ParseMaybe<T>(this string that, ParseDelegate<T> parser)
		{
			T res;
			return parser(that, out res)
				? res.ToMaybe()
				: Maybe<T>.Nothing;
		}

		internal delegate bool ParseDelegate<T>(string val, out T res);
	}
}
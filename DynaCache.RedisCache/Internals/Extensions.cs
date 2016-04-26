using System.Text;
using Functional.Maybe;

namespace DynaCache.RedisCache.Internals
{
	internal static class StringBuilderExtensions
	{
		public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char that)
		{
			return builder.Length > 0
				? builder.Append(that)
				: builder;
		}
	}

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
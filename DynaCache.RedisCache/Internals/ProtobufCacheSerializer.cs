using System.IO;
using System.Text;
using NLog;
using NLog.Extension;
using ProtoBuf;

namespace DynaCache.RedisCache.Internals
{
	internal class ProtobufCacheSerializer : ICacheSerializer
	{
		private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

		public string Serialize<T>(T @object)
		{
			using (new TracingLogProxy(logger))
			using (var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, @object);
				var bytes = stream.ToArray();
				return Encoding.Default.GetString(bytes);
			}
		}

		public T Deserialize<T>(string @object)
		{
			using (new TracingLogProxy(logger))
			{
				var bytes = Encoding.Default.GetBytes(@object.ToCharArray());
				using (var stream = new MemoryStream(bytes))
					return Serializer.Deserialize<T>(stream);
			}
		}
	}
}
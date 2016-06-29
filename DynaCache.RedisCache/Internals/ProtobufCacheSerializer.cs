using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
				var builder = new StringBuilder();
				foreach (var b in bytes)
					builder.Append(b.ToString("X2"));
				return builder.ToString();
			}
		}

		public T Deserialize<T>(string @object)
		{
			using (new TracingLogProxy(logger))
			{
				if (@object.Length % 2 != 0)
					throw new InvalidOperationException("Data is corrupt, it should contain 2*n amount of chars");
				var length = @object.Length/2;
				var bytes = new byte[length];
				for (var i = 0; i < length; i++)
					bytes[i] =  byte.Parse($"{@object[i*2]}{@object[i*2+1]}", NumberStyles.HexNumber);
				using (var stream = new MemoryStream(bytes))
					return Serializer.Deserialize<T>(stream);
			}
		}
	}
}
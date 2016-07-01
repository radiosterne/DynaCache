using System;
using System.Runtime.CompilerServices;

namespace NLog.Extension
{
	public class TracingLogProxy : IDisposable
	{
		private const string UndefinedCaller = "UNDEFINED";

		private readonly string _callerName;
		private readonly Logger _logger;

		public TracingLogProxy(Logger wrappedLogger, [CallerMemberName] string callerName = UndefinedCaller)
		{
			_logger = wrappedLogger;
			_callerName = callerName;
			if (_logger.IsTraceEnabled)
				_logger.Trace($"{_callerName} called");
		}

		public void Dispose()
		{
			if (_logger.IsTraceEnabled)
				_logger.Trace($"{_callerName} return");
		}
	}
}
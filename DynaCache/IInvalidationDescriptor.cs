using System.Collections.Generic;

namespace DynaCache
{
	public interface IInvalidationDescriptor
	{
		IReadOnlyCollection<string> GetCommonKeyPatternsFrom(object invalidObject);
	}
}

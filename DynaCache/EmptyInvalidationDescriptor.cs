using System.Collections.Generic;
using System.Linq;

namespace DynaCache
{
	public class EmptyInvalidationDescriptor : IInvalidationDescriptor
	{
		public IReadOnlyCollection<string> GetCommonKeyPatternsFrom(object invalidObject)
			=> Enumerable.Empty<string>().ToList();
	}
}

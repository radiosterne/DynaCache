using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynaCache.Tests.TestClasses
{
    public class NullableReturnTypeMethod : INullableReturnTypeMethod
    {
        [CacheableMethod(5)]
        public virtual int? ReturnsNullable(int? data)
        {
            return data;
        }
    }
}

#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.Tests.TestClasses
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface that is implemented by various test classes.
    /// </summary>
    public interface ICacheableMethodsTester
    {
        void Execute();

        int Execute(DateTime data);

        object Execute(string data);

        List<string> Execute(int data1, object data2);
    }
}
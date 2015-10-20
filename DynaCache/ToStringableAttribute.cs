#region Copyright 2014 Andrey Kurnoskin
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
	using System;

	/// <summary>
	/// An attribute that can be applied to a class, causing DynaCache to assume thatthis class implementation of ToString()
	/// returns unique keys
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct)]
	public sealed class ToStringableAttribute : Attribute
	{
	}
}

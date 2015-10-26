#region Copyright 2015 Andrey Kurnoskin
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents an error that occurs during DynaCache asynchronous value loading.
	/// </summary>
	[Serializable]
	public class DynaCacheAsynchronousLoadingException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DynaCacheAsynchronousLoadingException"/> class.
		/// </summary>
		public DynaCacheAsynchronousLoadingException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynaCacheAsynchronousLoadingException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public DynaCacheAsynchronousLoadingException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynaCacheAsynchronousLoadingException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public DynaCacheAsynchronousLoadingException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynaCacheAsynchronousLoadingException"/> class.
		/// </summary>
		/// <param name="inner">The inner.</param>
		public DynaCacheAsynchronousLoadingException(Exception inner)
			: base("Asynchronous value loading task failed.", inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynaCacheAsynchronousLoadingException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is null.
		/// </exception>  
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
		/// </exception>
		protected DynaCacheAsynchronousLoadingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

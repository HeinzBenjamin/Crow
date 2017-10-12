using System;
using System.Runtime.Serialization;

namespace Crow.Core.Initializers
{
    /// <summary>
    /// An <see cref="IInitializer"/> using zero function
    /// </summary>
    [Serializable]
    public class ZeroFunction : ConstantFunction
    {
        /// <summary>
        /// Creates a new zero initializer function
        /// </summary>
        public ZeroFunction()
            : base(0d)
        {
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">
        /// Serialization information to deserialize and obtain the data
        /// </param>
        /// <param name="context">
        /// Serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public ZeroFunction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Seringa.Engine.Exceptions
{
    [Serializable]
    public class SqlInjException : Exception
    {
        /// <summary>
        /// Creates a new instance of class <cref="DatabaseException"/>
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The wrappedinner exception.</param>
        public SqlInjException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Creates a new instance of class <cref="DatabaseException"/>
        /// </summary>
        /// <param name="message">The error message.</param>
        public SqlInjException(String message) : base(message) { }

        /// <summary>
        /// Creates a new instance of class <cref="DatabaseException"/>
        /// </summary>
        //public DatabaseException() { }

        /// <summary>
        /// Creates a new instance of class <cref="DatabaseException"/>
        /// </summary>
        /// <param name="si">Serialization Info</param>
        /// <param name="sc">Streaming context</param>
        protected SqlInjException(SerializationInfo si, StreamingContext sc) : base(si, sc) { }

    }
}
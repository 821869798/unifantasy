using System;

namespace UniFan.Network
{

    /// <summary>
    /// 运行时异常
    /// </summary>
    public class RuntimeException : Exception
    {
        /// <summary>
        /// 运行时异常
        /// </summary>
        public RuntimeException()
        {
        }

        /// <summary>
        /// 运行时异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public RuntimeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 运行时异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public RuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
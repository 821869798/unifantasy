using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace UniFan.Network
{
    /// <summary>
    /// 守卫
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// 验证一个条件,并在该协定的条件失败时引发异常。
        /// </summary>
        /// <typeparam name="TException">异常</typeparam>
        /// <param name="condition">条件</param>
        [DebuggerNonUserCode]
        public static void Requires<TException>(bool condition) where TException : Exception, new()
        {
            if (condition)
            {
                return;
            }
            throw new TException();
        }

        /// <summary>
        /// 不为空或者null
        /// </summary>
        /// <param name="argumentValue">参数值</param>
        /// <param name="argumentName">参数名</param>
        [DebuggerNonUserCode]
        public static void NotEmptyOrNull(string argumentValue, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// 长度大于0
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="argumentValue">参数值</param>
        /// <param name="argumentName">参数名</param>
        [DebuggerNonUserCode]
        public static void CountGreaterZero<T>(IList<T> argumentValue, string argumentName)
        {
            if (argumentValue.Count <= 0)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// 元素部位空或者null
        /// </summary>
        /// <param name="argumentValue">参数值</param>
        /// <param name="argumentName">参数名</param>
        [DebuggerNonUserCode]
        public static void ElementNotEmptyOrNull(IList<string> argumentValue, string argumentName)
        {
            foreach (string item in argumentValue)
            {
                if (string.IsNullOrEmpty(item))
                {
                    throw new ArgumentNullException(argumentName, "Argument element can not be Empty or Null.");
                }
            }
        }

        /// <summary>
        /// 内容不为空
        /// </summary>
        /// <param name="argumentValue">参数值</param>
        /// <param name="argumentName">参数名</param>
        [DebuggerNonUserCode]
        public static void NotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}

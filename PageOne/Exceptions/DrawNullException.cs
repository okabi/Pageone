using System;

namespace PageOne.Exceptions
{
    /// <summary>
    /// ドローカードが null だったときに発行する例外クラスです。
    /// </summary>
    public class DrawNullException : Exception
    {
        /// <summary>
        /// ドローカードが null だったときの例外を作成します。
        /// </summary>
        public DrawNullException() { }
    }
}

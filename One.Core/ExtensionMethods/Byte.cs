using static System.Runtime.InteropServices.JavaScript.JSType;

namespace One.Base.ExtensionMethods
{
    public static class ExtensionMethodsForByte
    {
        /// <summary>转换当前byte[]为对应的string</summary>
        /// <param name="e">string字符编码</param>
        /// <returns></returns>
        public static string ToString(this byte[] args, System.Text.Encoding encoding)
        {
            return encoding.GetString(args);
        }

        public static string ToUTF8String(this byte[] args)
        {
            var msg = System.Text.Encoding.UTF8.GetString(args);
            return msg;
        }
    }
}
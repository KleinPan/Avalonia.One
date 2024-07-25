using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace One.SimpleLog.Helpers
{
    internal class StackTraceHelper
    {
        private static readonly Assembly nlogAssembly = typeof(StackTraceHelper).Assembly;

        private static readonly Assembly mscorlibAssembly = typeof(string).Assembly;

        private static readonly Assembly systemAssembly = typeof(Debug).Assembly;

        public static Assembly LookupAssemblyFromStackFrame(StackFrame stackFrame)
        {
            MethodBase stackMethod = GetStackMethod(stackFrame);
            if ((object)stackMethod == null)
            {
                return null;
            }

            Assembly assembly = stackMethod.DeclaringType?.Assembly ?? stackMethod.Module?.Assembly;
            if (assembly == nlogAssembly)
            {
                return null;
            }

            if (assembly == mscorlibAssembly)
            {
                return null;
            }

            if (assembly == systemAssembly)
            {
                return null;
            }

            return assembly;
        }

        public static MethodBase GetStackMethod(StackFrame stackFrame)
        {
            return stackFrame?.GetMethod();
        }
    }
}

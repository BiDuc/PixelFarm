﻿//MIT, 2015, Mauricio David
using System.IO;
using System.Runtime.InteropServices;

namespace Numeria.IO
{
    internal static class IOExceptionExtensions
    {
        public static bool IsLockException(this IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }
    }
}

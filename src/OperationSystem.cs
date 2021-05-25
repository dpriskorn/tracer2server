using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tracer2Server
{
    public static class OperationSystem
    {
        public enum OSType { WINDOWS, UNIX, UNKNOWN };
        
        static private OSType s_oOSType;

        static OperationSystem()
        {
            Type t = Type.GetType ("Mono.Runtime");
            if (t != null)
            {
                Console.WriteLine("You are running with the Mono VM");

                int p = (int)Environment.OSVersion.Platform;
                if ((p == 4) || (p == 6) || (p == 128))
                {
                    s_oOSType = OSType.UNIX;
                    Console.WriteLine("Running on Unix");
                }
                else
                {
                    s_oOSType = OSType.UNKNOWN;
                    Console.WriteLine("NOT running on Unix");
                }
            }
            else
            {
                Console.WriteLine("You are running something else");
                s_oOSType = OSType.WINDOWS;
            }
        }

        public static OSType oOSType
        {
            get { return s_oOSType; }
        }

        public static bool isWindows
        {
            get { return (s_oOSType == OSType.WINDOWS); }
        }

        public static bool isUnix
        {
            get { return (s_oOSType == OSType.UNIX); }
        }

    }

}

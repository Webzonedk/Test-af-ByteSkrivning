using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test_af_ByteSkrivning
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct MyTestStruct
    {

        [FieldOffset(0)]
        public long id;
        [FieldOffset(8)]
        public unsafe fixed float temperature[8];
        [FieldOffset(0)]
        public fixed byte byteArray[40];




    }
}

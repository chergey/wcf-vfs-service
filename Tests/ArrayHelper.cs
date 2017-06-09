using System;
using System.Linq;

namespace Emroy.Vfs.Tests
{
    public class ArrayHelper
    {
        public static int[] ByteArrayToIntArray(byte[] byteArray)
        {
            var size = byteArray.Length / sizeof(int);
            var ints = new int[size];
            for (var index = 0; index < size; index++)
            {
                ints[index] = BitConverter.ToInt32(byteArray, index * sizeof(int));
            }
            return ints;
        }

        public static byte[] IntArayToByte(int[] intArray)
        {
            var result = new byte[intArray.Length * sizeof(int)];
            Buffer.BlockCopy(intArray, 0, result, 0, result.Length);
            return result;
        }
    }
}
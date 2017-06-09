using System;
using System.Runtime.InteropServices;

namespace Emroy.Vfs.Service
{
    static class Extensions
    {
     
        /// <summary>
        /// Get type size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetStructSize<T>()
        {
            var type = typeof(T);

            if (type.StructLayoutAttribute?.Value!=LayoutKind.Sequential 
                && type?.StructLayoutAttribute?.Value != LayoutKind.Explicit)
            {
                throw new ArgumentException("Only accept StructLayoutAttribute annotated classes or structs");
            }

            return Marshal.SizeOf(type);
        }

        /// <summary>
        /// Transform structure to byte
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure"></param>
        /// <returns></returns>
        public static byte[] StructToByte<T>(this T structure) where T : new()
        {
            int size = Marshal.SizeOf(structure);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        /// <summary>
        /// Transform byte to structure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static T ByteToStruct<T>(this byte[] arr) where T : new()
        {
            T obj = new T();

            int size = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());
            Marshal.FreeHGlobal(ptr);

            return obj;
        }
        /// <summary>
        /// Replace paths with disk c: to \ and separator / with \
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string BringToInternalForm(this string inputStr)
        {
            var str= inputStr.Replace("c:", VfsSystem.Separator).
                Replace("C:", VfsSystem.Separator).
                Replace("\\", VfsSystem.Separator);
            return str.Replace("//", "/").Replace("//", "/");
        }
        /// <summary>
        /// Performs backward operation to <see cref="BringToInternalForm"/>
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string BringToExternalForm(this string inputStr)
        {
            string str = inputStr;
            if (inputStr.StartsWith(VfsSystem.DiskRoot))
            {
              str= "c:" + inputStr.TrimStart(VfsSystem.SeparatorChar);
            }
             str = str.Replace(VfsSystem.Separator, "\\");
            return str;
        }


    }
}

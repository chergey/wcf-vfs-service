using System;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Service.Impl
{
    /// <summary>
    /// In-memory  storage
    /// </summary>
    public class MemoryStorage : IStorage
    {
        private readonly uint _sizeInBytes;
        readonly byte[] _data;


        public MemoryStorage(uint sizeInBytes)
        {
            _sizeInBytes = sizeInBytes;
            _data = new byte[sizeInBytes];
        }

        public  uint Size => _sizeInBytes;

        public  T Read<T>(long position) where T: new()
        {
            int size = Extensions.GetStructSize<T>();
            byte[] data = new byte[size];
            Buffer.BlockCopy(_data, (int)position, data, 0, size);

            return data.ByteToStruct<T>();

        }

        public  T[] ReadArray<T>(long position, int count) where T : new()
        {
            var data = new T[count];
            int acount = count * Extensions.GetStructSize<T>();
            Buffer.BlockCopy(_data, (int)position, data, 0, acount);

            return data;
        }

        public  void ReadArray<T>(long position, T[] data, int offset, int count) where T : new()
        {
            int acount = count * Extensions.GetStructSize<T>();
            Buffer.BlockCopy(_data, (int)position, data, offset, acount);
        }

        public  void Write<T>(long position, T structure) where T : new()
        {
            byte[] data = structure.StructToByte(); 
            Buffer.BlockCopy(data, 0, _data, (int)position, data.Length);

        }

        public  void WriteArray<T>(long position, T[] data, int offset, int count) where T : new()
        {
            int acount = count * Extensions.GetStructSize<T>();
            Buffer.BlockCopy(data, offset, _data, (int)position, acount);
            //  Array.Copy(Array.ConvertAll(data, f => new byte [data.Length]), offset, _data, position, count);

        }

    }
}

using System;
using VirtualFileSystem.Impl;
using VirtualFileSystem.Interfaces;

namespace VirtualFileSystem
{
    public class Block
    {
        /// <summary>
        /// block index
        /// </summary>
        public uint Index;


        private readonly IVfsEngine _vfs;
        
        public Block(IVfsEngine vfs, uint index = uint.MaxValue)
        {
            _vfs = vfs;
            Index = index;
        }
        
        public uint GetPosition(uint offset) => _vfs.SuperBlock.PBlockData + Index * _vfs.SuperBlock.Data.BlockSize + offset;

        /// <summary>
        /// Read data 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset">offset to read from</param>
        /// <returns></returns>
        public T Read<T>(uint offset) where T : struct
        {
            uint position = GetPosition(offset);
            return _vfs.Storage.Read<T>(position);
        }

        /// <summary>
        ///  Read data into array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset">offset to read from</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public T[] ReadArray<T>(uint offset, int count) where T : struct
        {
            uint position = GetPosition(offset);
            return _vfs.Storage.ReadArray<T>(position, count);
        }

        /// <summary>
        /// Read data into array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset"></param>
        /// <param name="array"></param>
        /// <param name="arrOffset"></param>
        /// <param name="count"></param>
        public void ReadArray<T>(uint offset, T[] array, int arrOffset, int count) where T : struct
        {
            uint position = GetPosition(offset);
            _vfs.Storage.ReadArray(position, array, arrOffset, count);
        }

        /// <summary>
        /// Write data to block
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="offset"></param>
        /// <param name="structure"></param>
        public void Write<T>(uint offset, T structure) where T : struct
        {
            uint position = GetPosition(offset);
            _vfs.Storage.Write(position, structure);
        }

        /// <summary>
        /// Writes array to block
        /// </summary>
        /// <typeparam name="T">block type</typeparam>
        /// <param name="offset"></param>
        /// <param name="array"></param>
        /// <param name="arrOffset"></param>
        /// <param name="count"></param>
        public void WriteArray<T>(uint offset, T[] array, int arrOffset, int count) where T : struct
        {
            uint position = GetPosition(offset);
            _vfs.Storage.WriteArray(position, array, arrOffset, count);
        }

        /// <summary>
        /// Creates block 
        /// </summary>
        /// <param name="vfs"></param>
        /// <param name="index">index of newly created block</param>
        /// <param name="fill">byte to fill with</param>
        /// <returns></returns>
        public static Block Create(IVfsEngine vfs, uint index, byte fill = 0)
        {
            if (index >= vfs.SuperBlock.Data.BlockCapacity)
            {
                throw new ArgumentException("Index can't be larger than BlockCapacity");
            }
            var block = new Block(vfs, index);

            byte[] data = new byte[vfs.SuperBlock.Data.BlockSize];
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = fill;
            }
            block.WriteArray(0, data, 0, data.Length);

            return block;
        }
    }
}

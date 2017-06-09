namespace Emroy.Vfs.Service.Interfaces
{
    public interface IStorage 
    {
        /// <summary>
        /// Storage size (can't be changed)
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Get T from storage
        /// </summary>
        /// <typeparam name="T">type to retrieve</typeparam>
        /// <param name="position">position to read from</param>
        /// <returns></returns>
        T Read<T>(long position) where T : new();

        /// <summary>
        /// Get array of T from storage
        /// </summary>
        /// <typeparam name="T">type to retrieve</typeparam>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        T[] ReadArray<T>(long position, int count) where T : new();

        /// <summary>
        /// Write array of T to storage
        /// </summary>
        /// <typeparam name="T">type to retrieve</typeparam>
        /// <param name="position"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void ReadArray<T>(long position, T[] array, int offset, int count) where T : new();

        /// <summary>
        /// Write T to storage
        /// </summary>
        /// <typeparam name="T">type to write</typeparam>
        /// <param name="position"></param>
        /// <param name="structure"></param>
        void Write<T>(long position, T structure) where T : new();

        /// <summary>
        /// Write array to storage
        /// </summary>
        /// <typeparam name="T">type to write</typeparam>
        /// <param name="position"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void WriteArray<T>(long position, T[] array, int offset, int count) where T : new();
    }
}

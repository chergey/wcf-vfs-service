using System;
using System.Diagnostics;
using System.IO;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Interfaces;

namespace Emroy.Vfs.Service.Impl
{
    /*
   [DebuggerDisplay("Name={" + nameof(Name) + "}")]
   public class VfsFile : IFile
   {

       public string Name;

       private uint _position;

    //   private INode _node;

       private byte[] _contents=new byte[1000];

       public VfsFile(string path, VfsFileMode vfsFileMode)
       {

       }



       public VfsFile(string path, VfsFileMode vfsFileMode)
       {
           //_vfs = vfs;

           var directory = VfsSystem.GetPathDirectory(path);
           var name = VfsSystem.GetPathName(path);
           VfsSystem.AssertNameValid(name);

           var dir = Register.Resolve(vfs, directory);
           if (dir == null)
           {
               throw new DirectoryNotFoundException("The dir doesn't exist!");
           }
           switch (vfsFileMode)
           {
               case VfsFileMode.CreateNew:

                   if (dir.Contains(name))
                   {
                       throw new IOException("File with the same name already exists!");
                   }
                   CreateFile(dir, name);
                   break;
               case VfsFileMode.Create:

                   if (dir.Contains(name))
                   {
                       OpenFile(dir, name);
                       _node.Resize(0);
                   }
                   else
                   {
                       CreateFile(dir, name);
                   }
                   break;
               case VfsFileMode.Open:

                   if (!dir.Contains(name))
                   {
                       throw new FileNotFoundException("File doesn't exist!");
                   }
                   OpenFile(dir, name);
                   break;
               case VfsFileMode.OpenOrCreate:

                   if (dir.Contains(name))
                   {
                       OpenFile(dir, name);
                   }
                   else
                   {
                       CreateFile(dir, name);
                   }
                   break;

               case VfsFileMode.Truncate:

                   if (!dir.Contains(name))
                   {
                       throw new FileNotFoundException("File doesn't exist!");
                   }
                   OpenFile(dir, name);
                   _node.Resize(0);
                   break;

               case VfsFileMode.Append:

                   if (!dir.Contains(name))
                   {
                       CreateFile(dir, name);
                   }
                   else
                   {
                       OpenFile(dir, name);
                       _position = _node.Data.SizeByte;
                   }
                   break;
               default:
                   throw new ArgumentException(vfsFileMode.ToString());
           }
       }


       public uint Size => _node.Data.SizeByte;

       private readonly object _lockObj = new object();

       public void Lock(string userName, bool val)
       {
           _node.Lock(userName, val);
       }


       //public bool IsLocked(string userName)
       //{
       //    return _node.IsLocked(userName);
       //}


       /// <summary>
       /// Creates file
       /// </summary>
       /// <param name="name">file name</param>
       /// <param name="dir">file dir</param>
       private void CreateFile(IRegister dir, string name)
       {
           _node = _vfs.AllocateNode(0);
           dir.Add(name, _node);
       }

       /// <summary>
       /// Opens file
       /// </summary>
       /// <param name="dir"></param>
       /// <param name="name"></param>
       private void OpenFile(IRegister dir, string name)
       {
           _node = Node.Load(_vfs, dir.Find(name));
       }


       public void Seek(uint position)
       {
           _position = position;
       }


       public void Write(byte[] array)
       {
           Write(array, 0, (uint)array.Length);
       }

       public void CopyTo(VfsFile destFile)
       {
           var buffer = new byte[Size];
           if (Read(buffer) > 0)
           {
               destFile.Write(buffer);
           }
       }

       public void Write(byte[] array, uint offset, uint count)
       {
           byte[] arr = new byte[count];
           Buffer.BlockCopy(array, (int)offset, arr, 0, (int)count);
           _node.Write(_position, arr);
           _position += count;
       }


       public uint Read(byte[] array)
       {
           if (_position >= _node.Data.SizeByte)
           {
               return 0;
           }
           var count = _node.Data.SizeByte - _position;
           _node.Read(_position, array, count);
           _position += count;
           return count;
       }


       public uint Read(byte[] array, uint offset, uint count)
       {
           if (_position >= _node.Data.SizeByte)
           {
               return 0;
           }
           if (_position + count > _node.Data.SizeByte)
           {
               count = _node.Data.SizeByte - _position;
           }
           byte[] arr = new byte[count];
           _node.Read(_position, arr, count);
           Buffer.BlockCopy(arr, 0, array, (int)offset, (int)count);
           _position += count;
           return count;
       }

  
}
 */
}


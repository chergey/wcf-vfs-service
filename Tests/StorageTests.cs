using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emroy.Vfs.Service;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emroy.Vfs.Tests
{
    [TestClass]
    public class StorageTests
    {
        readonly VfsDirectory _root = VfsDirectory.Root;

        [TestInitialize]
        public void SetUp()
        {

        }

        [TestMethod]
        public void TestCreateDirectoryAndFileInIt()
        {


            var directory = _root.CreateSubDirectory("dir1");
            directory.CreateFile("file2.txt");

            Assert.IsTrue(directory.Contains("file2.txt"));
            Assert.IsTrue(_root.Contains("dir1"));
        }

        [TestMethod]
        public void TestCreateDirectoryAndFileInItUsingPath()
        {

            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            _root.CreateFile("dir1\\file22.txt");

            var dir2 = _root.CreateSubDirectory("dir1\\dir2");
            _root.CreateFile("dir1\\dir2\\file3.txt");
            Assert.IsTrue(dir1.Contains("file22.txt"));

            Assert.IsTrue(dir1.Contains("file22.txt"));
            Assert.IsTrue(dir1.Contains("dir2"), "dir1 does not contain dir2");

            Assert.IsTrue(dir2.Contains("file3.txt"), "dir2 does not contain file3");
        }


        [TestMethod]
        public void TestCopyDirectory()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            _root.CreateFile("file1.txt");
            _root.CreateFile("file123.txt");
            _root.CreateFile("file1234.txt");
            _root.CreateFile("file12345.txt");
            _root.CreateFile("file123456.txt");

            _root.CreateFile("dir1\\file22.txt");

            _root.CreateSubDirectory("dir1\\dir2");
            Assert.IsTrue(dir1.Contains("file22.txt"));


            var dir11 = _root.CreateSubDirectory("dir11");
            var dir23 = dir11.CreateSubDirectory("dir23");
            var dir12 = _root.CreateSubDirectory("dir12");

            dir12.CreateSubDirectory("dir22");
            _root.CreateFile("dir12\\dir22\\file31.txt");
            _root.CopyEntity("dir1", "dir11\\dir23");


            Assert.IsTrue(dir23.Contains("dir1"));
            Assert.IsTrue((dir23 as VfsEntity).Parent.Name == "dir11");

            ShouldThrow(() => _root.CopyEntity("dir1\\file22.txt", "dir12\\dir22\\file31.txt"),
                "File got copied to a file! AAAA!!!");

            ShouldThrow(() => _root.CopyEntity("dir1", "dir11\\dir23"), 
                "Directory got copied twice! AAAAA!");


        }

      


        [TestMethod]
        public void TestMove()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            var file1 = _root.CreateFile("file1.txt");
            _root.CreateFile("file123.txt");
            _root.CreateFile("file1234.txt");
            _root.CreateFile("file12345.txt");
            _root.CreateFile("file123456.txt");
            _root.MoveEntity("file123456.txt", "dir1");

   
            Assert.IsFalse(_root.Contains("file123456.txt"));

        }


        [TestMethod]
        public void TestDelete()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateSubDirectory("dir2");
            ShouldThrow(() => _root.DeleteSubDirectory("dir1", false),
                "Directory with subdiretories got deleted! AAAA!!");

        }

        
        [TestMethod]
        public void TestDeleteWithSubdirectories()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            var dir2 = dir1.CreateSubDirectory("dir2");
            dir2.CreateSubDirectory("dir3");
            _root.DeleteSubDirectory("dir1", true);

        }
        [TestMethod]
        public void TestLockedFile()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            var file1 = _root.CreateFile("file1.txt");
            _root.CreateFile("file123.txt");
            _root.CreateFile("file1234.txt");
            _root.CreateFile("file12345.txt");
            _root.CreateFile("file123456.txt");
            file1.LockFile("Antonio");

            ShouldThrow(() => _root.DeleteFile("file1.txt"), "Locked file got deleted! AAAA!");
    
          

        }

        [TestMethod]
        public void TestLockedFileInDirectory()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            var dir2 = dir1.CreateSubDirectory("dir2");
             dir2.CreateFile("file3.txt");
            _root.LockFile("dir1\\dir2\\file3.txt", "default", true);
            dir2.CreateSubDirectory("dir3");
            ShouldThrow(() => _root.DeleteSubDirectory("dir1", true), "Locked file got deleted! AAAA!!");

           

        }

     
        [TestCleanup]
        public void TearDown()
        {
           _root.Clean();

        }
        public void ShouldThrow(Action action, string message)
        {
            try
            {
                action();
                Assert.Fail(message);
            }
            catch (VfsException) { }
        }
    }
}

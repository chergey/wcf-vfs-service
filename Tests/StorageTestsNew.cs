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
    public class StorageTestsNew
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
            Assert.IsTrue(dir23.Parent.Name == "dir11");


         //  _root.CopyEntity("dir1\\file22.txt", "dir12\\dir22\\file31.txt");
         //    _root.CopyEntity("dir1\\file22.txt", "dir12\\dir22\\file22.txt");

        }

        [TestMethod]
        public void TestLock()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            var file1 = _root.CreateFile("file1.txt");
            _root.CreateFile("file123.txt");
            _root.CreateFile("file1234.txt");
            _root.CreateFile("file12345.txt");
            _root.CreateFile("file123456.txt");


            file1.LockFile("Antonio");
            file1.UnLockFile("Antonio");
            _root.DeleteFile("file1.txt");

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

            var a=_root.GetContents("");
            Assert.IsFalse(_root.Contains("file123456.txt"));

        }

    }
}

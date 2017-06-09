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
        readonly Directory _root = VfsSystem.Root;

        [TestInitialize]
        public void SetUp()
        {

        }

        [TestMethod]
        public void TestCreateDirectoryAndFileInIt()
        {


            var directory = _root.CreateSubDirectory("dir1");
            directory.CreateFile("file2.txt", VfsFileMode.CreateNew);

            Assert.IsTrue(directory.Contains("file2.txt"));
            Assert.IsTrue(_root.Contains("dir1"));
        }

        [TestMethod]
        public void TestCreateDirectoryAndFileInItUsingPath()
        {

            var directory = _root.CreateSubDirectory("dir1");
            directory.CreateFile("file2.txt", VfsFileMode.CreateNew);

            _root.CreateFile("dir1/file22.txt", VfsFileMode.CreateNew);
            _root.CreateSubDirectory("dir1/dir2");
            Assert.IsTrue(directory.Contains("file22.txt"));

            Assert.IsTrue(directory.Contains("file22.txt"));
            Assert.IsTrue(directory.Contains("dir2"));
        }
        [TestMethod]
        public void TestCopyDirectory()
        {
            var directory = _root.CreateSubDirectory("dir1");
            directory.CreateFile("file2.txt", VfsFileMode.CreateNew);

            _root.CreateFile("dir1/file22.txt", VfsFileMode.CreateNew);
            _root.CreateSubDirectory("dir1/dir2");
            directory.Contains("file22.txt");

            _root.CreateSubDirectory("dir11");
            _root.CopyEntity("dir1", "dir11");

        }

    }
}

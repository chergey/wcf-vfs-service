using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emroy.Vfs.Service;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;
using Emroy.Vfs.Service.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emroy.Vfs.Tests
{
    /*
    [TestClass]
    public class StorageTests
    {
        private VfsSystem _system;
        const int Count = 100;

        private string _someText = @"
            There are a few cases in the .NET Framework object model where the type of a property is purposely made to be non-specific. For example, the DataSource property on the DataGridView class is typed as object. This design permits the data source to accept several kinds of input, but it provides no common place to add metadata to describe the characteristics of the property. Each data-source property throughout the .NET Framework needs to have identical metadata for type converters and user interface (UI) type editors.
            The AttributeProviderAttribute class addresses this situation. When this attribute is placed on a property, the rules change for obtaining attributes for the property descriptor's Attributes collection. Usually, the property descriptor gathers local attributes and merges them with attributes from the property type. When the AttributeProviderAttribute attribute is applied, the attributes are taken from the type returned from AttributeProviderAttribute, not from the actual property type. The AttributeProviderAttribute is used on data sources to point the data source’s specific type to IListSource, and the appropriate metadata is placed on IListSource to enable data binding. This redirection allows external parties such as Visual Studio to easily add metadata to all data sources.
            Attributes obtained from a type declared in the AttributeProviderAttribute have a priority between the attributes of the property’s type and the attributes on the property. The full set of attributes available is the merger, in priority order, as shown in the following list
            ";

        [TestInitialize]
        public void SetUp()
        {
            var storage = new MemoryStorage(1 << 10 << 10);
            _system = new VfsSystem(storage);
            _system.Format(1 << 10, 4);
        }

        [TestMethod]
        public void TestCreatectory()
        {

            var root = _system.Root;
            root.CreateSubDirectory("dir1");

            Assert.IsTrue(root.Contains("dir1"));

        }

        [TestMethod]
        public void TestCreateFile()
        {
            const int count = 100;

            var file = _system.CreateFile("/file1.txt", VfsFileMode.CreateNew);
            int[] originalData = WriteToFileSomeStuff(file, count);
            file.Seek(0);
            var readData = new byte[count * sizeof(int)];
            var bytesRead = file.Read(readData);
            Assert.AreEqual(bytesRead, (uint)count * sizeof(int));

            var processedData = ArrayHelper.ByteArrayToIntArray(readData);
            Assert.IsTrue(processedData.SequenceEqual(originalData));



        }

      

        [TestMethod]
        public void TestIgnoreLock()
        {
            var root = _system.Root;
            root.CreateSubDirectory("dir1");
            var dir1 = _system.FindDirectory("/dir1");

            var file1 = _system.CreateFile("/dir1/file.txt", VfsFileMode.Create);
            file1.Lock("test_user", true);
            root.Delete("dir1", true, false, false);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestLock()
        {
            var root = _system.Root;
            root.CreateSubDirectory("dir1");
            _system.CreateFile("/file2.txt", VfsFileMode.Create);
            _system.FindDirectory("/dir1");

            var file1 = _system.CreateFile("/dir1/file.txt", VfsFileMode.Create);
            file1.Lock("test_user", true);
            root.Delete("dir1", false, false, false);
            //_system.DeleteDirectory("/dir1");

        }

        [TestMethod]

        public void TestLockUnLock()
        {
            var root = _system.Root;
            root.CreateSubDirectory("dir1");
            _system.CreateFile("/file2.txt", VfsFileMode.Create);
            _system.FindDirectory("/dir1");

            var file1 = _system.CreateFile("/dir1/file.txt", VfsFileMode.Create);
            file1.Lock("test_user", true);
            file1.Lock("test_user", false);
            root.Delete("dir1", false, false, false);
            //_system.DeleteDirectory("/dir1");

        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestDeleteFile()
        {
            var root = _system.Root;
            root.CreateSubDirectory("dir1");
            _system.CreateFile("/file2.txt", VfsFileMode.Create);


            _system.CreateFile("/dir1/file.txt", VfsFileMode.Create);
            var dir = _system.FindDirectory("/dir1");
            dir.Delete("file.txt", false,false, false);
            _system.CreateFile("/dir1/file.txt", VfsFileMode.Open);

        }


        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestDeleteFileWithRelativePath()
        {
            var root = _system.Root;
            root.CreateSubDirectory("dir1");
            _system.CreateFile("/file1.txt", VfsFileMode.Create);


            _system.CreateFile("/dir1/file2.txt", VfsFileMode.Create);
            var dir = _system.FindDirectory("/dir1");
            dir.CreateSubDirectory("dir2");
            _system.CreateFile("/dir1/dir2/file3.txt", VfsFileMode.Create);
            root = _system.Root;
            root.Delete("dir1/dir2/file3.txt", false, false, false);
            _system.CreateFile("/dir1/dir2/file2.txt", VfsFileMode.Open);

        }


        [TestMethod]
        public void TestGetRecursiveDirs()
        {

            _system.CreateFile("/file.txt", VfsFileMode.Create);
            var root = _system.Root;

            root.CreateSubDirectory("dir1");

            _system.CreateFile("/dir1/file1.txt", VfsFileMode.Create);
            var dir1 = _system.FindDirectory("/dir1");
            dir1.CreateSubDirectory("dir2");
            _system.CreateFile("/dir1/dir2/file1.txt", VfsFileMode.Create);
            var dirList = _system.GetRecursiveDirs().Keys;

            var expectedResult = new List<string> {  "/file.txt", "/dir1", "/dir1/file1.txt", "/dir1/dir2", "/dir1/dir2/file1.txt" };
            Assert.IsTrue(dirList.SequenceEqual(expectedResult));

        }
       
        [TestMethod]
        public void TestCopy()
        {
            //TODO: make common initialization

            var originalData = CreateDir1AndDir2AndFile2WithSomeData();
            _system.Root.CreateSubDirectory("dir1!Copy");
            _system.Copy("/dir1", "/dir1!Copy");

            _system.FindDirectory("/dir1!Copy");
            var dirList = _system.GetRecursiveDirs("/dir1!Copy/").Keys;
            var expectedResult = new List<string> { "/dir1!Copy/file1.txt", "/dir1!Copy/dir2", "/dir1!Copy/dir2/file2.txt" };
            Assert.IsTrue(dirList.SequenceEqual(expectedResult));

            IFile copiedFile1 = _system.CreateFile("/dir1!Copy/dir2/file2.txt", VfsFileMode.Open);

            byte[] readData = new byte[Count * sizeof(int)];


            var bytesRead = copiedFile1.Read(readData);
            Assert.AreEqual(bytesRead, (uint)Count * sizeof(int));

            var processedData = ArrayHelper.ByteArrayToIntArray(readData);
            Assert.IsTrue(processedData.SequenceEqual(originalData));

        }
      

       


        [TestMethod]
    //    [ExpectedException(typeof(InvalidOperationException))]
        public void TestMove()
        {
      
            CreateDir1AndDir2AndFile2AndDir3AndFile3();

            _system.Move("/dir1", "/dir11");
            _system.FindDirectory("/dir11");

        }




        [TestMethod]
        public void TestPrint()
        {
            CreateDir1AndDir2AndFile2WithSomeData();
            string useName = "Antonio";
            Console.WriteLine(_system.GetTextualRepresentation(useName));


        }

        [TestCleanup]
        public void TearDown()
        {


        }



        #region  Helper methods
        private static int[] WriteToFileSomeStuff(IFile file, int count)
        {
            var originalData = Enumerable.Range(0, count).ToArray();

            var byteData = ArrayHelper.IntArayToByte(originalData);
            file.Write(byteData);
            return originalData;
        }

        private int[] CreateDir1AndDir2AndFile2WithSomeData()
        {


            _system.CreateFile("/file.txt", VfsFileMode.Create);
            var root = _system.Root;

            root.CreateSubDirectory("dir1");

            _system.CreateFile("/dir1/file1.txt", VfsFileMode.Create);
            var dir1 = _system.FindDirectory("/dir1");
            dir1.CreateSubDirectory("dir2");
            var file2 = _system.CreateFile("/dir1/dir2/file2.txt", VfsFileMode.Create);


            var originalData = WriteToFileSomeStuff(file2, Count);


            return originalData;
        }

        private void CreateDir1AndDir2AndFile2AndDir3AndFile3()
        {


            _system.CreateFile("/file.txt", VfsFileMode.Create);
            var root = _system.Root;

            root.CreateSubDirectory("dir1");
            root.CreateSubDirectory("dir11");

            _system.CreateFile("/dir1/file1.txt", VfsFileMode.Create);
            var dir1 = _system.FindDirectory("/dir1");
            dir1.CreateSubDirectory("dir2");
            _system.CreateFile("/dir1/dir2/file2.txt", VfsFileMode.Create);


            var dir2 = _system.FindDirectory("/dir1/dir2");
            dir2.CreateSubDirectory("dir3");
            _system.CreateFile("/dir1/dir2/dir3/file3.txt", VfsFileMode.Create);


        }


        #endregion

    }
    */
}
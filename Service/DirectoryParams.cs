namespace VirtualFileSystem
{

        /// <summary>
        /// Directory attributes
        /// </summary>
        public class DirectoryParams
        {
            public bool IsDirectory { get; set; }

            public string Name { get; set; }

            public string Path { get; set; }

            public uint Size { get; set; }

            public ulong AccessTime { get; set; }

            public ulong ModifyTime { get; set; }

            public ulong CreationTime { get; set; }

            public uint Flags { get; set; }

            public uint Owner { get; set; }

            public NodeData Inode { get; set; }

            public uint InodeIndex { get; set; }
        }
    }

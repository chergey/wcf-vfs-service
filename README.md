# Description
- Client - console client, через ChannelFactory<T>
- Service - WCF-service
- WinService - windows service
- СonsoleHost - console host
- Tests - Vfs storage tests

# API
- Vfs operations are performed in Root directory (default = c:), e.g., Root.DeleteFile("directory\subdirectory\file1.txt"). With this approach, however, we don't remmember full path when throwing expceptions
- Can also find neeed directory using FindEntity, then call appropriate method (e.g. dir=FindEntity("directory\subdirectory"); dir.DeleteFile("file1.txt"))


# Found bugs
- Debugging does not work correctly (Step Into, member variables don't show in Locals and Autos windows) when adding CNeptune to references (which is necessary to make it work with NConcern) - bug is hiding in Mono.Cecil

# Misc
- VfsFile has a public constructor (which is bad), can be remedied by making VfsDirectory implemening IVfsFile but then will have to store type of entity a a file in VfsEntity



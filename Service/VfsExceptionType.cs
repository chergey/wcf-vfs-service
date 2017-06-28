namespace Service
{
    public enum VfsExceptionType
    {
        VfsFileExists,
        VfsFileNotExists,
        VfsDirExists,
        VfsDirNotExists,
        ObjNotExists,
        ObjAlreadyExists,
        DirContainsSubdirs,
        DirContainLockedFiles,
        DirCorrespondToFile,
        CantMoveLockedFile,
        CantDeleteLockedFile,
        CantMoveDeleteLockedFile,
        CantLockDir,
        FileIsAlreadyLocked,
        FileIsNotLocked,
        WrongNumberOfArguments,
        MovingDeletingCurDirIsNotAllowed


    }
}
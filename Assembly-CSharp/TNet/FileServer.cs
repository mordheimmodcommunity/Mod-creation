namespace TNet
{
    public class FileServer
    {
        private struct FileEntry
        {
            public string fileName;

            public byte[] data;
        }

        private List<FileEntry> mSavedFiles = new List<FileEntry>();

        protected void Error(string error)
        {
        }

        public void SaveFile(string fileName, byte[] data)
        {
            bool flag = false;
            for (int i = 0; i < mSavedFiles.size; i++)
            {
                FileEntry fileEntry = mSavedFiles[i];
                if (fileEntry.fileName == fileName)
                {
                    fileEntry.data = data;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                FileEntry item = default(FileEntry);
                item.fileName = fileName;
                item.data = data;
                mSavedFiles.Add(item);
            }
            Tools.WriteFile(fileName, data);
        }

        public byte[] LoadFile(string fileName)
        {
            for (int i = 0; i < mSavedFiles.size; i++)
            {
                FileEntry fileEntry = mSavedFiles[i];
                if (fileEntry.fileName == fileName)
                {
                    return fileEntry.data;
                }
            }
            return Tools.ReadFile(fileName);
        }

        public void DeleteFile(string fileName)
        {
            int num = 0;
            while (true)
            {
                if (num < mSavedFiles.size)
                {
                    FileEntry fileEntry = mSavedFiles[num];
                    if (fileEntry.fileName == fileName)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            mSavedFiles.RemoveAt(num);
            Tools.DeleteFile(fileName);
        }
    }
}

using System;

namespace ActivityHistory
{
    public class MultiFileRowWriter : IRowWriter
    {
        private SingleFileRowWriter writer;

        public string FileNamePattern { get; private set; }

        public MultiFileRowWriter(string fileNamePattern)
        {
            FileNamePattern = fileNamePattern;
        }

        public void WriteRow(FocusChange focusChange)
        {
            WriteRow(DetermineFileName(focusChange), focusChange);
        }

        private string DetermineFileName(FocusChange focusChange)
        {
            return string.Format(FileNamePattern, FocusChange.DateFromTimestamp(focusChange));
        }

        private void WriteRow(string fileName, FocusChange focusChange)
        {
            EnsureOpenedLogFile(fileName).WriteRow(focusChange);
        }

        private SingleFileRowWriter EnsureOpenedLogFile(string fileName)
        {
            if (writer == null || writer.OpenedFile != fileName)
            {
                Refs.Set(ref writer, OpenLogFile(fileName));
            }
            return writer;
        }
        private SingleFileRowWriter OpenLogFile(string fileName) {
            return new SingleFileRowWriter(fileName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Refs.Dispose(writer);
        }

        ~MultiFileRowWriter()
        {
            Dispose(false);
        }
    }
}

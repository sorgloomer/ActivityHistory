using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.IO;
using System.Text;

namespace ActivityHistory
{
    public class SingleFileRowWriter : IDisposable, IRowWriter
    {
        public string OpenedFile { get; private set; }
        public CsvWriter CsvWriter { get; private set; }

        public SingleFileRowWriter(string fileName)
        {
            OpenedFile = fileName;
            CsvWriter = new CsvWriter(
                EnsureDirAndOpenStreamWriter(fileName),
                new Configuration() { Delimiter = ";" }
            );
        }

        public static StreamWriter EnsureDirAndOpenStreamWriter(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            return new StreamWriter(fileName, true, Encoding.UTF8);
        }

        public void WriteRow(FocusChange focusChange)
        {
            CsvWriter.WriteRecord(focusChange);
            CsvWriter.NextRecord();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Refs.Dispose(CsvWriter);
        }

        ~SingleFileRowWriter()
        {
            Dispose(false);
        }
    }
}

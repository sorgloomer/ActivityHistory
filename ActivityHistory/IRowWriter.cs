using System;

namespace ActivityHistory
{
    public interface IRowWriter : IDisposable
    {
        void WriteRow(FocusChange focusChange);
    }
}
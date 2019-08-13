using ComparerExtensions;
using System.Collections;
using System.Collections.Generic;

namespace ActivityHistory
{
    public class FocusInfo
    {
        public static readonly IEqualityComparer<FocusInfo> Equality = KeyEqualityComparer<FocusInfo>
            .Using(f => f.WindowTitle)
            .And(f => f.PID)
            .And(f => f.HWND)
            .And(f => f.ExecutableName)
            .And(f => f.ExecutableFile)
            .And(f => f.Error)
            .AndHandleNulls();
        public string WindowTitle { get; set; }
        public long PID { get; set; }
        public long HWND { get; set; }
        public string ExecutableName { get; set; }
        public string ExecutableFile { get; set; }
        public string Error { get; set; }
    }
}
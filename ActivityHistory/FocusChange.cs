using ComparerExtensions;
using System;
using System.Collections.Generic;

namespace ActivityHistory
{
    public class FocusChange
    {
        public static readonly IEqualityComparer<FocusChange> Equality = KeyEqualityComparer<FocusChange>
            .Using(f => f.Timestamp)
            .And(f => f.FocusInfo)
            .AndHandleNulls();

        public const string DateFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public string Timestamp { get; set; }
        public FocusInfo FocusInfo { get; set; }

    }
}
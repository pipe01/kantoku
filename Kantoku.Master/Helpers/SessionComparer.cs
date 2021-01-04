using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Windows.Media.Control;

namespace Kantoku.Master.Helpers
{
    public class SessionComparer : IEqualityComparer<GlobalSystemMediaTransportControlsSession>
    {
        public bool Equals(GlobalSystemMediaTransportControlsSession? x, GlobalSystemMediaTransportControlsSession? y)
        {
            if (x is null)
                throw new ArgumentNullException(nameof(x));
            if (y is null)
                throw new ArgumentNullException(nameof(y));

            return x.SourceAppUserModelId == y.SourceAppUserModelId;
        }

        public int GetHashCode([DisallowNull] GlobalSystemMediaTransportControlsSession obj) => obj.SourceAppUserModelId.GetHashCode();
    }
}

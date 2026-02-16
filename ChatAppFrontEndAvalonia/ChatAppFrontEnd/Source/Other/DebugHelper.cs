using System;

namespace ChatAppFrontEnd.Source.Other
{
    public static class DebugHelper
    {
        public static bool IS_DEBUG { get; } = string.Equals(Environment.GetEnvironmentVariable("DEBUG_MODE"), "true", StringComparison.OrdinalIgnoreCase);
    }
}


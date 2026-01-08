using System;

namespace RemainderLambda
{
    // boilerplate code to load .env files for local development
    public static class EnvLoader
    {
        // Call this ONLY for local dev/tests.
        public static void LoadDotEnv(string path = ".env")
        {
            if (!File.Exists(path)) return;

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#")) continue;

                var idx = line.IndexOf('=');
                if (idx <= 0) continue;

                var key = line.Substring(0, idx).Trim();
                var value = line.Substring(idx + 1).Trim().Trim('"');

                // Don't override if already set
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
}

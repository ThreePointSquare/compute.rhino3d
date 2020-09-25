using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Formatting.Json;

namespace compute.geometry
{
    static class Logging
    {
        static bool _enabled = false;

        public static void Init()
        {
            if (_enabled)
                return;

            var dir = GetLogDir(Path.Combine(Path.GetTempPath(), "Compute", "Logs"));
            var path = Path.Combine(dir, "log-geometry-.txt"); // log-geometry-20180925.txt, etc.
            var limit = GetLogLimit(10);

            var logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .Enrich.FromLogContext()
                //.Enrich.WithProperty("Source", "geometry")
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);

            Log.Logger = logger.CreateLogger();

            // log warnings if deprecated env vars used (see helpers below)
            foreach (var msg in _warnings)
                Log.Warning(msg);

            Log.Debug("Logging to {LogPath}", Path.GetDirectoryName(path));

            _enabled = true;
        }

        // helper methods to handle renamed environment variables without breaking things for anyone
        // using the existing names
        // TODO: clean up this mess in a few months time!

        private static List<string> _warnings = new List<string>();

        private static string GetLogDir(string default_dir)
        {
            var dir = Env.GetEnvironmentString("RHINO_COMPUTE_LOG_PATH", null);
            if (dir == null)
            {
                dir = Env.GetEnvironmentString("COMPUTE_LOG_PATH", null);
                if (dir != null)
                    _warnings.Add($"COMPUTE_LOG_PATH is deprecated; use RHINO_COMPUTE_LOG_PATH instead");
            }
            return dir ?? default_dir;
        }

        private static int GetLogLimit(int default_limit)
        {
            var limit = Env.GetEnvironmentInt("RHINO_COMPUTE_LOG_RETAIN_DAYS", 0);
            if (limit == 0)
            {
                limit = Env.GetEnvironmentInt("COMPUTE_LOG_RETAIN_DAYS", 0);
                if (limit != 0)
                    _warnings.Add($"COMPUTE_LOG_RETAIN_DAYS is deprecated; use RHINO_COMPUTE_LOG_RETAIN_DAYS instead");
            }
            return limit != 0 ? limit : default_limit;
        }
    }
}

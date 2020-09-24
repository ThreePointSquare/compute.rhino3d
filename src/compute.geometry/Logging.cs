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

            var default_dir = Path.Combine(Path.GetTempPath(), "Compute", "Logs");
            var dir = Env.GetEnvironmentString("RHINO_COMPUTE_LOG_PATH", default_dir);
            var path = Path.Combine(dir, "log-geometry-.txt"); // log-geometry-20180925.txt, etc.
            var limit = Env.GetEnvironmentInt("RHINO_COMPUTE_LOG_RETAIN_DAYS", 10);

            var logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .Enrich.FromLogContext()
                //.Enrich.WithProperty("Source", "geometry")
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);

            Log.Logger = logger.CreateLogger();
            Log.Debug("Logging to {LogPath}", Path.GetDirectoryName(path));

            _enabled = true;
        }
    }
}

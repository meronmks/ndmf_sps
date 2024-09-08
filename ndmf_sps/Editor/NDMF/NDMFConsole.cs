using System;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace com.meronmks.ndmfsps
{
    internal static class NDMFConsole
    {
        private static readonly Localizer Localizer = new Localizer("en-us", () =>
        {
            return Localization.GetCodes().Select(code => (code, LocalizationFunction(code))).ToList();
        });
        
        private static Func<string, string> LocalizationFunction(string code)
        {
            return key => Localization.S(key, code);
        }

        public static void LogInfo(string key, params object[] args)
        {
            ErrorReport.ReportError(Localizer, ErrorSeverity.Information, key, args);
        }
        
        public static void LogWarning(string key, params object[] args)
        {
            ErrorReport.ReportError(Localizer, ErrorSeverity.NonFatal, key, args);
        }
        
        public static void LogError(string key, params object[] args)
        {
            ErrorReport.ReportError(Localizer, ErrorSeverity.Error, key, args);
        }
    }
}
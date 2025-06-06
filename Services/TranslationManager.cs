using System.Globalization;
using System.Threading;

namespace WKFTournamentIS.Services
{
    public static class TranslationManager
    {
        public static void SetLanguage(string languageCode)
        {
            try
            {
                var cultureInfo = new CultureInfo(languageCode);
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }
            catch (CultureNotFoundException)
            {
                var defaultCultureInfo = new CultureInfo("sr-Latn-BA");
                Thread.CurrentThread.CurrentUICulture = defaultCultureInfo;
                Thread.CurrentThread.CurrentCulture = defaultCultureInfo;
            }
        }
    }
}
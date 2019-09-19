using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Entities
{
    // This class is like a strin enum
    // We need to find a better implemenation
    public class LanguageType
    {
        private string _languageType;

        private LanguageType(string languageType)
        {
            _languageType = languageType;
        }

        public override string ToString()
        {
            return _languageType;
        }

        // https://cloud.google.com/translate/docs/languages?refresh=1
        public static LanguageType ENGLISH = new LanguageType("en");
        public static LanguageType FRENCH = new LanguageType("fr");
        public static LanguageType SPANISH = new LanguageType("es");
        public static LanguageType PORTUGUESE = new LanguageType("pt");
        public static LanguageType GERMAN = new LanguageType("de");

        public static string GetDescription(string languageType)
        {
            switch(languageType)
            {
                case @"en":
                    return @"English (en)";
                case @"fr":
                    return @"French (fr)";
                case @"es":
                    return @"Spanish (es)";
                case @"pt":
                    return @"Portuguese (pt)";
                case @"de":
                    return @"German (de)";
                default:
                    return $"Unknown ({languageType})";
            }
        }
    }
}

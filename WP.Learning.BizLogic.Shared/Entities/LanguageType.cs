using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Entities
{
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
    }
}

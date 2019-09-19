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
        public static LanguageType HINDI = new LanguageType("hi");

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
                case @"hi":
                    return @"Hindi (hi)";
                default:
                    return $"Unknown ({languageType})";
            }
        }

        public static string GetSupportedLanguages(string cmd)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"To change Buzz's language use one of these cmds");
            sb.AppendLine($"---------------------");
            sb.AppendLine($"{cmd}-en  for English");
            sb.AppendLine($"{cmd}-fr  for French");
            sb.AppendLine($"{cmd}-es  for Spanish");
            sb.AppendLine($"{cmd}-pt  for Portuguese");
            sb.AppendLine($"{cmd}-de  for German");
            sb.AppendLine($"{cmd}-hi  for Hindi");

            return sb.ToString();
        }
    }
}

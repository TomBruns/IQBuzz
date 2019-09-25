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
        public static readonly LanguageType ENGLISH = new LanguageType("en");
        public static readonly LanguageType FRENCH = new LanguageType("fr");
        public static readonly LanguageType SPANISH = new LanguageType("es");
        public static readonly LanguageType PORTUGUESE = new LanguageType("pt");
        public static readonly LanguageType GERMAN = new LanguageType("de");
        public static readonly LanguageType HINDI = new LanguageType("hi");
        public static readonly LanguageType SWEDISH = new LanguageType("sv");

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
                case @"sv":
                    return @"Swedish (sv)";
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
            sb.AppendLine($"{cmd}-sv  for Swedish");

            return sb.ToString();
        }
    }
}

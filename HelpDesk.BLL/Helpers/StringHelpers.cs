using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HelpDesk.BLL.Helpers
{
    public class StringHelpers
    {
        public static string UrlFormatConverter(string name)
        {
            string sonuc = name.ToLower();
            sonuc = sonuc.Replace("'", "");
            sonuc = sonuc.Replace(" ", "-");
            sonuc = sonuc.Replace("<", "");
            sonuc = sonuc.Replace(">", "");
            sonuc = sonuc.Replace("&", "");
            sonuc = sonuc.Replace("[", "");
            sonuc = sonuc.Replace("!", "");
            sonuc = sonuc.Replace("]", "");
            sonuc = sonuc.Replace("ı", "i");
            sonuc = sonuc.Replace("ö", "o");
            sonuc = sonuc.Replace("ü", "u");
            sonuc = sonuc.Replace("ş", "s");
            sonuc = sonuc.Replace("ç", "c");
            sonuc = sonuc.Replace("ğ", "g");
            sonuc = sonuc.Replace("İ", "I");
            sonuc = sonuc.Replace("Ö", "O");
            sonuc = sonuc.Replace("Ü", "U");
            sonuc = sonuc.Replace("Ş", "S");
            sonuc = sonuc.Replace("Ç", "C");
            sonuc = sonuc.Replace("Ğ", "G");
            sonuc = sonuc.Replace("|", "");
            sonuc = sonuc.Replace(".", "-");
            sonuc = sonuc.Replace("?", "-");
            sonuc = sonuc.Replace(";", "-");
            sonuc = sonuc.Replace("#", "-sharp");
            sonuc = sonuc.Replace("/", "-");
            sonuc = sonuc.Replace(@"\", "-");
            sonuc = sonuc.Replace("ä", "a");
            sonuc = sonuc.Replace("á", "a");
            sonuc = sonuc.Replace("é", "e");
            sonuc = sonuc.Replace("ß", "ss");
            sonuc = sonuc.Replace("æ", "ae");

            return sonuc;
        }

        public static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string[] items = text.Split(' ');
            string result = string.Empty;
            foreach (string item in items)
            {
                if (item.Length > 1)
                {
                    result += $"{(item.Substring(0, 1).ToUpper())}{item.Substring(1).ToLower()}";
                }
                else
                {
                    result += $"{item}";
                }
            }

            return result.Trim();
        }

        public static string GetCode() => Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            "[/+=]", "").ToLower(new CultureInfo("en-US", false));

    }
}

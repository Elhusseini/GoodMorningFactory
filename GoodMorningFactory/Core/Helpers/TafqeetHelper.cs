using System;

namespace GoodMorningFactory.Core.Helpers
{
    /// <summary>
    /// A helper class to convert numbers to Arabic words (Tafqeet).
    /// </summary>
    public static class TafqeetHelper
    {
        private static string[] Ones = { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة", "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
        private static string[] Tens = { "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
        private static string[] Hundreds = { "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };
        private static string[] Thousands = { "", "ألف", "ألفان", "آلاف" };
        private static string[] Millions = { "", "مليون", "مليونان", "ملايين" };

        private static string ConvertNumber(long number)
        {
            if (number == 0) return "صفر";

            if (number < 20)
                return Ones[number];

            if (number < 100)
                return Ones[number % 10] + (string.IsNullOrEmpty(Ones[number % 10]) ? "" : " و") + Tens[number / 10];

            if (number < 1000)
                return Hundreds[number / 100] + (number % 100 == 0 ? "" : " و" + ConvertNumber(number % 100));

            if (number < 1000000)
            {
                long thousands = number / 1000;
                string thousandsStr;
                if (thousands == 1) thousandsStr = Thousands[1];
                else if (thousands == 2) thousandsStr = Thousands[2];
                else if (thousands >= 3 && thousands <= 10) thousandsStr = thousands.ToString() + " " + Thousands[3];
                else thousandsStr = thousands.ToString() + " " + Thousands[1];

                return ConvertNumber(thousands) + " " + thousandsStr + (number % 1000 == 0 ? "" : " و" + ConvertNumber(number % 1000));
            }

            long millions = number / 1000000;
            string millionsStr;
            if (millions == 1) millionsStr = Millions[1];
            else if (millions == 2) millionsStr = Millions[2];
            else if (millions >= 3 && millions <= 10) millionsStr = millions.ToString() + " " + Millions[3];
            else millionsStr = millions.ToString() + " " + Millions[1];

            return ConvertNumber(millions) + " " + millionsStr + (number % 1000000 == 0 ? "" : " و" + ConvertNumber(number % 1000000));
        }

        public static string ToWords(decimal number, string currencyName, string fractionalCurrencyName)
        {
            if (number == 0) return $"فقط صفر {currencyName} لا غير";

            long integerPart = (long)Math.Truncate(number);
            int fractionalPart = (int)Math.Round((number - integerPart) * 100);

            string result = "فقط " + ConvertNumber(integerPart) + " " + currencyName;

            if (fractionalPart > 0)
            {
                result += " و " + ConvertNumber(fractionalPart) + " " + fractionalCurrencyName;
            }

            return result + " لا غير";
        }
    }
}

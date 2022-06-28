using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace pow.aidkit
{
    public class Converter : MonoBehaviour
    {
        public static string NumberTextFormatterV2(float number, string format = "F2")
        {
            if (number < 1000) return number.ToString(format, CultureInfo.CreateSpecificCulture("en-US"));

            string[] simpleText = {"", "K", "M", "B", "T"};
            string[] complexText =
            {
                "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
                "u", "v", "w", "x", "y", "z"
            };

            var divideTry = 0;

            double currentNumber = number;

            while (currentNumber >= 1000)
            {
                divideTry++;
                currentNumber /= 1000;
            }

            var key = string.Empty;

            if (divideTry <= 0) return currentNumber.ToString(format, CultureInfo.CreateSpecificCulture("en-US")) + key;

            if (divideTry < simpleText.Length)
            {
                key = simpleText[divideTry];
            }
            else
            {
                if (divideTry < complexText.Length)
                {
                    key = complexText[divideTry / complexText.Length] + complexText[divideTry % complexText.Length];
                }
            }

            return currentNumber.ToString(format, CultureInfo.CreateSpecificCulture("en-US")) + key;
        }

        public static string NumberTextFormatter(double number, int cap = 1000, string format = "F2")
        {
            if (number < cap)
            {
                return number.ToString("F0", CultureInfo.CreateSpecificCulture("en-US"));
            }

            string[] shortText =
            {
                "", "K", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am",
                "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az", "ba", "bb", "bc", "bd",
                "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq"
            };

            var divideTry = 0;

            double currentNumber = number;

            while (currentNumber >= 1000)
            {
                divideTry++;
                currentNumber /= 1000;
            }

            return currentNumber.ToString(format, CultureInfo.CreateSpecificCulture("en-US")) + shortText[divideTry];
        }

        public static string FirstCharToUpper(string input)
        {
            return input.First().ToString().ToUpper() +
                   string.Join("", input.ToLower(CultureInfo.CreateSpecificCulture("en-US")).Skip(1));
        }

        public static string SplitStringByChar(string input, char c = '_')
        {
            string[] parse = input.Split(c);
            return parse.Aggregate(string.Empty, (current, item) => current + FirstCharToUpper(item) + " ");
        }

        /// <summary>
        /// Second to formatted time like (hh):mm:ss, shows hh if at least one hour
        /// </summary>
        /// <param name="time">Seconds</param>
        /// <returns>(hh):mm:ss</returns>
        public static string SecondToFormattedDate(int time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.TotalHours >= 24
                ? $"{ts.TotalHours - 1:F0}:{ts:mm\\:ss}"
                : ts.ToString(ts.TotalMinutes >= 60 ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        public static string SecondToFormattedHour(int time)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.TotalHours + "h";
        }

        /// <summary>
        /// Second to formatted time like hh:mm:ss
        /// </summary>
        /// <param name="time">Seconds</param>
        /// <param name="withHour">Show hour information</param>
        /// <returns>hh:mm:ss</returns>
        public static string SecondToFormattedDate(int time, bool withHour = false)
        {
            TimeSpan ts = TimeSpan.FromSeconds(time);
            return ts.TotalHours >= 24
                ? $"{ts.TotalHours - 1:F0}:{ts:mm\\:ss}"
                : ts.ToString(withHour ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        public static int GetTotalHour(int time)
        {
            return (int) TimeSpan.FromSeconds(time).TotalHours;
        }

        /// <summary>
        /// Find color by hex code
        /// </summary>
        /// <param name="hex">Hex code of color</param>
        /// <returns>new Color32</returns>
        public static Color HexToColor(string hex)
        {
            // in case the string is formatted 0xFFFFFF
            hex = hex.Replace("0x", "");
            // in case the string is formatted #FFFFFF
            hex = hex.Replace("#", "");
            // assume fully visible unless specified in hex
            byte a = 255;
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            // only use alpha if the string has enough characters
            if (hex.Length == 8) a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Custom number rounder
        /// </summary>
        public static float RoundNumber(float number, bool isFloor = true, int afterDot = 2)
        {
            float m = Mathf.Pow(10, afterDot);
            number *= m;
            number = isFloor ? Mathf.Floor(number) : Mathf.Ceil(number);
            return number / m;
        }

        public static IEnumerable<int> GetStringAsIEnumerable(string value, char separator)
        {
            return value.Split(separator).Select(int.Parse);
        }

        public static string GetIEnumerableAsString(IEnumerable<int> values, string separator)
        {
            return string.Join(separator, values);
        }

        /// <summary>
        /// This function will be used if needed hour minutes or seconds info
        /// </summary>
        /// <param name="timeSeconds"></param>
        /// <returns>
        /// Return 12h 30m 22s format
        /// </returns>
        public static string ConvertSecondsToFormattedTime(double timeSeconds)
        {
            int mySeconds = Convert.ToInt32(timeSeconds);
            int myHours = mySeconds / 3600; //3600 Seconds in 1 hour
            mySeconds %= 3600;
            int myMinutes = mySeconds / 60; //60 Seconds in a minute
            mySeconds %= 60;
            string mySec = mySeconds.ToString(),
                myMin = myMinutes.ToString(),
                myHou = myHours.ToString();
            var text = "";
            if (myHours > 0)
            {
                text += myHou + "h ";
            }

            if (myMinutes > 0)
            {
                text += myMin + "m ";
            }

            if (mySeconds > 0)
            {
                text += mySec + "s";
            }

            return text;
        }

        /// <summary>
        /// </summary>
        /// <param name="timeSeconds"></param>
        /// <returns>
        /// Return 12:30:00 format
        /// </returns>
        public static string ConvertToFormattedTime(double timeSeconds)
        {
            int mySeconds = Convert.ToInt32(timeSeconds);
            int myHours = mySeconds / 3600; //3600 Seconds in 1 hour
            mySeconds %= 3600;
            int myMinutes = mySeconds / 60; //60 Seconds in a minute
            mySeconds %= 60;
            string mySec = mySeconds.ToString(),
                myMin = myMinutes.ToString(),
                myHou = myHours.ToString();
            if (myHours < 10)
            {
                myHou = myHou.Insert(0, "0");
            }

            if (myMinutes < 10)
            {
                myMin = myMin.Insert(0, "0");
            }

            if (mySeconds < 10)
            {
                mySec = mySec.Insert(0, "0");
            }

            return myHou + ":" + myMin + ":" + mySec;
        }
    }
}
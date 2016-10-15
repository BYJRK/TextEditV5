using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Microsoft.International.Converters.PinYinConverter;
using System.IO;
using TextEditV5.PairExchange;

namespace TextEditV5
{
    public enum RegexMode
    {
        Replace, ReplaceAndMatch, Match
    }
    public enum IndexFormat
    {
        Normal, Chinese, Roman, Circle, Bracket
    }
    public enum TextFormatMode
    {
        Tranditional, Simplified, Upper, Lower, InitialUpper, Pinyin
    }

    /// <summary>
    /// 整合了所有可能使用到的功能函数
    /// </summary>
    class Functions
    {
        public static string NL = Environment.NewLine;
        public static string EM = string.Empty;
        public static string TA = "\t";

        private static Dictionary<string, string> SpecialSymbols = new Dictionary<string, string> { };
        public static void Initialize()
        {
            SpecialSymbols.Add(@"(?<!\\)\\n", NL);
            SpecialSymbols.Add(@"(?<!\\)\\t", TA);
            SpecialSymbols.Add(@"\\\\", @"\");
        }

        #region 主要功能函数

        /*
         * 所有的字符串操作函数，如果只有一个输出文本，则采用return string的方式
         * 如果有超过一个输出文本，则主要的输出内容采用return string
         * 其余的采用ref string
         */

        /// <summary>
        /// 使用正则表达式
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="output">匹配到的内容</param>
        /// <param name="pattern">模式</param>
        /// <param name="replacement">替换为</param>
        /// <param name="mode">该功能的几种模式</param>
        /// <param name="options">正则表达式选项，默认为无</param>
        /// <returns>修改后的文本</returns>
        public static string UseRegex(string input, ref string output, string pattern, string replacement, RegexMode mode = RegexMode.Replace, RegexOptions options = RegexOptions.None)
        {
            if (mode == RegexMode.Replace)
            {
                replacement = ReplaceSpecialWords(replacement);
                return Regex.Replace(input, pattern, replacement, options);
            }
            else if (mode == RegexMode.ReplaceAndMatch)
            {
                replacement = ReplaceSpecialWords(replacement);
                output = CombineWith(Regex.Matches(input, pattern, options), NL);
                return Regex.Replace(input, pattern, replacement, options);
            }
            else if (mode == RegexMode.Match)
            {
                output = CombineWith(Regex.Matches(input, pattern, options), NL);
                return input;
            }
            return EM;
        }
        /// <summary>
        /// 常规的替换功能
        /// </summary>
        /// <param name="input">输入内容</param>
        /// <param name="pattern">匹配内容</param>
        /// <param name="replacement">替换为</param>
        /// <param name="isCaseSensitive">是否区分大小写</param>
        /// <returns>修改后的文本</returns>
        public static string Replace(string input, string pattern, string replacement, bool isCaseSensitive = true)
        {
            pattern = ReplaceSpecialWords(pattern);
            pattern = Regex.Escape(pattern);
            // 接下来使用正则表达式时，replacement进行了特殊字符转换，此处不需要进行

            if (isCaseSensitive)
            {
                string temp = null;
                return UseRegex(input, ref temp, pattern, replacement);
            }
            else
            {
                string temp = null;
                return UseRegex(input, ref temp, pattern, replacement, options: RegexOptions.IgnoreCase);
            }
        }
        /// <summary>
        /// 关于括号的函数
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="output">展示匹配到的内容</param>
        /// <param name="left">左括号</param>
        /// <param name="right">右括号</param>
        /// <param name="keepBracket">保留括号</param>
        /// <param name="checkPair">检查成对括号</param>
        /// <returns>修改后的文本</returns>
        public static string Bracket(string input, ref string output, string left, string right, bool keepBracket = false, bool checkPair = false)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return input;
            if (left.Length > input.Length || right.Length > input.Length) return input;
            // 如果要匹配括号对，但是left或right非单个字符，则无效
            if ((left.Length > 1 || right.Length > 1) & checkPair) return input;

            left = ReplaceSpecialWords(left);
            right = ReplaceSpecialWords(right);
            string pair = left + right;
            left = Regex.Escape(left);
            right = Regex.Escape(right);

            string replacement = keepBracket ? pair : EM;
            if (!checkPair)
            {
                output = CombineWith(Regex.Matches(input, "(?<=" + left + ").*?" + "(?=" + right + ")", RegexOptions.Singleline), NL);
                return Regex.Replace(input, left + ".*?" + right, replacement);
            }
            else
            {
                Regex r = new Regex(left + "[^" + left + right + "]*" + "(" + "(" + "(?'d'" + left + "[^" + right + "]*" + right + ")" + "[^" + left + right + "]*" + ")+" + "(" + "(?'-d'" + right + ")[^" + left + right + "]*)+" + ")*" + "(?(d)(?!)" + ")" + right, RegexOptions.Singleline);
                output = CombineWith(r.Matches(input), NL);
                return r.Replace(input, replacement);
            }
        }
        /// <summary>
        /// 删除空白
        /// </summary>
        /// <param name="input">输入内容</param>
        /// <param name="mode">模式：1-全部；2-行首；3-行尾；4-行首与行尾</param>
        /// <param name="includeTab">是否包含制表符</param>
        /// <returns></returns>
        public static string Blank(string input, int mode)
        {
            List<string> list = SplitByString(input, NL);
            if (mode == 1)
            {
                Regex r = new Regex(@"\s");
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = r.Replace(list[i], EM);
                }
                return CombineWith(list, NL);
            }
            else if (mode == 2)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = list[i].TrimStart(null);
                }
                return CombineWith(list, NL);
            }
            else if (mode == 3)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = list[i].TrimEnd(null);
                }
                return CombineWith(list, NL);
            }
            else if (mode == 4)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = list[i].Trim();
                }
                return CombineWith(list, NL);
            }
            return input;
        }
        /// <summary>
        /// 换行符相关
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="mode">模式：1-删除换行符；2-合并换行符；2-添加换行符</param>
        /// <returns></returns>
        public static string Return(string input, int mode)
        {
            if (mode == 1)
            {
                return Replace(input, @"\n", EM);
            }
            else if (mode == 2)
            {
                return Replace(input, @"\n\n", @"\n");
            }
            else if (mode == 3)
            {
                return Replace(input, @"\n", @"\n\n");
            }
            return input;
        }
        /// <summary>
        /// 文本顺序调换
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="mode">模式：1-全文逐行；2-全文逐字；3-逐行逐字</param>
        /// <returns></returns>
        public static string Reorder(string input, int mode)
        {
            if (mode == 1)
            {
                List<string> list = SplitByString(input, NL);
                list.Reverse();
                return CombineWith(list, NL);
            }
            else if (mode == 2)
            {
                return StringReverse(input.Replace(NL, "\n")).Replace("\n", NL);
            }
            else if (mode == 3)
            {
                List<string> list = SplitByString(input, NL);
                list.Reverse();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = StringReverse(list[i]);
                }
                return CombineWith(list, NL);
            }

            return input;
        }
        /// <summary>
        /// 批量重复文本
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="time">重复次数</param>
        /// <param name="autoNewLine">自动添加换行符</param>
        /// <returns></returns>
        public static string Repeat(string input, string time, bool autoNewLine)
        {
            int times;
            if (!int.TryParse(time, out times)) return input;
            if (times < 1) return input;
            StringBuilder sb = new StringBuilder((input.Length + NL.Length) * (times + 1));
            for (int i = 0; i <= times; i++)
            {
                sb.Append(input);
                if (autoNewLine && i < times) sb.Append(NL);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 逐行插入相同内容
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="insert">插入文本</param>
        /// <param name="position">位置</param>
        /// <param name="ignoreEmpty">是否忽略空行</param>
        /// <param name="ignoreBlank">是否忽略空白行</param>
        /// <returns></returns>
        public static string AddTextByLine(string input, string insert, string position, bool ignoreEmpty, bool ignoreBlank)
        {
            List<string> list = SplitByString(input, NL);
            for (int i = 0; i < list.Count; i++)
            {
                if (!ignoreEmpty || ignoreBlank)
                    list[i] = InsertTextAt(list[i], insert, position);
                else if (ignoreEmpty && !string.IsNullOrEmpty(list[i]))
                    list[i] = InsertTextAt(list[i], insert, position);
                else if (ignoreBlank && !string.IsNullOrWhiteSpace(list[i]))
                    list[i] = InsertTextAt(list[i], insert, position);
            }
            return CombineWith(list, NL);
        }
        /// <summary>
        /// 逐行插入不同内容
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="insert">插入文本</param>
        /// <param name="position">位置</param>
        /// <param name="ignoreEmpty">是否忽略空行</param>
        /// <returns></returns>
        public static string AddTextsByLine(string input, string insert, string position, bool ignoreEmpty)
        {
            return AddTextsByLine(SplitByString(input, NL), SplitByString(insert, NL), position, ignoreEmpty);
        }
        public static string AddTextsByLine(List<string> input, List<string> insert, string position, bool ignoreEmpty)
        {
            int j = 0;
            int length2 = insert.Count;
            for (int i = 0; i < input.Count; i++)
            {
                if (j >= length2) break;
                if (!ignoreEmpty || !string.IsNullOrEmpty(input[i]))
                {
                    input[i] = InsertTextAt(input[i], insert[j], position);
                    j++;
                }
            }
            return CombineWith(input, NL);
        }
        /// <summary>
        /// 隔行删除
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="output">被删除的文本</param>
        /// <param name="reserve">保留行数</param>
        /// <param name="remove">删除行数</param>
        /// <returns></returns>
        public static string DeleteByLine(string input, ref string output, string reserve, string remove)
        {
            int r1, r2;
            if (!int.TryParse(reserve, out r1)) return input;
            if (!int.TryParse(remove, out r2)) return input;
            if (r1 < 1 || r2 < 1) return input;
            int rt = r1 + r2;
            List<string> list = SplitByString(input, NL);
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                if (i % rt < r1) list1.Add(list[i]);
                else list2.Add(list[i]);
            }
            output = CombineWith(list2, NL);
            return CombineWith(list1, NL);
        }
        /// <summary>
        /// 隔行插入
        /// </summary>
        /// <param name="input1">输入文本</param>
        /// <param name="input2">插入文本</param>
        /// <param name="reserve">保留行数</param>
        /// <param name="insert">插入行数</param>
        /// <returns></returns>
        public static string InsertByLine(string input1, string input2, string reserve, string insert)
        {
            int r1, r2;
            if (!int.TryParse(reserve, out r1)) return input1;
            if (!int.TryParse(insert, out r2)) return input1;
            if (r1 < 1 || r2 < 1) return input1;

            StringBuilder sb = new StringBuilder();

            List<string> list1 = SplitByString(input1, NL, true);
            List<string> list2 = SplitByString(input2, NL, true);

            int l1 = 0, l2 = 0;
            while (l1 < list1.Count || l2 < list2.Count)
            {
                int k;
                for (k = 0; k < r1; k++)
                {
                    if (l1 >= list1.Count) break;
                    sb.Append(list1[l1]);
                    l1++;
                    if (l1 < list1.Count || l2 < list2.Count)
                        sb.Append(NL);
                }
                for (k = 0; k < r2; k++)
                {
                    if (l2 >= list2.Count) break;
                    sb.Append(list2[l2]);
                    l2++;
                    if (l1 < list1.Count || l2 < list2.Count)
                        sb.Append(NL);
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 逐行添加序号
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="start">起始数</param>
        /// <param name="digits">数位</param>
        /// <param name="position">插入位置</param>
        /// <param name="mode">序号格式</param>
        /// <returns></returns>
        public static string InsertIndexAt(string input, string left, string right, int start, int digits, string position, IndexFormat mode, bool ignoreEmpty, bool alignment)
        {
            List<string> list1 = SplitByString(input, NL);
            List<string> list2 = new List<string>();
            int length = list1.Count;
            if (ignoreEmpty) length -= list1.FindAll(s => s.Length == 0).Count;

            if (mode == IndexFormat.Normal)
            {
                digits = alignment ? digits : 0;
                for (int i = 0; i < length; i++)
                {
                    int number = start + i;
                    list2.Add(left + GetNumberInDigits(number, digits) + right);
                }
            }
            else if (mode == IndexFormat.Chinese)
            {
                for (int i = 0; i < length; i++)
                {
                    list2.Add(left + GetChineseNumber(i + 1) + right);
                }
            }
            else if (mode == IndexFormat.Roman)
            {
                length = length <= 3999 ? length : 3999;
                for (int i = 0; i < length; i++)
                {
                    list2.Add(left + GetRomanNumber(i + 1) + right);
                }
            }
            else if (mode == IndexFormat.Circle)
            {
                length = length <= 10 ? length : 10;
                for (int i = 0; i < length; i++)
                {
                    list2.Add(left + GetSpecialNumber(i + 1, 1) + right);
                }
            }
            else if (mode == IndexFormat.Bracket)
            {
                length = length <= 20 ? length : 20;
                for (int i = 0; i < length; i++)
                {
                    list2.Add(left + GetSpecialNumber(i + 1, 2) + right);
                }
            }

            return AddTextsByLine(list1, list2, position, ignoreEmpty);
        }
        /// <summary>
        /// 文本格式转换
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="mode">模式</param>
        /// <returns></returns>
        public static string TextFormat(string input, TextFormatMode mode)
        {
            if (mode == TextFormatMode.Tranditional)
            {
                return Strings.StrConv(input, VbStrConv.TraditionalChinese, 0);
            }
            else if (mode == TextFormatMode.Simplified)
            {
                return Strings.StrConv(input, VbStrConv.SimplifiedChinese, 0);
            }
            else if (mode == TextFormatMode.Upper)
            {
                return Strings.StrConv(input, VbStrConv.Uppercase, 0);
            }
            else if (mode == TextFormatMode.Lower)
            {
                return Strings.StrConv(input, VbStrConv.Lowercase, 0);
            }
            else if (mode == TextFormatMode.InitialUpper)
            {
                return Strings.StrConv(input, VbStrConv.ProperCase, 0);
            }
            else if (mode == TextFormatMode.Pinyin)
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in input)
                {
                    if (Regex.IsMatch(c.ToString(), @"[\u4e00-\u9fbb]"))
                    {
                        ChineseChar ch = new ChineseChar(c);
                        // 虽然有多音字的情况，但是并没有判断的能力，故默认选取第一个
                        string s = ch.Pinyins[0];
                        sb.Append(s.ToLower().Substring(0, s.Length - 1));
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }
            return input;
        }
        /// <summary>
        /// 自定义转换列表
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="manager">转换列表</param>
        /// <returns></returns>
        public static string TransformList(string input, PairListManager manager, bool reverse = false)
        {
            string temp = input,
                   output = EM;
            if (manager.UsingRex)
            {
                foreach (var line in manager.GetTransformList())
                {
                    if (!reverse)
                        temp = UseRegex(temp, ref output, line.Left, line.Right, RegexMode.Replace, manager.RexOptions);
                    else
                        temp = UseRegex(temp, ref output, line.Right, line.Left, RegexMode.Replace, manager.RexOptions);
                }
            }
            else
            {
                foreach (var line in manager.GetTransformList())
                {
                    if (!reverse)
                        temp = Replace(temp, line.Left, line.Right);
                    else
                        temp = Replace(temp, line.Right, line.Left);
                }
            }
            return temp;
        }

        #endregion

        #region 辅助功能函数

        /// <summary>
        /// 使用分隔符将列表中全部字符串进行连接，不在最后产生额外字符
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">列表</param>
        /// <param name="separator">分隔符</param>
        /// <returns>组合后的字符串</returns>
        public static string CombineWith<T>(IEnumerable<T> list, string separator)
        {
            return string.Join(separator, from line in list select line.ToString());
        }
        public static string CombineWith(MatchCollection list, string separator)
        {
            StringBuilder sb = new StringBuilder();
            int count = list.Count;
            if (count == 0) return EM;
            for (int i = 0; i < count - 1; i++)
            {
                sb.Append(list[i].ToString() + separator);
            }
            sb.Append(list[count - 1]);
            return sb.ToString();
        }
        /// <summary>
        /// 使用分隔符将列表中全部字符串进行连接
        /// </summary>
        /// <param name="list">列表</param>
        /// <param name="separator">分隔符</param>
        /// <returns>返回组合后的字符串</returns>
        /// <summary>
        /// 根据程序中定义的转义字符对传入文本进行替换
        /// </summary>
        /// <param name="input">传入文本</param>
        /// <returns>输出替换后的内容</returns>
        static string ReplaceSpecialWords(string input)
        {
            foreach (var pair in SpecialSymbols)
            {
                input = Regex.Replace(input, pair.Key, pair.Value);
            }
            return input;
        }
        /// <summary>
        /// 使用特定的分隔符将字符串分割为字符串列表
        /// </summary>
        /// <param name="input">输入内容</param>
        /// <param name="separator">分隔符</param>
        /// <param name="keepEmpty">是否保留空内容</param>
        /// <returns></returns>
        public static List<string> SplitByString(string input, string separator, bool keepEmpty = true)
        {
            List<string> list = new List<string>(Regex.Split(input, separator));
            if (!keepEmpty)
                return (from line in list where !string.IsNullOrEmpty(line) select line).ToList();
            else
                return list;
        }
        /// <summary>
        /// 在一行的指定位置插入文字
        /// </summary>
        /// <param name="input">原文本</param>
        /// <param name="content">插入文本</param>
        /// <param name="position">位置</param>
        /// <returns></returns>
        public static string InsertTextAt(string input, string content, string position)
        {
            int p;
            int l = input.Length;
            if (!int.TryParse(position, out p)) return input;
            p = position[0] != '-' ? p : l + p;
            if (p > l) p = l;
            if (p < 0) p = 0;
            return input.Insert(p, content);
        }
        /// <summary>
        /// 将字符串文字前后翻转
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <returns></returns>
        static string StringReverse(string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }
        /// <summary>
        /// 用于在单行中的某个位置插入文字
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="content">插入文本</param>
        /// <param name="position">插入位置</param>
        /// <returns></returns>
        string InsertAt(string input, string content, string position)
        {
            int i;
            if (!int.TryParse(position, out i)) return input;
            if (i < 0) i = input.Length + i;
            if (i > input.Length) i = input.Length;
            if (i < 0) i = 0;

            return input.Insert(i, content);
        }
        /// <summary>
        /// 将数字转为指定位数的数字
        /// 如果所需位数大于给定位数，则按照所需位数为准
        /// </summary>
        /// <param name="n">数字</param>
        /// <param name="digits">位数（默认为0，表示不对齐）</param>
        /// <returns></returns>
        static string GetNumberInDigits(int n, int digits = 0)
        {
            string result = n.ToString();
            if (digits - result.Length > 0)
            {
                result = result.PadLeft(digits, '0');
            }
            return result;
        }
        /// <summary>
        /// 将数字转为罗马数字
        /// 范围是1-3999
        /// 代码摘自：http://bbs.csdn.net/topics/290069921
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string GetRomanNumber(int n)
        {
            int[] arabic = new int[13];
            string[] roman = new string[13];
            int i = 0;
            string o = "";

            arabic[0] = 1000;
            arabic[1] = 900;
            arabic[2] = 500;
            arabic[3] = 400;
            arabic[4] = 100;
            arabic[5] = 90;
            arabic[6] = 50;
            arabic[7] = 40;
            arabic[8] = 10;
            arabic[9] = 9;
            arabic[10] = 5;
            arabic[11] = 4;
            arabic[12] = 1;

            roman[0] = "M";
            roman[1] = "CM";
            roman[2] = "D";
            roman[3] = "CD";
            roman[4] = "C";
            roman[5] = "XC";
            roman[6] = "L";
            roman[7] = "XL";
            roman[8] = "X";
            roman[9] = "IX";
            roman[10] = "V";
            roman[11] = "IV";
            roman[12] = "I";

            while (n > 0)
            {
                while (n >= arabic[i])
                {
                    n = n - arabic[i];
                    o = o + roman[i];
                }
                i++;
            }
            return o;
        }
        /// <summary>
        /// 把一个整数转为大写数字
        /// </summary>
        /// <param name="value">阿拉伯数字，可以为负，最大999,999,999,999</param>
        /// <returns></returns>
        static string GetChineseNumber(int value)
        {
            bool negative = value < 0;
            value = Math.Abs(value);

            string temp = "";
            int part1 = value / 100000000;
            int part2 = (value - part1 * 100000000) / 10000;
            int part3 = value % 10000;
            if (part1 > 0) temp += KiloToChinese(part1) + "亿";
            if (part2 < 1000 && part1 > 0) temp += "零";
            if (part2 > 0) temp += KiloToChinese(part2) + "万";
            else if (part2 == 0 && part1 * part3 > 0) temp += "零";
            if (part3 < 1000 && part1 * part2 > 0) temp += "零";
            if (part3 > 0) temp += KiloToChinese(part3);

            if (negative) temp = "负" + temp;
            return temp;
        }
        /// <summary>
        /// 把四位数转为大写数字
        /// </summary>
        /// <param name="value">阿拉伯数字，最大9,999</param>
        /// <returns></returns>
        static string KiloToChinese(int value)
        {
            if (value < 0) return "";
            if (value > 9999) return "";
            string Cn = "零一二三四五六七八九";
            string temp = "";
            int n1 = value / 1000;
            int n2 = (value - n1 * 1000) / 100;
            int n3 = (value - n1 * 1000 - n2 * 100) / 10;
            int n4 = value % 10;
            if (n1 > 0) temp += Cn[n1] + "千";
            if (n2 > 0) temp += Cn[n2] + "百";
            else if (n2 == 0 && n1 > 0 && n3 + n4 > 0) temp += Cn[0];
            if (n3 > 1) temp += Cn[n3] + "十";
            else if (n3 == 1) temp += "十";
            else if (n1 * n2 * n4 > 0 && n3 == 0) temp += Cn[0];
            if (n4 > 0) temp += Cn[n4];
            return temp;
        }
        /// <summary>
        /// 获取特殊格式的数字
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="mode">类型：1-①；2-⑴</param>
        /// <returns></returns>
        static string GetSpecialNumber(int number, int mode)
        {
            if (mode == 1)
            {
                if (number > 0 && number <= 10)
                {
                    return "①②③④⑤⑥⑦⑧⑨⑩".Substring(number - 1, 1);
                }
            }
            else if (mode == 2)
            {
                if (number > 0 && number <= 20)
                {
                    return "⑴⑵⑶⑷⑸⑹⑺⑻⑼⑽⑾⑿⒀⒁⒂⒃⒄⒅⒆⒇".Substring(number - 1, 1);
                }
            }
            return EM;
        }
        /// <summary>
        /// 用来获取当前正则表达式的配置
        /// </summary>
        /// <param name="IgnoreCase">忽略大小写</param>
        /// <param name="Multiline">多行模式</param>
        /// <param name="Singleline">单行模式</param>
        /// <param name="IgnorePatternWhitespace">忽略空白</param>
        /// <param name="ExplicitCapture">显式捕获</param>
        /// <returns></returns>
        public static RegexOptions GetRexOptions(bool IgnoreCase, bool Multiline, bool Singleline, bool IgnorePatternWhitespace, bool ExplicitCapture)
        {
            RegexOptions option = new RegexOptions();
            if (IgnoreCase) option |= RegexOptions.IgnoreCase;
            if (Multiline) option |= RegexOptions.Multiline;
            if (Singleline) option |= RegexOptions.Singleline;
            if (IgnorePatternWhitespace) option |= RegexOptions.IgnorePatternWhitespace;
            if (ExplicitCapture) option |= RegexOptions.ExplicitCapture;
            return option;
        }
        /// <summary>
        /// 判断读入文本的编码格式为Unicode或ANSI
        /// 来自：http://www.jb51.net/article/36745.htm
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <returns></returns>
        public static Encoding GetEncoding(string FileName)
        {
            if (FileName == null)
            {
                throw new ArgumentNullException("文件路径无效");
            }
            Encoding encoding1 = Encoding.Default;
            if (File.Exists(FileName))
            {
                try
                {
                    using (FileStream stream1 = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    {
                        if (stream1.Length > 0)
                        {
                            using (StreamReader reader1 = new StreamReader(stream1, true))
                            {
                                char[] chArray1 = new char[1];
                                reader1.Read(chArray1, 0, 1);
                                encoding1 = reader1.CurrentEncoding;
                                reader1.BaseStream.Position = 0;
                                if (encoding1 == Encoding.UTF8)
                                {
                                    byte[] buffer1 = encoding1.GetPreamble();
                                    if (stream1.Length >= buffer1.Length)
                                    {
                                        byte[] buffer2 = new byte[buffer1.Length];
                                        stream1.Read(buffer2, 0, buffer2.Length);
                                        for (int num1 = 0; num1 < buffer2.Length; num1++)
                                        {
                                            if (buffer2[num1] != buffer1[num1])
                                            {
                                                encoding1 = Encoding.Default;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        encoding1 = Encoding.Default;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("尝试解析文本文件编码格式失败。原因：\n" + e.Message);
                }
                if (encoding1 == null)
                {
                    encoding1 = Encoding.UTF8;
                }
            }
            return encoding1;
        }

        #endregion
    }
}
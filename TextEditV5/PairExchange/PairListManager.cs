using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace TextEditV5.PairExchange
{
    public class PairListManager
    {
        List<PairElement> list = new List<PairElement>();
        public List<PairElement> List { get { return list; } }
        public PairElement this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }
        public string GetLineInfo(int index)
        {
            string left = list[index].Left,
                  right = list[index].Right;
            if (left.Length > 0)
                return left + Separator + right;
            else
                return Comment + right;
        }
        public IEnumerable<PairElement> GetTransformList()
        {
            foreach (var line in list)
            {
                if (line.Left.Length > 0)
                    yield return line;
            }
        }

        public string Name { get; set; }
        public PairListManager()
        {
            UedoLimit = 20;
        }
        public PairListManager(string filePath) : this()
        {
            LoadFromFile(filePath);
        }

        public void Add(string left, string right)
        {
            Add(new PairElement(left, right));
        }
        public void Add(PairElement p)
        {
            list.Add(p);
        }
        public void Insert(string left, string right, int index)
        {
            Insert(new PairElement(left, right), index);
        }
        public void Insert(PairElement p, int index)
        {
            list.Insert(index, p);
        }
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }
        public void AmendAt(string left, string right, int index)
        {
            AmendAt(new PairElement(left, right), index);
        }
        public void AmendAt(PairElement p, int index)
        {
            list[index] = p;
        }
        public void Switch(int index1, int index2)
        {
            if (index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count) return;
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
        public void Clear()
        {
            list.Clear();
        }
        public void Sort()
        {
            list.Sort();
        }

        public int UedoLimit { get; set; }
        List<List<PairElement>> HistoryList = new List<List<PairElement>>();
        private int Index = 0;
        public void AddHistory()
        {
            if (Index >= UedoLimit - 1)
            {
                HistoryList.RemoveAt(0);
            }
            else if (Index < HistoryList.Count - 1)
            {
                HistoryList.RemoveRange(Index + 1, HistoryList.Count - Index - 1);
                Index++;
            }
            HistoryList.Add(list);
        }
        public void Undo()
        {
            if (Index > 0)
            {
                Index--;
                list = HistoryList[Index];
            }
        }
        public void Redo()
        {
            if (Index < HistoryList.Count - 1)
            {
                Index++;
                list = HistoryList[Index];
            }
        }

        public Encoding CurrentEncoding = Encoding.Unicode;
        public void LoadFromText(string text)
        {
            // 首先清空列表
            list.Clear();
            // 将文本转为多行内容，忽略空白行
            List<string> stringList = new List<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            // 逐行分析
            foreach (var line in stringList)
            {
                // 如果为空行，则忽略
                if (string.IsNullOrWhiteSpace(line)) continue;
                // 如果为属性“[]”，则调用相关函数进行分析
                else if (line.StartsWith("[") && line.EndsWith("]")) ReadSetting(line.Substring(1, line.Length - 2));
                // 如果包含分隔符，则认为是信息行
                else if (line.Contains(Separator)) ReadPair(line);
                // 如果以CommentSymbol开头，则认为是注释
                else if (line.StartsWith(Comment)) ReadComment(line);
                // 否则则认为是无效信息，忽略
                //else { }
            }
        }
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;
            CurrentEncoding = Functions.GetEncoding(filePath);
            using (StreamReader sr = new StreamReader(filePath, CurrentEncoding))
            {
                LoadFromText(sr.ReadToEnd());
            }
        }
        public string StringContents
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(WriteSetting("Separator", Separator));
                sb.AppendLine(WriteSetting("Comment", Comment));
                sb.AppendLine(WriteSetting("UsingRex", UsingRex));
                if (UsingRex)
                {
                    if (IgnoreCase) sb.AppendLine(WriteSetting("IgnoreCase", IgnoreCase));
                    if (Multiline) sb.AppendLine(WriteSetting("Multiline", Multiline));
                    if (Singleline) sb.AppendLine(WriteSetting("Singleline", Singleline));
                    if (IgnorePatternWhitespace) sb.AppendLine(WriteSetting("IgnorePatternWhitespace", IgnorePatternWhitespace));
                    if (ExplicitCapture) sb.AppendLine(WriteSetting("ExplicitCapture", ExplicitCapture));
                }
                if (list.Count > 0)
                {
                    var temp = from line in list select WritePairOrComment(line);
                    sb.Append(string.Join(Environment.NewLine, temp));
                }
                return sb.ToString();
            }
        }
        void ReadSetting(string content)
        {
            // 传递进来的content已经去除左右方括号
            string name, value;
            if (FindLeftRight(content, out name, out value, "="))
            {
                switch (name)
                {
                    case "Separator": Separator = value; break;
                    case "Comment": Comment = value; break;
                    case "UsingRex": UsingRex = Convert.ToBoolean(value); break;
                    // 以下几项，如果没有使用正则表达式，则忽略
                    case "IgnoreCase": if (UsingRex) IgnoreCase = Convert.ToBoolean(value); break;
                    case "Multiline": if (UsingRex) Multiline = Convert.ToBoolean(value); break;
                    case "Singleline": if (UsingRex) Singleline = Convert.ToBoolean(value); break;
                    case "IgnorePatternWhitespace": if (UsingRex) IgnorePatternWhitespace = Convert.ToBoolean(value); break;
                    case "ExplicitCapture": if (UsingRex) ExplicitCapture = Convert.ToBoolean(value); break;
                }
            }
        }
        string WriteSetting(string property, object value)
        {
            return "[" + property + "=" + value.ToString() + "]";
        }
        void ReadPair(string content)
        {
            string left, right;
            if (FindLeftRight(content, out left, out right, Separator))
            {
                // 如果left没有内容，则会导致将来在转换时出现错误，直接退出
                // 事实上，left为空的pair是用来表示注释的
                if (left.Length == 0) return;
                Add(left, right);
            }
        }
        void ReadComment(string content)
        {
            Add(string.Empty, content.Substring(Comment.Length));
        }
        string WritePairOrComment(PairElement p)
        {
            if (p.Left.Length > 0)
                return p.Left + Separator + p.Right;
            else
                return Comment + p.Right;
        }
        bool FindLeftRight(string line, out string left, out string right, string separator)
        {
            string[] pair = line.Split(new string[] { separator }, StringSplitOptions.None);
            if (pair.Length != 2)
            {
                left = right = string.Empty;
                return false;
            }
            left = pair[0];
            right = pair[1];
            return true;
        }

        // 常规属性
        public string Separator { get; set; }
        public string Comment { get; set; }
        // 正则表达式属性
        public bool UsingRex { get; set; }
        public bool IgnoreCase { get; set; }
        public bool Multiline { get; set; }
        public bool Singleline { get; set; }
        public bool IgnorePatternWhitespace { get; set; }
        public bool ExplicitCapture { get; set; }
        public RegexOptions RexOptions
        {
            get
            {
                RegexOptions option = RegexOptions.None;
                if (IgnoreCase) option |= RegexOptions.IgnoreCase;
                if (Multiline) option |= RegexOptions.Multiline;
                if (Singleline) option |= RegexOptions.Singleline;
                if (IgnorePatternWhitespace) option |= RegexOptions.IgnorePatternWhitespace;
                if (ExplicitCapture) option |= RegexOptions.ExplicitCapture;
                return option;
            }
        }
    }
}
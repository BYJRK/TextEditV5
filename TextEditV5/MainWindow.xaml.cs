using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using TextEditV5.KeyboardHookMethods;
using TextEditV5.UserControls;
using TextEditV5.PairExchange;
using System.Windows.Input;
using F = TextEditV5.Functions;
using Form = System.Windows.Forms;

namespace TextEditV5
{
    public enum RightContent
    {
        None, Editor, Option, PairList
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RightContentControl.Content = EditorPanel;
            OptionPanel.mainForm = this;
            EditorPanel.mainForm = this;
            PairListPanel.mainForm = this;

            InitializeElements();
        }
        private void InitializeElements()
        {
            // 读取配置文件
            if (!File.Exists(configFilePath))
            {
                MessageBox.Show("没有找到配置文件。自动生成新的默认配置文件。");
                CreateDefaultConfig(configFilePath);
            }
            ReadConfig(configFilePath);
            ApplyConfig();
            // 设定窗口大小
            Height = SystemParameters.PrimaryScreenHeight * 0.8;
            Width = SystemParameters.PrimaryScreenWidth * 0.7;
            // 初始化Functions
            F.Initialize();
            // 文本框的默认文本显示
            T1 = F.EM;
            T2 = F.EM;
            T3 = F.EM;
            T4 = F.EM;
            // 读取缓存文本
            LoadTemp();

            // 功能：自定义转换列表
            LoadTransformListFiles();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            // 在退出时，保存所有的缓存，并更新配置文件
            SaveTemp();
            SaveConfig(configFilePath);
        }

        #region 软件参数及其相关功能

        string configFilePath = "config.xml";// 配置文件名称
        public string version;
        public string color;
        public bool alwaysShowingBox2;
        public bool autoReset;
        public bool detailedCountInfo;
        public bool autoCache;
        public bool cutButton;
        public bool showRunTime;
        public bool autoCheckUpdate;
        public int maxHistory;
        public string pairFolder = "pair folder";// 自定义转换列表的存放文件夹名称
        // 当前处理的文本文件的名称及编码格式（用于保存时使用）
        string FileName;
        Encoding FileEncoding;

        string T1
        {
            get { return EditorPanel.Box1; }
            set { EditorPanel.Box1 = value; }
        }
        string T2
        {
            get { return EditorPanel.Box2; }
            set { EditorPanel.Box2 = value; }
        }
        string T3
        {
            get { return EditorPanel.Box3; }
            set { EditorPanel.Box3 = value; }
        }
        string T4
        {
            get { return EditorPanel.Box4; }
            set { EditorPanel.Box4 = value; }
        }

        public RightContent CurrentRightContent { get; set; }
        public EditorControl EditorPanel = new EditorControl();
        public OptionControl OptionPanel = new OptionControl();
        public PairListControl PairListPanel = new PairListControl();
        public void ShowEditorControl(object sender, RoutedEventArgs e)
        {
            ShowRightContent(RightContent.Editor);
        }
        public void ShowOptionControl(object sender, RoutedEventArgs e)
        {
            ShowRightContent(RightContent.Option);
        }
        public void ShowPairListControl(object sender, RoutedEventArgs e)
        {
            ShowRightContent(RightContent.PairList);
        }
        private void UpdatePairListControl(object sender, RoutedEventArgs e)
        {
            LoadTransformListFiles();
        }
        public void ShowRightContent(RightContent content, bool foldLeft = true)
        {
            if (content == RightContent.None)
            {
                RightContentControl = null;
                ShowLeftMenu(true);
            }
            else if (content == RightContent.Editor)
            {
                RightContentControl.Content = EditorPanel;
            }
            else if (content == RightContent.Option)
            {
                OptionPanel.UpdatePanel();
                RightContentControl.Content = OptionPanel;
                OptionButtonPopup.IsOpen = false;
                if (foldLeft) ShowLeftMenu(false);
            }
            else if (content == RightContent.PairList)
            {
                if (TransformListFiles.SelectedItem == null) return;
                // 在此处检查文件是否存在
                string path = @"pair folder\" + TransformListFiles.SelectedItem.ToString() + ".txt";
                if (!File.Exists(path))
                {
                    MessageBox.Show("找不到文件：" + path);
                    return;
                }
                RightContentControl.Content = PairListPanel;
                PairListPanel.LoadFile(path);
                if (foldLeft) ShowLeftMenu(false);
            }
            CurrentRightContent = content;
        }

        #endregion

        #region 上方的全部按钮功能

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((Popup)(FindName(((Button)sender).Name + "Popup"))).IsOpen = true;
        }
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            // 如果不写这一句，则Popup不会消失
            OptionButtonPopup.IsOpen = false;

            Form.OpenFileDialog open = new Form.OpenFileDialog();
            if (!RenameFiles.IsChecked.Value)
            {
                open.Filter = "文本文件|*.txt";
                if (open.ShowDialog() == Form.DialogResult.OK)
                {
                    FileName = open.FileName;
                    FileEncoding = F.GetEncoding(FileName);
                    using (StreamReader sr = new StreamReader(FileName, FileEncoding))
                    {
                        T1 = sr.ReadToEnd();
                    }
                }
            }
            else
            {
                open.Multiselect = true;
                if (open.ShowDialog() == Form.DialogResult.OK)
                {
                    ImportRenameFileList(open.FileNames);
                }
            }
        }
        private void OpenDictionary_Click(object sender, RoutedEventArgs e)
        {
            OptionButtonPopup.IsOpen = false;
            System.Diagnostics.Process.Start(Environment.CurrentDirectory);
        }
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            OptionButtonPopup.IsOpen = false;

            if (FileName == null || FileEncoding == null)
            {
                // 说明并没有打开过文件，或之前的打开过程中出现问题，故使用另存为
                SaveAsFile();
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(FileName, FileMode.Create), FileEncoding))
                {
                    sw.Write(GetSaveText());
                }
            }
        }
        private void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            OptionButtonPopup.IsOpen = false;
            SaveAsFile();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void CountWord_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(EditorPanel.GetTextLengthInfo(detailedCountInfo), "字数统计");
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;

            string TT1 = T1;// T1 Temp
            string TT2 = T2;// T2 Temp

            // 内容查找与替换
            if (ReplaceText.IsChecked.Value)
            {
                TT1 = F.Replace(TT1, rtFindText.Text, rtReplacement.Text, !rtIgnoreCase.IsChecked.Value);
            }
            // 删除括号内容
            else if (Bracket.IsChecked.Value)
            {
                TT1 = F.Bracket(TT1, ref TT2, brLeft.Text, brRight.Text, brKeepBracket.IsChecked.Value, brNestBracket.IsChecked.Value);
                if (!brShowBracketContent.IsChecked.Value)
                {
                    TT2 = F.EM;
                }
            }
            // 删除空白
            else if (DeleteBlank.IsChecked.Value)
            {
                int mode = 0;
                if (dbRemoveAll.IsChecked.Value) mode = 1;
                else if (dbRemoveFront.IsChecked.Value) mode = 2;
                else if (dbRemoveEnd.IsChecked.Value) mode = 3;
                else if (dbRemoveFrontAndEnd.IsChecked.Value) mode = 4;
                TT1 = F.Blank(TT1, mode);
            }
            // 换行符相关
            else if (NewlineSymbol.IsChecked.Value)
            {
                int mode = 0;
                if (nsRemoveAll.IsChecked.Value) mode = 1;
                else if (nsRemoveUseless.IsChecked.Value) mode = 2;
                else if (nsAddNewlines.IsChecked.Value) mode = 3;
                TT1 = F.Return(TT1, mode);
            }
            // 文本顺序调换
            else if (TextOrder.IsChecked.Value)
            {
                int mode = 0;
                if (toByLine.IsChecked.Value) mode = 1;
                else if (toByWord.IsChecked.Value) mode = 2;
                else if (toByWordInLine.IsChecked.Value) mode = 3;
                TT1 = F.Reorder(TT1, mode);
            }
            // 批量重复文本
            else if (RepeatText.IsChecked.Value)
            {
                TT1 = F.Repeat(TT1, rptTime.Text, rptAutoNewLine.IsChecked.Value);
            }
            // 逐行插入相同内容
            else if (AddByLine.IsChecked.Value)
            {
                TT1 = F.AddTextByLine(TT1, ablInsertContent.Text, ablPosition.Text, ablIgnoreEmpty.IsChecked.Value, ablIgnoreBlank.IsChecked.Value);
            }
            // 逐行插入不同内容
            else if (SpecialInsert.IsChecked.Value)
            {
                TT1 = F.AddTextsByLine(TT1, TT2, siPosition.Text, siIngoreEmpty.IsChecked.Value);
            }
            // 隔行删除
            else if (DeleteByLine.IsChecked.Value)
            {
                TT1 = F.DeleteByLine(TT1, ref TT2, dblReserve.Text, dblRemove.Text);
            }
            // 隔行插入
            else if (InsertByLine.IsChecked.Value)
            {
                TT1 = F.InsertByLine(TT1, TT2, iblReserve.Text, iblInsert.Text);
            }
            // 逐行添加序号
            else if (AddIndexByLine.IsChecked.Value)
            {
                IndexFormat mode = IndexFormat.Normal;
                if (aiblNormal.IsChecked.Value) mode = IndexFormat.Normal;
                else if (aiblChinese.IsChecked.Value) mode = IndexFormat.Chinese;
                else if (aiblRoman.IsChecked.Value) mode = IndexFormat.Roman;
                else if (aiblCircle.IsChecked.Value) mode = IndexFormat.Circle;
                else if (aiblBracket.IsChecked.Value) mode = IndexFormat.Bracket;

                int start = 1;
                int digits = 0;
                if (int.TryParse(aiblStartValue.Text, out start) && int.TryParse(aiblDigits.Text, out digits))
                    TT1 = F.InsertIndexAt(TT1, aiblLeft.Text, aiblRight.Text, start, digits, aiblPosition.Text, mode, aiblIgnoreEmpty.IsChecked.Value, aiblAlignNumber.IsChecked.Value);
            }
            // 文本格式转换
            else if (FormatText.IsChecked.Value)
            {
                TextFormatMode mode = TextFormatMode.Tranditional;
                if (ftST.IsChecked.Value) mode = TextFormatMode.Tranditional;
                else if (ftTS.IsChecked.Value) mode = TextFormatMode.Simplified;
                else if (ftLU.IsChecked.Value) mode = TextFormatMode.Upper;
                else if (ftUL.IsChecked.Value) mode = TextFormatMode.Lower;
                else if (ftFU.IsChecked.Value) mode = TextFormatMode.InitialUpper;
                else if (ftCP.IsChecked.Value) mode = TextFormatMode.Pinyin;
                TT1 = F.TextFormat(TT1, mode);
            }
            // 正则表达式
            else if (RegularExpression.IsChecked.Value)
            {
                RegexOptions option = F.GetRexOptions(reIgnoreCase.IsChecked.Value, reMultiline.IsChecked.Value, reSingleline.IsChecked.Value,
                    reIgnorePatternWhitespace.IsChecked.Value, reExplicitCapture.IsChecked.Value);
                RegexMode mode = RegexMode.Match;
                if (reReplaceOnly.IsChecked.Value) mode = RegexMode.Replace;
                else if (reReplaceAndShow.IsChecked.Value) mode = RegexMode.ReplaceAndMatch;
                else if (reShowOnly.IsChecked.Value) mode = RegexMode.Match;

                TT1 = F.UseRegex(TT1, ref TT2, reFind.Text, reReplace.Text, mode, option);
            }
            // 自定义转换列表
            else if (TransformList.IsChecked.Value)
            {
                PairListManager manager;
                if (TransformListFiles.Text == PairListPanel.CurrentListName.Text)
                    manager = PairListPanel.Manager;
                else
                {
                    manager = new PairListManager(pairFolder + @"\" + TransformListFiles.Text + ".txt");
                }
                TT1 = F.TransformList(TT1, manager, tlR.IsChecked.Value);
            }
            // 文件批量重命名
            else if (RenameFiles.IsChecked.Value)
            {
                ApplyRename();
                return;
            }
            // 剪贴板辅助工具
            else if (ClipboardHelper.IsChecked.Value)
            {
                if (isUsingPasteHelper) return;
                PasteLines = F.SplitByString(TT1, F.NL, !chIgnoreBlank.IsChecked.Value);
                if (PasteLines.Count == 0)
                {
                    MessageBox.Show("当前无可复制文本，剪贴板辅助工具开启失败。");
                    return;
                }
                MessageBox.Show("剪贴板辅助工具已启用，\n共采集到文本信息 " + PasteLines.Count.ToString() + " 行。");
                isUsingPasteHelper = true;
                PasteLineIndex = 0;
                Clipboard.SetText(PasteLines[0]);

                // 改变相关控件的IsEnabled，防止用户在使用期间更改相关参数，导致程序出错
                chStopButton.Visibility = Visibility.Visible;
                chIgnoreBlank.IsEnabled = false;
                chCycle.IsEnabled = false;
                chAutoPaste.IsEnabled = false;
                chAutoKeyDown.IsEnabled = false;
                chDelay.IsEnabled = false;

                if (chAutoPaste.IsChecked.Value)
                {
                    hotkey = new HotKey(this, HotKey.KeyFlags.MOD_CONTROL, Form.Keys.V);
                    hotkey.OnHotKey += HotKeyEvent;
                }
                else
                {
                    kh = new KeyboardHook();
                    kh.SetHook();
                    kh.OnKeyDownEvent += HookOnKeyDownEvent;
                }
            }

            // 最终执行效果
            T1 = TT1;
            if (EditorPanel.IsShowingBox2 || EditorPanel.AlwaysShowingBox2) // 当Box2不显示时，使其缓存的文本内容不改变
                T2 = TT2;

            DateTime stopTime = DateTime.Now;
            if (showRunTime)
            {
                TimeSpan span = stopTime - startTime;
                MessageBox.Show("执行耗时" + span.TotalMilliseconds.ToString() + "毫秒");
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            EditorPanel.ClearShownTextBoxs();
            // 另外，还要清空重命名文件列表
            ClearRenameFileList();
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetSaveText());
            if (CutCopyButtonText.Text == "剪切")
            {
                T1 = T2 = F.EM;
            }
        }
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            EditorPanel.TextBox1.Undo();
        }
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            EditorPanel.TextBox1.Redo();
        }

        // 在文本框内容变化时调用，更新字数统计
        public void ShowWordCount(string info)
        {
            CountWordButton.Content = "字数统计：" + info;
        }
        // 另存为文件
        private void SaveAsFile()
        {
            Form.SaveFileDialog save = new Form.SaveFileDialog();
            save.Filter = "文本文件|*.txt";
            Form.DialogResult result = save.ShowDialog();
            if (result == Form.DialogResult.OK)
            {
                FileName = save.FileName;
                // 默认使用Unicode
                FileEncoding = FileEncoding ?? Encoding.Unicode;
                using (StreamWriter sw = new StreamWriter(new FileStream(FileName, FileMode.Create), FileEncoding))
                {
                    sw.Write(GetSaveText());
                }
            }
        }
        // 获取用于保存或复制的文本
        private string GetSaveText()
        {
            StringBuilder sb = new StringBuilder(T1, T1.Length + (EditorPanel.IsShowingBox2 ? T2.Length + 10 : 0));
            if (EditorPanel.IsShowingBox2)
            {
                sb.Append(F.NL + F.NL);
                sb.Append(T2);
            }
            return sb.ToString();
        }

        #endregion

        #region 程序配置方法

        // 存取缓存文本
        void SaveTemp()
        {
            if (!autoCache) return;
            if (T1.Length > 0)
            {
                using (StreamWriter sw = new StreamWriter("temp1.txt", false, System.Text.Encoding.Unicode))
                {
                    sw.Write(T1);
                    sw.Close();
                }
            }
            else if (File.Exists("temp1.txt"))
            {
                File.Delete("temp1.txt");
            }
            if (T2.Length > 0)
            {
                using (StreamWriter sw = new StreamWriter("temp2.txt", false, System.Text.Encoding.Unicode))
                {
                    sw.Write(T2);
                    sw.Close();
                }
            }
            else if (File.Exists("temp2.txt"))
            {
                File.Delete("temp2.txt");
            }
        }
        void LoadTemp()
        {
            if (!autoCache) return;
            if (File.Exists("temp1.txt"))
            {
                using (StreamReader sr = new StreamReader("temp1.txt"))
                {
                    T1 = sr.ReadToEnd();
                    sr.Close();
                }
            }
            if (File.Exists("temp2.txt"))
            {
                using (StreamReader sr = new StreamReader("temp2.txt"))
                {
                    T2 = sr.ReadToEnd();
                    sr.Close();
                }
            }
        }
        // 更改软件主题
        public void ChangeTheme(string Style)
        {
            if (Style == "DarkTheme")
            {
                Resources.Remove("bColor");
                Resources.Add("bColor", new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)));
                Resources.Remove("fColor");
                Resources.Add("fColor", new SolidColorBrush(Colors.White));
            }
            else if (Style == "LightTheme")
            {
                Resources.Remove("fColor");
                Resources.Add("fColor", new SolidColorBrush(Colors.Black));
                Resources.Remove("bColor");
                Resources.Add("bColor", new SolidColorBrush(Colors.White));
            }
            color = Style;
        }
        // 读取自定义转换列表
        public void LoadTransformListFiles()
        {
            CheckFolderExist();
            TransformListFiles.Items.Clear();
            string[] filelist = Directory.GetFiles(pairFolder + @"\");
            foreach (string fileName in filelist)
            {
                TransformListFiles.Items.Add(Path.GetFileNameWithoutExtension(fileName));
            }
            if (TransformListFiles.HasItems)
            {
                //TransformListFiles.Text = TransformListFiles.Items[0].ToString();
                TransformListFiles.SelectedIndex = 0;
            }
        }
        void CheckFolderExist()
        {
            if (!Directory.Exists(pairFolder))
            {
                MessageBox.Show(pairFolder + "文件夹不存在，自动创建新的文件夹。");
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\" + pairFolder);
            }
        }

        // 创建新的默认软件配置文件
        void CreateDefaultConfig(string filePath)
        {
            Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("TextEditV5.config.xml");
            XDocument doc = XDocument.Load(sm);
            doc.Save(filePath);
        }
        // 读取配置文件
        void ReadConfig(string filePath)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);

                XElement property = doc.Root;
                version = property.Element("version").Value;
                color = property.Element("color").Value;

                XElement setting = property.Element("setting");
                alwaysShowingBox2 = Convert.ToBoolean(setting.Element("alwaysShowingBox2").Attribute("value").Value);
                autoReset = Convert.ToBoolean(setting.Element("autoReset").Attribute("value").Value);
                detailedCountInfo = Convert.ToBoolean(setting.Element("detailedCountInfo").Attribute("value").Value);
                autoCache = Convert.ToBoolean(setting.Element("autoCache").Attribute("value").Value);
                cutButton = Convert.ToBoolean(setting.Element("cutButton").Attribute("value").Value);
                showRunTime = Convert.ToBoolean(setting.Element("showRunTime").Attribute("value").Value);
                autoCheckUpdate = Convert.ToBoolean(setting.Element("autoCheckUpdate").Attribute("value").Value);

                XElement parameter = property.Element("parameter");
                maxHistory = Convert.ToInt32(parameter.Element("maxHistory").Value);
                pairFolder = parameter.Element("pairFolder").Value;
            }
            catch (Exception e)
            {
                MessageBox.Show("配置文件有问题。原因：\n" + e.Message);
            }
        }
        void SaveConfig(string filePath)
        {
            XElement property = new XElement("property",
                new XElement("version", version),
                new XElement("color", color),
                new XElement("setting",
                    new XElement("alwaysShowingBox2", new XAttribute("value", alwaysShowingBox2)),
                    new XElement("autoReset", new XAttribute("value", autoReset)),
                    new XElement("detailedCountInfo", new XAttribute("value", detailedCountInfo)),
                    new XElement("autoCache", new XAttribute("value", autoCache)),
                    new XElement("cutButton", new XAttribute("value", cutButton)),
                    new XElement("showRunTime", new XAttribute("value", showRunTime)),
                    new XElement("autoCheckUpdate", new XAttribute("value", autoCheckUpdate))
                ),
                new XElement("parameter",
                    new XElement("maxHistory", maxHistory.ToString()),
                    new XElement("pairFolder", pairFolder)
                    )
                );
            XDocument doc = new XDocument(property);
            doc.Save(configFilePath);
        }
        public void ApplyConfig()
        {
            Title = "文本编辑器 v" + version;
            ChangeTheme(color);
            EditorPanel.AlwaysShowingBox2 = alwaysShowingBox2;
            SetCutCopyButton(cutButton ? "剪切" : "复制");
            EditorPanel.TextBox1.UndoLimit = maxHistory;
            EditorPanel.TextBox2.UndoLimit = maxHistory;
            EditorPanel.TextBox3.UndoLimit = maxHistory;
            EditorPanel.TextBox4.UndoLimit = maxHistory;
        }
        public void SetCutCopyButton(string mode)
        {
            CutCopyButtonText.Text = mode;
            if (mode == "剪切")
            {
                CopyIcon.Visibility = Visibility.Collapsed;
                CutIcon.Visibility = Visibility.Visible;
            }
            else if (mode == "复制")
            {
                CopyIcon.Visibility = Visibility.Visible;
                CutIcon.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 左侧元件的相关事件

        // 重置所有功能
        private void ResetAllFunctions()
        {
            // 内容查找与替换
            rtFindText.Clear();
            rtReplacement.Clear();
            rtIgnoreCase.IsChecked = false;
            // 删除括号内容
            brLeft.Clear();
            brRight.Clear();
            brPairList.SelectedIndex = -1;
            brKeepBracket.IsChecked = false;
            brShowBracketContent.IsChecked = false;
            brNestBracket.IsChecked = false;
            // 删除空白
            dbRemoveAll.IsChecked = true;
            // 换行符相关
            nsRemoveAll.IsChecked = true;
            // 文本顺序调换
            toByLine.IsChecked = true;
            // 批量重复文本
            rptTime.Text = "1";
            rptAutoNewLine.IsChecked = true;
            // 隔行删除
            dblReserve.Text = "1";
            dblRemove.Text = "1";
            dblShowRemovedResults.IsChecked = false;
            // 隔行删除
            iblReserve.Text = "1";
            iblInsert.Text = "0";
            // 逐行插入相同内容
            ablInsertContent.Text = "";
            ablPosition.Text = "";
            ablNoIgnore.IsChecked = true;
            // 逐行插入不同内容
            siPosition.Text = "0";
            siIngoreEmpty.IsChecked = false;
            // 逐行添加序号
            aiblLeft.Text = "";
            aiblRight.Text = "";
            aiblPosition.Text = "0";
            aiblStartValue.Text = "1";
            aiblDigits.Text = "3";
            aiblAlignNumber.IsChecked = false;
            aiblIgnoreEmpty.IsChecked = false;
            aiblNormal.IsChecked = true;
            // 文本格式转换
            ftST.IsChecked = true;
            // 自定义转换列表
            tlR.IsChecked = false;
            // 正则表达式
            reFind.Text = "";
            reReplace.Text = "";
            reIgnoreCase.IsChecked = false;
            reMultiline.IsChecked = false;
            reSingleline.IsChecked = false;
            reIgnorePatternWhitespace.IsChecked = false;
            reExplicitCapture.IsChecked = false;
            reReplaceOnly.IsChecked = true;
            // 剪贴板辅助工具
            chCycle.IsChecked = false;
            chIgnoreBlank.IsChecked = false;
            chAutoPaste.IsChecked = false;
            chDelay.Text = "250";
            chAutoKeyDown.SelectedIndex = 2;
        }
        
        // 左侧的标题单选按钮的单击事件
        private void TitleRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (autoReset) ResetAllFunctions();
        }

        // 交换按钮
        private void ExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Name == "rtExchangeButton")
            {
                var temp = rtFindText.Text;
                rtFindText.Text = rtReplacement.Text;
                rtReplacement.Text = temp;
            }
            else if (b.Name == "brExchangeButton")
            {
                var temp = brLeft.Text;
                brLeft.Text = brRight.Text;
                brRight.Text = temp;
            }
        }

        // 常用括号对
        private void brPairList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string pair = ((TextBlock)brPairList.SelectedValue).Text;
            if (string.IsNullOrEmpty(pair)) return;
            brLeft.Text = pair.Substring(0, 1);
            brRight.Text = pair.Substring(2);
        }

        // 展开/折叠左侧功能栏
        public bool isShowingLeftMenu = true;
        public void FoldLeftMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowLeftMenu(!isShowingLeftMenu);
        }
        public void ShowLeftMenu(bool value)
        {
            if (value)
            {
                LeftMenuContainer.Width = new GridLength(225);
            }
            else
            {
                LeftMenuContainer.Width = new GridLength(0);
            }
            isShowingLeftMenu = value;
        }

        // 在批量重复文本时，重复次数可能会出现一些问题，视情况显示提示文字
        private void RepeatTextWarning_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rptWarning == null || rptNANWarning == null) return;
            int count;
            // 输入的不是正整数
            if (!int.TryParse(rptTime.Text, out count))
            {
                rptNANWarning.Visibility = Visibility.Visible;
            }
            else
            {
                if (count < 1)
                    rptNANWarning.Visibility = Visibility.Visible;
                else
                    rptNANWarning.Visibility = Visibility.Collapsed;
            }
            // 最终生成的文字字数太大
            if (count * T1.Length > 50000)
            {
                rptWarning.Visibility = Visibility.Visible;
            }
            else
            {
                rptWarning.Visibility = Visibility.Collapsed;
            }
        }

        // 涉及第二文本框显示与否的按钮
        private void Box2Button_Checked(object sender, RoutedEventArgs e)
        {
            EditorPanel.ShowHideSecondBox(true);
        }
        private void Box2Button_Unchecked(object sender, RoutedEventArgs e)
        {
            EditorPanel.ShowHideSecondBox(false);
        }

        // 剪贴板辅助工具的延迟时间的错误提示
        private void chDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (chNANWarning == null) return;
            uint output;
            if (!uint.TryParse(chDelay.Text, out output))
                chNANWarning.Visibility = Visibility.Visible;
            else
                chNANWarning.Visibility = Visibility.Collapsed;
        }

        // 自定义转换列表的选择更改在右侧为列表编辑功能时自动更新右侧内容
        private void TransformListFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RightContentControl.Content == PairListPanel)
            {
                ShowRightContent(RightContent.PairList, false);
            }
        }

        #endregion

        #region 剪贴板辅助工具

        HotKey hotkey;
        void HotKeyEvent()
        {
            if (!isUsingPasteHelper) return;
            // 自动粘贴功能
            if (chAutoPaste.IsChecked.Value)
            {
                hotkey.Dispose();
                Thread.Sleep(800);// 这里如果不暂停，可能会出现用户按键时间过长，导致在第一次粘贴时内容错乱                        
                int delay = int.Parse(chDelay.Text) / 2;// 延迟为每次粘贴的时间间隔，而每次循环有两次延迟，故除以2
                                                        // 关于SendKey的更多内容，详见：https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys(v=vs.110).aspx
                string key = "{" + chAutoKeyDown.Text + "}";
                IsEnabled = false;// 将窗口挂起
                while (PasteLineIndex < PasteLines.Count)
                {
                    Form.SendKeys.SendWait("^{V}");
                    Thread.Sleep(delay);
                    if (++PasteLineIndex >= PasteLines.Count)
                    {
                        hotkey.Dispose();
                        ClearPasteHelper();
                        IsEnabled = true;
                        break;
                    }
                    if (key != "{NONE}")
                        Form.SendKeys.SendWait(key);
                    Clipboard.SetText(PasteLines[PasteLineIndex]);
                    Thread.Sleep(delay);
                }
            }
        }

        KeyboardHook kh;
        void HookOnKeyDownEvent(object sender, Form.KeyEventArgs e)
        {
            if (!isUsingPasteHelper) return;
            // 如果按下Ctrl+V
            if (e.KeyData == (Form.Keys.V | Form.Keys.Control))
            {
                try
                {
                    // 如果还没有循环一遍
                    if (PasteLineIndex < PasteLines.Count)
                    {
                        Clipboard.SetText(PasteLines[PasteLineIndex]);
                        PasteLineIndex++;
                    }
                    // 如果已经粘贴完全部的内容
                    // 此处不能写else if，因为在上面的if中粘贴后，PasteLineIndex在递增，需要再次检查是否超出范围
                    if (PasteLineIndex >= PasteLines.Count)
                    {
                        // 如果开启了粘贴循环，则重头开始
                        if (chCycle.IsChecked.Value)
                        {
                            PasteLineIndex = 0;
                        }
                        // 否则，关闭剪贴板辅助工具
                        else
                        {
                            kh.UnHook();
                            ClearPasteHelper();
                        }
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("剪贴板辅助工具出现错误。\n原因：" + err.Message);
                    kh.UnHook();
                    ClearPasteHelper();
                    IsEnabled = true;
                }
            }
        }

        List<string> PasteLines = new List<string>();
        int PasteLineIndex = 0;
        bool isUsingPasteHelper = false;
        void ClearPasteHelper()
        {
            isUsingPasteHelper = false;
            PasteLineIndex = 0;
            PasteLines.Clear();

            chStopButton.Visibility = Visibility.Collapsed;
            chIgnoreBlank.IsEnabled = true;
            chCycle.IsEnabled = true;
            chAutoPaste.IsEnabled = true;
            chAutoKeyDown.IsEnabled = true;
            chDelay.IsEnabled = true;
        }
        private void ClipboardHelperStop(object sender, RoutedEventArgs e)
        {
            if (isUsingPasteHelper)
            {
                ClearPasteHelper();
                if (chAutoPaste.IsChecked.Value) hotkey.Dispose();
                else kh.UnHook();
                MessageBox.Show("剪贴板辅助工具已手动停止");
            }
        }

        #endregion

        #region 文件重命名

        List<string> RenameList = new List<string>();
        Dictionary<string, string> RenamePairs = new Dictionary<string, string>();
        void ImportRenameFileList(string[] list)
        {
            ImportRenameFileList(new List<string>(list));
        }
        void ImportRenameFileList(List<string> list)
        {
            RenameList = new List<string>(list);
            RenamePairs.Clear();
            T2 = F.CombineWith(RenameList, F.NL);
            T1 = F.CombineWith(from line in RenameList select Path.GetFileName(line), F.NL);
        }
        void ClearRenameFileList()
        {
            RenameList.Clear();
            RenamePairs.Clear();
        }
        void ApplyRename()
        {
            if (RenameList.Count == 0)
            {
                MessageBox.Show("当前无需要重命名的文件。");
                return;
            }
            List<string> NewNameList = F.SplitByString(T1, F.NL);
            if (NewNameList.Count != RenameList.Count)
            {
                MessageBox.Show("新文件名称数量与导入文件数量不同，请重试。");
                return;
            }
            try
            {
                for (int i = 0; i < NewNameList.Count; i++)
                {
                    string origin = RenameList[i], newname = NewNameList[i];
                    newname = NewNameList[i] = Path.GetDirectoryName(origin) + @"\" + newname;
                    File.Move(origin, newname);
                    RenamePairs.Add(origin, newname);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("重命名失败，尝试还原已修改文件。错误原因：\n" + e.Message);
                foreach (var pair in RenamePairs)
                {
                    File.Move(pair.Value, pair.Key);
                }
                ImportRenameFileList(RenameList);
                return;
            }
            // 无论成功与否，都会更新文件列表，以及文本框的显示内容
            ImportRenameFileList(NewNameList);
            MessageBox.Show("重命名成功。");
        }

        #endregion

        #region 程序快捷键

        private void OpenFileShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile_Click(this, null);
        }
        private void SaveFileShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile_Click(this, null);
        }
        private void SaveFileAsShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileAs_Click(this, null);
        }
        private void ApplyShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ApplyButton_Click(this, null);
        }
        private void ClearShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ClearButton_Click(this, null);
        }
        private void FoldLeftShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ShowLeftMenu(!isShowingLeftMenu);
        }
        private void FoldRightShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            EditorPanel.FoldRightTextBox(!EditorPanel.IsShowingBox34);
        }

        #endregion
    }
}
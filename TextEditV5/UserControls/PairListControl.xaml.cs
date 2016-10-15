using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text;
using System;
using System.Windows.Input;
using Form = System.Windows.Forms;
using TextEditV5.PairExchange;

namespace TextEditV5.UserControls
{
    /// <summary>
    /// PairListControl.xaml 的交互逻辑
    /// </summary>
    public partial class PairListControl : UserControl
    {
        public PairListControl()
        {
            InitializeComponent();
        }

        public PairListManager Manager = new PairListManager();
        string path;
        public void LoadFile(string FileName)
        {
            // 读取并更新显示
            path = Path.GetDirectoryName(FileName);
            try
            {
                Manager.LoadFromFile(FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("文件内容格式有误。");
            }
            CurrentListName.Text = Path.GetFileNameWithoutExtension(FileName);
            Update();
        }
        public void Update()
        {
            // 初始化
            PairListBox.Items.Clear();
            LeftContent.Clear();
            RightContent.Clear();
            //SeparatorSymbol.Clear();
            //CommentSymbol.Clear();
            UsingRex.IsChecked = false;
            IgnoreCase.IsChecked = false;
            Multiline.IsChecked = false;
            Singleline.IsChecked = false;
            IgnorePatternWhitespace.IsChecked = false;
            ExplicitCapture.IsChecked = false;
            // 左侧内容
            SeparatorSymbol.Text = Manager.Separator;
            CommentSymbol.Text = Manager.Comment;
            // 正则表达式
            UsingRex.IsChecked = Manager.UsingRex;
            if (Manager.UsingRex)
            {
                IgnoreCase.IsChecked = Manager.IgnoreCase;
                Multiline.IsChecked = Manager.Multiline;
                Singleline.IsChecked = Manager.Singleline;
                IgnorePatternWhitespace.IsChecked = Manager.IgnorePatternWhitespace;
                ExplicitCapture.IsChecked = Manager.ExplicitCapture;
            }
            // 右侧内容
            for (int i = 0; i < Manager.List.Count; i++)
            {
                PairListBox.Items.Add(Manager.GetLineInfo(i));
            }
            // 下方信息栏
            InfoBar.Text = string.Format("共{0}行，编码格式：{1}", Manager.List.Count, Manager.CurrentEncoding.EncodingName);
        }
        public MainWindow mainForm;

        public string L { get { return LeftContent.Text; } }
        public string R { get { return RightContent.Text; } }

        private void PairListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = PairListBox.SelectedIndex;
            if (index >= 0)
            {
                LeftContent.Text = Manager[index].Left;
                RightContent.Text = Manager[index].Right;
            }
        }
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int index = PairListBox.SelectedIndex;
            if (index < 1) return;//-1表示没有选择，0表示选中最上边的
            Manager.Switch(index, index - 1);
            Update();
            PairListBox.SelectedIndex = index - 1;
        }
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int index = PairListBox.SelectedIndex;
            if (index < 0 || index >= Manager.List.Count - 1) return;
            Manager.Switch(index, index + 1);
            Update();
            PairListBox.SelectedIndex = index + 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = path + @"\" + CurrentListName.Text + ".txt";
            StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Create), Manager.CurrentEncoding);
            try
            {
                sw.Write(Manager.StringContents);
            }
            catch (Exception)
            {
                // 出现这个问题，极大可能是文件名不正确
                MessageBox.Show("您的文件名无效。");
            }
            finally
            {
                sw.Dispose();
                MessageBox.Show("保存成功。\n" + fileName);
            }
        }

        private void LeftRightContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int index = PairListBox.SelectedIndex;
                if (index < 0) return;
                Manager.AmendAt(L, R, index);
                PairListBox.Items[index] = Manager.GetLineInfo(index);
            }
        }
        private void SeparatorSymbol_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SeparatorSymbol.Text.Length > 0)
                {
                    Manager.Separator = SeparatorSymbol.Text;
                    Update();
                }
            }
        }
        private void CommentSymbol_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CommentSymbol.Text.Length > 0)
                {
                    Manager.Comment = CommentSymbol.Text;
                    Update();
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            switch (c.Name)
            {
                case "UsingRex": Manager.UsingRex = c.IsChecked.Value; break;
                case "IgnoreCase": Manager.IgnoreCase = c.IsChecked.Value; break;
                case "Multiline": Manager.Multiline = c.IsChecked.Value; break;
                case "Singleline": Manager.Singleline = c.IsChecked.Value; break;
                case "IgnorePatternWhitespace": Manager.IgnorePatternWhitespace = c.IsChecked.Value; break;
                case "ExplicitCapture": Manager.ExplicitCapture = c.IsChecked.Value; break;
            }
        }

        private void TopRightButton_Click(object sender, RoutedEventArgs e)
        {
            string header = ((Button)sender).Content.ToString();
            if (header == "排序")
            {
                Manager.Sort();
                Update();
            }
            else if (header == "添加")
            {
                int index = PairListBox.SelectedIndex + 1;
                Manager.Insert(new PairElement(), index);
                Update();
                PairListBox.SelectedIndex = index;
            }
            else if (header == "删除")
            {
                int index = PairListBox.SelectedIndex;
                if (index < 0) return;
                Manager.RemoveAt(index);
                Update();
                if (PairListBox.Items.Count > 0)
                {
                    if (index == 0) PairListBox.SelectedIndex = 0;
                    else PairListBox.SelectedIndex = index - 1;
                }
                else PairListBox.SelectedIndex = -1;
            }
            else if (header == "清空")
            {
                Manager.Clear();
                Update();
                //PairListBox.SelectedIndex = -1;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            mainForm.ShowLeftMenu(true);
            mainForm.ShowRightContent(TextEditV5.RightContent.Editor);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Form.SaveFileDialog save = new Form.SaveFileDialog();
            save.Filter = "文本文件|*.txt";
            save.InitialDirectory = Environment.CurrentDirectory + @"\" + mainForm.pairFolder;
            if (save.ShowDialog() == Form.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(save.FileName, FileMode.Create), Manager.CurrentEncoding))
                {
                    sw.Write(Manager.StringContents);
                }
                path = save.FileName;
                LoadFile(path);
                Update();
            }
        }
    }
}
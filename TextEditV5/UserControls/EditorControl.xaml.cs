using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextEditV5.UserControls
{
    /// <summary>
    /// EditorControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditorControl : UserControl
    {
        public EditorControl()
        {
            InitializeComponent();
            IsShowingBox2 = IsShowingBox34 = false;
        }
        public MainWindow mainForm;

        public string Box1
        {
            get { return TextBox1.Text; }
            set { TextBox1.Text = value; }
        }
        public string Box2
        {
            get { return TextBox2.Text; }
            set { TextBox2.Text = value; }
        }
        public string Box3
        {
            get { return TextBox3.Text; }
            set { TextBox3.Text = value; }
        }
        public string Box4
        {
            get { return TextBox4.Text; }
            set { TextBox4.Text = value; }
        }
        public bool Box2Visible
        {
            get
            {
                return TextBox2.Visibility == Visibility.Visible;
            }
            set
            {
                TextBox2.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool Box3Visible
        {
            get
            {
                return TextBox3.Visibility == Visibility.Visible;
            }
            set
            {
                TextBox3.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool Box4Visible
        {
            get
            {
                return TextBox4.Visibility == Visibility.Visible;
            }
            set
            {
                TextBox4.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public string TextLength1
        {
            get
            {
                Regex r1 = new Regex(@"\w");
                Regex r2 = new Regex(@"^", RegexOptions.Multiline);
                return string.Format("{0}/{1}", r1.Matches(Box1).Count, r2.Matches(Box1).Count);
            }
        }
        public string TextLength2
        {
            get
            {
                Regex r1 = new Regex(@"\w");
                Regex r2 = new Regex(@"^", RegexOptions.Multiline);
                return string.Format("{0}/{1}", r1.Matches(Box2).Count, r2.Matches(Box2).Count);
            }
        }
        public string GetTextLengthInfo(bool detailed = false)
        {
            // 总字数
            Regex r1 = new Regex(@"\w");
            // 总行数
            Regex r2 = new Regex(@"(?m)^");
            // 数字数
            Regex r3 = new Regex(@"\d");
            // 中文字数
            Regex r4 = new Regex(@"[\u4E00-\u9FA5]");

            StringBuilder sb = new StringBuilder();
            if (Box2Visible)
                sb.AppendLine("文本框一：");

            sb.AppendLine("总字数：" + r1.Matches(Box1).Count.ToString());
            sb.AppendLine("总字符数：" + Box1.Length.ToString());
            if (detailed)
            {
                sb.AppendLine("数字数：" + r3.Matches(Box1).Count.ToString());
                sb.AppendLine("中文字数：" + r4.Matches(Box1).Count.ToString());
                sb.AppendLine("总字节数：" + Encoding.Default.GetByteCount(Box1).ToString());
            }
            sb.AppendLine("总行数：" + r2.Matches(Box1).Count.ToString());

            if(Box2Visible)
            {
                sb.AppendLine("--------------");
                sb.AppendLine("文本框二：");

                sb.AppendLine("总字数：" + r1.Matches(Box2).Count.ToString());
                sb.AppendLine("总字符数：" + Box2.Length.ToString());
                if (detailed)
                {
                    sb.AppendLine("数字数：" + r3.Matches(Box2).Count.ToString());
                    sb.AppendLine("中文字数：" + r4.Matches(Box2).Count.ToString());
                    sb.AppendLine("总字节数：" + Encoding.Default.GetByteCount(Box2).ToString());
                }
                sb.AppendLine("总行数：" + r2.Matches(Box2).Count.ToString());
            }

            return sb.ToString();
        }

        private bool alwaysShowingBox2;
        public bool AlwaysShowingBox2
        {
            get { return alwaysShowingBox2; }
            set
            {
                alwaysShowingBox2 = value;

                ShowHideSecondBox(IsShowingBox2);
            }
        }
        public bool IsShowingBox2 { get; set; }
        public void ShowHideSecondBox(bool show)
        {
            if (AlwaysShowingBox2)
            {
                if (!IsShowingBox2)
                {
                    ShowSecondBox();
                }
            }
            else
            {
                if (show && !IsShowingBox2)
                {
                    ShowSecondBox();
                }
                else if (!show && IsShowingBox2)
                {
                    HideSecondBox();
                }
            }
        }
        public void ShowSecondBox()
        {
            Box2Container.Height = new GridLength(1, GridUnitType.Star);
            Box2Visible = true;
            IsShowingBox2 = true;
            Box_TextChanged(this, null);
        }
        public void HideSecondBox()
        {
            if (AlwaysShowingBox2)
            {
                ShowSecondBox();
                return;
            }
            Box2Container.Height = new GridLength(0, GridUnitType.Star);
            Box2Visible = false;
            IsShowingBox2 = false;
            Box_TextChanged(this, null);
        }

        public bool IsShowingBox34 { get; set; }
        private void FoldRightTextBox_Click(object sender, RoutedEventArgs e)
        {
            FoldRightTextBox(!IsShowingBox34);
        }
        public void FoldRightTextBox(bool value)
        {
            if (value)
            {
                Box34Container.Width = new GridLength(1, GridUnitType.Star);
                Box3Visible = true;
                Box4Visible = true;
                IsShowingBox34 = true;
            }
            else
            {
                Box34Container.Width = new GridLength(0, GridUnitType.Star);
                Box3Visible = false;
                Box4Visible = false;
                IsShowingBox34 = false;
            }
        }

        // 清空正在显示的文本框的内容
        public void ClearShownTextBoxs()
        {
            Box1 = Functions.EM;
            if (Box2Visible) Box2 = Functions.EM;
            if (Box3Visible) Box3 = Functions.EM;
            if (Box4Visible) Box4 = Functions.EM;
        }

        // 双击交换文本框内容
        private void ExchangeTextBox13_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            string temp = Box1;
            Box1 = Box3;
            Box3 = temp;
        }
        private void ExchangeTextBox24_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            string temp = Box2;
            Box2 = Box4;
            Box4 = temp;
        }
        private void ExchangeTextBox12_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsShowingBox2) return;
            string temp = Box1;
            Box1 = Box2;
            Box2 = temp;
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mainForm == null) return;
            if (Box2Visible)
                mainForm.ShowWordCount(TextLength1 + ", " + TextLength2);
            else
                mainForm.ShowWordCount(TextLength1);
        }

    }
}

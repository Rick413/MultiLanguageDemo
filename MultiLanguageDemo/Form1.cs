using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MultiLanguageDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();

            this.cb_Language.SelectedIndexChanged += new System.EventHandler(this.cb_Language_SelectedIndexChanged);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language.xml");
            MultiLanguage.GetInstance().LoadParamFile(path);//加载xml
            this.cb_Language.DataSource = MultiLanguage.GetInstance().Language;//语言可选项数据绑定cb
        }
        //测试按钮
        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Font = new System.Drawing.Font("宋体", 20F);

            //MultiLanguage.GetInstance().UpdateText(this, "");
        }

        //语言选择
        private void cb_Language_SelectedIndexChanged(object sender, EventArgs e)
        {
            //int i = lb_Language.Controls.Count;
            MultiLanguage.GetInstance().SelectLanguage(this, cb_Language.Text);

        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

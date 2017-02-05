using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

namespace DataCloneTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            transpanel.Width = this.Width;
            transpanel.Height = this.Height;
        }

        private TransparentPanel transpanel = new TransparentPanel();
        private EntityConfig _entityConfig;

        private void Form1_Load(object sender, EventArgs e)
        {
            transpanel.SendToBack();
            BindEntityConfigMenu("EntityConfig_Test_Debtor.xml");
            BindEntityConfigMenu("EntityConfig_Test_Agency.xml");
            BindEntityConfigMenu("EntityConfig_Agency.xml");
            BindEntityConfigMenu("EntityConfig_Policy.xml");

            LoadEntiryConfig("EntityConfig_Test_Agency.xml");
        }

        private void BindEntityConfigMenu(string configName)
        {
            var item = new ToolStripMenuItem(configName);
            item.Click += item_Click;
            toolStripMenuItem1.DropDownItems.Add(item);
        }

        void item_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            LoadEntiryConfig(item.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Controls.Add(transpanel);
            transpanel.BringToFront();

            Task.Factory.StartNew(() =>
            {
                var entityTreeBuilder = new EntityTreeBuilder();
                var dalHelper = new ScriptDalHelper();
                var scriptEngine = new ScriptEngine(entityTreeBuilder, new ScriptHelper(entityTreeBuilder, dalHelper), dalHelper);
                var script = scriptEngine.BuildScript(_entityConfig, textBox1.Text);

                this.Invoke(new MethodInvoker(() =>
                {
                    this.richTextBox1.Text = script;
                    this.Controls.Remove(transpanel);
                    transpanel.SendToBack();
                }));
            });
        }

        private void LoadEntiryConfig(string configName)
        {
            this.Text = configName;
            using (var fs = new FileStream(configName, FileMode.Open))
            {
                _entityConfig = new XmlSerializer(typeof(EntityConfig)).Deserialize(fs) as EntityConfig;
            }

            var entityRootNodes = new EntityTreeBuilder().BuildEntityTrees(_entityConfig);
            var realEntityRootNode = entityRootNodes.First(entityNode => !entityNode.CurrentEntity.Global);

            this.label1.Text = string.Format("{0} {1}:", realEntityRootNode.CurrentEntity.Name, realEntityRootNode.CurrentEntity.IdentityColumnName);
        }
    }
}

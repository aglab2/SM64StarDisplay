using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace StarDisplay
{
    public partial class ActionMaskForm : Form
    {
        public Dictionary<Type, Boolean> configs;

        public ActionMaskForm(Dictionary<Type, Boolean> configs)
        {
            InitializeComponent();

            this.configs = configs;

            int height = 10;

            foreach (var component in configs.Keys)
            {
                PropertyInfo info = component.GetProperty("configureName");
                string componentDescription = (string)info.GetValue(null);

                CheckBox cb = new CheckBox
                {
                    Name = component.FullName,
                    Text = componentDescription,
                    Location = new Point(10, height),
                    Checked = configs[component],
                    AutoSize = true
                };

                cb.CheckedChanged += checkBox_CheckedChanged;

                height += 20;
                this.Controls.Add(cb);
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            configs[Type.GetType(cb.Name)] = cb.Checked;
        }
    }
}

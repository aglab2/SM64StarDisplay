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
        public SettingsManager sm;

        public ActionMaskForm(IEnumerable<Type> actions, SettingsManager sm)
        {
            InitializeComponent();

            this.sm = sm;

            int height = 5;

            foreach (var component in actions)
            {
                MethodInfo mi = component.GetMethod("DrawConfigs");
                object addedHeight = mi.Invoke(null, new object[] { height, this });
                height += (int)addedHeight + 5;
            }

            this.Height = height + (this.Height - this.ClientRectangle.Height);
        }
    }
}

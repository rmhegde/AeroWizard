﻿using System.Drawing;
using System.Windows.Forms;
using Vanara.Windows.Forms;

namespace TestWizard
{
	public partial class MyWizard : Form
	{
		private readonly Button extraBtn;
		private readonly System.Text.StringBuilder events = new System.Text.StringBuilder(1024);

		public MyWizard()
		{
			InitializeComponent();
			//this.wizardControl1.TitleIcon = null;
			foreach (var i in wizardControl1.Pages)
				i.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(i_Commit);
			wizardControl1.Finished += new System.EventHandler(wizardControl1_Finished);
			extraBtn = new Button { Text = "Events", AutoSize = true, AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink, Anchor = AnchorStyles.Top | AnchorStyles.Right, Margin = Padding.Empty };
			wizardControl1.AddCommandControl(extraBtn);
			extraBtn.Click += extraBtn_Click;
			SystemColorsChanged += MyWizard_SystemColorsChanged;
			StyleChanged += MyWizard_StyleChanged;
			if (System.Environment.OSVersion.Version.Major >= 6)
			{
				DesktopWindowManager.ColorizationColorChanged += DesktopWindowManager_ColorizationColorChanged;
				DesktopWindowManager.CompositionChanged += DesktopWindowManager_CompositionChanged;
			}
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
		}

		private void MyWizard_StyleChanged(object sender, System.EventArgs e)
		{
			bool ncre = false, clk = false; Rectangle cbb = Rectangle.Empty, efb = Rectangle.Empty;
			try
			{
				ncre = this.IsNonClientRenderingEnabled();
				efb = this.GetExtendedFrameBounds();
				cbb = this.GetCaptionButtonBounds();
				clk = System.Environment.OSVersion.Version.Minor >= 2 && this.GetCloakingSource() > 0;
			}
			catch { }
			events.AppendFormat("{0:s}: Style (NCRend:{1}, Clk:{2}, CapBtn:{3}, ExtFrm:{4}\n", System.DateTime.Now, ncre, clk, cbb, efb);
		}

		private void MyWizard_SystemColorsChanged(object sender, System.EventArgs e)
		{
			var ncre = this.IsNonClientRenderingEnabled();
			var clk = System.Environment.OSVersion.Version.Minor >= 2 && this.GetCloakingSource() > 0;
			var cbb = this.GetCaptionButtonBounds();
			var efb = this.GetExtendedFrameBounds();
			events.AppendFormat("{0:s}: System colors (NCRend:{1}, Clk:{2}, CapBtn:{3}, ExtFrm:{4}\n", System.DateTime.Now, ncre, clk, cbb, efb);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
			if (System.Environment.OSVersion.Version.Major >= 6)
			{
				DesktopWindowManager.ColorizationColorChanged -= DesktopWindowManager_ColorizationColorChanged;
				DesktopWindowManager.CompositionChanged -= DesktopWindowManager_CompositionChanged;
			}
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
		}

		private void extraBtn_Click(object sender, System.EventArgs e) => MessageBox.Show(events.ToString());

		private void SystemEvents_DisplaySettingsChanged(object sender, System.EventArgs e) => events.AppendFormat("{0:s}: Display settings\n", System.DateTime.Now);

		private void DesktopWindowManager_CompositionChanged(object sender, System.EventArgs e) => events.AppendFormat("{0:s}: Composition ({1})\n", System.DateTime.Now, DesktopWindowManager.CompositionEnabled ? "On" : "Off");

		private void DesktopWindowManager_ColorizationColorChanged(object sender, System.EventArgs e) => events.AppendFormat("{0:s}: Colorization color (0x{1:x})\n", System.DateTime.Now, DesktopWindowManager.CompositionColor.ToArgb());

		private void wizardControl1_Finished(object sender, System.EventArgs e) => System.Diagnostics.Debug.WriteLine("--> Wizard finished.");

		private void i_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e) => System.Diagnostics.Debug.WriteLine($"--> Page {e.Page.Name} committed.");

		private void commandLink1_Click(object sender, System.EventArgs e) => wizardControl1.NextPage();

		private void commandLink2_Click(object sender, System.EventArgs e) => wizardControl1.NextPage(endPage);

		private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!initMiddle)
				DesktopWindowManager.EnableComposition(checkBox1.Checked);
		}

		private bool initMiddle = false;

		private void middlePage_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e)
		{
			initMiddle = true;
			if (!System.IO.File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System), "dwmapi.dll")))
				checkBox1.Enabled = false;
			else
				checkBox1.Checked = DesktopWindowManager.CompositionEnabled;
			wizardControl1.FinishButtonText = "Finish";
			initMiddle = false;
		}

		private void checkBox2_CheckedChanged(object sender, System.EventArgs e)
		{
			wizardControl1.SelectedPage.AllowNext = checkBox2.Checked;
			wizardControl1.SelectedPage.AllowBack = !checkBox2.Checked;
		}

		private void endPage_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e) => wizardControl1.FinishButtonText = "Sorry, but you are hosed.";

		private void introPage_HelpClicked(object sender, System.EventArgs e) => MessageBox.Show("Clicked help");

		private void introPage_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e)
		{
			//MessageBox.Show("Page initialized");
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => new OpenFileDialog().ShowDialog(this);
	}
}

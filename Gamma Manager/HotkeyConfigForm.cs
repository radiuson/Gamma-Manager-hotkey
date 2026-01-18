using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Gamma_Manager
{
    public class HotkeyConfigForm : Form
    {
        private ListView listViewHotkeys;
        private Button buttonAdd;
        private Button buttonEdit;
        private Button buttonRemove;
        private Button buttonClose;
        private ComboBox comboBoxPresets;
        private CheckBox checkCtrl;
        private CheckBox checkAlt;
        private CheckBox checkShift;
        private CheckBox checkWin;
        private ComboBox comboBoxKey;
        private Button buttonSet;
        private Label labelPreset;
        private Label labelModifiers;
        private Label labelKey;
        private GroupBox groupBoxConfig;

        public Dictionary<string, HotkeyConfig> HotkeyConfigs { get; private set; }
        private List<string> availablePresets;

        public class HotkeyConfig
        {
            public uint Modifiers { get; set; }
            public Keys Key { get; set; }

            public HotkeyConfig(uint modifiers, Keys key)
            {
                Modifiers = modifiers;
                Key = key;
            }
        }

        public HotkeyConfigForm(List<string> presets, Dictionary<string, HotkeyConfig> existingConfigs = null)
        {
            availablePresets = presets;
            HotkeyConfigs = existingConfigs ?? new Dictionary<string, HotkeyConfig>();

            InitializeComponent();
            LoadHotkeys();
        }

        private void InitializeComponent()
        {
            this.Text = "Hotkey Configuration";
            this.Size = new Size(600, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // ListView for showing configured hotkeys
            listViewHotkeys = new ListView
            {
                Location = new Point(12, 12),
                Size = new Size(560, 200),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewHotkeys.Columns.Add("Preset", 200);
            listViewHotkeys.Columns.Add("Hotkey", 150);
            listViewHotkeys.Columns.Add("Monitor", 180);
            listViewHotkeys.SelectedIndexChanged += ListViewHotkeys_SelectedIndexChanged;

            // Group box for configuration
            groupBoxConfig = new GroupBox
            {
                Text = "Configure Hotkey",
                Location = new Point(12, 220),
                Size = new Size(560, 140)
            };

            labelPreset = new Label
            {
                Text = "Preset:",
                Location = new Point(10, 25),
                Size = new Size(80, 20)
            };
            groupBoxConfig.Controls.Add(labelPreset);

            comboBoxPresets = new ComboBox
            {
                Location = new Point(100, 22),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            groupBoxConfig.Controls.Add(comboBoxPresets);

            labelModifiers = new Label
            {
                Text = "Modifiers:",
                Location = new Point(10, 60),
                Size = new Size(80, 20)
            };
            groupBoxConfig.Controls.Add(labelModifiers);

            checkCtrl = new CheckBox
            {
                Text = "Ctrl",
                Location = new Point(100, 58),
                Size = new Size(60, 24)
            };
            groupBoxConfig.Controls.Add(checkCtrl);

            checkAlt = new CheckBox
            {
                Text = "Alt",
                Location = new Point(170, 58),
                Size = new Size(60, 24)
            };
            groupBoxConfig.Controls.Add(checkAlt);

            checkShift = new CheckBox
            {
                Text = "Shift",
                Location = new Point(240, 58),
                Size = new Size(60, 24)
            };
            groupBoxConfig.Controls.Add(checkShift);

            checkWin = new CheckBox
            {
                Text = "Win",
                Location = new Point(310, 58),
                Size = new Size(60, 24)
            };
            groupBoxConfig.Controls.Add(checkWin);

            labelKey = new Label
            {
                Text = "Key:",
                Location = new Point(10, 95),
                Size = new Size(80, 20)
            };
            groupBoxConfig.Controls.Add(labelKey);

            comboBoxKey = new ComboBox
            {
                Location = new Point(100, 92),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateKeyComboBox();
            groupBoxConfig.Controls.Add(comboBoxKey);

            buttonSet = new Button
            {
                Text = "Set Hotkey",
                Location = new Point(270, 90),
                Size = new Size(100, 28)
            };
            buttonSet.Click += ButtonSet_Click;
            groupBoxConfig.Controls.Add(buttonSet);

            buttonRemove = new Button
            {
                Text = "Remove",
                Location = new Point(380, 90),
                Size = new Size(100, 28)
            };
            buttonRemove.Click += ButtonRemove_Click;
            groupBoxConfig.Controls.Add(buttonRemove);

            // Bottom buttons
            buttonClose = new Button
            {
                Text = "Close",
                Location = new Point(472, 375),
                Size = new Size(100, 30)
            };
            buttonClose.Click += (s, e) => this.DialogResult = DialogResult.OK;

            // Add controls to form
            this.Controls.Add(listViewHotkeys);
            this.Controls.Add(groupBoxConfig);
            this.Controls.Add(buttonClose);
        }

        private void PopulateKeyComboBox()
        {
            // Add commonly used keys
            var keys = new[]
            {
                Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6,
                Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
                Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5,
                Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0,
                Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G,
                Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N,
                Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U,
                Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,
                Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3,
                Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7,
                Keys.NumPad8, Keys.NumPad9
            };

            foreach (var key in keys)
            {
                comboBoxKey.Items.Add(key);
            }

            if (comboBoxKey.Items.Count > 0)
                comboBoxKey.SelectedIndex = 0;
        }

        private void LoadHotkeys()
        {
            listViewHotkeys.Items.Clear();
            comboBoxPresets.Items.Clear();

            // Load available presets
            foreach (var preset in availablePresets)
            {
                comboBoxPresets.Items.Add(preset);

                // Add to list if configured
                if (HotkeyConfigs.ContainsKey(preset))
                {
                    var config = HotkeyConfigs[preset];
                    var item = new ListViewItem(preset);
                    item.SubItems.Add(HotkeyManager.GetHotkeyString(config.Modifiers, config.Key));

                    // Extract monitor name from preset (format: "MonitorName: PresetName")
                    string monitorName = preset.Contains(":") ? preset.Substring(0, preset.IndexOf(":")) : "Unknown";
                    item.SubItems.Add(monitorName);

                    listViewHotkeys.Items.Add(item);
                }
            }

            if (comboBoxPresets.Items.Count > 0)
                comboBoxPresets.SelectedIndex = 0;
        }

        private void ListViewHotkeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewHotkeys.SelectedItems.Count > 0)
            {
                var selectedPreset = listViewHotkeys.SelectedItems[0].Text;

                // Set preset in combobox
                int index = comboBoxPresets.Items.IndexOf(selectedPreset);
                if (index >= 0)
                    comboBoxPresets.SelectedIndex = index;

                // Load hotkey config
                if (HotkeyConfigs.ContainsKey(selectedPreset))
                {
                    var config = HotkeyConfigs[selectedPreset];
                    checkCtrl.Checked = (config.Modifiers & HotkeyManager.MOD_CONTROL) != 0;
                    checkAlt.Checked = (config.Modifiers & HotkeyManager.MOD_ALT) != 0;
                    checkShift.Checked = (config.Modifiers & HotkeyManager.MOD_SHIFT) != 0;
                    checkWin.Checked = (config.Modifiers & HotkeyManager.MOD_WIN) != 0;

                    int keyIndex = comboBoxKey.Items.IndexOf(config.Key);
                    if (keyIndex >= 0)
                        comboBoxKey.SelectedIndex = keyIndex;
                }
            }
        }

        private void ButtonSet_Click(object sender, EventArgs e)
        {
            if (comboBoxPresets.SelectedItem == null)
            {
                MessageBox.Show("Please select a preset.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxKey.SelectedItem == null)
            {
                MessageBox.Show("Please select a key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build modifiers
            uint modifiers = 0;
            if (checkCtrl.Checked) modifiers |= HotkeyManager.MOD_CONTROL;
            if (checkAlt.Checked) modifiers |= HotkeyManager.MOD_ALT;
            if (checkShift.Checked) modifiers |= HotkeyManager.MOD_SHIFT;
            if (checkWin.Checked) modifiers |= HotkeyManager.MOD_WIN;

            if (modifiers == 0)
            {
                MessageBox.Show("Please select at least one modifier key (Ctrl, Alt, Shift, or Win).",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string preset = comboBoxPresets.SelectedItem.ToString();
            Keys key = (Keys)comboBoxKey.SelectedItem;

            // Check if this hotkey is already used
            foreach (var kvp in HotkeyConfigs)
            {
                if (kvp.Key != preset && kvp.Value.Modifiers == modifiers && kvp.Value.Key == key)
                {
                    MessageBox.Show($"This hotkey is already assigned to preset: {kvp.Key}",
                        "Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Save configuration
            HotkeyConfigs[preset] = new HotkeyConfig(modifiers, key);

            // Reload display
            LoadHotkeys();

            MessageBox.Show($"Hotkey set for preset: {preset}\n{HotkeyManager.GetHotkeyString(modifiers, key)}",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            if (comboBoxPresets.SelectedItem == null)
            {
                MessageBox.Show("Please select a preset.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string preset = comboBoxPresets.SelectedItem.ToString();

            if (HotkeyConfigs.ContainsKey(preset))
            {
                HotkeyConfigs.Remove(preset);
                LoadHotkeys();
                MessageBox.Show($"Hotkey removed for preset: {preset}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("This preset doesn't have a hotkey configured.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

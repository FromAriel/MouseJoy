using System;
using System.Drawing;
using System.Windows.Forms;

namespace MouseJoy
{
    public partial class Form1 : Form
    {
        private TrackBar trackBarDeadzone;
        private Label labelDeadzoneValue;
        private Label labelThumbstickX;
        private Label labelThumbstickY;
        private ComboBox comboBoxLeftClick;
        private ComboBox comboBoxRightClick;
        private ComboBox comboBoxMiddleClick;
        private Label labelLeftClick;
        private Label labelRightClick;
        private Label labelMiddleClick;
        private Label labelScrollMult;
        private NumericUpDown numericUpDownScrollMult;


        public Form1()
        {
            InitializeComponent();
            InitializeDeadzoneControls();
            InitializeThumbstickLabels();
            InitializeClickAssignmentControls();
            this.Text = "MouseJoy - Xbox Controller to Mouse Utility";

        }

        private void InitializeInstructionTextBox()
        {
            RichTextBox instructionTextBox = new RichTextBox
            {
                Location = new Point(10, 210), // Adjust the location as needed
                Size = new Size(560, 260), // Adjust the size as needed
                Text = "",
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Append and style "Instructions:" text
            instructionTextBox.SelectionFont = new Font(instructionTextBox.Font, FontStyle.Bold);
            instructionTextBox.SelectionFont = new Font(instructionTextBox.Font.FontFamily, 12, FontStyle.Bold); // Increase font size to 12 and make it bold
            instructionTextBox.AppendText("Instructions:" + Environment.NewLine + Environment.NewLine);

            // Reset the font style for the rest of the text and append it
            instructionTextBox.SelectionFont = new Font(instructionTextBox.Font.FontFamily, 10, FontStyle.Regular); // Change to desired size and style
            instructionTextBox.AppendText(
                "- Left Trigger: Speed up mouse" + Environment.NewLine + 
                "- Right Trigger: Slow down mouse" + Environment.NewLine + Environment.NewLine +
                "- D-pad: Arrow keys" + Environment.NewLine + Environment.NewLine +
                "- Start: Enter key" + Environment.NewLine +
                "- Select: Esc key" + Environment.NewLine + Environment.NewLine +
                "- Start + Select Enable or Disable Joystick control" + Environment.NewLine +
                "    (enabled by default)" + Environment.NewLine + Environment.NewLine +
                "the Right Thumbstick acts as a mousescroll where the cursor hovers over or has focus.");

            this.Controls.Add(instructionTextBox);
        }

        private void InitializeScrollMultControls()
        {
            // Label for the Mouse Scroll Multiplier
            labelScrollMult = new Label();
            labelScrollMult.Text = "Mouse Scroll Mult:";
            labelScrollMult.Location = new Point(270, 40); // Adjust the location according to your layout
            labelScrollMult.AutoSize = true;
            this.Controls.Add(labelScrollMult);

            // NumericUpDown for adjusting the Mouse Scroll Multiplier
            numericUpDownScrollMult = new NumericUpDown();
            numericUpDownScrollMult.Location = new Point(280, 60); // Adjust the location next to the label
            numericUpDownScrollMult.Size = new Size(40, 20); // Adjusted size for up to 3 digits
            numericUpDownScrollMult.Minimum = 1; // Set a sensible minimum value
            numericUpDownScrollMult.Maximum = 100; // Set a sensible maximum value
            numericUpDownScrollMult.Value = 14; // Default value for scrollmult
            numericUpDownScrollMult.DecimalPlaces = 0; // No decimal places, as scrollmult is an integer
            numericUpDownScrollMult.Increment = 1; // Adjust step size
            this.Controls.Add(numericUpDownScrollMult);

            // Event handler for value change
            numericUpDownScrollMult.ValueChanged += new EventHandler(numericUpDownScrollMult_ValueChanged);
        }



        private void numericUpDownScrollMult_ValueChanged(object sender, EventArgs e)
        {
            Program.scrollmult = Convert.ToInt32(numericUpDownScrollMult.Value);
        }




        private void InitializeDeadzoneControls()
        {
            labelDeadzoneValue = new Label
            {
                Text = $"Deadzone: {Program.DEADZONE}",
                Location = new Point(10, 10),
                Size = new Size(120, 20)
            };
            this.Controls.Add(labelDeadzoneValue);

            trackBarDeadzone = new TrackBar
            {
                Minimum = 0,
                Maximum = 32768,
                TickFrequency = 3276,
                Value = Program.DEADZONE,
                Location = new Point(10, 40),
                Size = new Size(250, 45)
            };
            trackBarDeadzone.ValueChanged += TrackBarDeadzone_ValueChanged;
            this.Controls.Add(trackBarDeadzone);
        }

        private void InitializeThumbstickLabels()
        {
            labelThumbstickX = new Label
            {
                Text = "X: 0",
                Location = new Point(10, 90),
                Size = new Size(60, 20)
            };
            this.Controls.Add(labelThumbstickX);

            labelThumbstickY = new Label
            {
                Text = "Y: 0",
                Location = new Point(80, 90),
                Size = new Size(60, 20)
            };
            this.Controls.Add(labelThumbstickY);
        }

        private void InitializeClickAssignmentControls()
        {
            // Labels for clarity
            labelLeftClick = new Label { Text = "Left Click:", Location = new Point(10, 120), Size = new Size(100, 20) };
            labelRightClick = new Label { Text = "Right Click:", Location = new Point(120, 120), Size = new Size(100, 20) };
            labelMiddleClick = new Label { Text = "Middle Click:", Location = new Point(230, 120), Size = new Size(100, 20) };
            this.Controls.AddRange(new Control[] { labelLeftClick, labelRightClick, labelMiddleClick });

            // Left Click ComboBox Initialization
            comboBoxLeftClick = new ComboBox
            {
                Location = new Point(10, 140),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateComboBoxWithButtons(comboBoxLeftClick);
            comboBoxLeftClick.SelectedItem = Program.LeftClickAssignment; // Reflects new default
            comboBoxLeftClick.SelectedValueChanged += (sender, e) =>
                Program.LeftClickAssignment = comboBoxLeftClick.SelectedItem.ToString();
            this.Controls.Add(comboBoxLeftClick);

            // Right Click ComboBox Initialization
            comboBoxRightClick = new ComboBox
            {
                Location = new Point(120, 140),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateComboBoxWithButtons(comboBoxRightClick);
            comboBoxRightClick.SelectedItem = Program.RightClickAssignment; // Reflects new default
            comboBoxRightClick.SelectedValueChanged += (sender, e) =>
                Program.RightClickAssignment = comboBoxRightClick.SelectedItem.ToString();
            this.Controls.Add(comboBoxRightClick);

            // Middle Click ComboBox Initialization
            comboBoxMiddleClick = new ComboBox
            {
                Location = new Point(230, 140), // Adjust as necessary
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateComboBoxWithButtons(comboBoxMiddleClick);
            comboBoxMiddleClick.SelectedItem = Program.MiddleClickAssignment; // Set the ComboBox to show "Y" as selected
            comboBoxMiddleClick.SelectedValueChanged += (sender, e) =>
            {
                Program.MiddleClickAssignment = comboBoxMiddleClick.SelectedItem.ToString();
            };
            this.Controls.Add(comboBoxMiddleClick);
        }

        private void PopulateComboBoxWithButtons(ComboBox comboBox)
        {
            comboBox.Items.AddRange(new object[] { "None", "A", "B", "X", "Y", "LB", "RB", "LT", "RT", "Start", "Back" });
            comboBox.SelectedIndex = 0; // Default to "None"
        }

        private void TrackBarDeadzone_ValueChanged(object sender, EventArgs e)
        {
            Program.DEADZONE = trackBarDeadzone.Value;
            labelDeadzoneValue.Text = $"Deadzone: {Program.DEADZONE}";
        }

        public void UpdateThumbstickLabels(int x, int y)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int>(UpdateThumbstickLabels), new object[] { x, y });
                return;
            }
            labelThumbstickX.Text = $"X: {x}";
            labelThumbstickY.Text = $"Y: {y}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeInstructionTextBox();
            InitializeScrollMultControls();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

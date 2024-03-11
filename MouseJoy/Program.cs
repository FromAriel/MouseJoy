using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpDX.XInput;
using System.Windows.Forms;

namespace MouseJoy
{
    static class Program
    {
        // Define all static variables at the class level
        public static volatile int DEADZONE;
        public static volatile string LeftClickAssignment = "X"; // Default for left click
        public static volatile string RightClickAssignment = "A"; // Default for right click
        public static volatile string MiddleClickAssignment = "Y"; //Defualt for Middle click
        private static Controller controller;
        static bool isControlEnabled = true;
        private static Form1 mainForm;
        private static int baseMouseSpeed = 20;
        private static ButtonStateTracker leftClickTracker = new ButtonStateTracker(LeftClickAssignment);
        private static ButtonStateTracker rightClickTracker = new ButtonStateTracker(RightClickAssignment);
        private static ButtonStateTracker middleClickTracker = new ButtonStateTracker(MiddleClickAssignment);
        // Define previous assignment tracking variables
        private static string prevLeftClickAssignment = LeftClickAssignment;
        private static string prevRightClickAssignment = RightClickAssignment;
        private static string prevMiddleClickAssignment = MiddleClickAssignment;


        [STAThread]

        static void CheckAndUpdateAssignments()
        {
            if (LeftClickAssignment != prevLeftClickAssignment)
            {
                leftClickTracker.UpdateAssignment(LeftClickAssignment);
                prevLeftClickAssignment = LeftClickAssignment;
            }
            if (RightClickAssignment != prevRightClickAssignment)
            {
                rightClickTracker.UpdateAssignment(RightClickAssignment);
                prevRightClickAssignment = RightClickAssignment;
            }
            if (MiddleClickAssignment != prevMiddleClickAssignment)
            {
                middleClickTracker.UpdateAssignment(MiddleClickAssignment);
                prevMiddleClickAssignment = MiddleClickAssignment;
            }
        }

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            controller = new Controller(UserIndex.One);

            // Update DEADZONE based on initial controller input
            UpdateDeadzoneBasedOnInitialInput(controller);

            mainForm = new Form1();

            Task.Run(() => ControllerPolling());

            Application.Run(mainForm);
        }

        static void ControllerPolling()
        {
            while (true)
            {
                if (controller.IsConnected)
                {
                    var state = controller.GetState();
                    var gamepad = state.Gamepad;

                    // Toggle control when Start and Back are pressed together
                    if (gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) && gamepad.Buttons.HasFlag(GamepadButtonFlags.Back))
                    {
                        // Debounce the button press to prevent rapid toggling
                        System.Threading.Thread.Sleep(500); // Adjust the sleep time as needed
                        isControlEnabled = !isControlEnabled;
                    }

                    if (isControlEnabled)
                    {
                        // Now wrap all the input handling code with this condition
                        CheckAndUpdateAssignments();

                        leftClickTracker.Update(gamepad);
                        rightClickTracker.Update(gamepad);
                        middleClickTracker.Update(gamepad);

                        PerformMouseActions(leftClickTracker, MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP);
                        PerformMouseActions(rightClickTracker, MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP);
                        PerformMouseActions(middleClickTracker, MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP);

                        MoveMouseBasedOnThumbstick(gamepad.LeftThumbX, gamepad.LeftThumbY, gamepad.LeftTrigger, gamepad.RightTrigger);

                        HandleDPad(gamepad.Buttons);
                        HandleSpecialButtons(gamepad.Buttons);
                    }

                    System.Threading.Thread.Sleep(20); // Sleep to reduce polling rate
                }
            }
        }


        static void UpdateDeadzoneBasedOnInitialInput(Controller controller)
        {
            if (!controller.IsConnected) return;

            var state = controller.GetState();
            int initialLeftThumbX = Math.Abs(state.Gamepad.LeftThumbX);
            int initialLeftThumbY = Math.Abs(state.Gamepad.LeftThumbY);
            int initialRightThumbX = Math.Abs(state.Gamepad.RightThumbX);
            int initialRightThumbY = Math.Abs(state.Gamepad.RightThumbY);

            // Find the maximum initial value across all axes
            int maxInitialValue = Math.Max(Math.Max(initialLeftThumbX, initialLeftThumbY), Math.Max(initialRightThumbX, initialRightThumbY));

            // Set DEADZONE slightly above the maximum initial value to account for stick drift, with a minimum threshold
            int minimumThreshold = 5000; // Minimum DEADZONE value to ensure minor drift is ignored
            DEADZONE = Math.Max(maxInitialValue + 1000, minimumThreshold); // Increase by 1000 to provide buffer or use minimumThreshold
        }



        static void MoveMouseBasedOnThumbstick(float thumbstickX, float thumbstickY, float leftTrigger, float rightTrigger)
        {
            if (Math.Abs(thumbstickX) < DEADZONE && Math.Abs(thumbstickY) < DEADZONE) return;

            // Normalize thumbstick values to a range [-1, 1]
            float normalizedX = thumbstickX / 32768f; // Use 32768f for precise division
            float normalizedY = thumbstickY / 32768f;

            // Adjust mouse speed based on trigger values
            // Left trigger speeds up, right trigger slows down
            // Normalize trigger values from [0, 255] to [0, 1] for calculation
            float speedMultiplier = 1 + (leftTrigger / 255f) - (rightTrigger / 255f);
            speedMultiplier = Math.Max(0.5f, Math.Min(2f, speedMultiplier)); // Ensure multiplier is between 0.5 and 2

            int mouseX = (int)(normalizedX * baseMouseSpeed * speedMultiplier);
            int mouseY = (int)(normalizedY * baseMouseSpeed * speedMultiplier);

            // Apply the calculated movement to the cursor position
            Cursor.Position = new System.Drawing.Point(Cursor.Position.X + mouseX, Cursor.Position.Y - mouseY);
        }



        static void PerformMouseActions(ButtonStateTracker tracker, uint mouseDown, uint mouseUp)
        {
            if (tracker.WasJustPressed)
            {
                mouse_event(mouseDown, 0, 0, 0, 0);
            }
            else if (tracker.WasJustReleased)
            {
                mouse_event(mouseUp, 0, 0, 0, 0);
            }
        }

        static void HandleDPad(GamepadButtonFlags buttons)
        {
            if (buttons.HasFlag(GamepadButtonFlags.DPadUp))
            {
                SendKeyPress(VK_UP);
            }
            if (buttons.HasFlag(GamepadButtonFlags.DPadDown))
            {
                SendKeyPress(VK_DOWN);
            }
            if (buttons.HasFlag(GamepadButtonFlags.DPadLeft))
            {
                SendKeyPress(VK_LEFT);
            }
            if (buttons.HasFlag(GamepadButtonFlags.DPadRight))
            {
                SendKeyPress(VK_RIGHT);
            }
        }

        static void HandleSpecialButtons(GamepadButtonFlags buttons)
        {
            if (buttons.HasFlag(GamepadButtonFlags.Start))
            {
                SendKeyPress(VK_RETURN);
            }
            if (buttons.HasFlag(GamepadButtonFlags.Back))
            {
                SendKeyPress(VK_ESCAPE);
            }
            // Xbox button handling could be here if accessible
        }

        static void SendKeyPress(byte vkCode)
        {
            keybd_event(vkCode, 0, 0, 0); // Key down
            keybd_event(vkCode, 0, KEYEVENTF_KEYUP, 0); // Key up
        }

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const byte VK_UP = 0x26;
        private const byte VK_DOWN = 0x28;
        private const byte VK_LEFT = 0x25;
        private const byte VK_RIGHT = 0x27;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_ESCAPE = 0x1B;
        private const uint KEYEVENTF_KEYUP = 0x0002;
    }

    class ButtonStateTracker
    {
        public bool IsPressed { get; private set; }
        public bool WasJustPressed { get; private set; }
        public bool WasJustReleased { get; private set; }
        private string buttonAssignment;

        public ButtonStateTracker(string assignedButton)
        {
            buttonAssignment = assignedButton;
        }

        public void UpdateAssignment(string newAssignment)
        {
            if (buttonAssignment != newAssignment)
            {
                buttonAssignment = newAssignment;
            }
        }

        public void Update(Gamepad gamepad)
        {
            var buttonFlag = GetButtonFlagFromString(buttonAssignment);
            bool currentlyPressed = gamepad.Buttons.HasFlag(buttonFlag);
            WasJustPressed = !IsPressed && currentlyPressed;
            WasJustReleased = IsPressed && !currentlyPressed;
            IsPressed = currentlyPressed;
        }






        private GamepadButtonFlags GetButtonFlagFromString(string buttonName)
        {
            return buttonName switch
            {
                "A" => GamepadButtonFlags.A,
                "B" => GamepadButtonFlags.B,
                "X" => GamepadButtonFlags.X,
                "Y" => GamepadButtonFlags.Y,
                // Add other mappings as necessary
                _ => GamepadButtonFlags.None,
            };
        }
    }

}

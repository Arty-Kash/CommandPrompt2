using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CommandPrompt
{
    public partial class MainPage : ContentPage
    {

        class CommandLine        
        {
            // Prompt
            public string Prompt { get; set; }
            private Label promptlabel()
            {
                return new Label() { Text = Prompt,
                                     TextColor = Color.Blue };
            }
            public Label PromptLabel { get { return this.promptlabel(); } }


            // Command            
            public string Command { get { return CommandEntry.Text; } }
            private Label commandlabel = new Label() { Text = " " };
            public Label CommandLabel { get { return this.commandlabel; } }


            // Command Input
            private Entry commandentry = new Entry()
            {
                BackgroundColor = Color.Transparent,
                TextColor = Color.Transparent,
                HeightRequest = 0,      // Not to display this entry                
                Keyboard = Keyboard.Plain
            };
            public Entry CommandEntry { get { return this.commandentry; } }


            // StackLayout including all parts of Command Line
            private StackLayout promptcommand()
            {
                StackLayout s1 = new StackLayout()
                {
                    BackgroundColor = Color.Transparent,
                    Orientation = StackOrientation.Horizontal,
                    Children = { PromptLabel, CommandLabel }
                };
                StackLayout s2 = new StackLayout()
                {
                    Spacing = 0,
                    Children = { s1, CommandEntry }
                };

                return s2;
            }
            public StackLayout PromptCommand { get { return promptcommand(); } }


            // Blink Cursor and its Switch            
            public string BlinkString = "|";
            public bool BlinkOff { get; set; }


            // Solution Results
            public string Results { get; set; }
        }
        List<CommandLine> CommandLines = new List<CommandLine>();


        int ConsoleSizeChanged = 0;
        int TextInputed = 0;
        int TextInputCompleted = 0;

        double ConsoleWidth, ConsoleHeight;
        double ScrollWidth, ScrollHeight;



        //Get "Console" size properly and Scroll "Console" to the end        
        void GetConsoleSize(object sender, EventArgs args)
        {
            ConsoleWidth = Console.Width;
            ConsoleHeight = Console.Height;
            ScrollHeight = ConsoleBack.Height;

            // Scroll "Console" to the End
            // Make ScrollToAsync into Timer due to timing probelm(?)
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                ConsoleBack.ScrollToAsync(0, ConsoleHeight - ScrollHeight, false);
                return false;   // Timer Cycle is only one time
            });

        }



        // Add a new Command Input Line
        void AddPrompt(string prompt)
        {
            CommandLine commandLine = new CommandLine();

            commandLine.Prompt = prompt;
            Label comLabel = commandLine.CommandLabel;
            Entry comEntry = commandLine.CommandEntry;

            Console.Children.Add(commandLine.PromptCommand);
            CommandLines.Add(commandLine);


            // Add TextChanged & Completed Event on entry
            comEntry.TextChanged += InputChanged;
            comEntry.Completed += InputCompleted;

            comEntry.Focus();


            // Blink Cursor in Command Line
            Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
            {
                // If Text Inputed in Command Line, finish this Timer                
                bool NextTimerStart = !commandLine.BlinkOff;
                if (!NextTimerStart)
                {
                    // Not to let label be "|             "
                    if (comEntry.Text == null) comLabel.Text = "";

                    // Delete BlinkString
                    comLabel.Text = comEntry.Text;

                    return NextTimerStart;
                }

                // Toggle Cursor to Blink                
                if (commandLine.BlinkString == "|") commandLine.BlinkString = "";
                else commandLine.BlinkString = "|";

                comLabel.Text = comEntry.Text + commandLine.BlinkString;

                return NextTimerStart;
            });
        }        

        
        // Display the Input Command (entry.Text) as Label
        void InputChanged(object sender, EventArgs args)
        {
            Entry entry = (Entry)sender;

            CommandLines.Last().CommandLabel.Text = entry.Text;

            TextInputed++;
        }


        // When CR is entered, Execute the Input Command
        void InputCompleted(object sender, EventArgs args)
        {
            Entry entry = (Entry)sender;            

            CommandLines.Last().BlinkOff = true;

            CommandExecute(entry.Text);
            entry.IsEnabled = false;    // Not to focus the previous entry

            TextInputCompleted++;
        }


        void ConsoleBackScrolled(object sender, EventArgs args)
        {
            //Label3.Text = string.Format("Scroll Y = {0}, Scrolled Y = {1}",
            //                            ConsoleHeight - ScrollHeight,
            //                            ConsoleBack.ScrollY);
        }

    }
}

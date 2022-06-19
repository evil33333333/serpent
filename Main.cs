using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Serpent
{
    public partial class Main : Form
    {
        public static bool busy = false;
        bool mouseDown;
        private Point offset;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool MessageBeep(int uType);
        public Main()
        {
            InitializeComponent();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            offset.X = e.X;
            offset.Y = e.Y;
            mouseDown = true;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point position = PointToScreen(e.Location);
                Location = new Point(position.X - offset.X, position.Y - offset.Y);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (busy)
            {
                MessageBeep((int)0x00000010L);
                return;
            }
            busy = true;
            
            
            Task.Factory.StartNew(() =>
            {
                while (busy)
                {
                    foreach (var element in new Dictionary<String, String> { { "\\", "" }, { "|", "." }, { "/", ".." }, { "-", "..." }, })
                    {
                        button3.Text = $"[{element.Key}] Please wait{element.Value}";
                        Thread.Sleep(TimeSpan.FromMilliseconds(200));
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(60));
                }
                button3.Text = "Search for Close";
            });
            Task.Factory.StartNew(() =>
            {
                string account = textBox1.Text;
                if (!Instagram.IsSession(account))
                {
                    string[] branches = account.Split(':');
                    account = Instagram.LoginWithUsernameAndPassword(branches[0], branches[1]); 
                    
                }
                listBox1.Items.Clear();

                if (account == null)
                {
                    MessageBeep((int)0x00000010L);
                }
                else
                {
                    List<String> bannedUsers = Instagram.GetBannedUsers(account);
                    if (bannedUsers != null)
                    {
                        foreach (String userResponse in bannedUsers)
                        {
                            listBox1.Items.Add(userResponse);
                        }
                    }
                    MessageBeep(0);
                }
                busy = false;
                
            });


        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
    }
}

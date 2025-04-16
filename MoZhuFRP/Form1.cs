using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Text;
namespace MoZhuFRP
{
    public partial class 墨竹FRP : Form
    {
        private Process frpProcess;
        private string port = "";
        private const string configPATH = @".\frpc\frpc.toml";
        private bool isChecked = false;
        private string protocol = "tcp";
        private const string runFRP = @".\frpc\frpc.exe -c .\frpc\frpc.toml";//反斜杠需要转义字符
        private string configSimple = "";

        public int generateRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(0,999); 
            return randomNumber;
        }
        public 墨竹FRP()
        {
            InitializeComponent();
        }
        public void createConfigFile(string port)
        {
            int number = generateRandomNumber();
            this.configSimple = $"""
                serverAddr = "120.27.158.236"
                serverPort = 7000

                [[proxies]]
                name = "{number}"
                type = "{this.protocol}"
                localIP = "127.0.0.1"
                localPort = {this.port}
                remotePort = {this.port}
                
                """;
                using (FileStream fs = new FileStream(configPATH, FileMode.Create, FileAccess.Write))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(configSimple);
                    fs.Write(info, 0, info.Length);
                }
         
        }


        // 执行命令并等待完成


        private async Task ExecuteCommand(string command, bool cmdView)
        {
            if (port == "")
            {
                MessageBox.Show("没有输入端口号!:(");
                return;
            }
            createConfigFile(port);

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = @".\frpc\frpc.exe",
                    Arguments = "-c .\\frpc\\frpc.toml",
                    UseShellExecute = cmdView,
                    CreateNoWindow = !cmdView,
                    RedirectStandardOutput = !cmdView,
                    RedirectStandardError = !cmdView
                };

                using (frpProcess = new Process())
                {
                    frpProcess.StartInfo = startInfo;
                    frpProcess.EnableRaisingEvents = true;

                    // 创建一个 TaskCompletionSource 来等待进程退出
                    var tcs = new TaskCompletionSource<bool>();
                    frpProcess.Exited += (s, e) => tcs.TrySetResult(true);

                    // 读取输出流和错误流
                    frpProcess.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            // 在这里检查输出的内容
                            if (e.Data.Contains("port already used"))
                            {
                                MessageBox.Show("端口已占用，请换一个端口吧~");
                                return;
                            }
                            else if(e.Data.Contains("start error"))
                            {
                                MessageBox.Show("出问题了，详情请打开控制台输出查看~");
                                return;
                            }
                            else if (e.Data.Contains("error"))
                            {
                                MessageBox.Show("出问题了，详情请打开控制台输出查看~");
                                return;
                            }
                        }
                    };

                    frpProcess.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            Console.WriteLine("frpc Error: " + e.Data);  // 打印错误输出
                        }
                    };

                    frpProcess.Start();

                    if (!cmdView)
                    {
                        // 开始异步读取输出和错误流
                        frpProcess.BeginOutputReadLine();
                        frpProcess.BeginErrorReadLine();
                    }


                    MessageBox.Show($"内网穿透启动成功(大概率成功了,如果不好用请打开控制台输出检查错误，是否有端口占用等问题)！使用地址120.27.158.236:{port}访问（IP可替换为zhuzimiko.com）,已经帮你复制到了粘贴板了~");
                    Clipboard.SetText("120.27.158.236:" + port);
                    await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}");
            }
            finally
            {
                frpProcess = null; // 确保释放引用
            }
        }

        //程序关闭时结束后台frp
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (frpProcess != null && !frpProcess.HasExited)
            {
                try
                {
                    frpProcess.Kill();
                    frpProcess.WaitForExit(1000); // 等待1秒确保终止
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"终止进程时出错: {ex.Message}");
                }
            }
            base.OnFormClosing(e);
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
         
        }

        private void button1_Click(object sender, EventArgs e)
        {
            port = textBox1.Text;
            ExecuteCommand(runFRP, isChecked);

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = comboBox1.FindString("TCP");
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.isChecked = checkBox1.Checked;


        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedProtocol = comboBox1.SelectedItem?.ToString();

            // 空值检查
            if (!string.IsNullOrEmpty(selectedProtocol))
            {
                switch (selectedProtocol)
                {
                    case "TCP":
                        // 执行TCP相关操作
                        this.protocol = "tcp";
                        break;
                    case "UDP":
                        // 执行UDP相关操作
                        this.protocol = "udp";
                        break;
                }
            }
        }
    }
}

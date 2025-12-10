using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using EchWorkersManager.Models;
using EchWorkersManager.Services;
using EchWorkersManager.Routing;
using EchWorkersManager.Helpers;
using EchWorkersManager.UI;

namespace EchWorkersManager.Forms
{
    public partial class MainForm : Form
    {
        // 修复 "语法错误，应输入 ','"
        // 将所有字段初始化为 null，以适应旧版 C# 编译器，同时满足字段初始化要求。
        // 如果您的 C# 版本支持可空引用类型 (C# 8.0+)，可以改为 private WorkerService? workerService = null;
        private WorkerService workerService = null!;
        private HttpProxyService httpProxyService = null!;
        private SystemProxyService systemProxyService = null!;
        private RoutingManager routingManager = null!;
        private TrayIconManager trayIconManager = null!;
        
        private ProxyConfig config = null!;
        private string echWorkersPath = null!;

        // ====================== TUN 模块新增 START ======================
        private TunService tunService = null!;
        private TunRoutingService tunRoutingService = null!;
        // ====================== TUN 模块新增 END ======================

        public MainForm()
        {
            InitializeServices();
            InitializeComponent();
            InitializeTrayIcon();
            LoadConfiguration();
        }

        private void InitializeServices()
        {
            try
            {
                echWorkersPath = ResourceHelper.ExtractEchWorkers();
                workerService = new WorkerService(echWorkersPath);
                routingManager = new RoutingManager();
                httpProxyService = new HttpProxyService(routingManager);
                systemProxyService = new SystemProxyService();
                config = new ProxyConfig();

                // 修复构造函数参数类型不匹配，传入 TunService, WorkerService, RoutingManager
                tunService = new TunService();
                tunRoutingService = new TunRoutingService(tunService, workerService, routingManager);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeTrayIcon()
        {
            // 在 MainForm 构造函数退出前，trayIconManager 必然被初始化
            trayIconManager = new TrayIconManager(
                this,
                ShowMainWindow,
                BtnStart_Click,
                BtnStop_Click
            );
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            trayIconManager.Hide();
        }

        private void LoadConfiguration()
        {
            config = SettingsHelper.Load();
            
            // 使用 '!' 忽略空值警告，因为我们确定这些控件在 CreateControls() 后存在。
            ((TextBox)this.Controls["txtDomain"]!).Text = config.Domain;
            ((TextBox)this.Controls["txtIP"]!).Text = config.IP;
            ((TextBox)this.Controls["txtToken"]!).Text = config.Token;
            ((TextBox)this.Controls["txtLocal"]!).Text = config.LocalAddress;
            ((TextBox)this.Controls["txtHttpPort"]!).Text = config.HttpProxyPort.ToString();
            
            ComboBox cmbRouting = (ComboBox)this.Controls["cmbRouting"]!;
            int index = cmbRouting.Items.IndexOf(config.RoutingMode);
            if (index >= 0)
            {
                cmbRouting.SelectedIndex = index;
            }
            
            routingManager.SetRoutingMode(config.RoutingMode);
            
            // 加载 TUN 模式状态
            CheckBox chkTun = (CheckBox)this.Controls["chkTun"]!;
            if (chkTun != null) // 尽管有 '!'，但保留检查以提高健壮性
            {
                chkTun.Checked = config.TunEnabled;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.ClientSize = new Size(500, 480);
            this.Text = "ECH Workers Manager";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateControls();

            // 修正委托的 sender 参数可空性警告
            this.Resize += Form1_Resize;
            this.FormClosing += Form1_FormClosing;
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Domain
            AddLabel("域名:", 20, 20);
            AddTextBox("txtDomain", 130, 20, 340, "ech.sjwayrhz9.workers.dev:443");

            // IP
            AddLabel("IP:", 20, 60);
            AddTextBox("txtIP", 130, 60, 340, "saas.sin.fan");

            // Token
            AddLabel("Token:", 20, 100);
            AddTextBox("txtToken", 130, 100, 340, "miy8TMEisePcHp$K");

            // Local SOCKS5
            AddLabel("本地SOCKS5:", 20, 140);
            AddTextBox("txtLocal", 130, 140, 340, "127.0.0.1:30000");

            // HTTP Proxy Port
            AddLabel("HTTP代理端口:", 20, 170);
            AddTextBox("txtHttpPort", 130, 170, 340, "10809");

            // Routing Mode
            AddLabel("路由模式:", 20, 200);
            ComboBox cmbRouting = new ComboBox();
            cmbRouting.Name = "cmbRouting";
            cmbRouting.Location = new Point(130, 200);
            cmbRouting.Size = new Size(340, 20);
            cmbRouting.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRouting.Items.AddRange(new string[] { "全局模式", "绕过大陆", "直连模式" });
            cmbRouting.SelectedIndex = 1;
            cmbRouting.SelectedIndexChanged += (s, e) => {
                // 忽略 s 参数的空值警告
                ComboBox? senderComboBox = s as ComboBox;
                if(senderComboBox != null && senderComboBox.SelectedItem != null)
                {
                    routingManager.SetRoutingMode(senderComboBox.SelectedItem.ToString()!);
                }
            };
            this.Controls.Add(cmbRouting);

            // TUN 模式开关
            CheckBox chkTun = new CheckBox();
            chkTun.Name = "chkTun";
            chkTun.Text = "启用 TUN 模式 (全系统流量接管)";
            chkTun.Location = new Point(20, 230);
            chkTun.Size = new Size(300, 20);
            chkTun.Checked = false;
            this.Controls.Add(chkTun);

            // Buttons (Y 坐标向下调整)
            Button btnStart = new Button();
            btnStart.Name = "btnStart";
            btnStart.Text = "启动服务";
            btnStart.Location = new Point(130, 260); 
            btnStart.Size = new Size(120, 40);
            btnStart.Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold);
            btnStart.BackColor = Color.LightGreen;
            btnStart.Click += (s, e) => BtnStart_Click();
            this.Controls.Add(btnStart);

            Button btnStop = new Button();
            btnStop.Name = "btnStop";
            btnStop.Text = "停止服务";
            btnStop.Location = new Point(270, 260);
            btnStop.Size = new Size(120, 40);
            btnStop.Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold);
            btnStop.BackColor = Color.LightCoral;
            btnStop.Enabled = false;
            btnStop.Click += (s, e) => BtnStop_Click();
            this.Controls.Add(btnStop);

            Button btnSave = new Button();
            btnSave.Text = "保存配置";
            btnSave.Location = new Point(400, 260);
            btnSave.Size = new Size(70, 40);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Status Label (Y 坐标向下调整)
            Label lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "状态: 未运行\nHTTP代理: 未启动\n系统代理: 未启用\n路由模式: 绕过大陆";
            lblStatus.Location = new Point(20, 320);
            lblStatus.Size = new Size(450, 100);
            lblStatus.ForeColor = Color.Blue;
            lblStatus.Font = new Font("Microsoft YaHei", 9F);
            this.Controls.Add(lblStatus);

            // Info Label (Y 坐标向下调整)
            Label lblInfo = new Label();
            lblInfo.Text = "💡 全局模式：代理所有(除内网)\n💡 绕过大陆：仅代理境外IP(除内网)\n💡 直连模式：不使用代理";
            lblInfo.Location = new Point(20, 420);
            lblInfo.Size = new Size(450, 60);
            lblInfo.ForeColor = Color.Green;
            lblInfo.Font = new Font("Microsoft YaHei", 8.5F);
            this.Controls.Add(lblInfo);
        }

        private void AddLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new Size(100, 20);
            this.Controls.Add(label);
        }

        private void AddTextBox(string name, int x, int y, int width, string defaultText)
        {
            TextBox textBox = new TextBox();
            textBox.Name = name;
            textBox.Location = new Point(x, y);
            textBox.Size = new Size(width, 20);
            textBox.Text = defaultText;
            this.Controls.Add(textBox);
        }

        private void BtnStart_Click()
        {
            try
            {
                UpdateConfigFromUI(); // 确保 config.TunEnabled 已更新

                workerService.Start(config);
                Thread.Sleep(1000);

                httpProxyService.Start(config);
                
                // 只有在非直连模式下且 TUN 未启用时，才设置系统代理
                if (config.RoutingMode != "直连模式" && !config.TunEnabled)
                {
                    systemProxyService.Enable(config.HttpProxyPort);
                }
                else if (config.TunEnabled)
                {
                    // 确保如果 TUN 启用，系统代理是关闭的，避免冲突
                    systemProxyService.Disable(); 
                }

                // ====================== TUN 模块启动逻辑 START ======================
                string tunStatus = "未启用";
                if (config.TunEnabled) // 只有当用户启用 TUN 时才启动服务和路由
                {
                    tunService.Start();
                    tunRoutingService.StartRouting(config); 
                    tunStatus = "已启动";
                }
                // ====================== TUN 模块启动逻辑 END ======================

                ((Button)this.Controls["btnStart"]!).Enabled = false;
                ((Button)this.Controls["btnStop"]!).Enabled = true;
                trayIconManager.UpdateMenuState(true);
                
                string proxyStatus;
                if (config.TunEnabled)
                {
                    proxyStatus = "已接管 (TUN)";
                }
                else if (config.RoutingMode != "直连模式")
                {
                    proxyStatus = $"已启用 (HTTP:{config.HttpProxyPort})";
                }
                else
                {
                    proxyStatus = "未启用 (直连)";
                }
                
                // 更新状态标签
                UpdateStatusLabel($"✅ 状态: 运行中\n✅ HTTP代理: 127.0.0.1:{config.HttpProxyPort}\n✅ 系统代理: {proxyStatus}\n🌐 TUN: {tunStatus}\n✅ 路由模式: {config.RoutingMode}");
                trayIconManager.UpdateText($"ECH Workers Manager - 运行中 ({config.RoutingMode})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStop_Click()
        {
            try
            {
                systemProxyService.Disable();
                httpProxyService.Stop();
                workerService.Stop();

                // ====================== TUN 模块停止逻辑 START ======================
                // 只有在运行时启用了 TUN 才需要停止
                if (config.TunEnabled) 
                {
                    tunRoutingService.StopRouting();
                    tunService.Stop();
                }
                // ====================== TUN 模块停止逻辑 END ======================

                ((Button)this.Controls["btnStart"]!).Enabled = true;
                ((Button)this.Controls["btnStop"]!).Enabled = false;
                trayIconManager.UpdateMenuState(false);
                
                UpdateStatusLabel("❌ 状态: 已停止\n❌ HTTP代理: 已停止\n❌ 系统代理: 已禁用\n❌ TUN: 已停止");
                trayIconManager.UpdateText("ECH Workers Manager - 已停止");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 修正 sender 参数可空性警告
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            UpdateConfigFromUI();
            SettingsHelper.Save(config);
            MessageBox.Show("配置已保存!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateConfigFromUI()
        {
            // 使用 '!' 忽略空值警告
            config.Domain = ((TextBox)this.Controls["txtDomain"]!).Text;
            config.IP = ((TextBox)this.Controls["txtIP"]!).Text;
            config.Token = ((TextBox)this.Controls["txtToken"]!).Text;
            config.LocalAddress = ((TextBox)this.Controls["txtLocal"]!).Text;
            config.HttpProxyPort = int.Parse(((TextBox)this.Controls["txtHttpPort"]!).Text);
            
            ComboBox cmbRouting = (ComboBox)this.Controls["cmbRouting"]!;
            config.RoutingMode = cmbRouting.SelectedItem!.ToString();
            
            // 保存 TUN 模式状态
            CheckBox chkTun = (CheckBox)this.Controls["chkTun"]!;
            config.TunEnabled = chkTun.Checked;

            routingManager.SetRoutingMode(config.RoutingMode);
        }

        private void UpdateStatusLabel(string text)
        {
            // 使用 '!' 忽略空值警告
            Label lblStatus = (Label)this.Controls["lblStatus"]!;
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = text));
            }
            else
            {
                lblStatus.Text = text;
            }
        }

        // 修正 sender 参数可空性警告
        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
                trayIconManager.Show();
                trayIconManager.ShowBalloonTip(1000, "ECH Workers Manager", "程序已最小化到系统托盘", ToolTipIcon.Info);
            }
        }

        // 修正 sender 参数可空性警告
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (workerService.IsRunning)
            {
                BtnStop_Click();
            }
            trayIconManager.Dispose();
        }
    }
}
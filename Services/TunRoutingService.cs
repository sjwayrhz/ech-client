using EchWorkersManager.Services;
using EchWorkersManager.Routing; // 引入 RoutingManager 所在的命名空间
using EchWorkersManager.Models; // 引入 ProxyConfig 所在的命名空间

namespace EchWorkersManager.Services
{
    public class TunRoutingService
    {
        private readonly TunService tun;
        private readonly WorkerService worker;
        private readonly RoutingManager routingManager; 
        private bool running;

        // 修复 1: 构造函数接受 TunService, WorkerService, 和 RoutingManager 三个服务
        public TunRoutingService(TunService tun, WorkerService worker, RoutingManager routingManager)
        {
            this.tun = tun;
            this.worker = worker;
            this.routingManager = routingManager; 
        }

        // 修复 2: StartRouting 现在接受 ProxyConfig config 参数
        public void StartRouting(ProxyConfig config)
        {
            if (running) return;
            running = true;
            
            // 注意: tun.Start() 已经在 MainForm.cs 的 BtnStart_Click() 中调用，
            // 这里主要负责设置 TUN 接口上的路由规则和数据转发逻辑。
            
            // TODO: 
            // 1. 使用 config 配置 TUN 路由（如网关、掩码、DNS等）。
            // 2. 根据 config.RoutingMode 和 routingManager 的规则，
            //    开始读取 TUN 接口流量，并将其转发给 worker 服务（SOCKS5）。
            
            // 示例：可以调用 worker.SetTunMode(true, config);
        }

        public void StopRouting()
        {
            if (!running) return;
            running = false;
            
            // 注意: tun.Stop() 已经在 MainForm.cs 的 BtnStop_Click() 中调用，
            // 这里主要负责清理路由规则和停止数据转发循环。
            
            // TODO: 清理 TUN 接口的路由规则。
        }
    }
}
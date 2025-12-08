# ECH Workers Manager 构建说明

## 项目结构

创建以下文件结构:

```
EchWorkersManager/
├── EchWorkersManager.csproj
├── Program.cs
├── Form1.cs
├── ech-workers.exe          ← 放在项目根目录!
├── app.ico                  ← 图标文件(可选)
└── README.md
```

**重要**:

- 将 `ech-workers.exe` 放在项目根目录(与 .csproj 文件同级),编译时会自动嵌入到最终的 exe 文件中
- 将 `app.ico` 图标文件放在项目根目录,编译后会显示在:
  - Windows 任务栏
  - 系统托盘
  - exe 文件图标
  - 窗口标题栏

## 构建步骤

### 方法1: 使用 Visual Studio 2019/2022

1. 打开 Visual Studio
2. 选择 "创建新项目"
3. 选择 "Windows 窗体应用(.NET)"
4. 项目名称输入: `EchWorkersManager`
5. 创建项目后,将提供的三个文件内容替换到对应文件中:
   - `Form1.cs` (替换整个文件)
   - `Program.cs` (替换整个文件)
   - `EchWorkersManager.csproj` (替换整个文件)
6. 按 `Ctrl+Shift+B` 或点击 "生成" -> "生成解决方案"
7. 编译后的 exe 在 `bin\Debug\net6.0-windows\` 或 `bin\Release\net6.0-windows\` 目录

### 方法2: 使用命令行编译 (需要安装 .NET 6 SDK)

1. 下载并安装 [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

2. 创建项目文件夹并保存文件:

```bash
mkdir EchWorkersManager
cd EchWorkersManager
```

3. 将三个代码文件保存到该文件夹

4. 编译项目:

```bash
dotnet build -c Release
```

5. 编译后的 exe 在: `bin\Release\net6.0-windows\EchWorkersManager.exe`

6. 或者直接运行:

```bash
dotnet run
```

### 方法3: 发布独立应用 (不需要安装 .NET 运行时) ⭐推荐⭐

这是**推荐的打包方法**,生成的 exe 文件可以在没有安装 .NET 6 的电脑上直接运行!

#### Windows x64 (64位系统)

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

#### Windows x86 (32位系统)

```bash
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

发布后的**单文件 exe** 在: `bin\Release\net6.0-windows\win-x64\publish\EchWorkersManager.exe`

**特点**:

- ✅ 单个 exe 文件,包含所有依赖
- ✅ 不需要目标电脑安装 .NET 6
- ✅ ech-workers.exe 已嵌入,无需额外文件
- ✅ 直接分发给用户即可使用
- ⚠️ 文件体积较大(约 60-80MB)

#### 减小文件体积(可选)

如果想减小 exe 文件大小,可以添加以下参数:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true
```

这样可以将文件大小压缩到 30-40MB 左右。

## 使用说明

### 快速开始(2步即可!)

1. **启动服务**: 点击"启动服务" → 自动启动代理并启用系统代理
2. **开始上网**: 打开任何浏览器即可使用代理 ✅

### 停止使用

- **停止服务**: 点击"停止服务" → 自动停止代理并禁用系统代理
- **关闭程序**: 直接关闭窗口 → 自动清理所有代理设置

### 详细说明

1. **配置参数**: 填写域名、IP、Token、本地SOCKS5地址
2. **HTTP代理端口**: 默认10809(可修改)
3. **保存配置**: 点击"保存配置"保存设置(下次自动加载)
4. **一键操作**: 启动/停止按钮自动处理所有代理设置,无需手动操作

## 🎯 核心功能说明

### 自动 SOCKS5 → HTTP 代理转换

程序**内置了代理转换器**,工作流程如下:

```
浏览器 → HTTP代理(127.0.0.1:10809) → SOCKS5代理(127.0.0.1:30000) → ech-workers → 目标网站
```

### 一键启动,自动配置

1. 点击"启动服务"按钮
2. ✅ **自动启动代理 + 自动启用系统代理**
3. ✅ **所有浏览器(Chrome/Firefox/Edge)立即生效!**
4. 点击"停止服务"或关闭程序时,自动清理所有代理设置

**无需安装任何插件!** 就像使用 v2rayN 一样简单!

## 注意事项

1. **ech-workers.exe 位置**:
   - 开发时:将 `ech-workers.exe` 放在项目根目录(与 .csproj 同级)
   - 编译后会自动嵌入到 EchWorkersManager.exe 中
   - 运行时会自动提取到临时目录

2. **图标文件 app.ico**:
   - 放在项目根目录(与 .csproj 同级)
   - 必须是 .ico 格式(不是 .png 或 .jpg)
   - 推荐包含多个尺寸: 16x16, 32x32, 48x48, 256x256
   - 在线转换工具: <https://convertio.co/zh/png-ico/>
   - 如果没有图标文件,会使用系统默认图标

3. **系统托盘**:
   - 点击最小化按钮,程序会隐藏到系统托盘
   - 双击托盘图标或右键菜单"显示主窗口"恢复
   - 右键托盘图标可快速启动/停止服务

4. **独立发布**:
   - 使用 `--self-contained` 参数发布后,用户无需安装 .NET 6
   - 单个 exe 文件包含所有依赖,直接分发即可

5. **配置保存**: 配置保存在注册表 `HKEY_CURRENT_USER\Software\EchWorkersManager` 中

## 功能特性

- ✅ 可视化配置界面
- ✅ **一键启动:自动启动服务 + 启用系统代理**
- ✅ **一键停止:自动停止服务 + 禁用系统代理**
- ✅ **最小化到系统托盘,双击恢复**
- ✅ **托盘右键菜单快速操作**
- ✅ **关闭自动清理:关闭程序时自动禁用系统代理**
- ✅ **ech-workers.exe 自动嵌入,无需额外文件**
- ✅ **内置 SOCKS5 → HTTP 代理转换器**
- ✅ **无需安装浏览器插件**
- ✅ **支持独立发布,无需安装 .NET 6**
- ✅ 配置自动保存/加载
- ✅ 状态实时显示
- ✅ 完全模拟 v2rayN 的使用体验
- ✅ 操作简化,无需确认对话框

## 系统要求

- Windows 7 SP1 或更高版本
- 如果使用独立发布版本,无需安装 .NET 6.0

## 快速打包指南 (3步完成)

### 步骤1: 准备项目文件

```bash
# 创建项目文件夹
mkdir EchWorkersManager
cd EchWorkersManager

# 复制以下文件到项目文件夹:
# - EchWorkersManager.csproj
# - Program.cs
# - Form1.cs
# - ech-workers.exe (重要!放在项目根目录)
# - app.ico (可选,自定义图标文件)
```

**关于图标文件 app.ico**:

- 图标文件必须是 `.ico` 格式
- 推荐尺寸: 16x16, 32x32, 48x48, 256x256 多尺寸
- 如果没有 app.ico,程序会使用系统默认图标
- 图标会显示在:任务栏、系统托盘、exe文件、窗口标题栏

### 步骤2: 发布独立应用

```bash
# 64位系统(推荐)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# 32位系统
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### 步骤3: 获取最终文件

生成的单个 exe 文件位于:

```
bin\Release\net6.0-windows\win-x64\publish\EchWorkersManager.exe
```

**直接分发这个 exe 文件给用户即可!** 用户无需安装任何运行时环境。

## 故障排除

**问题**: 点击启动服务提示找不到 ech-workers.exe
**解决**: 将 ech-workers.exe 复制到管理器程序所在目录

**问题**: 设置代理失败
**解决**: 尝试以管理员身份运行程序

**问题**: 程序无法启动
**解决**: 确保已安装 .NET 6.0 运行时或使用独立发布版本

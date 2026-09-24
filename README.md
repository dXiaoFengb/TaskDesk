# TaskDesk（装机任务管理器）

TaskDesk 是面向 Windows 11 x64 的便携式装机任务管理器。它采用接近 Windows 11 文件管理器的结构优先布局，集中展示任务、资源库说明文件和相关资源；默认不修改系统、不修改资源库源文件、不联网，也不要求管理员权限。

## 项目档案

- 项目名称：装机任务管理器
- 英文代号：TaskDesk
- EXE：`TaskDesk.exe`
- 命名空间根：`TaskDesk`
- 技术栈：C# WPF + .NET 8
- 目标平台：Windows 11 x64
- 发布方式：self-contained single-file
- 启动入口：软件根目录 `TaskDesk.exe`，覆盖式输出
- 起始版本：`v1.0.0`
- 数据文件：EXE 同级 `data\\tasks.json`
- 资源库：按 `config.json` 的相对路径、同级「资源库」文件夹、目录选择框的顺序解析
- Git：见 `CONSTRAINTS.md`；已启用提交，`push` 需单独授权

## 运行规则

### 软件根目录与资源库

软件根目录固定为 EXE 所在目录。资源库根目录按以下顺序定位：

1. 优先读取 EXE 同目录 `config.json` 的 `resourceLibraryPath`，按相对软件根目录解析；
2. 配置不存在或路径无效时，查找 EXE 同级名为「资源库」的文件夹；
3. 仍未找到时，启动目录选择框；用户选定后，将相对软件根目录的路径写回 `config.json`。

地址栏固定显示资源库根路径（相对软件根目录），只读，不可通过地址栏导航。

### 任务数据

任务数据保存到 `data\\tasks.json`。`data` 目录不存在时自动创建。若 EXE 同级目录不可写，TaskDesk 进入只读运行模式，保留资源浏览和预览，不偷偷回退写入其他路径。

### 主题与布局

启动时读取 Windows 系统主题：系统深色使用深色主题，系统浅色使用浅色主题；首版不做运行时切换动画。界面采用结构优先布局：顶部路径栏/工具栏、左侧任务区、中部列表、右侧介绍与预览区。

## 目录规划

```text
TaskDesk/
├─ src/                 # WPF 源码
├─ tests/               # 单元与集成测试
├─ data/                # 运行时 tasks.json（按需自动创建）
├─ TaskDesk.exe          # 根目录启动入口（发布后生成）
├─ TASK.md              # 已确认需求
├─ CONSTRAINTS.md       # 项目红线与加严约束
├─ VERSION.md           # 版本唯一来源
├─ handoff.md           # 当前交接状态
└─ README.md            # 项目总览
```

## 构建与测试

计划命令（项目骨架创建后以实际项目文件为准）：

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

不生成 zip，不打包安装器。发布前必须在 Windows 11 x64 环境验证直接启动、主题读取、资源库解析、任务持久化和只读目录降级。

## 安全边界

- 不写注册表、不安装服务、不创建计划任务、不修改网络或电源设置；
- 不请求管理员权限、不自动提权；
- 资源库默认只读，不覆盖、移动、删除或批量改写资源库源文件；
- 双击资源库中的 `.exe`、`.bat`、`.cmd`、`.ps1`、`.lnk` 前必须确认，并以普通用户权限启动；
- 不登录、不上传、不依赖网络服务；
- 发布物替换、删除/覆盖/迁移文件、真实资源执行和 `git push` 需按 `CONSTRAINTS.md` 重新确认。

## 协作顺序

下一个对话先读 `AGENTS.md`、`TASK.md`、`CONSTRAINTS.md`、`handoff.md`；`README.md` 作为项目总览。修改后执行最小必要验证，并把改动、验证、未完成项和 Git 状态写入 `handoff.md` 与 `对话记录/`。

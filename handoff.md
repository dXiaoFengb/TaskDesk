# 交接与状态

> 下一个对话：先读 `AGENTS.md`、`TASK.md`、`CONSTRAINTS.md`、`handoff.md`；`README.md` 作为项目总览。

## 项目

- 中文名：装机任务管理器
- 英文代号：TaskDesk
- EXE：`TaskDesk.exe`
- 命名空间根：`TaskDesk`
- 工作目录：`E:\Ai\Ai\任务管理器`；按用户确认保留当前目录，不移动或重命名为 `TaskDesk`。

## 需求与规则

- 产品愿景：为新装 Windows 11 的个人用户提供无需复杂配置、打开即可使用的装机任务管理器，集中查看任务、资源库说明文件和相关资源，并跟踪任务状态。
- 需求状态：`TASK.md` 五节与「边界与授权」已确认，提问状态为“提问结束”。
- 技术栈：C# WPF + .NET 8；目标 Windows 11 x64；self-contained single-file。
- 主题：启动时跟随 Windows 系统深浅色主题；首版不做运行时切换动画。
- 软件根目录：EXE 所在目录。
- 资源库根目录解析：优先读取同目录 `config.json` 的 `resourceLibraryPath` 相对路径；其次查找同级「资源库」文件夹；仍不存在时弹出目录选择框并写回 `config.json`。
- 地址栏：固定显示资源库根路径（相对软件根目录），只读且不可导航。
- 数据：EXE 同级 `data\\tasks.json`；`data` 不存在时自动创建；EXE 同级不可写时进入只读运行模式，不偷偷改写路径。
- 发布：EXE 同级 `publish\\`，覆盖式输出；版本从 `v1.0.0` 起。
- 硬约束：不生成 zip、不打包安装器、不修改资源库源文件、不写系统配置、不联网/上传、不自动提权；`push` 需单独授权。

## 本轮改动

- 已按用户补充要求重写 `TASK.md`，补齐资源库解析、数据目录、发布目录/版本、功能验收和边界授权。
- 已按 TaskDesk 项目实际情况重写 `CONSTRAINTS.md`，补齐只读路径、发布限制、Git 和外部副作用红线。
- 已按 TaskDesk 项目实际情况重写 `README.md`，补齐项目档案、运行规则、目录规划和构建/测试命令。
- 已重写本 `handoff.md`，移除与当前 TaskDesk 需求冲突的旧模板状态。
- 未修改资源库源文件；未生成 zip；未打包安装器。

## 验证

- 已回读并核对 `TASK.md`、`CONSTRAINTS.md`、`README.md`、`handoff.md`。
- 已确认四份文件包含一致的 TaskDesk 命名、资源库定位顺序、`data\\tasks.json`、`publish\\`、`v1.0.0`、主题规则和硬约束。
- 已通过 PowerShell XML 解析检查 `TaskDesk.csproj`、测试项目文件、`App.xaml` 和 `MainWindow.xaml`；已追加本轮记录到 `对话记录\\任务管理器.md`。
- 已初始化当前目录 Git 仓库；已创建 WPF/.NET 8 源码与测试骨架；尚未运行构建、测试或发布。
- 环境检查发现仅安装 .NET 运行时，未安装 .NET SDK，因此当前无法执行 `dotnet restore/build/test/publish`。

## 状态

- 版本：`v1.0.1`（本次文档与骨架提交版本，已写入 `VERSION.md`）
- 发布：未发布
- Git：已初始化；本轮待提交 `v1.0.1`；持续启用提交，禁止自动 push
- 当前授权：用户明确要求从本轮起持续提交 Git；允许提交本项目文件，禁止自动 push；不允许生成 zip、安装器或修改资源库源文件。

## 下一步

1. 回读校验四份文档内容和关键字段。
2. 按明确范围检查 Git 工作区，排除资源库和运行时数据。
3. 在具备 .NET 8 SDK 后运行 restore/build/test，并修正骨架编译问题。
4. 在不触碰资源库源文件的前提下，继续实现任务数据、资源库只读浏览/预览、主题读取和发布流程。

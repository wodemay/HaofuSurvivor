# 2026-09-28 崩溃日志排查

## 已确认的崩溃

- 时间：2026-09-26 23:39（本机时间）。Unity 2022.3.62f3c1，Direct3D 11，RTX 4060 Ti。
- 原始证据：`C:/Users/Administrator/AppData/Local/Temp/Unity/Editor/Crashes/Crash_2026-09-26_153930797/`，包含 `Editor.log` 和 `crash.dmp`，保留原件。
- 同一日志副本：`C:/Users/Administrator/AppData/Local/Unity/Editor/Editor-prev.log`，第 1460260 行为 Native Crash Reporting，第 1460294 行为 SIGSEGV。
- 托管调用链为 `SceneView.DoOnGUI -> DoDrawCamera -> Handles.DrawOutlineOrWireframeInternal -> Internal_DrawOutline_Injected`；原生栈包含 `ExecuteRenderQueue`、`ExtractSceneRenderNodeQueue`、`Camera::RenderEditorCamera`。
- 这是 Unity 编辑器 Scene 视图轮廓绘制路径上的原生崩溃。现有日志不能确定底层根因，不能据此认定某张新贴图、序列帧脚本、显卡驱动或内存不足导致崩溃。

## 同时发现的项目问题

崩溃会话日志中存在 162,019 条重复 EventSystem 警告，紧邻崩溃前仍在输出。累计 `Logs/game.log` 约 382 MB，包含 1,354,355 条同类警告；累计数量不等于单次会话数量。还分别出现 3 次、60 次 `RendererUpdateManager.UpdateAll must be called first`。针对这两份日志的检索未发现 OutOfMemory、NullReferenceException、MissingReferenceException 字样。

`Assets/Scenes/MainScene.unity` 与 `Assets/QFramework/Toolkits/UIKit/Scripts/Resources/UIRoot.prefab` 都含 EventSystem。当前非运行态通过 8800 MCP 只读检查，场景里只有一个启用的 MainScene/EventSystem；运行时加载 UIRoot 是需复现核验的重复来源，尚未在本轮启动游戏验证。

`Assets/Scripts/Architecture/Save/GameLogSystem.cs` 使用追加写入、AutoFlush，并为每条消息写出完整堆栈，没有重复合并或文件大小限制。刷屏会增加同步文件 I/O；它与原生崩溃的因果关系尚未建立。

## 排查边界与后续

该产品 Player.log 最后更新于 2026-09-11，Player-prev.log 最后更新于 2026-08-24，没有找到与上述时间匹配的打包游戏崩溃证据。本轮未修改运行时代码、场景或预制体，未删除日志，也未执行长时间游戏复现。

下一步优先复现并消除运行时双 EventSystem，给游戏日志增加重复抑制和大小限制，再进行持续游玩回归。Scene 视图原生崩溃仍需独立观察，不能把清理警告视为已经修复崩溃。

## 同日修复与验证

- 停用 MainScene 原有 EventSystem 对象，保留 QFramework UIRoot 的输入系统；场景仅修改一个启用字段，没有修改框架源码。
- GameLogSystem 将连续相同消息和堆栈合并，每五秒最多输出一次重复计数，消息变化或关闭时写出剩余计数。不同消息继续保留原始内容与堆栈。
- 活跃日志达到 10 MiB 后轮转到 game.log.previous，仅保留一份轮转备份；单条消息可能使文件略超阈值。启动时已有超限日志移入唯一 archive 文件，保留历史崩溃证据，不纳入日常轮转删除。
- dotnet 编译 0 错误、2 个既有引用警告。Unity Play Mode 主菜单只存在一个启用 EventSystem，current 和 StandaloneInputModule 正常，Console 查询无错误/警告。
- 独立临时文件验证 10,000 条相同消息压缩到不足 1 KiB、9999 次计数写出、10 MiB 轮转、关闭时重复计数落盘，全部通过。测试读取最初因共享模式不匹配失败，改用 FileShare.ReadWrite 后通过，未影响游戏日志。
- 已退出 Play Mode，临时测试文件清理，旧日志与崩溃转储保留。本轮没有持续游玩复现，原生 SceneView 崩溃根因仍未证实，不能宣称彻底修复。

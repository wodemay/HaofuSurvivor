# BGM 系统

音乐资源保留在 `Assets/Music/活全家BGM.WAV`，由 MainScene 的 GameStart.MainMenuBgm 直接引用，随场景打包。更换音乐时修改此引用。

`GameStart.Awake` 发送 ConfigureMainMenuBgmCommand；主菜单 OnShow 发送 PlayMainMenuBgmCommand。BgmSystem 订阅 RunStartedEvent，在开局或重开时播放同一首音乐。每次重播先调用 AudioKit.StopMusic，再 PlayMusic，位置归零、Pitch 重置为 1，循环播放且复用单个音乐音源。

Pitch 使用对局逻辑时间：`Clamp(1 + ElapsedSeconds / 1800, 1, 2)`。10 分钟约为 1.33，15 分钟为 1.5，30 分钟起保持 2。提高 Pitch 同时提高播放速度和音高。暂停及升级选择时音乐暂停，Pitch 冻结；恢复时从原播放位置继续。RunTimerSystem.Pause/Resume 发布 RunTimerPauseChangedEvent，由 BgmSystem 同步音频状态；AudioKit 恢复使用 Play，因此恢复后还原采样位置。继续存档时由 RunTimerSystem.Restore 发布时间事件，立即恢复对应 Pitch。结算保持最后的 Pitch，返回菜单重置。

不新增独立 Update，不修改 QFramework 源文件，保留 AudioKit 原有音乐开关及音量设置。

验证：2026-09-11，dotnet 编译 0 错误、2 个既有程序集引用警告。Unity Play Mode 中通过事件和临时计时状态验证循环播放、单音源、菜单/开局重播、Pitch 上限、暂停/升级冻结以及存档时间恢复，Console 无错误。未执行整局游玩、听感验收或发布包验证。

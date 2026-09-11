# BGM 任务交接

状态：实现及 Unity 运行状态验证完成（2026-09-11），未提交或推送。

已恢复原 stash 中的音乐资源、BgmSystem、BgmCommands 和场景引用；stash 保留作为备份，不要再次 apply。主菜单/开局从头循环播放，Pitch 从 1 随对局时间增加、30 分钟达到 2 并封顶；暂停和升级选择冻结 Pitch。修正了返回菜单沿用旧局 Pitch 的问题，并把音乐配置移到 GameStart.Awake。

编译 0 错误、2 个既有引用警告，git diff --check 和 Unity Console 检查通过。此前 50 分钟曲线的 Play Mode 注入事件/时间验证：正常循环播放、单音源、重播位置归零、0/600/1800/3000/6000 秒 Pitch=1/1.2/1.6/2/2，暂停/升级冻结，恢复存档时间即恢复 Pitch。已退出 Play Mode。

尚未覆盖：实际听感验收、整局游戏回归、发布包播放测试。核心实现细节见 Docs/Guides/BgmSystem.zh-CN.md。当前项目 MCP 是 8800，禁止操作 8801；没有 Git 提交/推送授权。

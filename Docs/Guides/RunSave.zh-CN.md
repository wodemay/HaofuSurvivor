# 局内进度存档

本文只说明局内快照；局外 Profile 不属于当前模块。

## 文件位置

Windows 运行时只允许在游戏根目录下生成 ASCII 路径。`GameStoragePath.TryGetPath` 会拒绝包含中文或其他非 ASCII 字符的安装根目录，并停用业务存档与游戏日志写入；请将游戏安装到例如 `D:\Games\ProjectSurvivor` 的纯英文路径。

`RunSaveStorage` 将 JSON 写入游戏根目录：

```text
SaveData/active-run.json
SaveData/selected-character.json
Logs/game.log
Settings/qframework-settings.json
```

根目录由 `Save/GameStoragePath.cs` 按 Windows、macOS Player 和 Unity Editor 分别解析。

Windows 版本的项目业务数据完全不使用 PlayerPrefs，也不写入系统用户目录。旧版本注册表数据不会再被读取；需要保留旧存档时，应在迁移前手动导出到 `SaveData/active-run.json`。`SaveData/` 已加入 Git 忽略规则，不会把本机存档提交到仓库。

QFramework 的 AudioKit、LocaleKit 和 ResKit 设置也写入 `Settings/qframework-settings.json`，不再使用 PlayerPrefs；音频开关、音量、语言选择和 AssetBundle 读取开关都能跨重启保留。

`GameStoragePath` 只接受 ASCII 英文相对路径，并拒绝绝对路径、`.`、`..` 和控制字符；当前生成的目录与文件名均为英文。程序不会自动修改应用安装根目录，因此请将 Windows 游戏安装在不含中文的路径中。

QFramework ResKit 的热更新资源缓存、Unity 原生日志和崩溃报告仍由第三方/Unity 使用平台默认目录，这是当前明确保留的例外。

## 快照内容

快照保存可重建对局所需的 ID、数值和运行时数据：角色与地图世界种子、主题和生成器版本、阶段时间、玩家位置与生命、等级经验、通用属性等级、Weapon/Skill/Dodge 运行时等级与 Attack 修正、角色专属升级等级与临时效果、已退役 Weapon 来源，以及当前敌人、经验球、投射物、地面火焰和弹幕状态。基础地图区块不逐格序列化，而是由 Seed、主题和生成器版本确定性重建。Unity 对象、Prefab、对象池实例和表现组件不直接序列化。

## 保存与恢复

`RunSaveSystem` 作为 `IRunUpdateable` 按逻辑时间周期保存，并在暂停、返回菜单、切后台和退出应用时触发即时保存。胜利或失败先保存结束状态，结算金币成功写入 Profile 后才清理局内存档；写入失败可在结算页确认重试，重启后也可继续该存档完成结算。

主菜单通过 `HasSavedRunQuery` 控制“继续游戏”。`ContinueSavedRunCommand` 先按 Seed、主题和生成器版本重建 PlayerRoot 与基础地图，再恢复 Loadout、属性、角色专属效果、位置、生命和时间；敌人、经验球、投射物、地面火焰和弹幕等池对象按快照数据重新生成。无效的可选能力会跳过，必要 Weapon 恢复失败则中止继续流程。

局内存档版本为 2，Profile 版本为 3。新局生成唯一 `RunId`，保存和继续游戏保持该 ID；Profile 保存已结算 RunId，重复结算不会重复入账。版本 0/1 的局内存档自动迁移：含地图快照时根据世界身份生成稳定 ID，保证同一旧局的不同快照迁移结果一致；不含地图身份的早期快照只能按完整内容生成 ID。

校验覆盖配置 ID、等级范围、重复实体 ID、阶段、有限数值和运行时对象数据。同位置经验球及已完成地图事件属于合法状态。损坏主档或备份会隔离为 `.corrupt-*`，可从有效备份恢复，文件不存在单独标记；高版本存档拒绝读取，Profile 禁止降级覆盖，恢复过程写入 `Logs/save-recovery.log`。尚无防篡改签名或校验和。

保存失败通过现有弹窗提示，同一轮持续失败不会在每次自动保存时重复弹窗；保存恢复正常后重新允许提示。继续失败会清理半恢复对象、停止 Tick、关闭 HUD/升级页并返回菜单。

2026-09-14 验证：27 项 Unity 检查通过，覆盖校验、迁移、备份恢复、同局 ID 稳定、结算去重、文件占用时回滚/重试、结束快照保留与清理、实际继续和失败清理。原存档已备份并校验恢复。dotnet 关闭分析器后 0 错误、2 个既有引用警告；标准命令遭遇 Roslyn 分析器 NullReferenceException，未修改项目编译设置。Unity 导入编译通过；两条故意锁定 Profile 的预期错误用于验证失败路径。未执行长时间整局回归。

2026-09-18 收尾复核：`origin/main` 仍为 `1c6a8fb`，冲突标记和 `git diff --check` 均通过；关闭分析器后的 dotnet 构建再次通过，0 错误、2 个既有程序集引用警告。标准命令在当前 .NET SDK 10 RC 环境触发 Roslyn `InvalidOperationException`，属于编译器进程异常。本轮未重复执行 Unity Play Mode 检查，沿用 2026-09-14 的 27 项结果；长时间整局回归仍未执行。

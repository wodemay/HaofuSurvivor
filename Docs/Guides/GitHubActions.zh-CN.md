# GitHub Actions 自动检查与打包

## 工作流

项目包含两个工作流：

- `.github/workflows/ci-review.yml`：在 PR、`main` 和 `feat/**` 分支提交时运行。先执行 `git diff --check` 与项目文件检查，再调用 Unity 验证启用场景和脚本编译入口。
- `.github/workflows/unity-windows-build.yml`：合并或推送到 `main` 时运行，也可以在 GitHub Actions 页面手动运行。它生成 Windows 版本并上传为 Artifact，不把 EXE 提交到 Git 仓库。

本地 Unity 版本为 `2022.3.62f3c1`。GameCI 使用 Docker 可用的兼容版本 `2022.3.62f3`，其中 `c1` 是本地发行渠道修订标记，不能直接作为 GameCI 镜像版本。

## 配置授权

在仓库 `Settings → Secrets and variables → Actions` 中新增仓库 Secret：

```text
UNITY_LICENSE
```

值应为 Unity CI 使用的授权文件内容。不要把 Unity 账号密码、序列号或授权文件写入仓库。

未配置 `UNITY_LICENSE` 时，静态检查仍可运行，但 Unity 编译和自动打包会失败并提示授权缺失。

## 构建结果

构建产物位于 Actions 运行页面的 Artifact：

```text
HaofuSurvivor-Windows-<commit-sha>
```

下载后解压整个目录运行 EXE，不能只复制单独的 `.exe` 文件。`Build/` 已加入 `.gitignore`，不会进入提交记录。

## 本地入口

Unity Editor 的构建入口位于 `Assets/Editor/ProjectBuild.cs`：

- `ValidateProject()`：检查启用场景和项目构建入口。
- `BuildWindows()`：使用启用场景生成 `StandaloneWindows64`。

正式打包会先调用 QFramework ResKit 生成 AssetBundles，并复制到 `Assets/StreamingAssets/AssetBundles/Windows/`，再生成 Windows Player。因此本地生成的 `AssetBundles/`、`Assets/StreamingAssets/`、`Assets/QFrameworkData/QAssets.cs` 和 `EXE/` 都属于构建产物，不应提交。

两个方法供 GitHub Actions 的 `buildMethod` 调用，不属于游戏运行时代码。

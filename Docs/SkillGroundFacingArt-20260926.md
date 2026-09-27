# 技能图标、石砖与角色朝向

2026-09-26 使用内置 image_gen，保留生成原图及 alpha，Unity importer 切片，不在运行时裁图。

## 接入

- `Assets/Art/Sprites/Characters/*-Side.png`：三个角色各四帧待机、四帧移动，右侧三分之四视角。Square/Triangle/Circle 内容预制体改用新图集，并启用 FaceMovement。左向使用 SpriteRenderer.flipX，不改变碰撞或根节点。纵向和停步保持最后朝向，复用/开局恢复右向。
- `Assets/Art/Sprites/Characters/{Survivor,Vanguard,Scout}.png`：继续供 CharacterConfig.Icon 选角使用，引用已验证。旧 Animation 图集保留供回退。
- `Assets/Art/Sprites/Icons/SkillAtlas.png`：16 个图标，绑定 12 个配置文件中的 22 处武器、技能、闪避、属性与专属升级图标。沿用现有 UI 图标绑定，不创建 UI。
- `Assets/Art/Sprites/Environment/Ground-Slate.png`：低对比深青灰石砖，Tile_Ground_Default 引用新版，保持每格 1×1；旧 Ground.png 保留。

## 验证

Unity 导入检查通过：三套图集各 8 帧、16 个图标、22 处图标引用、原选角图引用、地面 1×1 尺寸。Play Mode 临时实例验证三个角色左/右、停步保持、纵向保持、零时间增量不更新、禁用/启用重置均通过。dotnet build 启用分析器通过：0 错误，2 个既有程序集冲突警告。临时测试对象已销毁。验证截图为 Logs/art-upgrade-preview.png。

本轮方向为右侧图镜像得到左侧，服装不对称细节也随之镜像；没有新增前后朝向或攻击/受击动画。未做整局游玩、构建或本轮 PR。

## 最终提示词

### Survivor

Create a production-ready 2D game sprite sheet based on this exact blue hooded adventurer reference, preserving clothing colors, hairstyle and chibi proportions. Exactly 8 complete full-body character frames arranged in a perfectly regular 4 column by 2 row grid, each cell same size. ALL frames are a clear side-facing three-quarter profile looking RIGHT, nose and feet pointing right, not facing the viewer. Top row: 4 subtle breathing idle animation poses. Bottom row: 4 different consecutive running cycle poses with alternating legs and arms. Consistent character size, ground baseline and center in every cell, each figure fully contained with generous transparent gutters. Hand-painted fantasy style. True transparent background. No text, numbers, grid lines, scenery, ground shadow or effects. This will be sliced into 8 equal grid cells in Unity.

### Vanguard

Create a production-ready 2D game sprite sheet based on this exact silver and gold armored red caped knight reference, preserving clothing colors, hairstyle and chibi proportions. Exactly 8 complete full-body character frames arranged in a perfectly regular 4 column by 2 row grid, each cell same size. ALL frames are a clear side-facing three-quarter profile looking RIGHT, nose and feet pointing right, not facing the viewer. Top row: 4 subtle breathing idle animation poses. Bottom row: 4 different consecutive running cycle poses with alternating legs and arms. Consistent character size, ground baseline and center in every cell, each figure fully contained with generous transparent gutters. Hand-painted fantasy style. True transparent background. No text, numbers, grid lines, scenery, ground shadow or effects. This will be sliced into 8 equal grid cells in Unity.

### Scout

Create a production-ready 2D game sprite sheet based on this exact green leaf-hooded elf scout reference, preserving clothing colors, hairstyle and chibi proportions. Exactly 8 complete full-body character frames arranged in a perfectly regular 4 column by 2 row grid, each cell same size. ALL frames are a clear side-facing three-quarter profile looking RIGHT, nose and feet pointing right, not facing the viewer. Top row: 4 subtle breathing idle animation poses. Bottom row: 4 different consecutive running cycle poses with alternating legs and arms. Consistent character size, ground baseline and center in every cell, each figure fully contained with generous transparent gutters. Hand-painted fantasy style. True transparent background. No text, numbers, grid lines, scenery, ground shadow or effects. This will be sliced into 8 equal grid cells in Unity.

### Ground

A single seamless repeating square texture tile for a top-down 2D hand-painted fantasy survivor game. Orthographic directly overhead flat floor only. Dark desaturated blue gray slate paving, four large stone slabs arranged two by two with very thin recessed mortar at the center and matching half mortar at every outer edge so repeats form a continuous grid. Subtle worn bevels and restrained fine cracks, sparse muted moss only inside grooves, low contrast, evenly lit, no spotlight or vignette, no perspective, no raised blocks, no objects, no symbols or text. Completely opaque texture fills the canvas to every edge, tileable left-right and top-bottom. Keep the overall dark cool value of the reference so bright characters and orange effects remain readable.

### Icons

Production-ready fantasy game skill ICON ATLAS, exactly 16 distinct square icons in a precise 4 columns by 4 rows grid. Uniform equal-sized cells and wide transparent gutters, every icon centered inside its cell with 15 percent padding. Each icon is a polished hand-painted emblem with bold small-size readability, gold outlines, dark teal circular medallion backdrop, no text no letters no numbers. Row 1 left to right: (1) gold magic bolt projectile (2) three blue-white piercing magic bolts (3) orange fireball (4) crimson inferno flame with golden core. Row 2: (5) circular radial burst of eight golden bullets (6) teal winged boot dash (7) silver sword with red power glow (8) cyan hourglass cooldown. Row 3: (9) violet magnetic vortex collecting motes (10) blue experience crystal star (11) green swift boot (12) emerald healing heart with small gold cross. Row 4: (13) healing potion with rising green sparkles (14) gold shield containing red heart resilience (15) teal rolling figure with curved motion arrow (16) concentric blue and gold energy rings charge. Consistent premium chibi fantasy UI style matching hand-painted game art. Genuine transparent background outside circular medallions. No drop shadows beyond cell bounds, no border around entire sheet, exactly 16 icons.

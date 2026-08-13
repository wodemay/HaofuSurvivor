# Weapon 容器

Weapon 是角色持有的运行时容器，Attack 是容器中的攻击内容。角色可以拥有多把 Weapon；敌人通常直接使用 `EnemyConfig.AttackIds`。

## 数据边界

- `WeaponConfig`：静态模板、初始 Attack、等级规则和升级限制。
- `WeaponRuntimeData`：当前局的等级、Attack 列表和属性修正。
- `PlayerLoadoutSystem`：装备、替换、升级、进化和清理 Trigger。

升级和进化只修改运行时数据。进化保留原 WeaponRuntime 槽位，替换为 1 级且不可升级的新 Weapon。

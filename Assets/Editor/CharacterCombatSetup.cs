using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HaoFuSurvivor.Editor
{
	public static class CharacterCombatSetup
	{
		private const string Root = "Assets/Resources/Configs/Combat/CharacterKits/";
		private static T Asset<T>(string path) where T : ScriptableObject
		{
			var asset = AssetDatabase.LoadAssetAtPath<T>(path);
			if (asset != null) return asset;
			System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
			asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
		}
		private static int Next<T>(Func<T,int> selector) where T : UnityEngine.Object
			=> AssetDatabase.FindAssets("t:" + typeof(T).Name, new[]{"Assets/Resources/Configs"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>).Where(x => x != null).Select(selector).DefaultIfEmpty(0).Max() + 1;
		private static void Save(UnityEngine.Object obj) => EditorUtility.SetDirty(obj);
		private static GameObject Prefab(string name, Sprite sprite, bool projectile = false)
		{
			var path = "Assets/Art/Prefabs/Attack/" + name + ".prefab";
			var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			var go = existing == null ? new GameObject(name) : PrefabUtility.LoadPrefabContents(path);
			try
			{
				var renderer = go.GetComponent<SpriteRenderer>(); if (renderer == null) renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = 20;
				if (projectile)
				{
					var body = go.GetComponent<Rigidbody2D>(); if (body == null) body = go.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0;
					var collider = go.GetComponent<CircleCollider2D>(); if (collider == null) collider = go.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = 0.12f;
					go.transform.localScale = Vector3.one * 0.65f;
				}
				return PrefabUtility.SaveAsPrefabAsset(go, path);
			}
			finally { if (existing == null) UnityEngine.Object.DestroyImmediate(go); else PrefabUtility.UnloadPrefabContents(go); }
		}
		private static AttackConfig Attack(string slug, string executor, float damage, float cooldown, CharacterAttackParameterConfig parameters, AttackCatalogConfig catalog)
		{
			var attack = Asset<AttackConfig>(Root + "Attack_" + slug + ".asset");
			if (attack.Id == 0) attack.Id = Next<AttackConfig>(x => x.Id);
			attack.ExecutorId = executor; attack.Damage = damage; attack.Cooldown = cooldown; attack.ExecutorParameterConfig = parameters;
			if (!catalog.Attacks.Contains(attack)) catalog.Attacks.Add(attack);
			Save(attack); Save(parameters); return attack;
		}
		private static WeaponLevelUpgrade Upgrade(int level, int attack, string key, float value, string description)
			=> new WeaponLevelUpgrade { Level = level, Description = description, AttackModifiers = new List<WeaponAttackModifier>{ new WeaponAttackModifier{AttackId=attack,Key=key,Value=value} } };
		[MenuItem("ProjectSurvivor/Configure Character Combat")]
		public static string Configure()
		{
			if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
			const string texturePath = "Assets/Art/Sprites/Effects/CharacterCombat.png";
			AssetDatabase.ImportAsset(texturePath);
			var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
			importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.spritePixelsPerUnit = 512; importer.maxTextureSize = 2048;
			var names = new[]{"Sword","Slash","Drone","Ring","Bolt","Thrust"};
			var rects = new[]{new Rect(0,512,560,512),new Rect(565,512,459,512),new Rect(1024,512,512,512),new Rect(0,0,512,512),new Rect(512,0,512,512),new Rect(1024,0,512,512)};
			importer.spritesheet = names.Select((name,i) => new SpriteMetaData {name=name,rect=rects[i],alignment=9,pivot=new Vector2(i==0?0.12f:0.5f,0.5f)}).ToArray();
			importer.SaveAndReimport();
			var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToDictionary(x=>x.name);
			var sword=Prefab("VanguardSword",sprites["Sword"]); var slash=Prefab("SwordSlash",sprites["Slash"]);
			var drone=Prefab("OrbitDrone",sprites["Drone"]); var ring=Prefab("CombatRing",sprites["Ring"]);
			var bolt=Prefab("ScoutBolt",sprites["Bolt"],true); var thrust=Prefab("DodgeThrust",sprites["Thrust"]);
			var trail=Prefab("DodgeAfterimage",sprites["Ring"]);
			const string audioPath="Assets/Resources/Presentation/CharacterCombatAudio.prefab";
			System.IO.Directory.CreateDirectory("Assets/Resources/Presentation");
			var audioObject=new GameObject("CharacterCombatAudio");
			try
			{
				var audio=audioObject.AddComponent<CombatAudioView>();audio.Voices=new AudioSource[4];
				for(var voice=0;voice<audio.Voices.Length;voice++){audio.Voices[voice]=audioObject.AddComponent<AudioSource>();audio.Voices[voice].playOnAwake=false;audio.Voices[voice].volume=0.35f;audio.Voices[voice].spatialBlend=0;}
				PrefabUtility.SaveAsPrefabAsset(audioObject,audioPath);
			}
			finally { UnityEngine.Object.DestroyImmediate(audioObject); }
			var attacks=AssetDatabase.LoadAssetAtPath<AttackCatalogConfig>("Assets/Resources/Configs/Combat/Attack/AttackCatalog.asset");
			var weapons=AssetDatabase.LoadAssetAtPath<WeaponCatalogConfig>("Assets/Resources/Configs/Combat/Weapon/WeaponCatalog.asset");
			var skills=AssetDatabase.LoadAssetAtPath<SkillCatalogConfig>("Assets/Resources/Configs/Combat/Skill/SkillCatalog.asset");
			var groups=AssetDatabase.LoadAssetAtPath<SkillGroupCatalogConfig>("Assets/Resources/Configs/Combat/Skill/SkillGroupCatalog.asset");
			var dodges=AssetDatabase.LoadAssetAtPath<DodgeCatalogConfig>("Assets/Resources/Configs/Combat/Dodge/DodgeCatalog.asset");
			var upgrades=AssetDatabase.LoadAssetAtPath<CharacterExclusiveSkillUpgradeConfig>("Assets/Resources/Configs/Progression/CharacterExclusiveSkillUpgradeCatalog.asset");
			var swordParam=Asset<CharacterAttackParameterConfig>(Root+"Parameters_Sword.asset");
			swordParam.WeaponVisual=sword; swordParam.EffectVisual=slash; swordParam.Radius=2.2f; swordParam.Angle=120; swordParam.Knockback=0.35f; swordParam.Duration=0.35f;
			var swordAttack=Attack("Sword","sword",18,0.9f,swordParam,attacks);
			var spinParam=Asset<CharacterAttackParameterConfig>(Root+"Parameters_Spin.asset");
			spinParam.WeaponVisual=sword;spinParam.EffectVisual=ring;spinParam.Radius=3.5f;spinParam.Angle=360;spinParam.Windup=0.2f;spinParam.Duration=0.55f;spinParam.Knockback=1.2f;spinParam.ReductionDuration=2;
			spinParam.Levels=new List<CharacterAttackLevel>{new CharacterAttackLevel{Level=2,Damage=48,Radius=4,Cooldown=14,ReductionDuration=2},new CharacterAttackLevel{Level=3,Damage=48,Radius=4,Cooldown=12,ReductionDuration=2.5f}};
			var spinAttack=Attack("Spin","spin-sword",36,14,spinParam,attacks);
			var boltParam=Asset<CharacterAttackParameterConfig>(Root+"Parameters_Bolt.asset");
			boltParam.ProjectilePrefab=bolt;boltParam.MoveSpeed=16;boltParam.AttackRange=10;boltParam.Lifetime=10f/16;boltParam.Pierce=2;
			var boltAttack=Attack("Bolt","piercing-bolt",8,0.65f,boltParam,attacks);
			var volleyParam=Asset<CharacterAttackParameterConfig>(Root+"Parameters_Volley.asset");
			volleyParam.ProjectilePrefab=bolt;volleyParam.EffectVisual=thrust;volleyParam.MoveSpeed=20;volleyParam.AttackRange=10;volleyParam.Lifetime=0.5f;volleyParam.Pierce=2;volleyParam.Shots=6;volleyParam.Interval=0.2f;volleyParam.Duration=1.2f;
			volleyParam.Levels=new List<CharacterAttackLevel>{new CharacterAttackLevel{Level=2,Damage=20,Radius=2.2f,Cooldown=12,Shots=6,Duration=1.2f},new CharacterAttackLevel{Level=3,Damage=20,Radius=2.2f,Cooldown=10,Shots=8,Duration=1.6f}};
			var volleyAttack=Attack("Volley","hunter-volley",16,12,volleyParam,attacks);
			var starParam=Asset<CharacterAttackParameterConfig>(Root+"Parameters_Star.asset");
			starParam.ProjectilePrefab=bolt;starParam.WeaponVisual=drone;starParam.EffectVisual=ring;starParam.MoveSpeed=14;starParam.Lifetime=8f/14;starParam.AttackRange=8;starParam.Windup=0.35f;starParam.Duration=3;starParam.Shots=6;starParam.Interval=0.5f;starParam.Radius=4;starParam.FinalDamage=24;
			starParam.Levels=new List<CharacterAttackLevel>{new CharacterAttackLevel{Level=2,Damage=8,Radius=4,Cooldown=18,FinalDamage=24},new CharacterAttackLevel{Level=3,Damage=8,Radius=4,Cooldown=16,FinalDamage=36}};
			var starAttack=Attack("Star","star-overload",6,18,starParam,attacks);
			starParam.StartSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Combat/Deploy.wav");
			starParam.ShotSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Combat/Pulse.wav");
			starParam.EndSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Combat/Burst.wav");
			swordParam.StartSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Combat/Slash.wav");
			spinParam.StartSound=swordParam.StartSound;volleyParam.ShotSound=starParam.ShotSound;
			var survivor=skills.Skills.First(x=>x.Id==1);survivor.DisplayName="星环超载";survivor.Description="四炮展开并环绕齐射，收拢后释放终结冲击环。";survivor.InitialAttackIds=new List<int>{starAttack.Id};survivor.MaxLevel=3;survivor.Icon=sprites["Drone"];Save(survivor);
			foreach(var definition in upgrades.Upgrades.Where(x=>x.CharacterId==1)){definition.DisplayName="星环超载强化";definition.Description="提升浮游炮伤害，满级强化终结并缩短冷却。";definition.Icon=sprites["Drone"];}
			foreach(var dodge in dodges.Dodges){dodge.TrailPrefab=trail;dodge.BurstPrefab=ring;dodge.ThrustPrefab=thrust;Save(dodge);}
			for(var i=0;i<2;i++)
			{
				var slug=i==0?"Vanguard":"Scout";var display=i==0?"裂阵剑术":"贯穿弩";var basic=i==0?swordAttack:boltAttack;var special=i==0?spinAttack:volleyAttack;
				var weapon=Asset<WeaponConfig>("Assets/Resources/Configs/Combat/Weapon/"+slug+"/Weapon_"+slug+".asset");
				if(weapon.Id==0)weapon.Id=Next<WeaponConfig>(x=>x.Id);weapon.DisplayName=display;weapon.Description=i==0?"近身交替挥剑，斩击前方敌人。":"高速弩矢最多命中三个目标。";weapon.Icon=sprites[i==0?"Sword":"Bolt"];weapon.InitialAttackIds=new List<int>{basic.Id};weapon.MaxLevel=5;weapon.CanAcquireDuringRun=false;
				weapon.LevelUpgrades=new List<WeaponLevelUpgrade>{Upgrade(2,basic.Id,WeaponUpgradeModifierKeys.ProjectileDamageAdd,i==0?4:2,"提高基础伤害"),Upgrade(3,basic.Id,i==0?"Melee.RadiusAdd":WeaponUpgradeModifierKeys.ProjectilePierceAdd,i==0?0.4f:1,"扩大攻击覆盖"),Upgrade(4,basic.Id,WeaponUpgradeModifierKeys.AttackCooldownMultiplier,i==0?(-1f/6):(-2f/13),"缩短攻击间隔"),Upgrade(5,basic.Id,WeaponUpgradeModifierKeys.ProjectileDamageAdd,i==0?6:3,"强化伤害与覆盖")};
				weapon.LevelUpgrades[3].AttackModifiers.Add(new WeaponAttackModifier{AttackId=basic.Id,Key=i==0?"Melee.AngleAdd":WeaponUpgradeModifierKeys.ProjectilePierceAdd,Value=i==0?30:1});
				if(!weapons.Weapons.Contains(weapon))weapons.Weapons.Add(weapon);Save(weapon);
				var skill=Asset<SkillConfig>("Assets/Resources/Configs/Combat/Skill/"+slug+"/Skill_"+slug+".asset");
				if(skill.Id==0)skill.Id=Next<SkillConfig>(x=>x.Id);skill.DisplayName=i==0?"断阵回旋":"猎杀连射";skill.Description=i==0?"回旋挥剑击退周围敌人，并短暂减伤。":"移动中向锁定目标连续发射强化弩矢。";skill.Icon=sprites[i==0?"Slash":"Bolt"];skill.InitialAttackIds=new List<int>{special.Id};skill.MaxLevel=3;skill.CanUpgrade=true;if(!skills.Skills.Contains(skill))skills.Skills.Add(skill);Save(skill);
				var dodge=Asset<DodgeConfig>("Assets/Resources/Configs/Combat/Dodge/Dodge_"+slug+".asset");
				if(dodge.Id==0)dodge.Id=Next<DodgeConfig>(x=>x.Id);dodge.DisplayName=i==0?"锋刃突进":"掠影装填";dodge.Description=i==0?"持剑突进，对沿途敌人造成伤害并击退。":"闪避后下一枚贯穿弩伤害翻倍，额外贯穿两人。";dodge.ExecutorId=i==0?"sword-dash":"reload-dash";dodge.Distance=i==0?2.8f:3.6f;dodge.Duration=dodge.InvulnerabilityDuration=i==0?0.22f:0.18f;dodge.Cooldown=i==0?2.5f:2;dodge.MaxLevel=3;dodge.Icon=sprites["Thrust"];dodge.TrailPrefab=trail;dodge.BurstPrefab=ring;dodge.ThrustPrefab=thrust;
				dodge.LevelUpgrades=new List<DodgeLevelUpgrade>{new DodgeLevelUpgrade{Level=2,CooldownAdd=i==0?0:-0.2f,Description=i==0?"突进伤害提升至18":"冷却缩短至1.8秒"},new DodgeLevelUpgrade{Level=3,CooldownAdd=i==0?-0.3f:-0.2f,Description=i==0?"冷却缩短至2.2秒":"装填强化提升至2.5倍"}};if(!dodges.Dodges.Contains(dodge))dodges.Dodges.Add(dodge);Save(dodge);
				var group=Asset<SkillGroupConfig>("Assets/Resources/Configs/Combat/Skill/SkillGroup_"+slug+".asset");if(group.Id==0)group.Id=Next<SkillGroupConfig>(x=>x.Id);group.StartingWeaponIds=new List<int>{weapon.Id};group.StartingSkillIds=new List<int>{skill.Id};group.StartingDodgeId=dodge.Id;group.RequireStartingWeapons=true;if(!groups.SkillGroups.Contains(group))groups.SkillGroups.Add(group);Save(group);
				var character=AssetDatabase.LoadAssetAtPath<CharacterConfig>("Assets/Resources/Configs/Character/Character_"+slug+".asset");character.SkillGroupId=group.Id;character.SkillDescription=weapon.Description+" 专属技能："+skill.DisplayName+"；闪避："+dodge.DisplayName+"。";Save(character);
				upgrades.Upgrades.RemoveAll(x=>x.CharacterId==character.Id);upgrades.Upgrades.Add(new CharacterExclusiveSkillUpgradeDefinition{CharacterId=character.Id,SkillId=skill.Id,ExclusiveWeaponId=weapon.Id,ExclusiveDodgeId=dodge.Id,DisplayName=skill.DisplayName+"强化",Description="提升专属技能强度，满级缩短冷却。",Icon=skill.Icon});
			}
			foreach(var obj in new UnityEngine.Object[]{attacks,weapons,skills,groups,dodges,upgrades})Save(obj);
			AssetDatabase.SaveAssets();
			return "Character kits configured: " + string.Join(", ",attacks.Attacks.Where(x=>x.ExecutorParameterConfig is CharacterAttackParameterConfig).Select(x=>x.Id+":"+x.ExecutorId));
		}
	}
}

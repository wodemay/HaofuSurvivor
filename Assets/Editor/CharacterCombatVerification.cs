using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEditor;
using UnityEngine;

namespace HaoFuSurvivor.Editor
{
	public static class CharacterCombatVerification
	{
		private static readonly List<string> Results = new();
		private static IArchitecture A => GameArchitecture.Interface;
		private static void Check(bool condition, string message)
		{
			if (!condition) throw new InvalidOperationException(message);
			Results.Add(message);
		}
		private static void Begin(int character)
		{
			A.GetSystem<RunSystem>().ExitToCharacterSelection();
			A.GetSystem<CharacterSelectionSystem>().Select(character);
			A.SendCommand(new StartSelectedCharacterRunCommand());
			Check(A.GetModel<PlayerModel>().CharacterId == character && A.GetModel<PlayerModel>().IsRegistered, "spawn-"+character);
		}
		private static CombatEntity Target(float x=1.5f)
		{
			var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>("Assets/Resources/Configs/Enemy/Enemy_Fast01.asset");
			A.GetSystem<EnemySystem>().Restore(new[]{new EnemySaveData{ConfigId=config.Id,PositionX=x,CurrentHealth=1000,MoveSpeed=0}},0);
			Physics2D.SyncTransforms();
			return A.GetSystem<CombatTargetSystem>().FindClosestOpponent(Vector2.zero,CombatFaction.Player,10);
		}
		private static int ProjectileCount() => A.GetSystem<ProjectileSystem>().GetSaveData().Count();
		public static string Run()
		{
			if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play Mode required.");
			Results.Clear(); EditorApplication.isPaused=true;
			try
			{
				Begin(2);
				var target=Target();
				var attacks=A.GetSystem<AttackSystem>(); var combat=A.GetSystem<CharacterCombatSystem>();
				var health=A.GetSystem<EnemyHealthSystem>();
				var multiplier=A.GetSystem<StatSystem>().GetAttackDamageMultiplier();
				attacks.OnRunUpdate(0.01f); combat.OnRunUpdate(0.11f);
				Check(Mathf.Abs(health.GetCurrentHealth(target)-(1000-18*multiplier))<0.01f,"sword-damage-18-scaled");
				combat.OnRunUpdate(0.3f);
				Check(Mathf.Abs(health.GetCurrentHealth(target)-(1000-18*multiplier))<0.01f,"sword-single-hit");
				A.GetSystem<PlayerLoadoutSystem>().TryUseSkills(); combat.OnRunUpdate(0.21f);
				Check(Mathf.Abs(health.GetCurrentHealth(target)-(1000-54*multiplier))<0.01f,"spin-damage-36-scaled");
				var before=A.GetModel<PlayerModel>().CurrentHealth;A.GetSystem<PlayerSystem>().ApplyDamage(20);
				Check(Mathf.Abs(A.GetModel<PlayerModel>().CurrentHealth-(before-10))<0.01f,"spin-reduction-50pct");
				var snapshot=new RunSaveData();combat.Capture(snapshot);
				var json=JsonUtility.ToJson(snapshot);var restored=JsonUtility.FromJson<RunSaveData>(json);combat.Restore(restored);combat.OnRunUpdate(0.01f);
				Check(combat.State.Actions.Count==1 && combat.State.Actions[0].Visuals != null,"sequence-json-restore");
				var age=combat.State.Actions[0].Age;A.GetSystem<RunSystem>().Pause();A.GetSystem<GameLoopSystem>().TickFrame(1);
				Check(Mathf.Abs(combat.State.Actions[0].Age-age)<0.0001f,"pause-freezes-sequence");A.GetSystem<RunSystem>().Resume();
				combat.Reset();target=Target(1);var dodge=A.GetSystem<DodgeSystem>();
				Check(dodge.TryStart(),"sword-dodge-start");
				var dodgeState=A.GetModel<DodgeModel>().Runtime;var dodgeConfig=A.GetUtility<DodgeCatalog>().Get(dodgeState.DodgeId);
				combat.TickDodge(dodgeConfig,dodgeState,Vector2.zero,new Vector2(1,0));combat.TickDodge(dodgeConfig,dodgeState,Vector2.zero,new Vector2(1,0));
				Check(Mathf.Abs(health.GetCurrentHealth(target)-(1000-12*multiplier))<0.01f,"sword-dodge-deduplicates");
				Begin(3);target=Target(3);combat=A.GetSystem<CharacterCombatSystem>();
				multiplier=A.GetSystem<StatSystem>().GetAttackDamageMultiplier();
				A.GetSystem<DodgeSystem>().TryStart();dodgeState=A.GetModel<DodgeModel>().Runtime;dodgeConfig=A.GetUtility<DodgeCatalog>().Get(dodgeState.DodgeId);
				combat.EndDodge(dodgeConfig,dodgeState);
				A.GetSystem<AttackSystem>().OnRunUpdate(0.01f);
				var bolt=A.GetSystem<ProjectileSystem>().GetSaveData().Single();
				Check(Mathf.Abs(bolt.Damage-16*multiplier)<0.01f && bolt.RemainingPierce==4 && combat.State.EmpowerRemaining==0,"reload-bolt-16-five-targets-consumed");
				A.GetSystem<ProjectileSystem>().Reset();A.GetSystem<PlayerLoadoutSystem>().TryUseSkills();
				combat.OnRunUpdate(1.21f);Check(ProjectileCount()==6,"hunter-six-shots");
				A.GetSystem<ProjectileSystem>().Reset();A.GetSystem<EnemySystem>().Reset();combat.Reset();
				A.GetSystem<AttackSystem>().OnRunUpdate(20);A.GetSystem<PlayerLoadoutSystem>().TryUseSkills();
				Check(combat.State.Actions.Count==0 && A.GetSystem<AttackSystem>().GetSkillCooldownState().Remaining==0,"hunter-empty-no-cooldown");
				Begin(1);target=Target(3);combat=A.GetSystem<CharacterCombatSystem>();A.GetSystem<PlayerLoadoutSystem>().TryUseSkills();
				multiplier=A.GetSystem<StatSystem>().GetAttackDamageMultiplier();
				combat.OnRunUpdate(0.36f);Check(ProjectileCount()==4 && combat.State.Actions[0].Visuals.Count==4,"star-four-drones-first-volley");
				combat.OnRunUpdate(2.5f);Check(ProjectileCount()==24,"star-six-volleys");
				combat.OnRunUpdate(0.5f);Check(combat.State.Actions.Count==0 && Mathf.Abs(health.GetCurrentHealth(target)-(1000-24*multiplier))<0.01f,"star-final-24-once");
				combat.OnRunUpdate(0.5f);Check(Mathf.Abs(health.GetCurrentHealth(target)-(1000-24*multiplier))<0.01f,"star-no-repeated-final");
				A.GetSystem<RunSystem>().ExitToCharacterSelection();Check(combat.State.Actions.Count==0 && combat.State.EmpowerRemaining==0,"exit-clears-state");
				return string.Join("\n",Results);
			}
			finally { A.GetSystem<RunSystem>().ExitToCharacterSelection(); }
		}
		public static string Capture(int character, string path)
		{
			EditorApplication.isPaused=true; Begin(character);Target(3);
			if(character==2) { Target(1.5f);A.GetSystem<AttackSystem>().OnRunUpdate(0.01f);A.GetSystem<CharacterCombatSystem>().OnRunUpdate(0.16f); }
			else { A.GetSystem<PlayerLoadoutSystem>().TryUseSkills();A.GetSystem<CharacterCombatSystem>().OnRunUpdate(0.8f); }
			var go=new GameObject("CharacterCombatPreviewCamera");
			var camera=go.AddComponent<Camera>(); camera.CopyFrom(Camera.main);camera.orthographic=true;camera.orthographicSize=4;
			camera.transform.position=new Vector3(0,0,-10);camera.transform.rotation=Quaternion.identity;
			var render=new RenderTexture(1024,768,24);var texture=new Texture2D(1024,768,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
			try { camera.targetTexture=render;camera.Render();RenderTexture.active=render;texture.ReadPixels(new Rect(0,0,1024,768),0,0);texture.Apply();System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.WriteAllBytes(path,texture.EncodeToPNG()); }
			finally { RenderTexture.active=previous;camera.targetTexture=null;render.Release();UnityEngine.Object.DestroyImmediate(render);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(go);A.GetSystem<RunSystem>().ExitToCharacterSelection(); }
			return path;
		}
		public static string Boundaries()
		{
			Results.Clear();EditorApplication.isPaused=true;
			GameObject wall=null;
			try
			{
				Begin(2);var target=Target(1);
				wall=new GameObject("MoveBlockerTilemap");wall.AddComponent<UnityEngine.Tilemaps.Tilemap>();wall.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
				var blocker=new GameObject("TestWall");blocker.transform.SetParent(wall.transform);blocker.transform.position=new Vector3(2,0);var box=blocker.AddComponent<BoxCollider2D>();box.size=new Vector2(0.2f,10);Physics2D.SyncTransforms();
				A.GetSystem<EnemySystem>().Knockback(target,Vector2.right*4);
				Check(target.GetComponent<Rigidbody2D>().position.x<1.9f,"knockback-stops-at-wall");
				A.GetSystem<EnemySystem>().Reset();A.GetSystem<DodgeSystem>().TryStart();A.GetSystem<DodgeSystem>().OnRunFixedUpdate(1);
				Check(!A.GetModel<DodgeModel>().Runtime.IsActive && A.GetModel<PlayerModel>().Position.x<1.9f && A.GetModel<PlayerModel>().DodgeInvulnerabilityRemaining==0,"dodge-wall-stops-invulnerability");
				UnityEngine.Object.DestroyImmediate(wall);wall=null;
				Begin(3);Target(3);var loadout=A.GetSystem<PlayerLoadoutSystem>();var model=A.GetModel<PlayerLoadoutModel>();
				for(int i=0;i<4;i++) Check(loadout.UpgradeWeapon(model.Weapons[0].RuntimeId),"weapon-upgrade-"+(i+2));
				Check(A.GetSystem<DodgeSystem>().Upgrade() && A.GetSystem<DodgeSystem>().Upgrade(),"dodge-max-level");
				var upgrade=A.GetSystem<CharacterExclusiveSkillUpgradeSystem>();Check(upgrade.GetEligible()!=null,"exclusive-upgrade-eligible");
				Check(upgrade.Upgrade(model.Skills[0].SkillId)&&upgrade.Upgrade(model.Skills[0].SkillId),"skill-max-level");
				loadout.TryUseSkills();A.GetSystem<CharacterCombatSystem>().OnRunUpdate(1.61f);
				Check(ProjectileCount()==8,"max-hunter-eight-shots");
				Check(Mathf.Abs(A.GetSystem<AttackSystem>().GetSkillCooldownState().Duration-10*A.GetSystem<StatSystem>().GetCooldownMultiplier())<0.01f,"max-skill-cooldown");
				var saved=new RunSaveData();A.GetSystem<CharacterCombatSystem>().State.EmpowerRemaining=2;A.GetSystem<CharacterCombatSystem>().Capture(saved);
				A.GetSystem<CharacterCombatSystem>().Reset();A.GetSystem<CharacterCombatSystem>().Restore(JsonUtility.FromJson<RunSaveData>(JsonUtility.ToJson(saved)));
				Check(A.GetSystem<CharacterCombatSystem>().State.EmpowerRemaining==2,"empower-save-restore");
				return string.Join("\n",Results);
			}
			finally { if(wall!=null)UnityEngine.Object.DestroyImmediate(wall);A.GetSystem<RunSystem>().ExitToCharacterSelection(); }
		}
		public static string ProjectilesAndEffects()
		{
			Results.Clear();EditorApplication.isPaused=true;GameObject wall=null;
			try
			{
				Begin(3);
				var config=AssetDatabase.LoadAssetAtPath<EnemyConfig>("Assets/Resources/Configs/Enemy/Enemy_Fast01.asset");
				var entries=new[]{1f,2f,3f}.Select(x=>new EnemySaveData{ConfigId=config.Id,PositionX=x,CurrentHealth=1000,MoveSpeed=0}).ToArray();
				A.GetSystem<EnemySystem>().Restore(entries,0);Physics2D.SyncTransforms();
				var targets=A.GetSystem<CombatTargetSystem>().FindOpponentsInRange(Vector2.zero,CombatFaction.Player,3.2f).Where(x=>EnemyFactory.Instance.GetConfig(x.transform)!=null).OrderBy(x=>x.transform.position.x).ToArray();
				var parameters=AssetDatabase.LoadAssetAtPath<CharacterAttackParameterConfig>("Assets/Resources/Configs/Combat/CharacterKits/Parameters_Bolt.asset");
				var projectile=ProjectileFactory.Instance.Spawn(parameters,Vector2.zero,Vector2.right,CombatFaction.Player,8,16,2);projectile.AdvanceFixed(0.25f);
				Check(targets.Length==3 && targets.All(x=>Mathf.Abs(A.GetSystem<EnemyHealthSystem>().GetCurrentHealth(x)-992)<0.01f),"swept-bolt-three-targets");
				Check(!projectile.gameObject.activeSelf,"pierce-limit-recycles");
				A.GetSystem<EnemySystem>().Restore(entries,0);Physics2D.SyncTransforms();
				targets=A.GetSystem<CombatTargetSystem>().FindOpponentsInRange(Vector2.zero,CombatFaction.Player,3.2f).Where(x=>EnemyFactory.Instance.GetConfig(x.transform)!=null).OrderBy(x=>x.transform.position.x).ToArray();
				wall=new GameObject("ProjectileBlockerTilemap");wall.AddComponent<UnityEngine.Tilemaps.Tilemap>();wall.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
				var blocker=new GameObject("TestProjectileWall");blocker.transform.SetParent(wall.transform);blocker.transform.position=new Vector3(1.5f,0);blocker.AddComponent<BoxCollider2D>().size=new Vector2(0.1f,10);Physics2D.SyncTransforms();
				projectile=ProjectileFactory.Instance.Spawn(parameters,Vector2.zero,Vector2.right,CombatFaction.Player,8,16,2);projectile.AdvanceFixed(0.25f);
				Check(!projectile.gameObject.activeSelf && A.GetSystem<EnemyHealthSystem>().GetCurrentHealth(targets[1])==1000,"swept-bolt-wall-stops");
				UnityEngine.Object.DestroyImmediate(wall);wall=null;
				var combat=A.GetSystem<CharacterCombatSystem>();var dodge=A.GetUtility<DodgeCatalog>().Get(A.GetModel<DodgeModel>().Runtime.DodgeId);
				for(int round=0;round<30;round++)
				{
					combat.BeginDodge(dodge,A.GetModel<DodgeModel>().Runtime);
					for(int frame=0;frame<4;frame++)combat.TickDodge(dodge,A.GetModel<DodgeModel>().Runtime,Vector2.zero,Vector2.right);
					combat.OnRunUpdate(0.3f);
				}
				combat.Reset();Check(UnityEngine.Object.FindObjectsOfType<SpriteRenderer>().Count(x=>x.name.Contains("DodgeAfterimage"))==0,"afterimages-recycled");
				var clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Combat/Deploy.wav");combat.Sound(clip);var audio=UnityEngine.Object.FindObjectOfType<CombatAudioView>();
				Check(audio!=null && audio.Voices.Length==4,"sfx-four-voice-limit");
				A.GetSystem<RunSystem>().Pause();audio.SendMessage("Update");
				var field=typeof(CombatAudioView).GetField("mPaused",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
				Check((bool)field.GetValue(audio),"sfx-paused");A.GetSystem<RunSystem>().Resume();audio.SendMessage("Update");Check(!(bool)field.GetValue(audio),"sfx-resumed");
				A.GetSystem<RunSystem>().BeginLevelUpSelection();audio.SendMessage("Update");Check((bool)field.GetValue(audio),"sfx-levelup-paused");A.GetSystem<RunSystem>().EndLevelUpSelection();
				combat.Reset();Check(!audio.gameObject.activeSelf,"sfx-reset-recycled");
				return string.Join("\n",Results);
			}
			finally {if(wall!=null)UnityEngine.Object.DestroyImmediate(wall);A.GetSystem<RunSystem>().ExitToCharacterSelection();}
		}
	}
}

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor.Editor
{
	public static class UnifiedUISetup
	{
		private const string IconsPath = "Assets/Art/Sprites/Icons/UnifiedAbilityAtlas.png";
		private const string SkinPath = "Assets/Art/Sprites/UI/TealGoldSkin.png";
		private static Sprite[] mIcons;
		private static Sprite[] mSkin;

		[MenuItem("ProjectSurvivor/Apply Readable Loadout And Talents")]
		public static void ApplyReadableLoadout()
		{
			if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
			mIcons = AssetDatabase.LoadAllAssetsAtPath(IconsPath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
			ConfigurePerks();
			var path = "Assets/Art/UIPrefab/UIGameHUDPanel.prefab";
			var root = PrefabUtility.LoadPrefabContents(path);
			try { ConfigureHud(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
			finally { PrefabUtility.UnloadPrefabContents(root); }
			AssetDatabase.SaveAssets();
		}

		[MenuItem("ProjectSurvivor/Apply Unified UI And Perks")]
		public static void Apply()
		{
			if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
			ImportArt();
			ConfigurePerks();
			ConfigureIcons();
			foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Art/UIPrefab" }))
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (path.EndsWith("UIWorldGuideItem.prefab")) continue;
				var root = PrefabUtility.LoadPrefabContents(path);
				try
				{
					SkinPanel(root);
					if (root.name == "UIGameHUDPanel") ConfigureHud(root);
					PrefabUtility.SaveAsPrefabAsset(root, path);
				}
				finally { PrefabUtility.UnloadPrefabContents(root); }
			}
			AssetDatabase.SaveAssets();
			var healthPath = "Assets/Art/Prefabs/HealthBarCanvas.prefab";
			var health = PrefabUtility.LoadPrefabContents(healthPath);
			try { SkinPanel(health); PrefabUtility.SaveAsPrefabAsset(health, healthPath); }
			finally { PrefabUtility.UnloadPrefabContents(health); }
		}

		private static void ImportArt()
		{
			var icons = new SpriteMetaData[25];
			var xs = new[] { 16, 262, 508, 754, 1000 };
			var ys = new[] { 16, 254, 490, 727, 963 };
			for (var i = 0; i < icons.Length; i++)
				icons[i] = Slice($"Ability_{i:00}", new Rect(xs[i % 5], 1254 - ys[i / 5] - 236, 240, 236), 0);
			mIcons = Import(IconsPath, icons);
			mSkin = Import(SkinPath, new[]
			{
				Slice("Skin_0_Panel", new Rect(58, 510, 433, 414), 64),
				Slice("Skin_1_Selected", new Rect(555, 510, 430, 414), 64),
				Slice("Skin_2_Socket", new Rect(1102, 535, 339, 332), 40),
				Slice("Skin_3_Button", new Rect(50, 183, 447, 141), 36),
				Slice("Skin_4_ButtonActive", new Rect(548, 183, 445, 141), 36),
				Slice("Skin_5_Track", new Rect(1048, 214, 445, 70), 24)
			});
		}

		private static SpriteMetaData Slice(string name, Rect rect, float border) => new()
		{ name = name, rect = rect, alignment = 0, pivot = Vector2.one * 0.5f, border = Vector4.one * border };

		private static Sprite[] Import(string path, SpriteMetaData[] slices)
		{
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			var importer = (TextureImporter)AssetImporter.GetAtPath(path);
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritePixelsPerUnit = 100;
			importer.mipmapEnabled = false;
			importer.alphaIsTransparency = true;
			importer.textureCompression = TextureImporterCompression.Uncompressed;
			importer.maxTextureSize = 2048;
			importer.spritesheet = slices;
			importer.SaveAndReimport();
			return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(item => item.name).ToArray();
		}

		private static void ConfigurePerks()
		{
			var config = AssetDatabase.LoadAssetAtPath<CharacterExclusivePerkConfig>("Assets/Resources/Configs/Progression/CharacterExclusivePerkCatalog.asset");
			var survivor = config.Perks.Find(p => p.Id == 3);
			survivor.DisplayName = "星环蓄能";
			survivor.Description = "施放星环超载后，武器攻击冷却缩减12%/18%/24%，持续3/4/5秒。";
			SetPerk(config, 4, CharacterExclusivePerkType.MissingHealthDamage, "不屈剑心", "每损失10%生命，伤害提高4%/6%/8%；满血无加成。", new[] { .4f, .6f, .8f }, 0);
			SetPerk(config, 5, CharacterExclusivePerkType.DodgeDamageBoost, "破阵追击", "锋刃突进结束后2.5秒，伤害提高20%/30%/40%；再次触发刷新时间。", new[] { .2f, .3f, .4f }, 2.5f);
			SetPerk(config, 6, CharacterExclusivePerkType.SkillDamageReduction, "剑刃壁垒", "断阵回旋后3/4/5秒，所受伤害降低15%/20%/25%；与回旋减伤相乘。", new[] { .15f, .2f, .25f }, 3);
			for (var i = 0; i < 3; i++) config.Perks.Find(p => p.Id == 6).LevelUpgrades[i].Duration = 3 + i;
			SetPerk(config, 7, CharacterExclusivePerkType.MovingDamage, "游猎本能", "移动输入期间，伤害提高12%/20%/28%；停止移动后立即结束。", new[] { .12f, .2f, .28f }, 0);
			SetPerk(config, 8, CharacterExclusivePerkType.DodgeWeaponProjectileCount, "机动弹匣", "掠影装填结束后2/3/4秒，武器弹数增加1/1/2；再次触发刷新时间。", new[] { 1f, 1f, 2f }, 2);
			for (var i = 0; i < 3; i++) config.Perks.Find(p => p.Id == 8).LevelUpgrades[i].Duration = 2 + i;
			SetPerk(config, 9, CharacterExclusivePerkType.SkillWeaponCooldownReduction, "猎杀窗口", "猎杀连射后3/4/5秒，武器攻击冷却缩减15%/22%/30%。", new[] { .15f, .22f, .3f }, 3);
			for (var i = 0; i < 3; i++) config.Perks.Find(p => p.Id == 9).LevelUpgrades[i].Duration = 3 + i;
			foreach (var perk in config.Perks)
			{
				var index = perk.CharacterId == 1 ? 17 + perk.Id : perk.CharacterId == 2
					? perk.Type == CharacterExclusivePerkType.MissingHealthDamage ? 21 : perk.Type == CharacterExclusivePerkType.DodgeDamageBoost ? 22 : 23
					: perk.Type == CharacterExclusivePerkType.MovingDamage ? 24 : perk.Type == CharacterExclusivePerkType.DodgeWeaponProjectileCount ? 11 : 8;
				perk.Icon = mIcons[index];
			}
			EditorUtility.SetDirty(config);
			var survivorCharacter = AssetDatabase.LoadAssetAtPath<CharacterConfig>("Assets/Resources/Configs/Character/Character_Survivor.asset");
			survivorCharacter.SkillDescription = "专属技能：星环超载，浮游炮齐射后释放冲击环。\n专属被动：绝境意志、战术翻滚、星环蓄能。";
			EditorUtility.SetDirty(survivorCharacter);
			foreach (var slug in new[] { "Vanguard", "Scout" })
			{
				var character = AssetDatabase.LoadAssetAtPath<CharacterConfig>($"Assets/Resources/Configs/Character/Character_{slug}.asset");
				character.SkillDescription = slug == "Vanguard" ? "长剑近战，越战越勇。\n专属技能：断阵回旋；闪避：锋刃突进。\n独特天赋：不屈剑心、破阵追击、剑刃壁垒。" : "移动射击，闪避装填。\n专属技能：猎杀连射；闪避：掠影装填。\n独特天赋：游猎本能、机动弹匣、猎杀窗口。";
				EditorUtility.SetDirty(character);
			}
		}

		private static void SetPerk(CharacterExclusivePerkConfig config, int id, CharacterExclusivePerkType type, string name, string description, float[] values, float duration)
		{
			var perk = config.Perks.Find(p => p.Id == id);
			if (perk == null) throw new InvalidOperationException($"Missing perk {id}");
			perk.Type = type; perk.DisplayName = name; perk.Description = description;
			for (var i = 0; i < values.Length; i++) { perk.LevelUpgrades[i].Value = values[i]; perk.LevelUpgrades[i].Duration = duration; }
		}

		private static void AddPerk(CharacterExclusivePerkConfig config, int character, CharacterExclusivePerkType type, string name, string description, float[] values, float duration, float threshold, int dodge, int skill)
		{
			if (config.Perks.Any(p => p.CharacterId == character && p.Type == type)) return;
			var perk = new CharacterExclusivePerkDefinition { Id = config.Perks.Max(p => p.Id) + 1, CharacterId = character, Type = type, DisplayName = name, Description = description, HealthThreshold = threshold, TriggerDodgeId = dodge, TriggerSkillId = skill };
			for (var i = 0; i < 3; i++) perk.LevelUpgrades.Add(new CharacterExclusivePerkLevel { Level = i + 1, Value = values[i], Duration = duration });
			config.Perks.Add(perk);
		}

		private static void ConfigureIcons()
		{
			foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Resources/Configs" }))
			{
				var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
				if (asset is WeaponConfig weapon) weapon.Icon = mIcons[weapon.Id == 5 ? 5 : weapon.Id == 6 ? 7 : weapon.Id == 2 ? 1 : weapon.Id == 3 ? 2 : weapon.Id == 4 ? 3 : 0];
				else if (asset is SkillConfig skill) skill.Icon = mIcons[skill.Id == 2 ? 6 : skill.Id == 3 ? 8 : 4];
				else if (asset is DodgeConfig dodge) dodge.Icon = mIcons[dodge.Id == 2 ? 10 : dodge.Id == 3 ? 11 : 9];
				else
				{
					var serialized = new SerializedObject(asset);
					var iterator = serialized.GetIterator();
					while (iterator.Next(true))
					{
						if (iterator.propertyType != SerializedPropertyType.ObjectReference || iterator.name != "Icon" || !(iterator.objectReferenceValue is Sprite sprite)) continue;
						if (!AssetDatabase.GetAssetPath(sprite).EndsWith("/SkillAtlas.png")) continue;
						var index = sprite.name switch { "Projectile" => 0, "Piercing" => 1, "FireBall" => 2, "Inferno" => 3, "Barrage" => 4, "Dodge" => 9, "Attack" => 5, "Cooldown" => 14, "Absorb" => 16, "Experience" => 15, "Speed" => 13, "Heal" => 12, "Recovery" => 12, "Resilience" => 18, "Roll" => 19, "Charge" => 20, _ => 0 };
						iterator.objectReferenceValue = mIcons[index];
					}
					serialized.ApplyModifiedPropertiesWithoutUndo();
				}
				if (asset is WeaponConfig || asset is SkillConfig || asset is DodgeConfig) EditorUtility.SetDirty(asset);
			}
			var upgrades = AssetDatabase.LoadAssetAtPath<CharacterExclusiveSkillUpgradeConfig>("Assets/Resources/Configs/Progression/CharacterExclusiveSkillUpgradeCatalog.asset");
			var so = new SerializedObject(upgrades);
			var entries = so.FindProperty("Upgrades");
			if (entries != null) for (var i = 0; i < entries.arraySize; i++) { var entry = entries.GetArrayElementAtIndex(i); entry.FindPropertyRelative("Icon").objectReferenceValue = mIcons[entry.FindPropertyRelative("CharacterId").intValue == 2 ? 6 : entry.FindPropertyRelative("CharacterId").intValue == 3 ? 8 : 4]; }
			so.ApplyModifiedPropertiesWithoutUndo();
		}

		private static void SkinPanel(GameObject root)
		{
			foreach (var button in root.GetComponentsInChildren<Button>(true))
				foreach (var label in button.GetComponentsInChildren<Text>(true))
					if (label.color.r < .2f && label.color.g < .2f) label.color = new Color(.9f, .95f, .94f);
			foreach (var image in root.GetComponentsInChildren<Image>(true))
			{
				var name = image.name;
				if (name.Contains("Icon")) { image.color = Color.white; image.preserveAspect = true; continue; }
				if (name.Contains("Icon") || name == "Viewport" || image.type == Image.Type.Filled || name == "Fill" || name.Contains("UpgradePoint")) continue;
				if (name == "Image_Mask" || (root.name == "UIGameHUDPanel" && name == "Panel") || (root.name == "UIPopPanel" && (name == "BG" || name == "ButtonGroup"))) continue;
				var button = image.GetComponent<Button>();
				if (button != null && name == "Button_Select") { image.color = Color.clear; continue; }
				image.sprite = mSkin[button != null ? 3 : name.Contains("Track") || name == "Background" ? 5 : name == "Image_Select" ? 1 : 0];
				image.type = Image.Type.Sliced;
				image.pixelsPerUnitMultiplier = name.Contains("Track") ? 6 : 2;
				image.color = Color.white;
				if (button != null)
				{
					button.transition = Selectable.Transition.SpriteSwap;
					button.spriteState = new SpriteState { highlightedSprite = mSkin[4], pressedSprite = mSkin[4], selectedSprite = mSkin[4], disabledSprite = mSkin[3] };
				}
			}
		}

		private static void ConfigureHud(GameObject root)
		{
			var existing = root.transform.Find("LoadoutPanel");
			if (existing != null)
			{
				var panel = (RectTransform)existing; panel.sizeDelta = new Vector2(684, 232); panel.anchoredPosition = new Vector2(365, -300);
				var panelBackground = existing.GetComponent<Image>(); panelBackground.sprite = null; panelBackground.type = Image.Type.Simple; panelBackground.color = new Color(.035f, .055f, .075f, .9f);
				foreach (var pair in new[] { ("WeaponHeading", "武器", 94f), ("SkillHeading", "通用技能", -16f) })
				{
					var heading = existing.Find(pair.Item1).GetComponent<Text>(); heading.text = pair.Item2; heading.fontSize = 18; heading.alignment = TextAnchor.MiddleLeft;
					((RectTransform)heading.transform).anchoredPosition = new Vector2(-280, pair.Item3); ((RectTransform)heading.transform).sizeDelta = new Vector2(100, 24);
				}
				for (var row = 0; row < 2; row++) for (var i = 0; i < 6; i++)
				{
					var item = (RectTransform)existing.Find($"{(row == 0 ? "Weapon" : "Skill")}_{i}"); item.anchoredPosition = new Vector2(-270 + i * 108, 40 - row * 110); item.sizeDelta = new Vector2(104, 80);
					var socket = item.GetComponent<Image>(); socket.sprite = null; socket.type = Image.Type.Simple; socket.color = new Color(.08f, .11f, .14f, .95f);
					var icon = (RectTransform)item.Find("Icon"); icon.sizeDelta = new Vector2(48, 48); icon.anchoredPosition = new Vector2(0, 12);
					var name = item.Find("Name").GetComponent<Text>(); name.fontSize = 15; name.color = Color.white; name.resizeTextForBestFit = true; name.resizeTextMinSize = 12; name.resizeTextMaxSize = 15;
					((RectTransform)name.transform).sizeDelta = new Vector2(100, 22); ((RectTransform)name.transform).anchoredPosition = new Vector2(0, -27);
					var level = item.Find("Level").GetComponent<Text>(); level.fontSize = 12; level.color = new Color(.64f, .83f, .84f); ((RectTransform)level.transform).anchoredPosition = new Vector2(30, 27); ((RectTransform)level.transform).sizeDelta = new Vector2(42, 18);
				}
				return;
			}
			var parent = Rect(root.transform, "LoadoutPanel", new Vector2(550, 218), new Vector2(306, -230));
			parent.anchorMin = parent.anchorMax = new Vector2(0, 1);
			var background = parent.gameObject.AddComponent<Image>(); background.sprite = mSkin[0]; background.type = Image.Type.Sliced; background.pixelsPerUnitMultiplier = 3; background.raycastTarget = false;
			var font = root.GetComponentsInChildren<Text>(true).First().font;
			Text(parent, "WeaponHeading", "武器", new Vector2(-231, 92), new Vector2(64, 26), font, 19);
			Text(parent, "SkillHeading", "技能", new Vector2(-231, -8), new Vector2(64, 26), font, 19);
			var weaponSlots = new RunLoadoutView.Slot[6]; var skillSlots = new RunLoadoutView.Slot[6];
			for (var row = 0; row < 2; row++) for (var i = 0; i < 6; i++)
			{
				var item = Rect(parent, $"{(row == 0 ? "Weapon" : "Skill")}_{i}", new Vector2(76, 78), new Vector2(-207 + i * 82, 38 - row * 100));
				var socket = item.gameObject.AddComponent<Image>(); socket.sprite = mSkin[2]; socket.type = Image.Type.Sliced; socket.pixelsPerUnitMultiplier = 4; socket.raycastTarget = false;
				var icon = Rect(item, "Icon", new Vector2(49, 49), new Vector2(0, 9)).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
				var name = Text(item, "Name", "空位", new Vector2(0, -27), new Vector2(76, 20), font, 13);
				var level = Text(item, "Level", "", new Vector2(18, 28), new Vector2(38, 17), font, 13); level.color = new Color(1, .85f, .45f);
				(row == 0 ? weaponSlots : skillSlots)[i] = new RunLoadoutView.Slot { Icon = icon, Name = name, Level = level };
			}
			var view = parent.gameObject.AddComponent<RunLoadoutView>(); view.Weapons = weaponSlots; view.Skills = skillSlots;
		}

		private static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position)
		{
			var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
		}
		private static Text Text(Transform parent, string name, string value, Vector2 position, Vector2 size, Font font, int fontSize)
		{
			var text = Rect(parent, name, size, position).gameObject.AddComponent<Text>(); text.text = value; text.font = font; text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter; text.color = new Color(.9f, .95f, .94f); text.raycastTarget = false; return text;
		}
	}
}

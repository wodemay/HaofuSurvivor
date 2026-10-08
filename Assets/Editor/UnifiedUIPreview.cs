using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HaoFuSurvivor.Editor
{
	public static class UnifiedUIPreview
	{
		public static void Export()
		{
			Directory.CreateDirectory("Logs/UIRefreshReview");
			foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Art/UIPrefab" }))
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (path.EndsWith("UIWorldGuideItem.prefab")) continue;
				Render(path);
			}
		}
		private static void Render(string path)
		{
			var scene = EditorSceneManager.NewPreviewScene();
			var cameraObject = new GameObject("PreviewCamera", typeof(Camera));
			SceneManager.MoveGameObjectToScene(cameraObject, scene);
			var camera = cameraObject.GetComponent<Camera>(); camera.scene = scene; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.018f, .033f, .046f); camera.transform.position = new Vector3(0, 0, -10);
			var canvasObject = new GameObject("PreviewCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
			SceneManager.MoveGameObjectToScene(canvasObject, scene);
			var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
			var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
			var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), canvasObject.transform);
			var rect = root.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
			var target = new RenderTexture(1920, 1080, 24); var previous = RenderTexture.active; var texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
			try
			{
				root.SetActive(true); Populate(root);
				camera.targetTexture = target;
				Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(rect); Canvas.ForceUpdateCanvases();
				foreach (var layout in root.GetComponentsInChildren<LayoutGroup>(true))
				{
					layout.CalculateLayoutInputHorizontal(); layout.SetLayoutHorizontal(); layout.CalculateLayoutInputVertical(); layout.SetLayoutVertical();
				}
				Canvas.ForceUpdateCanvases();
				camera.Render(); RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); texture.Apply();
				File.WriteAllBytes("Logs/UIRefreshReview/" + Path.GetFileNameWithoutExtension(path) + ".png", texture.EncodeToPNG());
			}
			finally { RenderTexture.active = previous; camera.targetTexture = null; target.Release(); Object.DestroyImmediate(target); Object.DestroyImmediate(texture); EditorSceneManager.ClosePreviewScene(scene); }
		}
		private static void Populate(GameObject root)
		{
			var icons = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/Icons/UnifiedAbilityAtlas.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
			var hud = root.GetComponentInChildren<RunLoadoutView>(true);
			if (hud != null)
			{
				var names = new[] { "裂阵剑术", "火球术", "投射物武器", "空位", "空位", "空位", "攻击强化", "冷却缩减", "经验加成", "移动加速", "自然恢复", "空位" };
				var indexes = new[] { 5, 2, 0, -1, -1, -1, 5, 14, 15, 13, 12, -1 };
				var slots = hud.Weapons.Concat(hud.Skills).ToArray();
				for (var i = 0; i < slots.Length; i++) { slots[i].Name.text = names[i]; slots[i].Level.text = indexes[i] >= 0 ? "Lv.2" : ""; slots[i].Icon.enabled = indexes[i] >= 0; if (indexes[i] >= 0) slots[i].Icon.sprite = icons[indexes[i]]; }
			}
			var characterTemplate = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "UICharacterSelectItem");
			if (characterTemplate != null)
			{
				var characters = AssetDatabase.FindAssets("t:CharacterConfig", new[] { "Assets/Resources/Configs/Character" }).Select(g => AssetDatabase.LoadAssetAtPath<CharacterConfig>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(c => c.Id).ToArray();
				var content = root.GetComponentsInChildren<Transform>(true).First(t => t.name == "CharacterGroup_Content");
				foreach (var character in characters) { var item = Object.Instantiate(characterTemplate.gameObject, content); item.SetActive(true); var view = item.GetComponent<UICharacterSelectItem>() ?? item.AddComponent<UICharacterSelectItem>(); view.Initialize(character, _ => { }); view.SetSelected(character.Id == 2); }
				characterTemplate.gameObject.SetActive(false);
			}
			var upgradeTemplate = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "UILevelUpOptionItemTemplate");
			if (upgradeTemplate != null)
			{
				var perks = AssetDatabase.LoadAssetAtPath<CharacterExclusivePerkConfig>("Assets/Resources/Configs/Progression/CharacterExclusivePerkCatalog.asset").Perks.Where(p => p.CharacterId == 2);
				foreach (var perk in perks) { var item = Object.Instantiate(upgradeTemplate.gameObject, upgradeTemplate.parent); item.SetActive(true); item.transform.Find("Image_Icon").GetComponent<Image>().sprite = perk.Icon; item.transform.Find("TextGroup/Text_Name").GetComponent<Text>().text = perk.DisplayName; item.transform.Find("TextGroup/Text_Level").GetComponent<Text>().text = "专属被动 · Lv.1"; item.transform.Find("TextGroup/Text_Description").GetComponent<Text>().text = perk.Description; }
				upgradeTemplate.gameObject.SetActive(false);
			}
			var metaTemplate = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "UpgradeItem");
			if (metaTemplate != null)
			{
				var definitions = AssetDatabase.LoadAssetAtPath<MetaUpgradeCatalogConfig>("Assets/Resources/Configs/Progression/MetaUpgradeCatalog.asset").Upgrades;
				foreach (var definition in definitions)
				{
					var item = Object.Instantiate(metaTemplate.gameObject, metaTemplate.parent); item.SetActive(true);
					item.transform.Find("Icon").GetComponent<Image>().sprite = definition.Icon;
					item.transform.Find("Icon").GetComponent<Image>().color = Color.white;
					item.transform.Find("TextGroup/Text_Name").GetComponent<Text>().text = definition.DisplayName;
					item.transform.Find("TextGroup/Text_Description").GetComponent<Text>().text = definition.Description;
					item.transform.Find("TextGroup/Text_Coin").GetComponent<Text>().text = "金币：" + definition.GetCost(1);
				}
				metaTemplate.gameObject.SetActive(false);
			}
			if (root.name.StartsWith("UIPopPanel")) foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.SetActive(true);
			foreach (var text in root.GetComponentsInChildren<Text>(true))
			{
				if (text.name == "Text_RemainingTime") text.text = "08:32";
				if (text.name == "Text_SkillName") text.text = "断阵回旋";
				if (text.name == "Text_SkillCooldown") text.text = "冷却  4.2s";
				if (text.name == "Text_PlayerLevel") text.text = "Lv. 12";
				if (text.name == "Text_Experience") text.text = "经验  85 / 150";
				if (text.name == "Text_Content" && root.name.StartsWith("UIPopPanel")) text.text = "返回主菜单前将保存本局进度。\n你可以稍后继续这场冒险。";
				if (text.name == "Text_Title" && root.name.StartsWith("UIPopPanel")) text.text = "返回主菜单";
			}
		}
	}
}

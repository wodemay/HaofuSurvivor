using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HaoFuSurvivor.Editor
{
	public class ConfigurationCreatorWindow : EditorWindow
	{
		private const string CharacterConfigFolder = "Assets/Resources/Configs/Characters";
		private const string EnemyConfigFolder = "Assets/Resources/Configs/Enemies";
		private const string CharacterPrefabFolder = "Assets/Art/Prefabs/Characters";
		private const string EnemyPrefabFolder = "Assets/Art/Prefabs/Enemies";
		private const string EnemyCatalogPath = "Assets/Resources/Configs/Enemies/EnemyCatalog.asset";

		private int mTab;
		private int mCharacterId;
		private string mCharacterName = "New Character";
		private string mSkillDescription;
		private int mSkillGroupId;
		private int mCharacterSortOrder;
		private float mCharacterMaxHealth = 100f;
		private float mCharacterMoveSpeed = 5f;
		private float mCharacterAttackPower = 10f;
		private Sprite mCharacterIcon;
		private GameObject mCharacterPrefab;
		private bool mCreateCharacterPrefab = true;

		private int mEnemyId;
		private float mEnemyMoveSpeed = 2f;
		private string mEnemyAttackIds = "1001";
		private GameObject mEnemyPrefab;
		private bool mCreateEnemyPrefab = true;

		[MenuItem("ProjectSurvivor/Configuration Creator")]
		private static void Open()
		{
			GetWindow<ConfigurationCreatorWindow>("Config Creator");
		}

		private void OnEnable()
		{
			mCharacterId = GetNextCharacterId();
			mEnemyId = GetNextEnemyId();
		}

		private void OnGUI()
		{
			mTab = GUILayout.Toolbar(mTab, new[] { "Character", "Enemy" });
			EditorGUILayout.Space();

			if (mTab == 0) DrawCharacterCreator();
			else DrawEnemyCreator();
		}

		private void DrawCharacterCreator()
		{
			mCharacterId = EditorGUILayout.IntField("Id", mCharacterId);
			mCharacterName = EditorGUILayout.TextField("Display Name", mCharacterName);
			mSkillDescription = EditorGUILayout.TextArea(mSkillDescription, GUILayout.MinHeight(48f));
			mSkillGroupId = EditorGUILayout.IntField("Skill Group Id", mSkillGroupId);
			mCharacterSortOrder = EditorGUILayout.IntField("Sort Order", mCharacterSortOrder);
			mCharacterMaxHealth = EditorGUILayout.FloatField("Max Health", mCharacterMaxHealth);
			mCharacterMoveSpeed = EditorGUILayout.FloatField("Move Speed", mCharacterMoveSpeed);
			mCharacterAttackPower = EditorGUILayout.FloatField("Attack Power", mCharacterAttackPower);
			mCharacterIcon = (Sprite)EditorGUILayout.ObjectField("Icon", mCharacterIcon, typeof(Sprite), false);
			mCharacterPrefab = (GameObject)EditorGUILayout.ObjectField("Content Prefab", mCharacterPrefab, typeof(GameObject), false);
			mCreateCharacterPrefab = EditorGUILayout.Toggle("Create Empty Prefab", mCreateCharacterPrefab);

			using (new EditorGUI.DisabledScope(mCharacterId <= 0 || string.IsNullOrWhiteSpace(mCharacterName) || CharacterIdExists(mCharacterId) || (mCharacterPrefab == null && !mCreateCharacterPrefab)))
			{
				if (GUILayout.Button("Create Character")) CreateCharacter();
			}
			DrawIdWarning(CharacterIdExists(mCharacterId));
		}

		private void DrawEnemyCreator()
		{
			mEnemyId = EditorGUILayout.IntField("Id", mEnemyId);
			mEnemyMoveSpeed = EditorGUILayout.FloatField("Move Speed", mEnemyMoveSpeed);
			mEnemyAttackIds = EditorGUILayout.TextField("Attack Ids", mEnemyAttackIds);
			mEnemyPrefab = (GameObject)EditorGUILayout.ObjectField("Content Prefab", mEnemyPrefab, typeof(GameObject), false);
			mCreateEnemyPrefab = EditorGUILayout.Toggle("Create Empty Prefab", mCreateEnemyPrefab);

			using (new EditorGUI.DisabledScope(mEnemyId <= 0 || EnemyIdExists(mEnemyId) || (mEnemyPrefab == null && !mCreateEnemyPrefab)))
			{
				if (GUILayout.Button("Create Enemy")) CreateEnemy();
			}
			DrawIdWarning(EnemyIdExists(mEnemyId));
		}

		private static void DrawIdWarning(bool exists)
		{
			if (exists) EditorGUILayout.HelpBox("Id already exists.", MessageType.Error);
		}

		private void CreateCharacter()
		{
			EnsureFolder(CharacterConfigFolder);
			var prefab = mCharacterPrefab != null ? mCharacterPrefab : mCreateCharacterPrefab
				? CreateContentPrefab(CharacterPrefabFolder, $"Character_{mCharacterId}")
				: null;
			var config = CreateInstance<CharacterConfig>();
			config.Id = mCharacterId;
			config.DisplayName = mCharacterName;
			config.SkillDescription = mSkillDescription;
			config.SkillGroupId = mSkillGroupId;
			config.SortOrder = mCharacterSortOrder;
			config.MaxHealth = mCharacterMaxHealth;
			config.MoveSpeed = mCharacterMoveSpeed;
			config.AttackPower = mCharacterAttackPower;
			config.Icon = mCharacterIcon;
			config.PlayerPrefab = prefab;
			AssetDatabase.CreateAsset(config, $"{CharacterConfigFolder}/Character_{mCharacterId}.asset");
			AssetDatabase.SaveAssets();
			Selection.activeObject = config;
			mCharacterId = GetNextCharacterId();
		}

		private void CreateEnemy()
		{
			EnsureFolder(EnemyConfigFolder);
			var prefab = mEnemyPrefab != null ? mEnemyPrefab : mCreateEnemyPrefab
				? CreateContentPrefab(EnemyPrefabFolder, $"Enemy_{mEnemyId}")
				: null;
			var config = CreateInstance<EnemyConfig>();
			config.Id = mEnemyId;
			config.Prefab = prefab;
			config.MoveSpeed = mEnemyMoveSpeed;
			config.AttackIds = ParseIds(mEnemyAttackIds);
			AssetDatabase.CreateAsset(config, $"{EnemyConfigFolder}/Enemy_{mEnemyId}.asset");
			AddToEnemyCatalog(config);
			AssetDatabase.SaveAssets();
			Selection.activeObject = config;
			mEnemyId = GetNextEnemyId();
		}

		private static GameObject CreateContentPrefab(string folder, string name)
		{
			EnsureFolder(folder);
			var path = $"{folder}/{name}.prefab";
			var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (existing != null) return existing;

			var root = new GameObject(name);
			new GameObject("Visual").transform.SetParent(root.transform, false);
			var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
			DestroyImmediate(root);
			return prefab;
		}

		private static void AddToEnemyCatalog(EnemyConfig config)
		{
			var catalog = AssetDatabase.LoadAssetAtPath<EnemyCatalogConfig>(EnemyCatalogPath);
			if (catalog == null) throw new InvalidOperationException("EnemyCatalog.asset is missing.");
			var serializedCatalog = new SerializedObject(catalog);
			var enemies = serializedCatalog.FindProperty("Enemies");
			enemies.arraySize++;
			enemies.GetArrayElementAtIndex(enemies.arraySize - 1).objectReferenceValue = config;
			serializedCatalog.ApplyModifiedProperties();
		}

		private static System.Collections.Generic.List<int> ParseIds(string value)
		{
			return value.Split(',')
				.Select(id => int.TryParse(id.Trim(), out var parsedId) ? parsedId : 0)
				.Where(id => id > 0)
				.Distinct()
				.ToList();
		}

		private static void EnsureFolder(string path)
		{
			var current = "Assets";
			foreach (var segment in path.Split('/').Skip(1))
			{
				var next = $"{current}/{segment}";
				if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segment);
				current = next;
			}
		}

		private static int GetNextCharacterId()
		{
			return Resources.LoadAll<CharacterConfig>("Configs/Characters").Select(config => config.Id).DefaultIfEmpty(0).Max() + 1;
		}

		private static int GetNextEnemyId()
		{
			return Resources.LoadAll<EnemyConfig>("Configs/Enemies").Select(config => config.Id).DefaultIfEmpty(0).Max() + 1;
		}

		private static bool CharacterIdExists(int id)
		{
			return Resources.LoadAll<CharacterConfig>("Configs/Characters").Any(config => config.Id == id);
		}

		private static bool EnemyIdExists(int id)
		{
			return Resources.LoadAll<EnemyConfig>("Configs/Enemies").Any(config => config.Id == id);
		}
	}
}

using QFramework;

namespace HaoFuSurvivor
{
	public class ProfileSystem : AbstractSystem
	{
		private bool mInitialized;
		private bool mSavePending;
		private int mSaveAttempts;

		public void Initialize()
		{
			if (mInitialized) return;
			mInitialized = true;
			var model = this.GetModel<ProfileModel>();
			var storage = this.GetUtility<ProfileStorage>();
			if (!storage.HasProfile())
			{
				model.ProfileCoin = BigCoin.Zero;
				model.IsLoaded = true;
				if (!SaveNow()) this.SendEvent(new ProfileLoadCompletedEvent(true, "Profile 创建失败，请检查游戏目录权限。"));
				return;
			}

			if (storage.TryLoad(out var data, out var status))
			{
				model.ProfileCoin = new BigCoin(data.ProfileCoin);
				model.IsLoaded = true;
				this.SendEvent(new ProfileLoadCompletedEvent(false, string.Empty));
				return;
			}

			model.ProfileCoin = BigCoin.Zero;
			model.IsLoaded = true;
			if (status == ProfileLoadStatus.UnsupportedVersion)
			{
				this.SendEvent(new ProfileLoadCompletedEvent(true, "Profile 版本高于当前程序，已保留原文件。"));
				return;
			}

			if (SaveNow()) this.SendEvent(new ProfileLoadCompletedEvent(true, "Profile 已损坏，旧文件已保留并创建新的存档。"));
			else this.SendEvent(new ProfileLoadCompletedEvent(true, "Profile 已损坏，但新存档创建失败，请检查游戏目录权限。"));
		}

		public void AddCoins(BigCoin amount)
		{
			if (!mInitialized || amount == null) return;
			this.GetModel<ProfileModel>().ProfileCoin = this.GetModel<ProfileModel>().ProfileCoin.AddCoins(amount);
			this.SendEvent(new ProfileCoinsChangedEvent(this.GetModel<ProfileModel>().ProfileCoin));
			mSavePending = true;
			mSaveAttempts = 0;
		}

		public bool TrySpendCoins(BigCoin amount)
		{
			if (!mInitialized || amount == null) return false;
			var model = this.GetModel<ProfileModel>();
			if (!model.ProfileCoin.TrySpendCoins(amount, out var remaining)) return false;
			model.ProfileCoin = remaining;
			this.SendEvent(new ProfileCoinsChangedEvent(model.ProfileCoin));
			mSavePending = true;
			mSaveAttempts = 0;
			return true;
		}

		public void FlushPendingSave()
		{
			if (mSavePending && mSaveAttempts < 3) SaveNow();
		}

		public bool SaveNow()
		{
			var model = this.GetModel<ProfileModel>();
			var data = new ProfileData { ProfileCoin = model.ProfileCoin.ToString() };
			if (!this.GetUtility<ProfileStorage>().Save(data, out var error))
			{
				UnityEngine.Debug.LogError($"Profile save failed: {error}");
				mSavePending = true;
				mSaveAttempts++;
				return false;
			}
			mSavePending = false;
			mSaveAttempts = 0;
			return true;
		}

		protected override void OnInit()
		{
		}
	}

	public readonly struct ProfileState
	{
		public readonly BigCoin ProfileCoin;
		public readonly bool IsLoaded;

		public ProfileState(BigCoin profileCoin, bool isLoaded)
		{
			ProfileCoin = profileCoin;
			IsLoaded = isLoaded;
		}
	}

	public class GetProfileStateQuery : AbstractQuery<ProfileState>
	{
		protected override ProfileState OnDo()
		{
			var model = this.GetModel<ProfileModel>();
			return new ProfileState(model.ProfileCoin, model.IsLoaded);
		}
	}
}

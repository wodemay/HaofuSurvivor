using UnityEngine;
using QFramework;

// 1.请在菜单 编辑器扩展/Namespace Settings 里设置命名空间
// 2.命名空间更改后，生成代码之后，需要把逻辑代码文件（非 Designer）的命名空间手动更改
namespace HaoFuSurvivor
{
	public partial class Player : ViewController
	{
		public float MovementSpeed = 5f;
		public static Player Default;

		private void Awake()
		{
			Default = this;
		}

		private void OnDestroy()
		{
			Default = null;
		}

		void Start()
		{
			HurtBox.OnTriggerEnter2DEvent((collider2D) =>
			{
				this.DestroyGameObjGracefully();
				UIKit.OpenPanel<UIGameOverPanel>();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		private void Update()
		{
			var horizontal = Input.GetAxis("Horizontal");
			var vertical = Input.GetAxis("Vertical");

			var direction = new Vector2(horizontal, vertical).normalized;

			SelfRigidbody2D.velocity = direction * MovementSpeed;
		}
	}
}

using UnityEngine;
using QFramework;

namespace HaoFuSurvivor
{
	public partial class Enemy : ViewController
	{
		public float MovementSpeed = 2f;

		void Start()
		{
			// Code Here
		}

		private void Update()
		{
			if(Player.Default == null)
			{
				return;
			}
			var direction = (Player.Default.transform.position - transform.position).normalized;
			transform.Translate(direction * Time.deltaTime * MovementSpeed);
		}
	}
}

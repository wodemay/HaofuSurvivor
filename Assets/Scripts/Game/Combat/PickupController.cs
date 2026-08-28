using UnityEngine;

namespace HaoFuSurvivor
{
	public class PickupController : MonoBehaviour
	{
		public int TableId { get; private set; }
		public int EntryId { get; private set; }
		public PickupType Type { get; private set; }
		public string Amount { get; private set; }

		public void Configure(int tableId, DropEntry entry)
		{
			TableId = tableId;
			EntryId = entry == null ? 0 : entry.Id;
			Type = entry == null ? PickupType.Coin : entry.Type;
			Amount = entry == null ? "0" : entry.Amount;
		}

		public void MoveTowards(Vector2 target, float distance)
		{
			transform.position = Vector2.MoveTowards(transform.position, target, Mathf.Max(0f, distance));
		}
	}
}

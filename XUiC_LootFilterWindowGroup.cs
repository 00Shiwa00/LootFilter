using Audio;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Scripting;

namespace LootFilter
{
	[Preserve]
	public class XUiC_LootFilterWindowGroup : XUiController
	{
		public override void OnClose()
		{
			base.OnClose();
			int EntityId = GameManager.Instance.GetPersistentLocalPlayer().EntityId;
			EntityPlayerLocal localPlayer = GameManager.Instance.World.GetLocalPlayerFromID(EntityId);
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);

			playerUI.windowManager.CloseIfOpen("lootfilterdraganddrop");
		}
	}
}

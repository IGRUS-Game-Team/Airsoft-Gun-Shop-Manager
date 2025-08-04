using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

//박정민 8/4 추가
//ui 상호작용 시 커서 잠금 해제 기능 추가
//TODO : 커서 잠금 기능 하나의 클래스로 만들어서 그 클래스에서 모든 UI 설정 담당하도록 해야함.
namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		//public bool drop;
		//public bool set;
		//public bool hold;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook && (!MonitorUIModeManager.Instance.getInUIMode() || InGameSettingManager.Instance.GetIsSettingOpen()))
			{
				LookInput(value.Get<Vector2>());
			}
			// if(cursorInputForLook)
			// {
			// 	LookInput(value.Get<Vector2>());
			// }
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}


		// public void OnHold(InputValue value)
		// {
		// 	HoldInput(value.isPressed);
		// }
		// public void OnDrop(InputValue value)
		// {
		// 	DropInput(value.isPressed);
		// }
		// public void OnSet(InputValue value)
		// {
		// 	SetInput(value.isPressed);
		// }
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		// public void HoldInput(bool newHoldState)
		// {
		// 	hold = newHoldState;
		// }

		// public void SetInput(bool newSetState)
		// {
		// 	set = newSetState;
		// }
		// public void DropInput(bool newDropState)
		// {
		// 	drop = newDropState;
		// }



		private void OnApplicationFocus(bool hasFocus)
		{
			if (MonitorUIModeManager.Instance.getInUIMode() || InGameSettingManager.Instance.GetIsSettingOpen()) return;
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}
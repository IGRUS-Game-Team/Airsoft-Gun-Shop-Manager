/// <summary>
/// Project : Easy Build System
/// Class : Demo_InputHandler.cs
/// Namespace : EasyBuildSystem.Demos.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyBuildSystem.Examples.Bases.Scripts
{
	public class Demo_InputHandler : MonoBehaviour
	{
		public static Demo_InputHandler Instance;

		public bool LockMobileControls;

#if ENABLE_INPUT_SYSTEM

		public Vector2 Move { get; set; }

		public Vector2 Look { get; set; }

		public bool Jump { get; set; }

		public bool Sprint { get; set; }

		[SerializeField] InputActionReference m_MoveInputReference;
		[SerializeField] InputActionReference m_LookInputReference;
		[SerializeField] InputActionReference m_JumpInputReference;

		void OnEnable()
		{
			if (m_MoveInputReference == null) 
				m_MoveInputReference = InputReferencerFinder.FindReference("Move");

			m_MoveInputReference.action.Enable();

			if (m_LookInputReference == null)
				m_LookInputReference = InputReferencerFinder.FindReference("Look");

			m_LookInputReference.action.Enable();

			if (m_JumpInputReference == null)
				m_JumpInputReference = InputReferencerFinder.FindReference("Jump");

			m_JumpInputReference.action.Enable();
		}

		void OnDisable()
		{
			m_MoveInputReference.action.Disable();
			m_LookInputReference.action.Disable();
			m_JumpInputReference.action.Disable();
		}

		void Awake()
		{
			Instance = this;
		}

		void Update()
		{
			if (!LockMobileControls)
			{
				MoveInput(new Vector2(m_MoveInputReference.action.ReadValue<Vector2>().x, m_MoveInputReference.action.ReadValue<Vector2>().y));
				LookInput(new Vector2(m_LookInputReference.action.ReadValue<Vector2>().x, -m_LookInputReference.action.ReadValue<Vector2>().y));
				JumpInput(m_JumpInputReference.action.triggered);
			}

			Sprint = true;
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
#if UNITY_ANDROID
			Move = newMoveDirection;
#else
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Move = Vector2.zero;
				return;
			}

			Move = newMoveDirection;
#endif
		}

		public void LookInput(Vector2 newLookDirection)
		{
#if UNITY_ANDROID
			Look = newLookDirection;
#else
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Look = Vector2.zero;
				return;
			}

			Look = newLookDirection;
#endif
		}

		public void JumpInput(bool newJumpState)
		{
			Jump = newJumpState;
		}

#elif EBS_REWIRED

		public Vector2 Move { get; set; }

		public Vector2 Look { get; set; }

		public bool Jump { get; set; }

		public bool Sprint { get; set; }

		private Rewired.Player player;

		public const string REWIRED_PROFILE_NAME = "Easy Build System";

		private void Awake()
		{
			Instance = this;

			player = Rewired.ReInput.players.GetPlayer(REWIRED_PROFILE_NAME);
		}

		void Update()
		{
			if (!LockMobileControls)
			{
				MoveInput(new Vector2(player.GetAxis("Horizontal"), player.GetAxis("Vertical")));
				LookInput(new Vector2(player.GetAxis("Mouse X"), -player.GetAxis("Mouse Y")));
				JumpInput(player.GetButton("Jump"));
			}

			Sprint = true;
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
#if UNITY_ANDROID
			Move = newMoveDirection;
#else
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Move = Vector2.zero;
				return;
			}

			Move = newMoveDirection;
#endif
		}

		public void LookInput(Vector2 newLookDirection)
		{
#if UNITY_ANDROID
			Look = newLookDirection;
#else
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Look = Vector2.zero;
				return;
			}

			Look = newLookDirection;
#endif
		}

		public void JumpInput(bool newJumpState)
		{
			Jump = newJumpState;
		}
#else
		public Vector2 Move { get; set; }

		public Vector2 Look { get; set; }

		public bool Jump { get; set; }

		public bool Sprint { get; set; }

        private void Awake()
        {
			Instance = this;
		}

        void Update()
		{
			if (!LockMobileControls)
			{
				MoveInput(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
				LookInput(new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")));
				JumpInput(Input.GetButton("Jump"));
			}

			Sprint = true;
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
#if UNITY_ANDROID
			Move = newMoveDirection;
#else
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Move = Vector2.zero;
				return;
			}

			Move = newMoveDirection;
#endif
		}

		public void LookInput(Vector2 newLookDirection)
		{
#if UNITY_ANDROID
			Look = newLookDirection;
#else
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Look = Vector2.zero;
				return;
			}

			Look = newLookDirection;
#endif
		}

		public void JumpInput(bool newJumpState)
		{
			Jump = newJumpState;
		}
#endif
	}
}
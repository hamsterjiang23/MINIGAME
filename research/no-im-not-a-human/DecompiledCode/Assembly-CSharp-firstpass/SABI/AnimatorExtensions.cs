using UnityEngine;

namespace SABI;

public static class AnimatorExtensions
{
	public static Animator PauseAnimations(this Animator animator)
	{
		animator.speed = 0f;
		return animator;
	}

	public static Animator ResumeAnimations(this Animator animator)
	{
		animator.speed = 1f;
		return animator;
	}
}

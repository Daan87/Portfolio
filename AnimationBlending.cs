using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractEditor
{
	public class AnimationBlending: MonoBehaviour
	{
		#region VARIABLES
		private const string cFullbodyAnim = "fullbody";
		private const string cEquipmentAnim = "equipment";
		private const string cLeftAnim = "left";
		private const string cRightAnim = "right";
		private const int cAnimStartLayer = 20;

		public SimpleEvent onEnableEquipment;

		/// <summary>
		/// Everything which have a bodypart switch with full behind it
		/// needs to be changed when the proper animations are added with 
		/// the correct naming convention
		/// </summary>
		private Actor mActor;

		private List<AnimationState> mPlayingAnimations;

		private List<AnimationState> mBlendOutAnimations;
		private List<float> mBlendOutTimes;
		private List<float> mBlendOutConstTimes;
		private bool mBlendOutPerformance = false;

		private List<AnimationState> mBlendInAnimations;
		private List<float> mBlendInTimes;
		private List<float> mBlendInConstTimes;
		private bool mBlendInPerformance = false;
		#endregion

		#region PROPERTIES
		public Actor Actor
		{
			set { mActor = value; }
		}

		public bool BlendOutPerformance
		{
			get { return mBlendOutPerformance; }
		}

		public bool BlendInPerformance
		{
			get { return mBlendInPerformance; }
		}
		#endregion

		#region PUBLIC_FUNCTIONS
		/// <summary> Switch the gesture animations and set switch to true to let the animation run </summary>
		public void GestureBlendAnimation(AnimationState state, BodyPart part)
		{
			bool blend = BlendAnimation();
			bool priority = GetPriority(state, part, false);

			if(blend && priority)
			{
				SetBlendingAnimations(state.name, part);
				mBlendOutPerformance = true;
			}
		}

		/// <summary> Return the weight of the blend </summary>
		public float GetWeight(AnimationState stat, BodyPart part)
		{
			GetPlayingAnimations();

			if(GetPriority(stat, part, false))
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		/// <summary> Slowly decrease the weight of the old animation </summary>
		public void BlendOutAnimation(float deltatime)
		{
			int i = 0;
			int blendDone = 0;

			foreach(AnimationState state in mBlendOutAnimations)
			{
				mBlendOutTimes[i] -= deltatime;

				float weight = mBlendOutTimes[i] / mBlendOutConstTimes[i];

				if(weight > 0)
				{
					state.weight = weight;
				}
				else
				{
					++blendDone;
					if(blendDone == mBlendOutAnimations.Count)
					{
						state.weight = 0;
						mBlendOutPerformance = false;
					}
				}

				++i;
			}

			if(mBlendOutAnimations.Count == 0)
			{
				mBlendOutPerformance = false;
			}
		}

		/// <summary> Slowly increase the weight of the animation </summary>
		public void BlendInAnimation(float deltatime)
		{
			int i = 0;
			int blendDone = 0;

			foreach(AnimationState state in mBlendInAnimations)
			{
				mBlendInTimes[i] -= deltatime;

				float weight = 1 - (mBlendInTimes[i] / mBlendInConstTimes[i]);

				if(weight < 1)
				{
					state.weight = weight;
				}
				else
				{
					++blendDone;
					if(blendDone == mBlendInAnimations.Count)
					{
						mBlendInPerformance = false;
					}
				}

				++i;
			}

			if(mBlendInAnimations.Count == 0)
			{
				mBlendInPerformance = false;
			}
		}

		public void Reset()
		{
			mBlendOutPerformance = false;
			mBlendInPerformance = false;
		}

		public void AttachEvent(AnimationState state, BodyPart part, float percentage)
		{
			AnimationEvent ev = new AnimationEvent();
			ev.functionName = "AnimationFadeOut";
			ev.stringParameter = string.Format("{0}/{1}", state.name, part.ToString());
			ev.time = state.length * percentage;
			state.clip.AddEvent(ev);
		}
		#endregion

		#region PRIVATE_FUNCTIONS
		/// <summary>
		/// Check if the new animation should be blended
		/// </summary>
		private bool BlendAnimation()
		{
			GetPlayingAnimations();

			if(mPlayingAnimations.Count == 1)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Get All the current playing animations on the gesture layer
		/// </summary>
		private void GetPlayingAnimations()
		{
			mPlayingAnimations = new List<AnimationState>();

			foreach(AnimationState state in mActor.obj.GetComponent<Animation>())
			{
				if(state.layer > cAnimStartLayer && state.enabled)
				{
					mPlayingAnimations.Add(state);
				}
			}
		}

		/// <summary>
		/// Set all the animations that will be overwritten with the new animation
		/// </summary>
		private void SetBlendingAnimations(string animationName, BodyPart part)
		{
			CreateLists();

			foreach(AnimationState state in mPlayingAnimations)
			{
				if(state.name == animationName)
				{
					continue;
				}

				switch(part)
				{
					case BodyPart.FULL:
						UpdateBlendOutLists(state);
						break;
					case BodyPart.LEFTARM:
						if(state.name.Contains(cLeftAnim))
						{
							UpdateBlendOutLists(state);
						}
						break;
					case BodyPart.RIGHTARM:
						if(state.name.Contains(cRightAnim))
						{
							UpdateBlendOutLists(state);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Call this function when the primary animation is almost finished
		/// </summary>
		private void AnimationFadeOut(string animationName)
		{
			int index = animationName.IndexOf("/");
			string name = animationName.Substring(0, index);
			AnimationState animState = mActor.obj.GetComponent<Animation>()[name];
			string partName = animationName.Substring(index + 1, animationName.Length - (index + 1));
			BodyPart part = (BodyPart)System.Enum.Parse(typeof(BodyPart), partName);

			bool blend = BlendAnimation();
			bool priority = GetPriority(animState, part, true);

			if(!blend || !priority || mBlendOutPerformance || mBlendInPerformance)
			{
				return;
			}

			CreateLists();

			foreach(AnimationState state in mActor.obj.GetComponent<Animation>())
			{
				if(state.layer > cAnimStartLayer && state.enabled)
				{
					if(state.name == name)
					{
						UpdateBlendOutLists(state);
					}
					else
					{
						SetBlendInAnimations(state, animState);
					}
				}
			}

			mBlendOutPerformance = true;
			mBlendInPerformance = true;

			mBlendInAnimations = FilterBlendAnimations();
		}

		/// <summary>
		/// Update the blend out lists
		/// </summary>
		private void UpdateBlendOutLists(AnimationState state)
		{
			if(state.weight != 0)
			{
				mBlendOutAnimations.Add(state);
				mBlendOutTimes.Add(GetBlendTime(state));
				mBlendOutConstTimes.Add(GetBlendTime(state));
			}
		}

		private void SetBlendInAnimations(AnimationState state, AnimationState timeState)
		{
			string name = state.name.Substring(0, state.name.LastIndexOf('_'));
			string filterName = name.Substring(0, name.LastIndexOf('_'));
			AnimationState[] animSelection = mBlendInAnimations.Where(x => x.name.Contains(filterName)).ToArray();

			if(animSelection.Length > 0)
			{
				foreach(AnimationState anim in animSelection)
				{
					if(state.layer < anim.layer)
					{
						int index = mBlendInAnimations.IndexOf(anim);

						mBlendInTimes.RemoveAt(index);
						mBlendInConstTimes.RemoveAt(index);
						mBlendInAnimations.Remove(anim);

						UpdateBlendInLists(state, timeState);
					}
				}
			}
			else
			{
				UpdateBlendInLists(state, timeState);
			}
		}

		private void UpdateBlendInLists(AnimationState blendInState, AnimationState blendOutState)
		{
			mBlendInAnimations.Add(blendInState);
			mBlendInTimes.Add(GetBlendTime(blendOutState));
			mBlendInConstTimes.Add(GetBlendTime(blendOutState));
		}

		/// <summary>
		/// Filter the blending animation list
		/// </summary>
		private List<AnimationState> FilterBlendAnimations()
		{
			List<AnimationState> tempList = new List<AnimationState>();

			AnimationState[] full = mBlendInAnimations.Where(x => x.name.Contains(cFullbodyAnim)).ToArray();

			foreach(AnimationState state in mBlendInAnimations)
			{
				if(full.Length == 0 && state.weight == 0)
				{
					tempList.Add(state);
				}
				else
				{
					if(state.name.Contains(cFullbodyAnim) && state.weight == 0)
					{
						tempList.Add(full[0]);
					}
				}
			}

			return tempList;
		}

		/// <summary>
		/// Create Blending lists
		/// </summary>
		private void CreateLists()
		{
			mBlendOutAnimations = new List<AnimationState>();
			mBlendOutTimes = new List<float>();
			mBlendOutConstTimes = new List<float>();

			mBlendInAnimations = new List<AnimationState>();
			mBlendInTimes = new List<float>();
			mBlendInConstTimes = new List<float>();
		}

		/// <summary>
		/// Get animation priority
		/// </summary>
		private bool GetPriority(AnimationState animState, BodyPart part, bool fadeOut)
		{
			switch(part)
			{
				case BodyPart.FULL:
					foreach(AnimationState state in mPlayingAnimations)
					{
						if(state.name.Contains(cFullbodyAnim) || state.name.Contains(cEquipmentAnim))
						{
							if(animState.layer > state.layer && animState != state)
							{
								return false;
							}
						}
					}
					return true;
				case BodyPart.LEFTARM:
					return CheckFullArmPriority(animState, cLeftAnim, cLeftAnim, cLeftAnim, fadeOut);
				case BodyPart.RIGHTARM:
					return CheckFullArmPriority(animState, cRightAnim, cRightAnim, cRightAnim, fadeOut);
				case BodyPart.LEFTHAND:
					return CheckHandPriority(animState, cLeftAnim, cLeftAnim, cLeftAnim, fadeOut);
				case BodyPart.RIGHTHAND:
					return CheckHandPriority(animState, cRightAnim, cRightAnim, cRightAnim, fadeOut);
				default:
					foreach(AnimationState state in mPlayingAnimations)
					{
						if(animState != state)
						{
							if(state.name.Contains(cFullbodyAnim) || state.name.Contains(cRightAnim)
								|| state.name.Contains(cLeftAnim))
							{
								return false;
							}
						}
					}
					return true;
			}
		}

		private bool GetBlendInPriority(AnimationState animState, BodyPart part)
		{
			bool blendIn = false;

			foreach(AnimationState state in mPlayingAnimations)
			{
				if(state.name.Contains(cFullbodyAnim) && animState.layer > state.layer
					&& animState != state)
				{
					return false;
				}
			}
			return blendIn;
		}

		private bool CheckFullArmPriority(AnimationState animState, string fullArm, string arm, string hand, bool fadeOut)
		{
			List<AnimationState> fullBodyList = GetPlayingFullBodyAnims(animState);

			if(fullBodyList.Count > 0)
			{
				return false;
			}

			List<AnimationState> fullArmList = mPlayingAnimations.Where(x => x.name.Contains(fullArm)).ToList();

			if(fullArmList.Count > 0)
			{
				foreach(AnimationState state in fullArmList)
				{
					if(animState.layer > state.layer)
					{
						return false;
					}
				}
			}

			List<AnimationState> armList = mPlayingAnimations.Where(x => x.name.Contains(arm)).ToList();

			if(armList.Count > 0)
			{
				return true;
			}

			if(fadeOut)
			{
				return false;
			}

			return true;
		}

		private bool CheckArmPriority(AnimationState animState, string fullArm, string arm, string hand, bool fadeOut)
		{
			if(!GetFullPriority(fullArm))
			{
				return false;
			}

			List<AnimationState> armList = mPlayingAnimations.Where(x => x.name.Contains(arm)).ToList();

			if(!GetPartialPriority(animState, armList))
			{
				return false;
			}

			if(fadeOut)
			{
				return false;
			}

			return true;
		}

		private bool CheckHandPriority(AnimationState animState, string fullArm, string arm, string hand, bool fadeOut)
		{
			if(!GetFullPriority(fullArm))
			{
				return false;
			}

			List<AnimationState> handList = mPlayingAnimations.Where(x => x.name.Contains(hand)).ToList();

			if(!GetPartialPriority(animState, handList))
			{
				return false;
			}

			if(fadeOut)
			{
				return false;
			}

			return true;
		}

		private bool GetFullPriority(string fullArm)
		{
			List<AnimationState> fullBody = mPlayingAnimations.Where(x => x.name.Contains(cFullbodyAnim)).ToList();
			List<AnimationState> fullArmList = mPlayingAnimations.Where(x => x.name.Contains(fullArm)).ToList();

			if(fullBody.Count > 0 || fullArmList.Count > 0)
			{
				return false;
			}

			return true;
		}

		private bool GetPartialPriority(AnimationState animState, List<AnimationState> list)
		{
			if(list.Count > 0)
			{
				foreach(AnimationState state in list)
				{
					if(animState.layer > state.layer)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Get the time that is needed to blend
		/// </summary>
		private float GetBlendTime(AnimationState state)
		{
			float time = 0;

			if(state != null && state.time != 0)
			{
				time = state.clip.length - state.time;//* 0.2f;
			}
			if(time < 0)
			{
				time = 0;
			}

			if(time > 1)
			{
				time = 1;
			}

			return time;
		}

		private List<AnimationState> GetPlayingFullBodyAnims(AnimationState currAnim)
		{
			List<AnimationState> list = mPlayingAnimations.Where(x => x.name.Contains(cFullbodyAnim)).ToList();
			List<AnimationState> fullBody = new List<AnimationState>();

			foreach(AnimationState state in list)
			{
				if(state.layer != currAnim.layer)
				{
					fullBody.Add(state);
				}
			}

			return fullBody;
		}

		/// <summary> 
		/// Enable the equipment
		/// </summary>
		private void EnableEquipment()
		{
			if(onEnableEquipment != null)
			{
				onEnableEquipment();
			}
		}
		#endregion
	}
}
using Pathfinding.Serialization.JsonFx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorPlayer : PandoraSingleton<AnimatorPlayer>
{
	public TextAsset textAsset;

	private Dictionary<string, Dictionary<string, string[]>> animatorsData;

	private Dictionary<string, GameObject> units;

	private bool init;

	private string currentUnit;

	public AnimStyleId weaponStyle;

	private int currentState;

	private GameObject unitObject;

	private Animator animator;

	private float runningValue;

	private float timeScale = 1f;

	private int idleHash;

	private List<string> sequences;

	private bool needsRestart;

	private UnitMenuController ctrlr;

	public bool sheated;

	private bool parry;

	private bool knocked;

	private void Awake()
	{
		PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: true);
	}

	private void Start()
	{
		init = false;
		sheated = false;
		knocked = false;
		currentUnit = string.Empty;
		weaponStyle = AnimStyleId.NONE;
		idleHash = Animator.StringToHash("Base Layer.idle");
		sequences = new List<string>();
	}

	private void Update()
	{
		if ((bool)textAsset && !init)
		{
			string text = textAsset.text;
			animatorsData = new Dictionary<string, Dictionary<string, string[]>>();
			animatorsData = JsonReader.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(text);
			Debug.Log("json deserialized");
			units = new Dictionary<string, GameObject>();
			foreach (string key in animatorsData.Keys)
			{
				unitObject = GameObject.Find(key + "_menu");
				if (unitObject != null)
				{
					units[key] = unitObject;
					unitObject.SetActive(value: false);
				}
				else
				{
					unitObject = GameObject.Find(key + "_01_menu");
					if (unitObject != null)
					{
						units[key] = unitObject;
						unitObject.SetActive(value: false);
					}
				}
			}
			unitObject = null;
			init = true;
		}
		if ((bool)unitObject)
		{
			unitObject.transform.position = Vector3.zero;
			unitObject.transform.rotation = Quaternion.identity;
		}
	}

	private void LateUpdate()
	{
		if ((bool)unitObject)
		{
			unitObject.transform.position = Vector3.zero;
			unitObject.transform.rotation = Quaternion.identity;
		}
	}

	private bool HasSpecialAnims()
	{
		return !ctrlr.unit.IsImpressive && !ctrlr.unit.IsMonster;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 0f, 50f, 30f), "Back"))
		{
			Application.LoadLevel("main_menu");
		}
		if (!init)
		{
			return;
		}
		string text = currentUnit;
		int count = animatorsData.Count;
		int num = 0;
		foreach (string key in animatorsData.Keys)
		{
			int num2 = (num > 5) ? 5 : 0;
			if (GUI.Button(new Rect(Screen.width - (num + 1 - num2) * 200, (num > 5) ? 35 : 0, 200f, 30f), key))
			{
				text = key;
			}
			num++;
		}
		GUI.Label(new Rect(Screen.width / 2 - 90, 75f, 90f, 30f), "Time Scale :");
		timeScale = GUI.HorizontalSlider(new Rect(Screen.width / 2, 80f, 100f, 30f), timeScale, 0f, 2f);
		Time.timeScale = timeScale;
		if (GUI.Button(new Rect(Screen.width / 2 + 110, 75f, 22f, 22f), "R"))
		{
			timeScale = 1f;
		}
		if (text != string.Empty && text != currentUnit)
		{
			if (unitObject != null)
			{
				unitObject.SetActive(value: false);
				unitObject = null;
			}
			if (units.ContainsKey(text))
			{
				currentUnit = text;
				unitObject = units[text];
				unitObject.SetActive(value: true);
				ctrlr = unitObject.GetComponent<UnitMenuController>();
				animator = unitObject.GetComponent<Animator>();
				unitObject.transform.position = Vector3.zero;
				unitObject.transform.rotation = Quaternion.identity;
				Camera.main.GetComponent<CharacterFollowCamHack>().SetTarget(unitObject.transform);
				currentState = -1;
				sheated = false;
				knocked = false;
				weaponStyle = AnimStyleId.NONE;
				ctrlr.unit.currentAnimStyleId = weaponStyle;
				InitSequences();
				EquipWeapon();
			}
		}
		if (unitObject != null)
		{
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 30, 100f, 30f), "Running Speed");
			runningValue = GUI.HorizontalSlider(new Rect(Screen.width / 2, Screen.height - 25, 100f, 30f), runningValue, 0f, 1f);
			animator.SetFloat(AnimatorIds.speed, runningValue);
			if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 75, 150f, 30f), "Combat Idle"))
			{
				runningValue = -1f;
				animator.SetFloat(AnimatorIds.speed, -1f);
			}
			if (GUI.Button(new Rect(Screen.width / 2 + 50, Screen.height - 75, 150f, 30f), "Kneeling"))
			{
				knocked = !knocked;
				InitSequences();
			}
			SetWeaponStyleWeapons();
			GUI.Label(new Rect(Screen.width - 250, Screen.height / 2 - 230, 250f, 30f), weaponStyle.ToString());
			currentState = -1;
			currentState = GUI.SelectionGrid(new Rect(Screen.width - 400, (float)Screen.height / 2f - 200f, 400f, (float)Screen.height / 2f + 200f), currentState, sequences.ToArray(), 2);
			if (currentState != -1)
			{
				PlaySequence(sequences[currentState]);
			}
		}
	}

	public void SetWeaponStyleWeapons()
	{
		switch (currentUnit)
		{
		case "ska_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.ONE_HAND_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 60, 250f, 30f), AnimStyleId.ONE_HAND_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 90, 250f, 30f), AnimStyleId.SPEAR_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.SPEAR_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 120, 250f, 30f), AnimStyleId.SPEAR_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.SPEAR_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 150, 250f, 30f), AnimStyleId.CLAW.ToString()))
			{
				weaponStyle = AnimStyleId.CLAW;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 180, 250f, 30f), AnimStyleId.DUAL_WIELD.ToString()))
			{
				weaponStyle = AnimStyleId.DUAL_WIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 210, 250f, 30f), AnimStyleId.HALBERD.ToString()))
			{
				weaponStyle = AnimStyleId.HALBERD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 240, 250f, 30f), AnimStyleId.DUAL_PISTOL.ToString()))
			{
				weaponStyle = AnimStyleId.DUAL_PISTOL;
				EquipWeapon();
				InitSequences();
			}
			break;
		case "ska_ogre_base":
		case "pos_ogre_base":
		case "mon_horror_base":
		case "mon_daemonette_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.NONE.ToString()))
			{
				weaponStyle = AnimStyleId.NONE;
				EquipWeapon();
				InitSequences();
			}
			break;
		case "mer_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.ONE_HAND_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 60, 250f, 30f), AnimStyleId.ONE_HAND_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 90, 250f, 30f), AnimStyleId.SPEAR_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.SPEAR_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 120, 250f, 30f), AnimStyleId.SPEAR_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.SPEAR_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 150, 250f, 30f), AnimStyleId.DUAL_WIELD.ToString()))
			{
				weaponStyle = AnimStyleId.DUAL_WIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 180, 250f, 30f), AnimStyleId.TWO_HANDED.ToString()))
			{
				weaponStyle = AnimStyleId.TWO_HANDED;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 210, 250f, 30f), AnimStyleId.HALBERD.ToString()))
			{
				weaponStyle = AnimStyleId.HALBERD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 240, 250f, 30f), AnimStyleId.WARHAMMER.ToString()))
			{
				weaponStyle = AnimStyleId.WARHAMMER;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 270, 250f, 30f), AnimStyleId.DUAL_PISTOL.ToString()))
			{
				weaponStyle = AnimStyleId.DUAL_PISTOL;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 300, 250f, 30f), AnimStyleId.BOW.ToString()))
			{
				weaponStyle = AnimStyleId.BOW;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 330, 250f, 30f), AnimStyleId.CROSSBOW.ToString()))
			{
				weaponStyle = AnimStyleId.CROSSBOW;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 360, 250f, 30f), AnimStyleId.RIFLE.ToString()))
			{
				weaponStyle = AnimStyleId.RIFLE;
				EquipWeapon();
				InitSequences();
			}
			break;
		case "mer_ogre_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.ONE_HAND_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 60, 250f, 30f), AnimStyleId.DUAL_WIELD.ToString()))
			{
				weaponStyle = AnimStyleId.DUAL_WIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 90, 250f, 30f), AnimStyleId.WARHAMMER.ToString()))
			{
				weaponStyle = AnimStyleId.WARHAMMER;
				EquipWeapon();
				InitSequences();
			}
			break;
		case "sis_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.ONE_HAND_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 60, 250f, 30f), AnimStyleId.ONE_HAND_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 90, 250f, 30f), AnimStyleId.DUAL_WIELD.ToString()))
			{
				weaponStyle = AnimStyleId.DUAL_WIELD;
				EquipWeapon();
				InitSequences();
			}
			if (GUI.Button(new Rect(0f, Screen.height - 120, 250f, 30f), AnimStyleId.WARHAMMER.ToString()))
			{
				weaponStyle = AnimStyleId.WARHAMMER;
				EquipWeapon();
				InitSequences();
			}
			break;
		case "sis_ogre_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.ONE_HAND_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			break;
		case "mon_plaguebearer_base":
		case "mon_bloodletter_base":
			if (GUI.Button(new Rect(0f, Screen.height - 30, 250f, 30f), AnimStyleId.ONE_HAND_NO_SHIELD.ToString()))
			{
				weaponStyle = AnimStyleId.ONE_HAND_NO_SHIELD;
				EquipWeapon();
				InitSequences();
			}
			break;
		}
	}

	private void InitSequences()
	{
		sequences.Clear();
		if (!knocked)
		{
			switch (weaponStyle)
			{
			case AnimStyleId.NONE:
			case AnimStyleId.ONE_HAND_NO_SHIELD:
			case AnimStyleId.ONE_HAND_SHIELD:
			case AnimStyleId.SPEAR_NO_SHIELD:
			case AnimStyleId.SPEAR_SHIELD:
			case AnimStyleId.CLAW:
			case AnimStyleId.DUAL_WIELD:
			case AnimStyleId.TWO_HANDED:
			case AnimStyleId.HALBERD:
			case AnimStyleId.WARHAMMER:
				if (parry)
				{
					sequences.Add("parry");
				}
				sequences.Add("attack");
				sequences.Add("attack_fail");
				break;
			case AnimStyleId.DUAL_PISTOL:
			case AnimStyleId.BOW:
			case AnimStyleId.CROSSBOW:
			case AnimStyleId.RIFLE:
				sequences.Add("shoot");
				sequences.Add("reload");
				sequences.Add("aim");
				break;
			}
			if (HasSpecialAnims())
			{
				sequences.Add("disengage");
			}
			sequences.Add("hurt_back");
			sequences.Add("hurt_left");
			sequences.Add("hurt_right");
			sequences.Add("avoid_right");
			sequences.Add("avoid_high");
			sequences.Add("ooa_back");
			sequences.Add("ooa_front");
			sequences.Add("skill_01");
			if (currentUnit == "mon_horror_base")
			{
				sequences.Add("skill_02");
			}
			if (HasSpecialAnims())
			{
				sequences.Add("spell_area");
				sequences.Add("spell_point");
				sequences.Add("search");
				sequences.Add("interact");
			}
			if (currentUnit == "mer_ogre_base")
			{
				sequences.Add("cqc_special");
			}
			if (currentUnit == "mer_base")
			{
				sequences.Add("spell_special");
			}
			if (!ctrlr.unit.IsMonster)
			{
				sequences.Add("defeat");
				sequences.Add("perception");
				sequences.Add("stupidity");
				sequences.Add("cheer");
			}
			sequences.Add("climb3");
			if (HasSpecialAnims())
			{
				sequences.Add("climb6");
				sequences.Add("climb9");
				sequences.Add("climb3_fail");
				sequences.Add("climb6_fail");
				sequences.Add("climb9_fail");
			}
			sequences.Add("jump3");
			if (HasSpecialAnims())
			{
				sequences.Add("jump6");
				sequences.Add("jump9");
				sequences.Add("jump3_fail");
				sequences.Add("jump6_fail");
				sequences.Add("jump9_fail");
				sequences.Add("leap");
				sequences.Add("leap3_fail");
				sequences.Add("leap6_fail");
				sequences.Add("leap9_fail");
			}
		}
		else
		{
			sequences.Add("stunned");
			sequences.Add("stunned_hurt");
			sequences.Add("get_up");
			sequences.Add("ooa");
		}
	}

	private void PlaySequence(string seq)
	{
		ResetAll();
		float num = 0.1f;
		switch (seq)
		{
		case "climb3":
			animator.SetInteger(AnimatorIds.action, 2);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			if (currentUnit != "mon_bloodletter_base" && currentUnit != "mon_plaguebearer_base" && currentUnit != "ska_ogre_base" && currentUnit != "pos_ogre_base" && currentUnit != "mon_horror_base")
			{
				num = 2.5f;
			}
			break;
		case "climb6":
			animator.SetInteger(AnimatorIds.action, 3);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 2.5f;
			break;
		case "climb9":
			animator.SetInteger(AnimatorIds.action, 4);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 2.5f;
			break;
		case "climb3_fail":
			animator.SetInteger(AnimatorIds.action, 2);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "climb6_fail":
			animator.SetInteger(AnimatorIds.action, 3);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "climb9_fail":
			animator.SetInteger(AnimatorIds.action, 4);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "jump3":
			animator.SetInteger(AnimatorIds.action, 6);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			if (currentUnit != "mon_bloodletter_base" && currentUnit != "mon_plaguebearer_base" && currentUnit != "ska_ogre_base" && currentUnit != "pos_ogre_base" && currentUnit != "mon_horror_base")
			{
				num = 2.5f;
			}
			break;
		case "jump6":
			animator.SetInteger(AnimatorIds.action, 7);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 2.5f;
			break;
		case "jump9":
			animator.SetInteger(AnimatorIds.action, 8);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 2.5f;
			break;
		case "jump3_fail":
			animator.SetInteger(AnimatorIds.action, 6);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "jump6_fail":
			animator.SetInteger(AnimatorIds.action, 7);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "jump9_fail":
			animator.SetInteger(AnimatorIds.action, 8);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "leap":
			animator.SetInteger(AnimatorIds.action, 5);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 2.5f;
			break;
		case "leap3_fail":
			animator.SetInteger(AnimatorIds.action, 5);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "leap6_fail":
			animator.SetInteger(AnimatorIds.action, 5);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "leap9_fail":
			animator.SetInteger(AnimatorIds.action, 5);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.variation, 2);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "parry":
			animator.SetInteger(AnimatorIds.atkResult, 3);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "attack":
			animator.SetInteger(AnimatorIds.action, 19);
			animator.SetBool(AnimatorIds.actionSuccess, value: true);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "attack_fail":
			animator.SetInteger(AnimatorIds.action, 19);
			animator.SetBool(AnimatorIds.actionSuccess, value: false);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "shoot":
			animator.SetInteger(AnimatorIds.action, 16);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "aim":
			animator.SetInteger(AnimatorIds.action, 17);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "reload":
			animator.SetInteger(AnimatorIds.action, 18);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "disengage":
			animator.SetInteger(AnimatorIds.action, 15);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "spell_point":
			animator.SetInteger(AnimatorIds.action, 45);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 2.5f;
			break;
		case "spell_area":
			animator.SetInteger(AnimatorIds.action, 45);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "defeat":
			animator.SetInteger(AnimatorIds.action, 50);
			animator.SetInteger(AnimatorIds.variation, 4);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "perception":
			animator.SetInteger(AnimatorIds.action, 12);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "stupidity":
			animator.SetInteger(AnimatorIds.action, 50);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "cheer":
			animator.SetInteger(AnimatorIds.action, 50);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "interact":
			animator.SetInteger(AnimatorIds.action, 13);
			animator.SetInteger(AnimatorIds.variation, 4);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 3f;
			break;
		case "search":
			animator.SetInteger(AnimatorIds.action, 13);
			animator.SetInteger(AnimatorIds.variation, 3);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			num = 3f;
			break;
		case "hurt_back":
			animator.SetInteger(AnimatorIds.atkResult, 4);
			animator.SetInteger(AnimatorIds.variation, 2);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "hurt_left":
			animator.SetInteger(AnimatorIds.atkResult, 4);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "hurt_right":
			animator.SetInteger(AnimatorIds.atkResult, 4);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "avoid_right":
			animator.SetInteger(AnimatorIds.atkResult, 2);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "avoid_high":
			animator.SetInteger(AnimatorIds.atkResult, 2);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.SetInteger(AnimatorIds.unit_state, 0);
			animator.Play(idleHash);
			break;
		case "ooa_back":
			animator.SetInteger(AnimatorIds.atkResult, 5);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.SetInteger(AnimatorIds.unit_state, 3);
			animator.Play(idleHash);
			break;
		case "ooa_front":
			animator.SetInteger(AnimatorIds.atkResult, 5);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.SetInteger(AnimatorIds.unit_state, 3);
			animator.Play(idleHash);
			break;
		case "skill_01":
			animator.SetInteger(AnimatorIds.action, 40);
			animator.SetInteger(AnimatorIds.variation, 0);
			animator.Play(idleHash);
			break;
		case "skill_02":
			animator.SetInteger(AnimatorIds.action, 40);
			animator.SetInteger(AnimatorIds.variation, 1);
			animator.Play(idleHash);
			break;
		case "stunned":
			animator.SetInteger(AnimatorIds.atkResult, 5);
			animator.SetInteger(AnimatorIds.unit_state, 2);
			animator.Play(idleHash);
			break;
		case "stunned_hurt":
			animator.SetInteger(AnimatorIds.atkResult, 5);
			animator.SetInteger(AnimatorIds.unit_state, 2);
			break;
		case "get_up":
			animator.SetInteger(AnimatorIds.unit_state, 0);
			break;
		case "ooa":
			animator.SetInteger(AnimatorIds.unit_state, 3);
			break;
		}
		StartCoroutine("ResetAction", num);
	}

	private IEnumerator ResetAction(float time)
	{
		yield return new WaitForSeconds(time);
		animator.SetInteger(AnimatorIds.action, 0);
		animator.SetInteger(AnimatorIds.atkResult, 0);
		animator.SetBool(AnimatorIds.actionSuccess, value: false);
		animator.SetInteger(AnimatorIds.variation, 0);
	}

	private void ResetAll()
	{
		animator.SetInteger(AnimatorIds.action, 0);
		animator.SetInteger(AnimatorIds.atkResult, 0);
		animator.SetBool(AnimatorIds.actionSuccess, value: false);
		animator.SetInteger(AnimatorIds.variation, 0);
	}

	public void EquipWeapon()
	{
		parry = false;
		sheated = false;
		switch (weaponStyle)
		{
		case AnimStyleId.NONE:
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.NONE);
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			break;
		case AnimStyleId.ONE_HAND_NO_SHIELD:
			if (currentUnit == "ska_base" || currentUnit == "mer_base" || currentUnit == "mon_bloodletter_base" || currentUnit == "mon_plaguebearer_base" || currentUnit == "mer_ogre_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.SWORD);
				parry = true;
			}
			if (currentUnit == "sis_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.HAMMER);
			}
			if (currentUnit == "sis_ogre_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.SIGMARITE_WARHAMMER);
			}
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			break;
		case AnimStyleId.ONE_HAND_SHIELD:
			parry = true;
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.DAGGER);
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.SHIELD);
			break;
		case AnimStyleId.SPEAR_NO_SHIELD:
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.SPEAR);
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			break;
		case AnimStyleId.SPEAR_SHIELD:
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.SPEAR);
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.SHIELD);
			parry = true;
			break;
		case AnimStyleId.CLAW:
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.FIGHTING_CLAWS);
			break;
		case AnimStyleId.DUAL_WIELD:
			if (currentUnit == "sis_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.HAMMER);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.HAMMER);
			}
			else
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.SWORD);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.SWORD);
				parry = true;
			}
			break;
		case AnimStyleId.TWO_HANDED:
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.TWO_HANDED_SWORD);
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			break;
		case AnimStyleId.HALBERD:
			ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.HALBERD);
			ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			parry = true;
			break;
		case AnimStyleId.WARHAMMER:
			if (currentUnit == "sis_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.TWO_HANDED_HAMMER);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			}
			else
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.TWO_HANDED_AXE);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			}
			break;
		case AnimStyleId.DUAL_PISTOL:
			if (currentUnit == "mer_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.PISTOL);
			}
			if (currentUnit == "ska_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.WARPLOCK_PISTOL);
			}
			break;
		case AnimStyleId.BOW:
			if (currentUnit == "mer_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.BOW);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			}
			break;
		case AnimStyleId.CROSSBOW:
			if (currentUnit == "mer_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.CROSSBOW);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			}
			break;
		case AnimStyleId.RIFLE:
			if (currentUnit == "mer_base")
			{
				ctrlr.EquipItem(UnitSlotId.SET1_MAINHAND, ItemId.HUNTING_RIFLE);
				ctrlr.EquipItem(UnitSlotId.SET1_OFFHAND, ItemId.NONE);
			}
			break;
		}
		ctrlr.unit.currentAnimStyleId = weaponStyle;
		ctrlr.SetAnimStyle();
	}
}

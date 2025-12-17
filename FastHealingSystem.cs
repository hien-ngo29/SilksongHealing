using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using GlobalEnums;

namespace SilksongHealing
{
    public class FastHealingSystem : MonoBehaviour
    {
        private HeroController hc = HeroController.instance;
        private GameManager gm = GameManager.instance;
        private AudioSource hcAudioSource;
        PlayMakerFSM spellControl;

        private GameObject scarabGo;
        private PlayMakerFSM scarabFsm;

        private bool isHealing = false;
        private Coroutine healAnimationCoroutine;
        private bool takenDamageWhileHealing = false;

        private float healingDurationBySec = 1.37f;
        private float healingWithDeepFocusDurationBySec = 1.94f / 2; // We heal 1.94 / 2 seconds TWICE
        private float healingWithQuickFocusDurationBySec = 1.05f;
        private float healingWithQuickFocusAndDeepFocusDuractionBySec = 1.3f / 2; // We heal 1.3 / 2 seconds TWICE
        private int numberOfTimesHealedWithDeepFocus = 0;

        int healingAnimationFPS = 6;

        private void Awake()
        {
            spellControl = hc.spellControl;

            scarabGo = GameObject.Find("Blocker Shield");
            scarabFsm = scarabGo.LocateMyFSM("Control");

            hcAudioSource = hc.GetComponent<AudioSource>();
            hcAudioSource.clip = (AudioClip)spellControl.GetAction<AudioPlayerOneShotSingle>("Focus Heal", 3).audioClip.Value;

            ModHooks.SoulGainHook += OnSoulGained;
            On.HeroController.CanFocus += CheckIfCharmNotEquippedToAllowFocus;
            On.HeroController.TakeDamage += OnDamageTaken;
            On.PlayerData.TakeHealth += OnHealthTaken;
        }

        private bool CheckIfCharmNotEquippedToAllowFocus(On.HeroController.orig_CanFocus orig, HeroController self)
        {
            if (IsCharmEquipped())
            {
                return false;
            }
            return orig(self);
        }

        private int OnSoulGained(int n)
        {
            if (IsCharmEquipped())
                return Mathf.RoundToInt((float)n * 0.7f);
            else
                return n;
        }

        private void OnDamageTaken(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            if (isHealing && IsCharmEquipped())
            {
                DeactivateHealingState();

                if (hc.playerData.equippedCharm_5)
                {
                    scarabFsm.SendEvent("BLOCKER HIT");
                }

                if (!hc.playerData.equippedCharm_5)
                    hc.TakeMP(hc.playerData.MPCharge);

                StartCoroutine(FlashScreen());
                takenDamageWhileHealing = true;

                if (healAnimationCoroutine != null)
                    StopCoroutine(healAnimationCoroutine);
            }
            orig(self, go, damageSide, damageAmount, hazardType);
        }

        private void OnHealthTaken(On.PlayerData.orig_TakeHealth orig, PlayerData self, int amount)
        {   
            if (IsCharmEquipped() && hc.playerData.equippedCharm_5 && takenDamageWhileHealing && !hc.playerData.killedBlocker)
                return;

            orig(self, amount);
        }

        private bool IsHeroInStableState()
        {
            return !hc.cState.dead && !hc.cState.hazardDeath && !hc.inAcid && !gm.isPaused && !hc.cState.transitioning && !hc.cState.hazardRespawning && !hc.playerData.isInvincible;
        }

        private void Update()
        {
            if (IsQuickHealEventActivated() && IsCharmEquipped() && !hc.controlReqlinquished && IsHeroInStableState() && !isHealing && MasksWillBeHealedCurrenly() != -1)
            {
                StartCoroutine(StartHealing());
            }
        }

        private bool IsFullHealth()
        {
            return PlayerData.instance.GetInt("health") == PlayerData.instance.GetInt("maxHealth");
        }

        private bool IsQuickHealEventActivated()
        {
            return SilksongHealing.globalSettings.keybinds.quickHealKey.IsPressed
            || SilksongHealing.globalSettings.buttonbinds.quickHealButton.IsPressed;
        }

        private bool IsCharmEquipped()
        {
            return SilksongHealing.instantHealCharm.IsEquipped;
        }

        private IEnumerator StartHealing()
        {
            ActivateHealingState();
            
            if (hc.playerData.equippedCharm_5)
                scarabFsm.SendEvent("FOCUS START");

            healAnimationCoroutine = StartCoroutine(StartPlayingAnimation());
            var healingTime = GetHealingDuration();
            yield return new WaitForSeconds(healingTime);
            if (healAnimationCoroutine != null)
                StopCoroutine(healAnimationCoroutine);

            if (!takenDamageWhileHealing && numberOfTimesHealedWithDeepFocus >= 2 && hc.playerData.equippedCharm_34)
                HealThreeMasks(false);
            else if (!takenDamageWhileHealing)
                HealThreeMasks();

            takenDamageWhileHealing = false;

            if (numberOfTimesHealedWithDeepFocus < 2 && hc.playerData.equippedCharm_34 && !takenDamageWhileHealing && IsHeroInStableState())
            {
                StartCoroutine(StartHealing());
                yield break;
            }
            else if (numberOfTimesHealedWithDeepFocus >= 2  && !takenDamageWhileHealing)
            {
                numberOfTimesHealedWithDeepFocus = 0;
            }

            if (hc.playerData.equippedCharm_5)
                scarabFsm.SendEvent("FOCUS END");
            DeactivateHealingState();
        }

        private void ActivateHealingState()
        {
            isHealing = true;
            if (numberOfTimesHealedWithDeepFocus < 2 && hc.playerData.equippedCharm_34)
                numberOfTimesHealedWithDeepFocus++;
            HeroAnimationController animController = gameObject.GetComponent<HeroAnimationController>();
            animController.enabled = false;

            var knightRigidBody = gameObject.GetComponent<Rigidbody2D>();
            knightRigidBody.isKinematic = true;

            hc.RelinquishControl();
        }

        private void DeactivateHealingState()
        {
            HeroAnimationController animController = gameObject.GetComponent<HeroAnimationController>();
            animController.enabled = true;

            var knightRigidBody = gameObject.GetComponent<Rigidbody2D>();
            knightRigidBody.isKinematic = false;

            hc.RegainControl();
            isHealing = false;
        }

        private void HealThreeMasks(bool shouldTakeSoul = true)
        {
            int numberOfHealMasks = MasksWillBeHealedCurrenly();

            if (numberOfHealMasks == -1)
                return;

            hcAudioSource.Play();

            hc.AddHealth(numberOfHealMasks);
            if (shouldTakeSoul)
                hc.TakeMP(numberOfHealMasks * 33);

            StartCoroutine(FlashScreen());
        }

        private float GetHealingDuration()
        {
            var healingTime = healingDurationBySec;
            if (hc.playerData.equippedCharm_7)
            {
                healingTime = healingWithQuickFocusDurationBySec;
            }
            if (hc.playerData.equippedCharm_34)
            {
                healingTime = healingWithDeepFocusDurationBySec;
            }
            if (hc.playerData.equippedCharm_7 && hc.playerData.equippedCharm_34)
            {
                healingTime = healingWithQuickFocusAndDeepFocusDuractionBySec;
            }
            return healingTime;
        }

        private IEnumerator FlashScreen()
        {
            GameObject flashPrefab = spellControl.GetAction<SpawnObjectFromGlobalPool>("Focus Heal", 6).gameObject.Value;
            GameObject flash = Instantiate(flashPrefab, hc.transform.position, Quaternion.identity);
            flash.SetActive(true);

            yield return new WaitUntil(() => !flash.activeSelf);

            Destroy(flash);
        }

        private int MasksWillBeHealedCurrenly()
        {
            if (PlayerData.instance.soulLimited && PlayerData.instance.MPCharge >= 66)
                return 2;
            else if (PlayerData.instance.MPCharge >= 99)
                return 3;

            if (hc.playerData.equippedCharm_34)
            {
                return 2;
            }

            return -1;
        }

        private IEnumerator StartPlayingAnimation()
        {
            int currentFrame = 0;
            while (true)
            {
                SetFrameFromScreamAnimation(currentFrame);
                currentFrame = (currentFrame % 4) + 1;
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void SetFrameFromScreamAnimation(int index)
        {
            tk2dSpriteAnimator animator = gameObject.GetComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimationClip roarClip = animator.GetClipByName("Thorn Attack");
            tk2dSpriteAnimationFrame frame = roarClip.frames[index];
            tk2dSprite sprite = gameObject.GetComponent<tk2dSprite>();
            animator.Stop();
            sprite.SetSprite(frame.spriteId);
        }
    }
}

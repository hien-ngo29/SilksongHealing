using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using IL.InControl;
using System.Runtime.CompilerServices;
using System.IO;
using GlobalEnums;

namespace SilksongHealing
{
    public class FastHealingSystem : MonoBehaviour
    {
        private HeroController hc = HeroController.instance;
        PlayMakerFSM spellControl;

        AudioSource healAudioSource;

        private bool isHealing = false;
        private Coroutine healAnimationCoroutine;
        private bool takenDamageWhileHealing = false;

        private float healingDurationBySec = 1.533f;

        int healingAnimationFPS = 6;

        private void Awake()
        {
            spellControl = hc.spellControl;

            healAudioSource = hc.GetComponent<AudioSource>();
            healAudioSource.clip = (AudioClip)spellControl.GetAction<AudioPlayerOneShotSingle>("Focus Heal", 3).audioClip.Value;

            ModHooks.SoulGainHook += OnSoulGained;
            On.HeroController.CanFocus += CheckIfCharmNotEquippedToAllowFocus;
            On.HeroController.TakeDamage += OnDamageTaken;
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
            if (isHealing)
            {
                DeactivateHealingState();
                hc.TakeMP(hc.playerData.MPCharge);

                StartCoroutine(FlashScreen());
                takenDamageWhileHealing = true;

                if (healAnimationCoroutine != null)
                    StopCoroutine(healAnimationCoroutine);
            }
            orig(self, go, damageSide, damageAmount, hazardType);
        }

        private void Update()
        {
            // && !IsFullHealth() 
            if (IsQuickHealEventActivated() && IsCharmEquipped() && !hc.controlReqlinquished && !isHealing && MasksWillBeHealedCurrenly() != -1)
            {
                StartCoroutine(StartHealing());
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                healingAnimationFPS -= 1;
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                healingAnimationFPS += 1;
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

            healAnimationCoroutine = StartCoroutine(StartPlayingAnimation());
            yield return new WaitForSeconds(healingDurationBySec);
            if (healAnimationCoroutine != null)
                StopCoroutine(healAnimationCoroutine);

            if (!takenDamageWhileHealing)
                HealThreeMasks();

            takenDamageWhileHealing = false;

            DeactivateHealingState();
        }

        private void ActivateHealingState()
        {
            isHealing = true;
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

        private void HealThreeMasks()
        {
            int numberOfHealMasks = MasksWillBeHealedCurrenly();

            if (numberOfHealMasks == -1)
                return;

            healAudioSource.Play();

            hc.AddHealth(numberOfHealMasks);
            hc.TakeMP(numberOfHealMasks * 33);

            StartCoroutine(FlashScreen());
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
            Modding.Logger.Log("Length: " + roarClip.frames.Length);
            tk2dSpriteAnimationFrame frame = roarClip.frames[index];
            tk2dSprite sprite = gameObject.GetComponent<tk2dSprite>();
            animator.Stop();
            sprite.SetSprite(frame.spriteId);
        }
    }
}

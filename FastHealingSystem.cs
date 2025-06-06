using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;

namespace SilksongHealing
{
    public class FastHealingSystem : MonoBehaviour
    {
        private HeroController hc = HeroController.instance;
        PlayMakerFSM spellControl;

        AudioSource healAudioSource;

        private void Awake()
        {
            spellControl = hc.spellControl;

            healAudioSource = hc.GetComponent<AudioSource>();
            healAudioSource.clip = (AudioClip)spellControl.GetAction<AudioPlayerOneShotSingle>("Focus Heal", 3).audioClip.Value;

            MyLogger.Log($"{SilksongHealing.instantHealCharm.IsEquipped}");

            ModHooks.SoulGainHook += OnSoulGained;
            On.HeroController.CanFocus += CheckIfCharmNotEquippedToAllowFocus;
        }

        private bool CheckIfCharmNotEquippedToAllowFocus(On.HeroController.orig_CanFocus orig, HeroController self)
        {
            if (isCharmEquipped())
            {
                return false;
            }
            return orig(self);
        }

        private int OnSoulGained(int n)
        {
            if (isCharmEquipped())
                return Mathf.RoundToInt((float)n * 0.6f);
            else
                return n;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V) && isCharmEquipped())
            {
                StartCoroutine(HealThreeMasks());
            }
        }

        private bool isCharmEquipped()
        {
            MyLogger.Log($"{SilksongHealing.instantHealCharm.IsEquipped}");
            MyLogger.Log($"equipped_charm{SilksongHealing.instantHealCharm.Id}");
            return SilksongHealing.instantHealCharm.IsEquipped;
        }

        private IEnumerator HealThreeMasks()
        {
            if (PlayerData.instance.MPCharge >= 99)
            {
                healAudioSource.Play();

                hc.AddHealth(3);
                hc.TakeMP(99);
                hc.GetComponent<SpriteFlash>().flashFocusHeal();

                GameObject flashPrefab = spellControl.GetAction<SpawnObjectFromGlobalPool>("Focus Heal", 6).gameObject.Value;
                GameObject flash = GameObject.Instantiate(flashPrefab, hc.transform.position, Quaternion.identity);
                flash.SetActive(true);

                yield return new WaitUntil(() => !flash.activeSelf);

                GameObject.Destroy(flash);
            }
        }
    }
}

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
            if ((Input.GetKeyDown(KeyCode.V) || InputHandler.Instance.inputActions.cast.WasPressed) && isCharmEquipped() && !GameManager.instance.IsMenuScene())
            {
                StartCoroutine(HealThreeMasks());
            }
        }

        private bool isCharmEquipped()
        {
            return SilksongHealing.instantHealCharm.IsEquipped;
        }

        private IEnumerator HealThreeMasks()
        {
            int numberOfHealMasks;

            if (PlayerData.instance.soulLimited && PlayerData.instance.MPCharge >= 66)
                numberOfHealMasks = 2;
            else if (PlayerData.instance.MPCharge >= 99)
                numberOfHealMasks = 3;
            else
                yield break;

            healAudioSource.Play();

            hc.AddHealth(numberOfHealMasks);
            hc.TakeMP(numberOfHealMasks * 11);

            GameObject flashPrefab = spellControl.GetAction<SpawnObjectFromGlobalPool>("Focus Heal", 6).gameObject.Value;
            GameObject flash = Instantiate(flashPrefab, hc.transform.position, Quaternion.identity);
            flash.SetActive(true);

            yield return new WaitUntil(() => !flash.activeSelf);

            Destroy(flash);
        }
    }
}

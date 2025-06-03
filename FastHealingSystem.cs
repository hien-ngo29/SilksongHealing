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

        private void Awake()
        {
            ModHooks.SoulGainHook += OnSoulGained;
        }

        private int OnSoulGained(int n)
        {
            return n / 2;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                StartCoroutine(HealThreeMasks());
            }
        }

        private IEnumerator HealThreeMasks()
        {
            if (PlayerData.instance.MPCharge >= 99)
            {
                hc.AddHealth(3);
                hc.TakeMP(99);
                hc.GetComponent<SpriteFlash>().flashFocusHeal();

                PlayMakerFSM spellControl = hc.spellControl;
                GameObject flashPrefab = spellControl.GetAction<SpawnObjectFromGlobalPool>("Focus Heal", 6).gameObject.Value;
                GameObject flash = GameObject.Instantiate(flashPrefab, hc.transform.position, Quaternion.identity);
                flash.SetActive(true);

                yield return new WaitUntil(() => !flash.activeSelf);

                GameObject.Destroy(flash);
            }
        }
    }
}

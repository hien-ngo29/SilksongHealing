using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;

namespace SilksongHealing
{
    public class SilksongHealing : Mod
    {
        public override string GetVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            On.HeroController.Update += CheckFastHealPressed;
        }

        public void CheckFastHealPressed(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.V))
            {
                HealThreeMasks();
            }
        }

        private IEnumerator HealThreeMasks()
        {
            HeroController hc = HeroController.instance;

            if (PlayerData.instance.GetInt("MPCharge") >= 99)
            {
                hc.AddHealth(3);
                hc.TakeMP(99);
                hc.GetComponent<SpriteFlash>().flashFocusHeal();

                PlayMakerFSM spellControl = hc.spellControl;
                GameObject flashPrefab = spellControl.GetAction<SpawnObjectFromGlobalPool>("Focus Heal", 6).gameObject.Value;
                GameObject flash = GameObject.Instantiate(flashPrefab, hc.transform.position, Quaternion.identity);
                flash.SetActive(true);

                yield return new WaitUntil(() => !flash.active);

                GameObject.Destroy(flash);
            }
        }
    }
}

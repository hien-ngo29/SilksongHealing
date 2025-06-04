using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using SFCore;

namespace SilksongHealing
{
    public class SilksongHealing : Mod
    {
        public override string GetVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static EasyCharm instantHealCharm = new InstantHealCharm();

        public override void Initialize()
        {
            instantHealCharm.GiveCharm(true);
            On.HeroController.Awake += OnHeroAwake;
        }

        private void OnHeroAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            self.gameObject.AddComponent<FastHealingSystem>();
        }
    }
}

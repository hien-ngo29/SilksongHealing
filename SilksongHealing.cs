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
            On.HeroController.Awake += OnHeroAwake;
        }

        private void OnHeroAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            self.gameObject.AddComponent<FastHealingSystem>();
        }
    }
}

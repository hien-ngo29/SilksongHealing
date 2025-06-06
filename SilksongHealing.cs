using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using SFCore;

namespace SilksongHealing
{
    public class SilksongHealing : Mod, ILocalSettings<Settings>
    {
        public override string GetVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static EasyCharm instantHealCharm = new InstantHealCharm();

        public override void Initialize()
        {
            instantHealCharm.GiveCharm();
            On.HeroController.Awake += OnHeroAwake;
        }

        public void OnLoadLocal(Settings settings)
        {
            if (settings.instantHealCharmState != null)
            {
                instantHealCharm.RestoreCharmState(settings.instantHealCharmState);
            }
        }

        public Settings OnSaveLocal()
        {
            Settings settings = new();
            settings.instantHealCharmState = instantHealCharm.GetCharmState();
            return settings;
        }

        private void OnHeroAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            GameObject.Destroy(self.gameObject.GetComponent<FastHealingSystem>());
            self.gameObject.AddComponent<FastHealingSystem>();
        }
    }
}

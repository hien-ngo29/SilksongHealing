using System.Collections;
using UnityEngine;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;
using SFCore;
using Satchel.BetterMenus;

namespace SilksongHealing
{
    public class SilksongHealing : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>, ICustomMenuMod
    {
        public override string GetVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static EasyCharm instantHealCharm = new InstantHealCharm();

        public static GlobalSettings globalSettings { get; set; } = new GlobalSettings();

        private Menu MenuRef;

        public override void Initialize()
        {
            On.HeroController.Awake += OnHeroAwake;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates)
        {
            MenuRef ??= new Menu(
                name: "Silksong Healing",
                elements: new Element[]
                {
                    Blueprints.KeyAndButtonBind("Quick Heal", globalSettings.keybinds.quickHealKey, globalSettings.buttonbinds.quickHealButton),
                }

            );

            return MenuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu { get; }

        public void OnLoadGlobal(GlobalSettings s)
        {
            globalSettings = s;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return globalSettings;
        }

        public void OnLoadLocal(LocalSettings settings)
        {
            if (settings.instantHealCharmState != null)
            {
                instantHealCharm.RestoreCharmState(settings.instantHealCharmState);
            }
        }

        public LocalSettings OnSaveLocal()
        {
            LocalSettings settings = new();
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

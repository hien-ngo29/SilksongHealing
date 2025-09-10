using Modding.Converters;
using Newtonsoft.Json;
using SFCore;

namespace SilksongHealing
{
    public class LocalSettings
    {
        public EasyCharmState instantHealCharmState;
    }

    public class GlobalSettings
    {
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public ButtonBinds buttonbinds = new ButtonBinds();
    }
}
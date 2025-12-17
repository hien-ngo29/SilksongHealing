using UnityEngine;
using System.Reflection;
using SFCore;

namespace SilksongHealing
{
    public class InstantHealCharm : EasyCharm
    {
        protected override int GetCharmCost() => 3;
        protected override string GetDescription() => "Allows the bearer to restore 3 Masks quickly when SOUL is full, without the need to focus.\n\nDisables the ability to focus and reduces SOUL gained from striking foes by 70%.";
        protected override string GetName() => "Soulburst";
        protected override Sprite GetSpriteInternal()
        {
            Texture2D tex = new Texture2D(2, 2);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SilksongHealing.Resources.InstantHeal.png"))
            {
                if (stream == null)
                {
                    return null;
                }
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                if (buffer == null)
                {
                    return null;
                }
                tex.LoadImage(buffer);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}

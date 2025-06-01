using GameData;
using UnityEngine;
using Localization;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LSS
    {
        public static readonly Color OffColor = ColorExt.Hex("#00000000");

        public static readonly Color TargetBothColor = ColorExt.Hex("FFFA00");

        public static readonly Color TargetPlayerOnlyColor = ColorExt.Hex("#F0192EFF");

        public static readonly Color TargetEnemyOnlyColor = ColorExt.Hex("#00895CFF");

        public static readonly Color DoesNotTargetColor = ColorExt.Hex("#198AF0FF") * 0.5f;

        public static Color GetScanningColor(bool targetEnemy, bool targetPlayer)
        {
            Color c = Color.grey * 0.5f;
            if (targetEnemy && targetPlayer)
            {
                c = TargetBothColor;
            }
            else if (targetEnemy && !targetPlayer)
            {
                c = TargetEnemyOnlyColor;
            }
            else if (!targetEnemy && targetPlayer)
            {
                c = TargetPlayerOnlyColor;
            }
            else
            {
                c = DoesNotTargetColor;
            }

            return c;
        }

        public static string GetMarkerText(bool targetEnemy, bool targetPlayer)
        {
            string textDBName = string.Empty;
            string text = string.Empty;
            if (targetEnemy && targetPlayer)
            {
                textDBName = "LSS.State.Both";
                text = "HAVOC";
            }
            else if (targetEnemy && !targetPlayer)
            {
                textDBName = "LSS.State.Normal";
                text = "NORMAL";
            }
            else if (!targetEnemy && targetPlayer)
            {
                textDBName = "LSS.State.Player";
                text = "HOSTILE";
            }
            else
            {
                textDBName = "LSS.State.Idle";
                text = "IDLE";
            }

            var textDB = GameDataBlockBase<TextDataBlock>.GetBlock(textDBName);
            return textDB != null ? Text.Get(textDB.persistentID) : text;
        }
    }
}

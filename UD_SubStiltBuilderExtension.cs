using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.World;
using XRL.World.WorldBuilders;
using XRL.Wish;

namespace UD_SacredWellHole
{
    [HasWishCommand]
    [JoppaWorldBuilderExtension]
    public class UD_SubStiltBuilderExtension : IJoppaWorldBuilderExtension
    {
        public static ModInfo ThisMod = ModManager.GetMod("UD_SacredWellHole");

        public const string CathedralZoneID = "JoppaWorld.5.2.1.1.10";

        public static Zone CathedralZone => The.ZoneManager.GetZone(CathedralZoneID);

        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            // Assign the 10 yet-to-be-created map files for the 10 strata below the stilt.
        }

        [WishCommand(Command = "go stilt")]
        public static void GoStilt()
        {
            Zone Z = The.ZoneManager.GetZone(CathedralZoneID);
            The.Player.Physics.CurrentCell.RemoveObject(The.Player.Physics.ParentObject);
            Z.GetEmptyCells().GetRandomElement().AddObject(The.Player);
            The.ZoneManager.SetActiveZone(Z);
            The.ZoneManager.ProcessGoToPartyLeader();
        }
    }
}

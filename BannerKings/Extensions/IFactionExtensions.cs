using System.Linq;
using BannerKings.Utils;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Extensions
{
    public static class IFactionExtensions
    {
        public static bool IsKingdomAtWar(this IFaction faction) => Compat.GetStances(faction).Any(x => x.IsAtWar && 
        x.Faction1.IsKingdomFaction &&
        x.Faction2.IsKingdomFaction);
    }
}

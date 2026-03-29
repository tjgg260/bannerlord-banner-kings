using System;
using BannerKings.Utils;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using Helpers;
using TaleWorlds.Core;

namespace BannerKings.Utils
{
    /// <summary>
    /// Compatibility helpers for Bannerlord 1.3.x API changes
    /// </summary>
    public static class Compat
    {
        /// <summary>
        /// Get distance between a mobile party and a settlement (wrapper for 1.3.x API)
        /// </summary>
        public static float GetDistance(MobileParty from, Settlement to)
        {
            return Campaign.Current.Models.MapDistanceModel.GetDistance(
                from, to, false, MobileParty.NavigationType.Default, out float _);
        }

        /// <summary>
        /// Get distance between two settlements (wrapper for 1.3.x API)
        /// </summary>
        public static float GetDistance(Settlement from, Settlement to)
        {
            return Campaign.Current.Models.MapDistanceModel.GetDistance(
                from, to, false, false, MobileParty.NavigationType.Default);
        }

        /// <summary>
        /// Get distance between two mobile parties (wrapper for 1.3.x API)
        /// </summary>
        public static float GetDistance(MobileParty from, MobileParty to)
        {
            return Campaign.Current.Models.MapDistanceModel.GetDistance(
                from, to, MobileParty.NavigationType.Default, out float _);
        }

        /// <summary>
        /// Get total weight of items in an ItemRoster (TotalWeight was removed in 1.3.x)
        /// </summary>
        public static float GetTotalWeight(this ItemRoster roster)
        {
            float totalWeight = 0f;
            for (int i = 0; i < roster.Count; i++)
            {
                var element = roster[i];
                if (!element.IsEmpty)
                {
                    totalWeight += element.EquipmentElement.Weight * element.Amount;
                }
            }
            return totalWeight;
        }

        /// <summary>
        /// Get estimated strength for a PartyBase (CurrentTotalStrength was removed)
        /// </summary>
        public static float GetTotalStrength(this PartyBase party)
        {
            return party.EstimatedStrength;
        }

        /// <summary>
        /// Compatibility wrapper for GetLikelihoodForOutcome which was replaced by GetLikelihoodForSponsor in 1.3.x.
        /// outcomeNo 0 = proposer's outcome, outcomeNo 1 = opposition
        /// </summary>
        public static float GetLikelihoodForOutcome(this KingdomElection election, int outcomeNo)
        {
            float likelihood = election.GetLikelihoodForSponsor(election.PossibleOutcomes[0].SponsorClan);
            return outcomeNo == 0 ? likelihood : 1f - likelihood;
        }

        /// <summary>
        /// Compatibility wrapper for IsAllied which was removed from StanceLink in 1.3.x.
        /// </summary>
        public static bool IsAllied(this StanceLink stance)
        {
            return !stance.IsAtWar && !stance.IsNeutral;
        }

        /// <summary>
        /// Compatibility wrapper for GetBuildingEffectAmount which was replaced by AddEffectOfBuilding in 1.3.x.
        /// </summary>
        public static float GetBuildingEffectAmount(this Building building, BuildingEffectEnum effect)
        {
            var result = new TaleWorlds.CampaignSystem.ExplainedNumber(0f, false, null);
            building.AddEffectOfBuilding(effect, ref result);
            return result.ResultNumber;
        }

        /// <summary>
        /// Compatibility wrapper for Hero.GetHeroTraits which was removed in 1.3.x.
        /// Returns a simple wrapper that provides GetPropertyValue.
        /// </summary>
        public static CompatCharacterTraits GetHeroTraits(this Hero hero)
        {
            return new CompatCharacterTraits(hero);
        }

        /// <summary>
        /// Compatibility wrapper for BuildingType.IsDefaultProject which was removed in 1.3.x.
        /// Default/daily projects have zero production costs.
        /// </summary>
        public static bool IsDefaultProject(this BuildingType buildingType)
        {
            return buildingType.GetProductionCost(0) == 0;
        }

        /// <summary>
        /// Compatibility wrapper for Compat.FindNearestTown (renamed and split in 1.3.x)
        /// </summary>
        public static Town FindNearestTown(Func<Settlement, bool> condition, Settlement from)
        {
            return SettlementHelper.FindNearestTownToSettlement(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Town FindNearestTown(Func<Settlement, bool> condition, MobileParty from)
        {
            return SettlementHelper.FindNearestTownToMobileParty(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Town FindNearestTown(Func<Settlement, bool> condition)
        {
            return SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.Default,
                condition);
        }

        public static Town FindNearestTown()
        {
            return SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.Default, null);
        }

        public static Settlement FindNearestSettlement(Func<Settlement, bool> condition, Settlement from)
        {
            return SettlementHelper.FindNearestSettlementToSettlement(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Settlement FindNearestSettlement(Func<Settlement, bool> condition, MobileParty from)
        {
            return SettlementHelper.FindNearestSettlementToMobileParty(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Settlement FindNearestSettlement(Func<Settlement, bool> condition)
        {
            return SettlementHelper.FindNearestSettlementToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.Default,
                condition);
        }

        public static Settlement FindNearestFortification(Func<Settlement, bool> condition, Settlement from)
        {
            return SettlementHelper.FindNearestFortificationToSettlement(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Settlement FindNearestFortification(Func<Settlement, bool> condition)
        {
            return SettlementHelper.FindNearestFortificationToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.Default,
                condition);
        }

        public static Village FindNearestVillage(Func<Settlement, bool> condition, Settlement from)
        {
            return SettlementHelper.FindNearestVillageToSettlement(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Village FindNearestVillage(Func<Settlement, bool> condition, MobileParty from)
        {
            return SettlementHelper.FindNearestVillageToMobileParty(from, MobileParty.NavigationType.Default,
                condition);
        }

        public static Village FindNearestVillage(Func<Settlement, bool> condition)
        {
            return SettlementHelper.FindNearestVillageToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.Default,
                condition);
        }

        public static Hideout FindNearestHideout(Func<Settlement, bool> condition, Settlement from)
        {
            if (from != null)
                return SettlementHelper.FindNearestHideoutToSettlement(from, MobileParty.NavigationType.Default,
                    condition);
            return SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, MobileParty.NavigationType.Default,
                condition);
        }

        /// <summary>
        /// Compatibility wrapper for Compat.GetSuccessfulSieges (removed in 1.3.x).
        /// In 1.3.x, StanceLink.GetSuccessfulSieges(IFaction) returns only a count.
        /// Returns an empty list since individual settlement data is no longer available.
        /// </summary>
        public static List<Settlement> GetSuccessfulSieges(IFaction faction, StanceLink stance, Func<Settlement, bool> condition)
        {
            return new List<Settlement>();
        }

        /// <summary>
        /// Compatibility wrapper for Compat.GetStances(IFaction) which was removed in 1.3.x.
        /// Iterates all kingdoms to build stance links.
        /// </summary>
        public static IEnumerable<StanceLink> GetStances(IFaction faction)
        {
            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom != faction)
                {
                    var stance = faction.GetStanceWith(kingdom);
                    if (stance != null) yield return stance;
                }
            }
            foreach (var clan in Clan.All)
            {
                if (clan != faction && clan.Kingdom == null)
                {
                    var stance = faction.GetStanceWith(clan);
                    if (stance != null) yield return stance;
                }
            }
        }

        /// <summary>
        /// Compatibility replacement for PartyBaseHelper.IsSurrenderFeasible (removed in 1.3.x).
        /// Returns true if the surrendering party is much weaker than the other.
        /// </summary>
        public static bool IsSurrenderFeasible(MobileParty surrenderingParty, MobileParty otherParty, float threshold = 0.4f)
        {
            float surrenderStrength = surrenderingParty.Party.GetTotalStrength();
            float otherStrength = otherParty.Party.GetTotalStrength();
            if (otherStrength <= 0f) return false;
            return (surrenderStrength / otherStrength) < threshold;
        }
    }

    /// <summary>
    /// Wrapper for Hero trait access (GetHeroTraits was removed in 1.3.x)
    /// </summary>
    public class CompatCharacterTraits
    {
        private readonly Hero _hero;

        public CompatCharacterTraits(Hero hero)
        {
            _hero = hero;
        }

        public int GetPropertyValue(TraitObject trait)
        {
            return _hero.GetTraitLevel(trait);
        }

        public int Calculating => _hero.GetTraitLevel(DefaultTraits.Calculating);
        public int Mercy => _hero.GetTraitLevel(DefaultTraits.Mercy);
        public int Honor => _hero.GetTraitLevel(DefaultTraits.Honor);
        public int Generosity => _hero.GetTraitLevel(DefaultTraits.Generosity);
        public int Valor => _hero.GetTraitLevel(DefaultTraits.Valor);
    }
}

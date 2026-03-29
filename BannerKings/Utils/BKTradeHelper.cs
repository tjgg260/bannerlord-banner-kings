using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings
{
    /// <summary>
    /// Wraps SellItemsAction.Apply to handle the 1.3.x GetVillageTaxRatio NRE
    /// that occurs when the settlement is not a village.
    /// </summary>
    public static class BKTradeHelper
    {
        public static void SafeSellItems(PartyBase sellerParty, PartyBase buyerParty,
            ItemRosterElement item, int number, Settlement settlement)
        {
            // In 1.3.x, SellItemsAction.ApplyInternal calls GetVillageTaxRatio which
            // NREs when the settlement is not a village. Only pass settlement for villages.
            var safeSettlement = settlement != null && settlement.IsVillage ? settlement : null;
            try
            {
                SellItemsAction.Apply(sellerParty, buyerParty, item, number, safeSettlement);
            }
            catch (System.Exception)
            {
                // Fallback: direct transfer if SellItemsAction fails for any reason
                try
                {
                    int available = sellerParty.ItemRoster.GetItemNumber(item.EquipmentElement.Item);
                    int transferCount = System.Math.Min(number, available);
                    if (transferCount <= 0) return;

                    int price = 0;
                    if (settlement?.Town != null)
                        price = (int)settlement.Town.GetItemPrice(item.EquipmentElement) * transferCount;

                    sellerParty.ItemRoster.AddToCounts(item.EquipmentElement, -transferCount);
                    buyerParty.ItemRoster.AddToCounts(item.EquipmentElement, transferCount);
                    if (sellerParty.LeaderHero != null)
                        sellerParty.LeaderHero.Gold += price;
                    if (buyerParty.LeaderHero != null)
                        buyerParty.LeaderHero.Gold -= price;
                }
                catch (System.Exception)
                {
                    // Silently fail — better than crashing the game
                }
            }
        }
    }
}

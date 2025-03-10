﻿using UnboundLib.Cards;
using UnityEngine;
using UnboundLib;
using PCE.Extensions;
using PCE.MonoBehaviours;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

namespace PCE.Cards
{
    public abstract class PacifistCardBase : CustomCard
    {
        internal static CardCategory category = CustomCardCategories.instance.CardCategory("Pacifist");
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { PacifistCardBase.category };
            cardInfo.blacklistedCategories = new CardCategory[] { MasochistCardBase.category, SurvivalistCardBase.category, WildcardCardBase.category };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Traverse.Create(characterStats).Field("sinceDealtDamage").SetValue(0f);
            player.gameObject.GetOrAddComponent<PacifistEffect>();
            foreach (Player otherPlayer in PlayerStatus.GetOtherPlayers(player))
            {
                if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(otherPlayer.data.stats).blacklistedCategories.Contains(PacifistCardBase.category))
                {
                    ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(otherPlayer.data.stats).blacklistedCategories.Add(PacifistCardBase.category);
                }
            }
        }
        public override void OnRemoveCard()
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll(cardcat => cardcat == PacifistCardBase.category);
            }
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.NatureBrown;
        }
        public override string GetModName()
        {
            return "PCE";
        }
    }
    public class PacifistICard : PacifistCardBase
    {
        protected override string GetTitle()
        {
            return "Pacifist I";
        }
        protected override string GetDescription()
        {
            return "Increased reload speed the longer you go without dealing damage.";
        }

        protected override GameObject GetCardArt()
        {
            return PCE.ArtAssets.LoadAsset<GameObject>("C_Pacifist_I");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat
                {
                positive = true,
                stat = "Reload Speed",
                amount = "Up to 3×",
                simepleAmount = CardInfoStat.SimpleAmount.aLotOf
                }
            };
        }
    }
    public class PacifistIICard : PacifistCardBase
    {

        protected override string GetTitle()
        {
            return "Pacifist II";
        }
        protected override string GetDescription()
        {
            return "Decreased block cooldown the longer you go without dealing damage.";
        }

        protected override GameObject GetCardArt()
        {
            return PCE.ArtAssets.LoadAsset<GameObject>("C_Pacifist_II");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Uncommon;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat
                {
                positive = true,
                stat = "Block Cooldown",
                amount = "Up to 1/3×",
                simepleAmount = CardInfoStat.SimpleAmount.aLotLower
                }
            };
        }
    }
    public class PacifistIIICard : PacifistCardBase
    {
        protected override string GetTitle()
        {
            return "Pacifist III";
        }
        protected override string GetDescription()
        {
            return "Increased movement speed the longer you go without dealing damage.";
        }

        protected override GameObject GetCardArt()
        {
            return PCE.ArtAssets.LoadAsset<GameObject>("C_Pacifist_III");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Uncommon;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat
                {
                positive = true,
                stat = "Movement Speed",
                amount = "Up to 2×",
                simepleAmount = CardInfoStat.SimpleAmount.aLotOf
                }
            };
        }
    }
    public class PacifistIVCard : PacifistCardBase
    {
        protected override string GetTitle()
        {
            return "Pacifist IV";
        }
        protected override string GetDescription()
        {
            return "Increased damage the longer you go without dealing damage.";
        }

        protected override GameObject GetCardArt()
        {
            return PCE.ArtAssets.LoadAsset<GameObject>("C_Pacifist_IV");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat
                {
                positive = true,
                stat = "Damage",
                amount = "Up to 3×",
                simepleAmount = CardInfoStat.SimpleAmount.aLotOf
                }
            };
        }
    }
    public class PacifistVCard : PacifistCardBase
    {

        internal static CardInfo self = null;

        protected override string GetTitle()
        {
            return "Pacifist V";
        }
        protected override string GetDescription()
        {
            return "Double the charge speed of all Pacifist effects.";
        }

        protected override GameObject GetCardArt()
        {
            return PCE.ArtAssets.LoadAsset<GameObject>("C_Pacifist_V");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }

        protected override CardInfoStat[] GetStats()
        {
            return null;
        }
        public override bool GetEnabled()
        {
            return false;
        }
    }
}

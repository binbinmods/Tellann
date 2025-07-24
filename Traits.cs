using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Tellann.CustomFunctions;
using static Tellann.Plugin;
using static Tellann.DescriptionFunctions;
using static Tellann.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Tellann
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }
            string traitName = traitData.TraitName;
            string traitId = _trait;


            if (_trait == trait0)
            {
                // Zeal on heroes increases healing received by 20% per charge. Whenever a hero gains Zeal, they gain 1 Vitality.

            }


            else if (_trait == trait2a)
            {
                // trait2a
                // When you play a Healing Spell that costs energy, 
                // refund one and give the lowest hp hero 1 Zeal and 5 Burn. (3 times/turn)

                if (CanIncrementTraitActivations(traitId) && _castedCard.HasCardType(Enums.CardType.Healing_Spell) && MatchManager.Instance.energyJustWastedByHero > 0)
                {
                    LogDebug($"Handling Trait {traitId}: {traitName}");
                    _character?.ModifyEnergy(1);
                    Character lowestHpHero = GetLowestHPCharacter(teamHero);
                    if (lowestHpHero != null)
                    {
                        lowestHpHero.SetAuraTrait(lowestHpHero, "zeal", 1);
                        lowestHpHero.SetAuraTrait(lowestHpHero, "burn", 5);
                    }
                    IncrementTraitActivations(traitId);
                }
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Burn +1, Dark +1. 
                // Burn on you increases Shadow Resistance by 0.5% per charge. 
                // Dark on you enemies increases Fire Damage received by 1 per charge.

                LogDebug($"Handling Trait {traitId}: {traitName}");

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // Burn +2, Dark +2. When Dark explodes, Heal all heroes for 20% of the Dark stacks, 
                // and apply 1 Zeal and 5 Burn to all heroes. These effects do not benefit from modifiers.

                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // At the start of your turn, reduce the cost of your highest cost card by one for every 30 Burn on you.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                CardData highestCostCard = GetRandomHighestCostCard(Enums.CardType.None, heroHand);
                if (highestCostCard != null)
                {
                    int burnCharges = _character.GetAuraCharges("burn");
                    int costReduction = burnCharges / 30;
                    ReduceCardCost(ref highestCostCard, amountToReduce: costReduction);
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait0:
                // Zeal on heroes increases healing received by 20% per charge. Whenever a hero gains Zeal, they gain 1 Vitality.

                // trait2b:
                // Burn on you increases Shadow Resistance by 0.5% per charge. Dark on you enemies increases Fire Damage received by 1 per charge.

                // trait 4b:
                // Burn on allies no longer reduces resistances. At the start of your turn, reduce the cost of your highest cost card by one for every 30 Burn on you.

                case "zeal":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Heroes))
                    {
                        __result.HealReceivedPercentPerStack = 20;
                    }
                    break;
                case "burn":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result = AtOManager.Instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Shadow, 0, 0.5f); ;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Heroes))
                    {
                        __result.ResistModified = __result.ResistModified2 = __result.ResistModified3 = Enums.DamageType.None;
                        __result.ResistModifiedPercentagePerStack = __result.ResistModifiedPercentagePerStack2 = __result.ResistModifiedPercentagePerStack3 = 0;
                    }
                    break;
                case "dark":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.IncreasedDamageReceivedType = Enums.DamageType.Fire;
                        __result.IncreasedDirectDamageReceivedPerStack = 1;
                    }
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPostfix(
            Character __instance,
            Enums.EventActivation theEvent,
            Character target = null,
            int auxInt = 0,
            string auxString = "")
        {
            //Whenever a hero gains Zeal, they gain 1 Vitality.
            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "zeal" && IsLivingHero(__instance) && AtOManager.Instance.TeamHaveTrait(trait0))
            {
                __instance.SetAuraTrait(__instance, "vitality", 1);
            }

            // When Dark explodes, Heal all heroes for 20% of the Dark stacks, and apply 1 Zeal and 5 Burn to all heroes. These effects do not benefit from modifiers.

            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "dark" && __instance.Alive && __instance != null && AtOManager.Instance.TeamHaveTrait(trait4a))
            {
                int nCharges = __instance.GetAuraCharges("dark") + auxInt;
                bool ACExplodes = nCharges >= GetAuraCurseData(auxString).ExplodeAtStacks; // Assuming 10 is the threshold for explosion
                if (ACExplodes)
                {
                    LogDebug($"Dark Explodes on {__instance.Id} with {__instance.GetAuraCharges("dark")} + {auxInt} charges.");
                    // Heal all heroes for 20% of the Dark stacks
                    int healAmount = Mathf.RoundToInt(nCharges * 0.2f);
                    Hero[] teamHeroes = MatchManager.Instance.GetTeamHero();
                    for (int i = 0; i < teamHeroes.Length; i++)
                    {
                        Hero hero = teamHeroes[i];
                        if (IsLivingHero(hero))
                        {
                            TraitHeal(ref __instance, hero, healAmount, "");
                            hero.SetAuraTrait(hero, "zeal", 1);
                            hero.SetAuraTrait(hero, "burn", 5);
                        }
                    }

                }

            }
        }



        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        // public static void HealAuraCursePrefix(ref Character __instance, AuraCurseData AC, ref int __state)
        // {
        //     LogInfo($"HealAuraCursePrefix {subclassName}");
        //     string traitOfInterest = trait4b;
        //     if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth"))
        //     {
        //         __state = Mathf.FloorToInt(__instance.GetAuraCharges("stealth") * 0.25f);
        //         // __instance.SetAuraTrait(null, "stealth", 1);

        //     }

        // }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        // public static void HealAuraCursePostfix(ref Character __instance, AuraCurseData AC, int __state)
        // {
        //     LogInfo($"HealAuraCursePrefix {subclassName}");
        //     string traitOfInterest = trait4b;
        //     if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth") && __state > 0)
        //     {
        //         // __state = __instance.GetAuraCharges("stealth");
        //         __instance.SetAuraTrait(null, "stealth", __state);
        //     }

        // }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }



        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]
        // public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        // {
        //     // LogInfo("executing SetDescriptionNewPostfix");
        //     if (__instance == null)
        //     {
        //         LogDebug("Null Card");
        //         return;
        //     }
        //     if (!Globals.Instance.CardsDescriptionNormalized.ContainsKey(__instance.Id))
        //     {
        //         LogError($"missing card Id {__instance.Id}");
        //         return;
        //     }


        //     if (__instance.CardName == "Mind Maze")
        //     {
        //         StringBuilder stringBuilder1 = new StringBuilder();
        //         LogDebug($"Current description for {__instance.Id}: {stringBuilder1}");
        //         string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
        //         stringBuilder1.Append(currentDescription);
        //         // stringBuilder1.Replace($"When you apply", $"When you play a Mind Spell\n or apply");
        //         stringBuilder1.Replace($"Lasts one turn", $"Lasts two turns");
        //         BinbinNormalizeDescription(ref __instance, stringBuilder1);
        //     }
        // }

    }
}


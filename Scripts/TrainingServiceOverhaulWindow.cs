// Project:         TrainingServiceOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    12/22/2021, 8:50 PM
// Last Edit:		12/23/2020, 11:50 PM
// Version:			1.00
// Special Thanks:  John Doom, Kab the Bird Ranger
// Modifier:

using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.UserInterface;
using UnityEngine;
using DaggerfallWorkshop.Game.Formulas;
using TrainingServiceOverhaul;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class TrainingServiceOverhaulWindow : DaggerfallGuildServiceTraining
    {
        DFCareer.Skills skillToTrain;

        public static int PickedSkillIndex { get; set; }
        public static string PickedSkillName { get; set; }

        public TrainingServiceOverhaulWindow(IUserInterfaceManager uiManager, GuildNpcServices npcService, IGuild guild) : base(uiManager, npcService, guild)
        {
        }

        protected override void TrainingService()
        {
            CloseWindow();
            // Check enough time has passed since last trained
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            if ((now.ToClassicDaggerfallTime() - playerEntity.TimeOfLastSkillTraining) < TrainingServiceOverhaulMain.GetReqRecovHours() * 60) // 540 default
            {
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRandomTokens(TrainingToSoonId);
                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, uiManager.TopWindow);
                messageBox.SetTextTokens(tokens);
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
            }
            else
            {
                // Show skill picker loaded with guild training skills
                DaggerfallListPickerWindow skillPicker = new DaggerfallListPickerWindow(uiManager, this);
                skillPicker.OnItemPicked += HandleSkillPickEvent;

                foreach (DFCareer.Skills skill in GetTrainingSkills())
                    skillPicker.ListBox.AddItem(DaggerfallUnity.Instance.TextProvider.GetSkillName(skill));

                uiManager.PushWindow(skillPicker);
            }
        }

        public void HandleSkillPickEvent(int index, string skillName)
        {
            PickedSkillIndex = index;
            PickedSkillName = skillName;
            List<DFCareer.Skills> trainingSkills = GetTrainingSkills();
            DFCareer.Skills skillToTrain = trainingSkills[index];
            int guildhallQuality = 0;
            string facTitle = "Stranger";
            guildhallQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            if (Guild.IsMember())
                facTitle = Guild.GetTitle();

            CloseWindow();

            // Offer training price
            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, uiManager.TopWindow);

            int trainingPrice = CalculateTrainingPrice(guildhallQuality, player, skillToTrain);

            TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                    TextFile.Formatting.JustifyCenter,
                    "For a session in " + skillName + ",",
                    "it will cost you " + trainingPrice.ToString() + " gold.",
                    "Still interested, " + facTitle + "?");

            messageBox.SetTextTokens(tokens, Guild);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            messageBox.OnButtonClick += ConfirmTraining_OnButtonClick;
            messageBox.Show();
        }

        protected override void ConfirmTraining_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            int index = PickedSkillIndex;
            string skillName = PickedSkillName;
            int guildhallQuality = 0;
            guildhallQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            CloseWindow();

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                if (playerEntity.GetGoldAmount() >= CalculateTrainingPrice(guildhallQuality, player, skillToTrain))
                    TrainingPickedSkill(index, skillName);
                else
                    DaggerfallUI.MessageBox(DaggerfallTradeWindow.NotEnoughGoldId);
            }
        }

        public void TrainingPickedSkill(int index, string skillName)
        {
            CloseWindow();
            List<DFCareer.Skills> trainingSkills = GetTrainingSkills();
            DFCareer.Skills skillToTrain = trainingSkills[index];
            int guildhallQuality = 0;
            string facTitle = "Stranger";
            guildhallQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int trainingMaximum = CalculateTrainingMaximum(guildhallQuality);
            int MaxPossTrain = TrainingServiceOverhaulMain.GetMaxPossibleTrain();
            if (Guild.IsMember())
                facTitle = Guild.GetTitle();

            if (playerEntity.Skills.GetPermanentSkillValue(skillToTrain) > trainingMaximum)
            {
                TextFile.Token[] tokens = null;
                // Inform player they're too skilled to train
                if (playerEntity.Skills.GetPermanentSkillValue(skillToTrain) >= MaxPossTrain)
                {
                    tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "It seems the student has become the master, " + facTitle + ". There is nothing",
                        "more I can teach you. A true master in " + skillName,
                        "does not bother with theory their whole life. They",
                        "put those theories to practice and become innovators.",
                        "Now get out there and become a real master, " + facTitle + "!",
                        "",
                        "(Can't Be Trained Further Than " + MaxPossTrain + " In This Skill)");
                }
                else
                {
                    tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Train you, " + facTitle + "? Ha, you could probably",
                        "teach me a thing or two about " + skillName + "! If you want",
                        "more training, best find a trainer with more experience.",
                        "",
                        "(Max Training Up To " + trainingMaximum.ToString() + " Here)");
                }

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, uiManager.TopWindow);
                messageBox.SetTextTokens(tokens, Guild);
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
            }
            else
            {   // Train the skill
                bool reduceHealth = false;
                bool reduceMagicka = false;
                DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
                playerEntity.TimeOfLastSkillTraining = now.ToClassicDaggerfallTime();
                now.RaiseTime(DaggerfallDateTime.SecondsPerHour * TrainingServiceOverhaulMain.GetHoursPassedTraining()); // 3
                int trainingPrice = CalculateTrainingPrice(guildhallQuality, player, skillToTrain);
                int statReduceAmount = CalculateStatSessionReduction(skillToTrain, out reduceHealth, out reduceMagicka);
                playerEntity.DeductGoldAmount(trainingPrice);
                if (TrainingServiceOverhaulMain.GetAllowHPMPDamage() && reduceHealth)
                {
                    int hpDecreased = HealthDecreaseAmount(player);
                    playerEntity.DecreaseHealth(hpDecreased);
                }
                if (TrainingServiceOverhaulMain.GetAllowHPMPDamage() && reduceMagicka)
                {
                    int mpDecreased = MagickaDecreaseAmount(player);
                    playerEntity.DecreaseMagicka(mpDecreased);
                }
                playerEntity.DecreaseFatigue(statReduceAmount * 180); // Maybe eventually refuse training if player is too exhausted
                int trainingAmount = CalculateTrainingAmount(guildhallQuality, player, skillToTrain);
                int skillAdvancementMultiplier = DaggerfallSkills.GetAdvancementMultiplier(skillToTrain);
                short tallyAmount = (short)(trainingAmount * skillAdvancementMultiplier);
                playerEntity.TallySkill(skillToTrain, tallyAmount);
                DaggerfallUI.MessageBox(TrainSkillId);
            }
        }

        public int CalculateTrainingMaximum(int Quality)
        {
            if (Quality <= 3)       // 01 - 03 "Awful"
            {
                return TrainingServiceOverhaulMain.GetMaxTrainAwful(); // 35
            }
            else if (Quality <= 7)  // 04 - 07 "Poor"
            {
                return TrainingServiceOverhaulMain.GetMaxTrainPoor(); // 50
            }
            else if (Quality <= 13) // 08 - 13 "Decent"
            {
                return TrainingServiceOverhaulMain.GetMaxTrainDecent(); // 65
            }
            else if (Quality <= 17) // 14 - 17 "Good"
            {
                return TrainingServiceOverhaulMain.GetMaxTrainGood(); // 75
            }
            else                    // 18 - 20 "Great"
            {
                return TrainingServiceOverhaulMain.GetMaxTrainGreat(); // 85
            }
        }

        public int CalculateTrainingPrice(int Quality, PlayerEntity player, DFCareer.Skills skillToTrain)
        {
            int skillValue = playerEntity.Skills.GetPermanentSkillValue(skillToTrain); // Will likely want to change the pricing around later.
            int goldCost = 1;

            if (Quality <= 3)       // 01 - 03
            {
                if (skillValue >= 70)
                    goldCost = (int)((25 + Quality) * skillValue * 2.0);
                else
                    goldCost = (25 + Quality) * skillValue;
            }
            else if (Quality <= 7)  // 04 - 07
            {
                if (skillValue >= 70)
                    goldCost = (int)((25 + Quality) * skillValue * 1.90);
                else
                    goldCost = (25 + Quality) * skillValue;
            }
            else if (Quality <= 13) // 08 - 13
            {
                if (skillValue >= 70)
                    goldCost = (int)((25 + Quality) * skillValue * 1.80);
                else
                    goldCost = (25 + Quality) * skillValue;
            }
            else if (Quality <= 17) // 14 - 17
            {
                if (skillValue >= 70)
                    goldCost = (int)((25 + Quality) * skillValue * 1.70);
                else
                    goldCost = (25 + Quality) * skillValue;
            }
            else                    // 18 - 20
            {
                if (skillValue >= 70)
                    goldCost = (int)((25 + Quality) * skillValue * 1.60);
                else
                    goldCost = (25 + Quality) * skillValue;
            }

            if (!Guild.IsMember())
                goldCost = goldCost * TrainingServiceOverhaulMain.GetNonMembMulti(); // Cost mod for non-members
            goldCost = FormulaHelper.CalculateTradePrice(goldCost, Quality, false); // Cost modified by merchant skills like in a normal shop transaction (will test with and without to see effect.)
            goldCost = (int)Mathf.Ceil(goldCost * TrainingServiceOverhaulMain.GetFinalTrainCostMulti()); // Final training cost mod by settings
            player.TallySkill(DFCareer.Skills.Mercantile, (short)(Mathf.Floor(goldCost / 400) + 1));

            return goldCost;
        }

        public int CalculateTrainingAmount(int Quality, PlayerEntity player, DFCareer.Skills skillToTrain)
        {
            int playerLuck = (int)Mathf.Floor((player.Stats.PermanentLuck - 50) / 5f);
            int skillValue = playerEntity.Skills.GetPermanentSkillValue(skillToTrain);
            int trainingAmount = 1;

            if (Quality <= 3)       // 01 - 03
            {
                trainingAmount = UnityEngine.Random.Range(13 + Quality + playerLuck, 15 + Quality + playerLuck);
            }
            else if (Quality <= 7)  // 04 - 07
            {
                trainingAmount = UnityEngine.Random.Range(13 + Quality + playerLuck, 19 + Quality + playerLuck);
            }
            else if (Quality <= 13) // 08 - 13
            {
                trainingAmount = UnityEngine.Random.Range(13 + Quality + playerLuck, 23 + Quality + playerLuck);
            }
            else if (Quality <= 17) // 14 - 17
            {
                if (skillValue >= 70)
                    trainingAmount = UnityEngine.Random.Range(27 + Quality + playerLuck, 36 + Quality + playerLuck);
                else
                    trainingAmount = UnityEngine.Random.Range(15 + Quality + playerLuck, 26 + Quality + playerLuck);
            }
            else                    // 18 - 20
            {
                if (skillValue >= 70)
                    trainingAmount = UnityEngine.Random.Range(30 + Quality + playerLuck, 45 + Quality + playerLuck);
                else
                    trainingAmount = UnityEngine.Random.Range(20 + Quality + playerLuck, 29 + Quality + playerLuck);
            }

            trainingAmount = (int)Mathf.Floor(trainingAmount * TrainingServiceOverhaulMain.GetFinalTrainedAmountMulti());
            return trainingAmount;
        }

        public int CalculateStatSessionReduction(DFCareer.Skills skillToTrain, out bool reduceHealth, out bool reduceMagicka)
        {
            int skillId = (int)skillToTrain;
            reduceHealth = false;
            reduceMagicka = false;

            switch (skillId)
            {
                default:
                    return 11;
                case 0:
                case 1:
                case 2:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    return 8; // Mostly non-physical academic type activities.
                case 3:
                case 17:
                case 18:
                case 19:
                case 21:
                case 33:
                case 34:
                    return 15; // Very physically taxing activities, but not necessarily dangerous.
                case 20:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                    reduceHealth = true; // Physical and potentially dangerous activities.
                    return 12;
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                    reduceMagicka = true; // Non-physical magic based activites.
                    return 9;
            }
        }

        public int HealthDecreaseAmount(PlayerEntity player) // Maybe eventually refuse training if player is too hurt
        {
            float rolledHpPercent = UnityEngine.Random.Range(0.10f, 0.25f);
            int hpReduceValue = (int)Mathf.Floor(player.MaxHealth * rolledHpPercent);

            if (player.CurrentHealth > hpReduceValue)
                return hpReduceValue;
            else
                return 0;
        }

        public int MagickaDecreaseAmount(PlayerEntity player) // Maybe eventually refuse training if player is too low on magic
        {
            float rolledMpPercent = UnityEngine.Random.Range(0.05f, 0.20f);
            int mpReduceValue = Mathf.Max((int)Mathf.Floor(player.MaxMagicka * rolledMpPercent), 5);

            if (player.CurrentMagicka > mpReduceValue)
                return mpReduceValue;
            else
                return 0;
        }
    }
}

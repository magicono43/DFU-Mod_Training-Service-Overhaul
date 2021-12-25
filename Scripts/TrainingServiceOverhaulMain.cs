// Project:         TrainingServiceOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    12/22/2021, 8:50 PM
// Last Edit:		12/23/2020, 11:50 PM
// Version:			1.00
// Special Thanks:  John Doom, Kab the Bird Ranger
// Modifier:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace TrainingServiceOverhaul
{
    public class TrainingServiceOverhaulMain : MonoBehaviour
    {
        static TrainingServiceOverhaulMain instance;

        public static TrainingServiceOverhaulMain Instance
        {
            get { return instance ?? (instance = FindObjectOfType<TrainingServiceOverhaulMain>()); }
        }

        static Mod mod;

        public static int RequiredRecoveryHours { get; set; }
        public static int NonMemberCostMultiplier { get; set; }
        public static float FinalTrainingCostMultiplier { get; set; }
        public static int MaxTrainAwful { get; set; }
        public static int MaxTrainPoor { get; set; }
        public static int MaxTrainDecent { get; set; }
        public static int MaxTrainGood { get; set; }
        public static int MaxTrainGreat { get; set; }
        public static float FinalTrainedAmountMultiplier { get; set; }
        public static int HoursPassedDuringTraining { get; set; }
        public static bool AllowHealthMagicDamage { get; set; }
        public static int MaximumPossibleTraining { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            instance = new GameObject("TrainingServiceOverhaul").AddComponent<TrainingServiceOverhaulMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Training Service Overhaul");

            mod.LoadSettings();

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.GuildServiceTraining, typeof(TrainingServiceOverhaulWindow));

            Debug.Log("Finished mod init: Training Service Overhaul");
        }

        #region Settings

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            RequiredRecoveryHours = mod.GetSettings().GetValue<int>("TimeRelated", "HoursNeededBetweenSessions");
            NonMemberCostMultiplier = mod.GetSettings().GetValue<int>("GoldCost", "Non-MemberCostMultiplier");
            FinalTrainingCostMultiplier = mod.GetSettings().GetValue<float>("GoldCost", "FinalCostMultiplier");
            MaxTrainAwful = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxAwfulHallsCanTrain");
            MaxTrainPoor = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxPoorHallsCanTrain");
            MaxTrainDecent = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxDecentHallsCanTrain");
            MaxTrainGood = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxGoodHallsCanTrain");
            MaxTrainGreat = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxGreatHallsCanTrain");
            FinalTrainedAmountMultiplier = mod.GetSettings().GetValue<float>("TrainingExperience", "TrainedXPMultiplier");
            HoursPassedDuringTraining = mod.GetSettings().GetValue<int>("TimeRelated", "HoursPassedDuringSessions");
            AllowHealthMagicDamage = mod.GetSettings().GetValue<bool>("VitalsRelated", "AllowHealthMagicDamage");
            MaximumPossibleTraining = mod.GetSettings().GetValue<int>("MaxSkillsCanBeTrain", "MaxPossibleTraining");
        }

        #endregion

        public static int GetReqRecovHours()
        {
            return RequiredRecoveryHours;
        }

        public static int GetNonMembMulti()
        {
            return NonMemberCostMultiplier;
        }

        public static float GetFinalTrainCostMulti()
        {
            return FinalTrainingCostMultiplier;
        }

        public static int GetMaxTrainAwful()
        {
            return MaxTrainAwful;
        }

        public static int GetMaxTrainPoor()
        {
            return MaxTrainPoor;
        }

        public static int GetMaxTrainDecent()
        {
            return MaxTrainDecent;
        }

        public static int GetMaxTrainGood()
        {
            return MaxTrainGood;
        }

        public static int GetMaxTrainGreat()
        {
            return MaxTrainGreat;
        }

        public static float GetFinalTrainedAmountMulti()
        {
            return FinalTrainedAmountMultiplier;
        }

        public static int GetHoursPassedTraining()
        {
            return HoursPassedDuringTraining;
        }

        public static bool GetAllowHPMPDamage()
        {
            return AllowHealthMagicDamage;
        }

        public static int GetMaxPossibleTrain()
        {
            return MaximumPossibleTraining;
        }
    }
}

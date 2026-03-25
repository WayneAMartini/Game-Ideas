using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class WalletSaveData
    {
        public double Gold;
        public double Stardust;
        public double AscensionShards;
        public double AetherCrystals;
        public double ClassTokens;
        public double GuildCoins;
        public double StarFragments;
    }

    public class Wallet : MonoBehaviour
    {
        public static Wallet Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            LoadFromSave();
        }

        void LoadFromSave()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            var cm = CurrencyManager.Instance;
            if (cm == null) return;

            cm.SetCurrency(CurrencyType.Gold, save.Gold);

            if (save.WalletData != null)
            {
                cm.SetCurrency(CurrencyType.Stardust, save.WalletData.Stardust);
                cm.SetCurrency(CurrencyType.AscensionShards, save.WalletData.AscensionShards);
                cm.SetCurrency(CurrencyType.AetherCrystals, save.WalletData.AetherCrystals);
                cm.SetCurrency(CurrencyType.ClassTokens, save.WalletData.ClassTokens);
                cm.SetCurrency(CurrencyType.GuildCoins, save.WalletData.GuildCoins);
                cm.SetCurrency(CurrencyType.StarFragments, save.WalletData.StarFragments);
            }
        }

        public WalletSaveData GatherSaveData()
        {
            var cm = CurrencyManager.Instance;
            if (cm == null) return new WalletSaveData();

            return new WalletSaveData
            {
                Gold = cm.Gold,
                Stardust = cm.Stardust,
                AscensionShards = cm.AscensionShards,
                AetherCrystals = cm.AetherCrystals,
                ClassTokens = cm.ClassTokens,
                GuildCoins = cm.GuildCoins,
                StarFragments = cm.StarFragments
            };
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

﻿using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        private static Logger logger = LogManager.GetLogger("MainLogger");
        private static Logger headerLogger = LogManager.GetLogger("HeaderLogger");
        private Linker linker;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            headerLogger.Info("");
            logger.Info("Loading...");
            // Setup Loading text
            LoadingText = "Loading...";
            Progress<string> loadingProgress = new Progress<string>((data) =>
            {
                LoadingText = data;
            });

            // Setup Managers & Linkers
            linker = new Linker();
            await linker.Setup(loadingProgress);

            // Get Pool List
            logger.Info("Loading Pool List...");
            PoolList = new List<PoolData>(await linker.networkManager.GetPoolData(linker.networkManager.data.pools));
            PoolList.Add(PoolData.Custom);

            // Check Saved Address
            AddressInput = linker.minerManager.GetSavedAddress();
            logger.Info("Using saved address: " + AddressInput);

            //await linker.minerManager.InstallMiner("nvidia_win", loadingProgress);
            logger.Info("Miners Installed: " + String.Join(", ", linker.minerManager.data.installed.Keys.ToArray()));

            // Set Default values
            ReadyToShow = true;
            ShowStats = false;
            ShowStop = false;
            ShowCustomPool = false;
            InfoText = "Ready!";
            
            // Tell it we're done
            logger.Info("Finished Loading.");
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Progress<string> progress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            await linker.minerManager.StopMining(progress);

            await linker.minerManager.data.SaveAsync();

            logger.Info("Closed all processes...");
            headerLogger.Info("");
        }

        #region WPF Properties

        private bool readyToShow;
        public bool ReadyToShow
        {
            get
            {
                return readyToShow;
            }
            set
            {
                readyToShow = value;

                OnPropertyChanged(nameof(ReadyToShow));
            }
        }

        private string loadingText;
        public string LoadingText
        {
            get
            {
                return loadingText;
            }
            set
            {
                loadingText = value;

                OnPropertyChanged(nameof(LoadingText));
            }
        }

        private string infoText;
        public string InfoText
        {
            get
            {
                return infoText;
            }
            set
            {
                infoText = value;

                OnPropertyChanged(nameof(InfoText));
            }
        }

        private string addressInput;
        public string AddressInput
        {
            get
            {
                return addressInput;
            }
            set
            {
                addressInput = value;

                OnPropertyChanged(nameof(AddressInput));
            }
        }

        private string poolInput;
        public string PoolInput
        {
            get
            {
                return poolInput;
            }
            set
            {
                poolInput = value;

                OnPropertyChanged(nameof(PoolInput));
            }
        }

        private bool readyToStart;
        public bool ReadyToStart
        {
            get
            {
                return readyToStart;
            }
            set
            {
                readyToStart = value;

                OnPropertyChanged(nameof(ReadyToStart));
            }
        }

        private bool showStop;
        public bool ShowStop
        {
            get
            {
                return showStop;
            }
            set
            {
                showStop = value;

                OnPropertyChanged(nameof(ShowStop));
            }
        }
        
        private bool showStats;
        public bool ShowStats
        {
            get
            {
                return showStats;
            }
            set
            {
                showStats = value;

                OnPropertyChanged(nameof(ShowStats));
            }
        }

        private PoolData lastPoolDataValue;
        private List<PoolData> poolList;
        public List<PoolData> PoolList
        {
            get
            {
                return poolList;
            }
            set
            {
                poolList = value;

                OnPropertyChanged(nameof(PoolList));
            }
        }

        private bool showCustomPool;
        public bool ShowCustomPool
        {
            get
            {
                return showCustomPool;
            }
            set
            {
                showCustomPool = value;

                OnPropertyChanged(nameof(ShowCustomPool));
            }
        }
        
        private string miningInfoText;
        public string MiningInfoText
        {
            get
            {
                return miningInfoText;
            }
            set
            {
                miningInfoText = value;

                OnPropertyChanged(nameof(MiningInfoText));
            }
        }

        private string hashrateText;
        public string HashrateText
        {
            get
            {
                return hashrateText;
            }
            set
            {
                hashrateText = value;

                OnPropertyChanged(nameof(HashrateText));
            }
        }

        private string lastBlockText;
        public string LastBlockText
        {
            get
            {
                return lastBlockText;
            }
            set
            {
                lastBlockText = value;

                OnPropertyChanged(nameof(LastBlockText));
            }
        }

        private string acceptedSharesText;
        public string AcceptedSharesText
        {
            get
            {
                return acceptedSharesText;
            }
            set
            {
                acceptedSharesText = value;

                OnPropertyChanged(nameof(AcceptedSharesText));
            }
        }

        private string rejectedSharesText;
        public string RejectedSharesText
        {
            get
            {
                return rejectedSharesText;
            }
            set
            {
                rejectedSharesText = value;

                OnPropertyChanged(nameof(RejectedSharesText));
            }
        }

        private string temperatureText;
        public string TemperatureText
        {
            get
            {
                return temperatureText;
            }
            set
            {
                temperatureText = value;

                OnPropertyChanged(nameof(TemperatureText));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Start & Stop

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            Progress<MiningStatus> startingProgress = new Progress<MiningStatus>((data) =>
            {
                // TODO: Add an Amount Earned feature
                InfoText = data.info;
                HashrateText = "Hashrate: " + data.hashRate;
                LastBlockText = "Block: " + data.lastBlock;
                AcceptedSharesText = "Accepted Shares: " + data.acceptedShares;
                RejectedSharesText = "Rejected Shares: " + data.rejectedShares;
                TemperatureText = "Temperature: " + data.temperature;
            });

            ReadyToStart = false;
            ShowStop = true;
            ShowStats = true;

            string poolInfo = (lastPoolDataValue.id == -1 ? "Custom Pool (" + lastPoolDataValue.stratum + ")" : lastPoolDataValue.name + " (" + PoolInput.Trim() + ")");
            MiningInfoText = "Mining on " + poolInfo;
            logger.Info("Strating miners on " + poolInfo);

            await linker.minerManager.StartMining(AddressInput.Trim(), PoolInput.Trim(), startingProgress);

            ShowStats = false;
            ShowStop = false;
            ReadyToStart = true;
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            Progress<string> stoppingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            ReadyToStart = false;
            ShowStop = false;

            await linker.minerManager.StopMining(stoppingProgress);

            ShowStop = false;
            ReadyToStart = true;
        }

        #endregion
        
        #region Mining Buttons

        private void MiningNvidia_Checked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });
#pragma warning disable 4014 
            linker.minerManager.EnableMiner("nvidia", progress, installingProgress);
#pragma warning restore 4014 

        }

        private void MiningNvidia_Unchecked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner("nvidia", progress);
        }

        private void MiningAMD_Checked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });
#pragma warning disable 4014
            linker.minerManager.EnableMiner("amd", progress, installingProgress);
#pragma warning restore 4014
        }

        private void MiningAMD_Unchecked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner("amd", progress);
        }

        private void MiningCPU_Checked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });
#pragma warning disable 4014
            linker.minerManager.EnableMiner("cpu", progress, installingProgress);
#pragma warning restore 4014
        }

        private void MiningCPU_Unchecked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner("cpu", progress);
        }

        #endregion

        #region Input Value Change

        private void PoolListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count != 0)
            {
                PoolData poolData = e.AddedItems[0] as PoolData;

                // If it's the same value, don't change, it might override custom pool input
                if (lastPoolDataValue != null && lastPoolDataValue.id == poolData.id) return;

                logger.Info("Using pool: " + poolData);

                // If it's a custom, show the pool input
                if (poolData.name == "Custom")
                {
                    PoolInput = "";
                    ShowCustomPool = true;
                }
                // If it's not, set pool input to the first stratum
                else
                {
                    PoolInput = poolData.Value();
                    ShowCustomPool = false;
                }

                lastPoolDataValue = poolData;
            }
        }

        private void AddressInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            linker.minerManager.SaveAddress((sender as TextBox).Text.Trim());
        }

        #endregion
    }
}

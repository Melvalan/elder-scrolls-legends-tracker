﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ESLTracker.DataModel;
using ESLTracker.Properties;
using ESLTracker.Utils;
using ESLTracker.Utils.Messages;
using ESLTracker.ViewModels.Decks;

namespace ESLTracker.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool deckEditVisible;

        public bool DeckEditVisible
        {
            get { return deckEditVisible; }
            set
            {
                deckEditVisible = value;
                RaisePropertyChangedEvent("DeckEditVisible");
            }
        }

        private bool deckListVisible = true;

        public bool DeckListVisible
        {
            get { return deckListVisible; }
            set { deckListVisible = value; RaisePropertyChangedEvent("DeckListVisible"); }
        }

        private bool deckStatsVisible = true;

        public bool DeckStatsVisible
        {
            get { return deckStatsVisible; }
            set { deckStatsVisible = value; RaisePropertyChangedEvent("DeckStatsVisible"); }
        }

        private bool settingsVisible = false;

        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; RaisePropertyChangedEvent("SettingsVisible"); }
        }

        private bool editGameVisible = false;

        public bool EditGameVisible
        {
            get { return editGameVisible; }
            set { editGameVisible = value; RaisePropertyChangedEvent("EditGameVisible"); }
        }

        private bool allowCommands = true;

        public bool AllowCommands
        {
            get { return allowCommands; }
            set { allowCommands = value; RaisePropertyChangedEvent("AllowCommands"); }
        }

        private bool showInTaskBar = true;

        public bool ShowInTaskBar
        {
            get { return showInTaskBar; }
            set { showInTaskBar = value; RaisePropertyChangedEvent("ShowInTaskBar"); }
        }

        private WindowState windowState;

        public WindowState WindowState
        {
            get { return windowState; }
            set { windowState = value; RaisePropertyChangedEvent("WindowState"); }
        }

        #region Commands
        public ICommand CommandEditSettings
        {
            get { return new RelayCommand(new Action<object>(EditSettings)); }
        }

        public ICommand CommandNotifyIconLeftClick
        {
            get { return new RelayCommand(new Action<object>(NotifyIconLeftClick)); }
        }

        public ICommand CommandExit
        {
            get { return new RelayCommand(new Action<object>(Exit)); }
        }

        public ICommand CommandShowRewards
        {
            get { return new RelayCommand(new Action<object>(ShowRewards)); }
        }

        public ICommand CommandNewDeck
        {
            get { return new RelayCommand(new Action<object>(NewDeck)); }
        }

        public ICommand CommandShowOverlay
        {
            get { return new RelayCommand(new Action<object>(ShowOverlay)); }
        }

        public IAsyncCommand CommandRunGame
        {
            get
            {
                return new RealyAsyncCommand<object>(
                    new Func<Task<object>>(CommandRunGameExecute),
                    new Func<object, bool>(CommandRunGameCanExecute)
                    );
            }
        }

        public ICommand CommandShowArenaStats
        {
            get { return new RelayCommand(new Action<object>(CommandShowArenaStatsExecute)); }
        }

        public ICommand CommandShowGamesStats
        {
            get { return new RelayCommand(new Action<object>(CommandShowGamesStatsExecute)); }
        }

        public ICommand CommandShowRankedProgress
        {
            get { return new RelayCommand(new Action<object>(CommandShowRankedProgressExecute)); }
        }

        public ICommand CommandEditDeck
        {
            get { return new RelayCommand(new Action<object>(CommandEditDeckExecute)); }
        }

        #endregion

        ITrackerFactory trackerFactory;
        IMessenger messanger;
        ITracker tracker;

        public MainWindowViewModel() : this(new TrackerFactory())
        {
        }

        internal MainWindowViewModel(TrackerFactory trackerFactory)
        {
            this.trackerFactory = trackerFactory;
            tracker = trackerFactory.GetTracker();
            messanger = trackerFactory.GetMessanger();
            messanger.Register<Utils.Messages.EditGame>(this, EditGameStart, Utils.Messages.EditGame.Context.StartEdit);
            messanger.Register<Utils.Messages.EditGame>(this, EditGameFinished, Utils.Messages.EditGame.Context.EditFinished);
            messanger.Register<Utils.Messages.EditSettings>(this, EditSettingsFinished, Utils.Messages.EditSettings.Context.EditFinished);
        }

        public void NotifyIconLeftClick(object parameter)
        {
            if ((WindowState == WindowState.Normal) && (parameter as string != "show"))
            {
                WindowState = WindowState.Minimized;
                ShowInTaskBar = false;
            }
            else
            {
                if (WindowState == WindowState.Normal)
                {
                    //force change for context menu option
                    WindowState = WindowState.Minimized;
                }
                ShowInTaskBar = true;
                WindowState = WindowState.Normal;
            }
        }

        public void Exit(object parameter)
        {
            bool checkIfCanClose = false;
            if (parameter !=null && parameter is bool)
            {
                checkIfCanClose = (bool)parameter;
            }
            if (!checkIfCanClose || (checkIfCanClose && MainWindow.ot.CanClose(CommandExit)))
            {
                trackerFactory.GetFileManager().SaveDatabase();
                MainWindow.UpdateOverlay = false;
                MainWindow.ot.Close();
                ISettings settings = trackerFactory.GetSettings();
                settings.LastActiveDeckId = tracker.ActiveDeck?.DeckId;
                settings.Save();
                ((App)Application.Current).CloseApplication();
            }
        }

        public void ShowRewards(object parameter)
        {
            new RewardsSummary().Show();
        }

        public void NewDeck(object parameter)
        {
            messanger.Send(
                new Utils.Messages.EditDeck() { Deck = Deck.CreateNewDeck("New deck") },
                Utils.Messages.EditDeck.Context.StartEdit
                );
        }

        public void ShowOverlay(object parameter)
        {
            ((MainWindow)Application.Current.MainWindow).RestoreOverlay();
        }

        bool startingGame;
        public async Task<object> CommandRunGameExecute()
        {
            startingGame = true;
            CommandManager.InvalidateRequerySuggested();
            IWinAPI winApi = trackerFactory.GetWinAPI();
            bool isLauncherRunning = winApi.IsLauncherProcessRunning();

            if (winApi.GetEslProcess() == null && ! winApi.IsLauncherProcessRunning())
            {
                System.Diagnostics.Process.Start("bethesdanet://run/5");
                trackerFactory.GetMessanger().Send(new ApplicationShowBalloonTip("ESL Tracker", "Starting game..."));
                await Task.Delay(TimeSpan.FromSeconds(60)); //wait 10 sec
                if (winApi.GetEslProcess() == null)
                {
                    trackerFactory.GetMessanger().Send(new ApplicationShowBalloonTip("ESL Tracker", "There is probelm staring game, please check Bethesda.net Laucher."));
                }
            }
            else if (trackerFactory.GetWinAPI().IsLauncherProcessRunning())
            {
                trackerFactory.GetMessanger().Send(new ApplicationShowBalloonTip("ESL Tracker", "Bethesda.net Laucher is running - use it to start game."));
            }
            else
            {
                trackerFactory.GetMessanger().Send(new ApplicationShowBalloonTip("ESL Tracker", "Game is already running"));
            }
            startingGame = false;
            CommandManager.InvalidateRequerySuggested();
            return null;
        }

        private bool CommandRunGameCanExecute(object arg)
        {
            IWinAPI winApi = trackerFactory.GetWinAPI();
            return ! startingGame && winApi.GetEslProcess() == null;
        }

        public void EditSettings(object parameter)
        {
           // this.DeckStatsVisible = false;
            this.SettingsVisible = true;
            this.AllowCommands = false;

        }


        private void EditSettingsFinished(EditSettings obj)
        {
            this.SettingsVisible = false;
            this.AllowCommands = true;
        }

        private void EditGameStart(EditGame obj)
        {
            this.EditGameVisible = true;
            this.DeckStatsVisible = false;
            this.AllowCommands = false;
        }

        private void EditGameFinished(EditGame obj)
        {
            this.EditGameVisible = false;
            this.DeckStatsVisible = true;
            this.AllowCommands = true;
        }

        private void CommandShowArenaStatsExecute(object obj)
        {
            new ArenaStats().Show();
        }


        private void CommandShowGamesStatsExecute(object obj)
        {
            new GameStatistics().Show();
        }

        private void CommandShowRankedProgressExecute(object obj)
        {
            new RankedProgressChart().Show();
        }

        private void CommandEditDeckExecute(object obj)
        {
            messanger.Send(
                new EditDeck() { Deck = tracker.ActiveDeck },
                EditDeck.Context.StartEdit);
        }


    }
}

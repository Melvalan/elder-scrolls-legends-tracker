﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ESLTracker.DataModel;
using ESLTracker.Properties;
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
            get { return new RelayCommandWithSettings(new Action<object, ISettings>(Exit), Properties.Settings.Default); }
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

        public ICommand CommandRunGame
        {
            get { return new RelayCommand(
                                new Action<object>(CommandRunGameExecute), 
                                new Func<object, bool>(CommandRunGameCanExecute)
                                ); }
        }

        public MainWindowViewModel()
        {
            Utils.Messenger.Default.Register<Utils.Messages.EditDeck>(this, EditDeckStart, Utils.Messages.EditDeck.Context.StartEdit);
            Utils.Messenger.Default.Register<Utils.Messages.EditDeck>(this, EditDeckFinished, Utils.Messages.EditDeck.Context.EditFinished);
            Utils.Messenger.Default.Register<Utils.Messages.EditGame>(this, EditGameStart, Utils.Messages.EditGame.Context.StartEdit);
            Utils.Messenger.Default.Register<Utils.Messages.EditGame>(this, EditGameFinished, Utils.Messages.EditGame.Context.EditFinished);
        }

        public void NotifyIconLeftClick(object parameter)
        {
            if (parameter is Window)
            {
                Window w = parameter as Window;
                w.WindowState = WindowState.Normal;
                w.ShowInTaskbar = true;
                w.Activate();
                w.Focus();
            }
        }

        public void Exit(object parameter, ISettings settings)
        {
            Utils.FileManager.SaveDatabase();
            MainWindow.UpdateOverlay = false;
            settings.LastActiveDeckId = Tracker.Instance.ActiveDeck?.DeckId;
            settings.Save();
            ((App)Application.Current).Exit();
        }

        public void ShowRewards(object parameter)
        {
            new RewardsSummary().Show();
        }

        public void NewDeck(object parameter)
        {
            Utils.Messenger.Default.Send(
                new Utils.Messages.EditDeck() { Deck = EditDeckViewModel.CreateDefaultDeck() },
                Utils.Messages.EditDeck.Context.StartEdit
                );
        }

        public void ShowOverlay(object parameter)
        {
            ((MainWindow)Application.Current.MainWindow).RestoreOverlay();
        }

        public void CommandRunGameExecute(object parameter)
        {
            if (WindowsUtils.GetEslProcess() == null)
            {
                System.Diagnostics.Process.Start("bethesdanet://run/5");
            }
        }

        private bool CommandRunGameCanExecute(object arg)
        {
            return WindowsUtils.GetEslProcess() == null;
        }

        public void EditSettings(object parameter)
        {
           // this.DeckStatsVisible = false;
            this.SettingsVisible = true;
            this.AllowCommands = false;

        }

        private void EditDeckStart(Utils.Messages.EditDeck obj)
        {
            this.DeckEditVisible = true;
            this.AllowCommands = false;
        }

        private void EditDeckFinished(EditDeck obj)
        {
            this.DeckEditVisible = false;
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
    }
}

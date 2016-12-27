﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ESLTracker.DataModel;
using ESLTracker.Utils;

namespace ESLTracker.ViewModels.Packs
{
    public class OpenPackViewModel : ViewModelBase
    {
        private TrackerFactory trackerFactory;

        public Pack Pack { get; set; }

        public IEnumerable<string> CardNamesList
        {
            get
            {
                return trackerFactory.GetCardsDatabase().CardsNames;
            }
        }

        public ICommand CommandSave
        {
            get { return new RelayCommand(new Action<object>(CommandSaveExecute)); }
        }

        public OpenPackViewModel() : this(new TrackerFactory())
        {
            
        }

        public OpenPackViewModel(TrackerFactory trackerFactory)
        {
            this.trackerFactory = trackerFactory;
            Pack = new Pack();
        }

        private void CommandSaveExecute(object obj)
        {
            ITracker tracker = trackerFactory.GetTracker();
            Pack.DateOpened = trackerFactory.GetDateTimeNow();
           // trackerFactory.GetCardsDatabase().PopulateCollection(CardNames, Pack.Cards);
            tracker.Packs.Add(Pack);
            new FileManager(trackerFactory).SaveDatabase();
            Pack = new Pack();
        }
    }
}

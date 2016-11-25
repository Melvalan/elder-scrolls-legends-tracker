﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ESLTracker.DataModel;
using ESLTracker.DataModel.Enums;
using ESLTracker.Utils;

namespace ESLTracker.ViewModels
{
    public class DeckClassSelectorViewModel : ViewModelBase, IDeckClassSelectorViewModel
    {
        /// <summary>
        /// fiter of attributes, 
        /// bool value binded to isenabled property of trigger button
        /// </summary>
        public Dictionary<DeckAttribute, bool> FilterButtonState { get; set; }

        public ObservableCollection<DeckAttribute> FilterButtonStateCollection
        {
            get { return new ObservableCollection<DeckAttribute>(
                FilterButtonState.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList()
                ); }
        }

        /// <summary>
        /// source for drop down , list of classes that match current filter
        /// </summary>
        public ObservableCollection<DeckClass> FilteredClasses { get; set; }

        private DeckClass? selectedClass;
        public DeckClass? SelectedClass {
            get
            {
                return selectedClass;
            }
            set
            {
                selectedClass = value;
                if (value != null)
                {
                    ResetToggleButtons();
                    //toggle attributes buttons
                    if (selectedClass != null)
                    {
                        foreach (DeckAttribute da in SelectedClassAttributes)
                        {
                            FilterButtonState[da] = true;
                        }
                    }
                }
                RaisePropertyChangedEvent("FilterButtonStateCollection");
                RaisePropertyChangedEvent("SelectedClass");
            }
        }

        public DeckAttributes SelectedClassAttributes
        {
            get
            {
                if (SelectedClass.HasValue)
                {
                    return ClassAttributesHelper.Classes[SelectedClass.Value];
                }
                else
                {
                    return null;
                }
            }
        }

        //command for filter toggle button pressed
        public ICommand CommandFilterButtonPressed
        {
            get { return new RelayCommand(new Action<object>(FilterClicked)); }
        }

        public DeckClassSelectorViewModel()
        {
            FilterButtonState = new Dictionary<DeckAttribute, bool>();
            foreach (DeckAttribute a in Enum.GetValues(typeof(DeckAttribute)))
            {
                FilterButtonState.Add(a, false);
            }

            FilteredClasses = new ObservableCollection<DeckClass>();
            FilterCombo();
        }

        public void FilterClicked(object param)
        {
            DeckAttribute attrib;
            if (!Enum.TryParse<DeckAttribute>(param.ToString(), out attrib))
            {
                throw new ArgumentException(string.Format("Unknow value for deck attribute={0}", param));
            }

            //toggle filter value
            FilterButtonState[attrib] = ! FilterButtonState[attrib];

            FilterCombo();
        }

        public void FilterCombo()
        {
            var filteredClasses = Utils.ClassAttributesHelper.FindClassByAttribute(FilterButtonState.Where(f => f.Value).Select(f => f.Key)).ToList();
          
            if ((filteredClasses.Count >= 1)
                && (FilterButtonState.Any(f => f.Value)))
            {
                SelectedClass = filteredClasses.OrderBy( fc=> ClassAttributesHelper.Classes[fc].Count).First();
            }
            else
            {
                SelectedClass = null;
            }           
            //remove classes not in use.Clear() will trigger binding, as SelectedClass will be set to null by framework
            foreach (DeckClass dc in FilteredClasses.ToList())
            {
                if (!filteredClasses.Contains(dc))
                {
                    FilteredClasses.Remove(dc);
                }
            }
            // FilteredClasses.Clear();
            foreach (DeckClass dc in filteredClasses)
            {
                if (!FilteredClasses.Contains(dc))
                {
                    int i = 0;
                    IComparer<DeckClass> comparer = Comparer<DeckClass>.Default;
                    while (i < FilteredClasses.Count && comparer.Compare(FilteredClasses[i], dc) < 0)
                        i++;

                    FilteredClasses.Insert(i, dc);
                }
            }
            RaisePropertyChangedEvent("SelectedClass");
        }

        public void Reset()
        {
            ResetToggleButtons();
            FilterCombo();
            RaisePropertyChangedEvent("FilterButtonStateCollection");
        }

        private void ResetToggleButtons()
        {
            foreach (DeckAttribute a in Enum.GetValues(typeof(DeckAttribute)))
            {
                FilterButtonState[a] = false;
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESLTracker.DataModel;

namespace ESLTracker.Services
{
    public interface ICardsDatabase
    {
        IEnumerable<Card> Cards { get; }
        Version Version { get; set; }
        string VersionInfo { get; set; }
        DateTime VersionDate { get; set; }
        IEnumerable<CardSet> CardSets { get; }

        Card FindCardById(Guid value);
        Card FindCardByName(string name);
        ICardsDatabase RealoadDB();
        CardSet FindCardSetById(Guid? value);
        CardSet FindCardSetByName(string value);
        IEnumerable<string> GetCardsNames(string setFilter = null);
    }
}
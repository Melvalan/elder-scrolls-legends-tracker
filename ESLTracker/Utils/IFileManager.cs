﻿using System.Threading.Tasks;
using ESLTracker.DataModel;
using ESLTracker.Services;

namespace ESLTracker.Utils
{
    public interface IFileManager
    {
        void SaveDatabase();
        Tracker LoadDatabase(bool throwDataFileException = false);
        Task SaveScreenShot(string fileName);
        ICardsDatabase UpdateCardsDB(string newContent);
    }
}
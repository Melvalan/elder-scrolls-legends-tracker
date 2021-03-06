﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ESLTracker.ViewModels
{
    public interface IAsyncCommand<TResult> : ICommand
    {
        Task ExecuteAsync(object parameter);
        NotifyTaskCompletion<TResult> Execution { get; }
    }
}

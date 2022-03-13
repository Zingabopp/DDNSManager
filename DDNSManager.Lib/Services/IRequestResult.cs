﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DDNSManager.Lib.Services
{
    public interface IRequestResult
    {
        ResultStatus Status { get; }
        string Message { get; }
        string RawMessage { get; }
    }

    public enum ResultStatus
    {
        None,
        Completed,
        Skipped,
        Faulted
    }
}

namespace DDNSManager.Lib;

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;
    public bool IsError { get; }
    public TValue Value
    {
        get
        {
            if (IsError)
            {
                throw new InvalidOperationException("Cannot access Value when Result is in error state.");
            }
            return _value!;
        }
    }
    public TError Error
    {
        get
        {
            if (!IsError)
            {
                throw new InvalidOperationException("Cannot access Error when Result is not in error state.");
            }
            return _error!;
        }
    }
    public Result(TValue value)
    {
        _value = value;
        _error = default;
        IsError = false;
    }

    public Result(TError error)
    {
        _value = default;
        _error = error;
        IsError = true;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);


    public void Match(
        Action<TValue> success,
        Action<TError> error)
    {
        if (IsError)
        {
            error(_error!);
        }
        else
        {
            success(_value!);
        }
    }
    public async Task MatchAsync(
        Func<TValue, Task> success,
        Func<TError, Task> error)
    {
        if (IsError)
        {
            await error(_error!).ConfigureAwait(false);
        }
        else
        {
            await success(_value!).ConfigureAwait(false);
        }
    }

    public void Match<TArg>(
        TArg arg,
        Action<TValue, TArg> success,
        Action<TError, TArg> error)
    {
        if (IsError)
        {
            error(_error!, arg);
        }
        else
        {
            success(_value!, arg);
        }
    }
    public async Task MatchAsync<TArg>(
        TArg arg,
        Func<TValue, TArg, Task> success,
        Func<TError, TArg, Task> error)
    {
        if (IsError)
        {
            await error(_error!, arg).ConfigureAwait(false);
        }
        else
        {
            await success(_value!, arg).ConfigureAwait(false);
        }
    }


    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> error)
    {
        return IsError ? error(_error!) : success(_value!);
    }
    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> success,
        Func<TError, Task<TResult>> error)
        where TResult : Task
    {
        return IsError ? await error(_error!).ConfigureAwait(false) : await success(_value!).ConfigureAwait(false);
    }



    public TResult Match<TArg, TResult>(
        TArg arg,
        Func<TValue, TArg, TResult> success,
        Func<TError, TArg, TResult> error)
    {
        return IsError ? error(_error!, arg) : success(_value!, arg);
    }
    public async Task<TResult> MatchAsync<TArg, TResult>(
        TArg arg,
        Func<TValue, TArg, Task<TResult>> success,
        Func<TError, TArg, Task<TResult>> error)
        where TResult : Task
    {
        return IsError ? await error(_error!, arg).ConfigureAwait(false) : await success(_value!, arg).ConfigureAwait(false);
    }


}

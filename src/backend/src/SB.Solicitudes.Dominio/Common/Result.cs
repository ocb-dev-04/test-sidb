namespace SB.Solicitudes.Dominio.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.NONE)
        {
            throw new ArgumentException("A successful result cannot carry an error.", nameof(error));
        }

        if (!isSuccess && error == Error.NONE)
        {
            throw new ArgumentException("A failed result must carry an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.NONE);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.NONE);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

// Permite crear un fallo de un Result<T> conocido solo por su tipo genérico (pipeline de validación).
public interface IFailureFactory<out TSelf>
    where TSelf : Result
{
    static abstract TSelf CreateFailure(Error error);
}

public sealed class Result<TValue> : Result, IFailureFactory<Result<TValue>>
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
        => _value = value;

    static Result<TValue> IFailureFactory<Result<TValue>>.CreateFailure(Error error) => Failure<TValue>(error);

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    public Result<TNext> Map<TNext>(Func<TValue, TNext> mapper)
        => IsSuccess ? Success(mapper(Value)) : Failure<TNext>(Error);
}

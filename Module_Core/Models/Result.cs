using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_Core.Models
{
    /// <summary>
    /// Represents the result of an operation with success/failure state and optional error messages.
    /// </summary>
    public record Result
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; init; } = string.Empty;

        /// <summary>
        /// Collection of validation errors if applicable.
        /// </summary>
        public List<string> Errors { get; init; } = new();

        /// <summary>
        /// Indicates whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static Result Success() => new Result { IsSuccess = true };

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        public static Result Failure(string errorMessage) => new Result 
        { 
            IsSuccess = false, 
            ErrorMessage = errorMessage,
            Errors = new List<string> { errorMessage }
        };

        /// <summary>
        /// Creates a failed result with multiple error messages.
        /// </summary>
        public static Result Failure(IEnumerable<string> errors)
        {
            var errorList = errors.ToList();
            return new Result
            {
                IsSuccess = false,
                ErrorMessage = string.Join("; ", errorList),
                Errors = errorList
            };
        }
    }

    /// <summary>
    /// Represents the result of an operation that returns a value.
    /// </summary>
    /// <typeparam name="T">The type of the value being returned.</typeparam>
    public record Result<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// The value returned by the operation (null if failed).
        /// </summary>
        public T? Value { get; init; }

        /// <summary>
        /// Error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; init; } = string.Empty;

        /// <summary>
        /// Collection of validation errors if applicable.
        /// </summary>
        public List<string> Errors { get; init; } = new();

        /// <summary>
        /// Indicates whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        public static Result<T> Success(T value) => new Result<T> 
        { 
            IsSuccess = true, 
            Value = value 
        };

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        public static Result<T> Failure(string errorMessage) => new Result<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Errors = new List<string> { errorMessage }
        };

        /// <summary>
        /// Creates a failed result with multiple error messages.
        /// </summary>
        public static Result<T> Failure(IEnumerable<string> errors)
        {
            var errorList = errors.ToList();
            return new Result<T>
            {
                IsSuccess = false,
                ErrorMessage = string.Join("; ", errorList),
                Errors = errorList
            };
        }
    }
}

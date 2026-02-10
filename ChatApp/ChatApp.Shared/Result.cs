using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Shared
{
    public enum ResultType
    {
        Success,
        NotFound,
        InputError,
        FoundButInvalid,

        HttpFailure,
        UnknownFailure
    }

    public class Result
    {
        public ResultType ResultType { get; set; }
        public string ErrorMessage { get; set; }

        public bool IsSuccessful => this.ResultType == ResultType.Success;
        public bool IsException => this.ResultType == ResultType.HttpFailure || this.ResultType == ResultType.UnknownFailure;

        protected Result(ResultType resultType, string errorMessage)
        {
            this.ResultType = resultType;
            this.ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(ResultType.Success, string.Empty);
        public static Result Failure(string errorMessage) => new Result(ResultType.UnknownFailure, errorMessage);
    }

    public class Result<T> : Result where T : new()
    {
        public T Data { get; set; }

        protected Result(T data) : base(ResultType.Success, string.Empty)
        {
            this.Data = data;
        }

        public Result(ResultType resultType, string errorMessage) : base(resultType, errorMessage)
        {
            Data = new T();
        }

        public Result(ResultType resultType, string errorMessage, T data) : base(resultType, errorMessage)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new Result<T>(ResultType.Success, string.Empty) { Data = data };
        public static new Result<T> Failure(string errorMessage) => new Result<T>(ResultType.UnknownFailure, errorMessage) { Data = new T() };
    }
}

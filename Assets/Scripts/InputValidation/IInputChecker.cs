using System;

namespace Assets.Scripts.InputValidation
{
    internal interface IInputChecker
    {
        bool IsValid { get; }

        Type ValueType { get; }

        object GetValue();

        TReturnType GetValueAs<TReturnType>();

        void ForceUpdate();
    }
}
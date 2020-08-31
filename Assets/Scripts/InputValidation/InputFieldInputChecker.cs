using System;
using Assets.Scripts.InputValidation;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InputValidation
{
    internal class InputFieldInputChecker<TValueType> : IInputChecker
    {
        private TValueType _value;
        private bool _isValid;
        
        private InputField _inputField;
        private Color _normalColor;
        private Color _errorColor;

        private Func<string, TValueType> _parser;
        private Func<TValueType, bool> _validator;

        public InputFieldInputChecker(
            InputField inputField, 
            TValueType initialValue,
            Color normalColor, 
            Color errorColor,
            Func<string, TValueType> inputParser,
            Func<TValueType, bool> validator)
        {
            if (inputField == null)
                throw new ArgumentNullException(nameof(inputField));
            
            _inputField = inputField;

            if (inputParser == null)
                throw new ArgumentNullException(nameof(inputParser));

            _parser = inputParser;

            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _validator = validator;
            
            _normalColor = normalColor;
            _errorColor = errorColor;

            _value = initialValue;
            _isValid = validator(initialValue);

            SetColor(_isValid ? normalColor : errorColor);

            _inputField.text = initialValue.ToString();
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }

        public TValueType Value
        {
            get => _value;
            private set
            {
                _value = value;
                _isValid = _validator(value);
                
                SetColor(_isValid ? _normalColor : _errorColor);
            }
        }

        public bool IsValid => _isValid;

        public Type ValueType => typeof(TValueType);

        public object GetValue() => Value;

        public TReturnType GetValueAs<TReturnType>()
        {
            if (Value is TReturnType)
            {
                return (TReturnType)GetValue();
            }

            throw new NotSupportedException();
        }

        public void ForceUpdate()
        {
            Value = _parser(_inputField.text);
        }

        private void SetColor(Color color)
        {
            ColorBlock colorBlock = _inputField.colors;
            colorBlock.normalColor = color;
            colorBlock.highlightedColor = color;

            _inputField.colors = colorBlock;
        }

        private void OnEndEdit(string input)
        {
            Value = _parser(input);
        }
    }
}
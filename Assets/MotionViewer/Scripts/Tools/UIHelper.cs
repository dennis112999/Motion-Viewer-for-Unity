#if UNITY_EDITOR

using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Dennis.Tools
{
    /// <summary>
    /// Editor Window UI Helper
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// Creates Box UI element
        /// </summary>
        /// <param name="className">class USS name to add to the Box</param>
        /// <returns>Box UI</returns>
        public static Box CreateBox(string className = null)
        {
            Box box = new Box();
            AddClassIfNotEmpty(box, className);
            return box;
        }

        /// <summary>
        /// Creates Label UI element
        /// </summary>
        /// <param name="labelName">Label Name</param>
        /// <param name="className">class USS name to add to the Label</param>
        /// <returns>Label UI element</returns>
        public static Label CreateLabel(string labelName, string className = null)
        {
            Label label = new Label(labelName);
            AddClassIfNotEmpty(label, className);
            return label;
        }

        /// <summary>
        /// Create Button
        /// </summary>
        /// <param name="buttonText">text displayed</param>
        /// <param name="onClickAction">On Button click action</param>
        /// <param name="className">class USS name to add</param>
        /// <returns>Button</returns>
        public static Button CreateButton(string buttonText, Action onClickAction, string className = null)
        {
            Button button = new Button(onClickAction) { text = buttonText };
            AddClassIfNotEmpty(button, className);
            return button;
        }

        /// <summary>
        /// Create TextField
        /// </summary>
        /// <param name="initialValue">initial text value of the field</param>
        /// <param name="onValueChanged">action - the text value changes</param>
        /// <param name="className">class USS name to add</param>
        /// <returns>TextField with properties</returns>
        public static TextField CreateTextField(string initialValue, Action<string> onValueChanged, string className = null)
        {
            TextField textField = new TextField(string.Empty);
            textField.SetValueWithoutNotify(initialValue);
            textField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            AddClassIfNotEmpty(textField, className);
            return textField;
        }

        /// <summary>
        /// Create FloatField
        /// </summary>
        /// <param name="initialValue">initial text value of the field</param>
        /// <param name="onValueChanged">action - the value changes</param>
        /// <param name="className">class USS name to add</param>
        /// <returns>FloatField with properties</returns>
        public static FloatField CreateFloatField(float initialValue, Action<float> onValueChanged, string className = null)
        {
            FloatField floatField = new FloatField();
            floatField.SetValueWithoutNotify(initialValue);
            floatField.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            AddClassIfNotEmpty(floatField, className);
            return floatField;
        }

        /// <summary>
        /// Create Image
        /// </summary>
        /// <param name="className">class USS name to add</param>
        /// <returns>Image</returns>
        public static Image CreateImage(string className = null)
        {
            Image image = new Image();
            AddClassIfNotEmpty(image, className);
            return image;
        }

        /// <summary>
        /// Create Object Field
        /// </summary>
        /// <param name="initialValue">initial value of the object field</param>
        /// <param name="onValueChanged">action - value changes</param>
        /// <param name="classNames">class USS name to add</param>
        /// <returns></returns>
        public static ObjectField CreateObjectField<T>(T initialValue, Action<T> onValueChanged, string classNames = null) where T : UnityEngine.Object
        {
            // Create the ObjectField
            ObjectField objectField = new ObjectField
            {
                objectType = typeof(T),
                allowSceneObjects = false,
                value = initialValue
            };

            // Register a value change callback
            objectField.RegisterValueChangedCallback(value =>
            {
                onValueChanged?.Invoke(value.newValue as T);
            });

            AddClassIfNotEmpty(objectField, classNames);

            return objectField;
        }

        /// <summary>
        /// Creates an EnumField UI element
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="initialValue">Initial selected value</param>
        /// <param name="onValueChanged">Action triggered when value changes</param>
        /// <param name="className">USS class name to add</param>
        /// <returns>EnumField UI element</returns>
        public static EnumField CreateEnumField<T>(T initialValue, Action<T> onValueChanged, string className = null) where T : Enum
        {
            EnumField enumField = new EnumField(initialValue);

            enumField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is T newValue)
                {
                    onValueChanged?.Invoke(newValue);
                }
            });

            AddClassIfNotEmpty(enumField, className);

            return enumField;
        }

        /// <summary>
        /// Adds a USS class to a VisualElement
        /// </summary>
        /// <param name="element">The UI element to add the class to</param>
        /// <param name="className">The class name to add</param>
        private static void AddClassIfNotEmpty(VisualElement element, string className)
        {
            if (string.IsNullOrEmpty(className)) return;
            element.AddToClassList(className);
        }
    }
}

#endif
using UnityEngine.UIElements;
using UnityEngine;

using UnityEditor.UIElements;
using System;
using UnityEditor;

namespace Dennis.Tools.MotionViewer
{
    public class MotionItemElement : VisualElement
    {
        private MotionData _motionData;

        public MotionData MotionData { get { return _motionData; } }

        private Action<MotionData> _onRemoveButtonClick;

        public MotionItemElement(MotionData motion, Action<MotionData> OnCallBack = null)
        {
            _motionData = motion;
            _onRemoveButtonClick = OnCallBack;

            InitUIElements();
        }

        private void InitUIElements()
        {
            AddToClassList("motion-item");

            // Create MotionPhoto Preview
            Image previewImage = CreateMotionPhotoPreview();

            // Create MotionPhoto Sprite ObjectField
            ObjectField MotionPhotoObjectField = UIHelper.CreateObjectField<Sprite>(
                _motionData.MotionPhoto,
                (newSprite) =>
                {
                    _motionData.MotionPhoto = newSprite;

                    if (newSprite != null)
                    {
                        previewImage.image = newSprite.texture;
                        previewImage.RemoveFromClassList("motion-image-placeholder");
                    }
                    else
                    {
                        previewImage.image = null;
                        previewImage.AddToClassList("motion-image-placeholder");
                    }
                },
                "motion-photo-field"
            );
            Add(MotionPhotoObjectField);

            // Create MotionName Text Field
            TextField nameField = UIHelper.CreateTextField(
                    _motionData.MotionName,
                    newValue => { _motionData.MotionName = newValue; },
                    "motion-name"
            );
            Add(nameField);

            // Create Description Text Field
            TextField descField = UIHelper.CreateTextField(
                    _motionData.Description,
                    newValue => { _motionData.Description = newValue; },
                    "motion-desc"
            );
            Add(descField);

            // Create RuntimeAnimatorController ObjectField
            ObjectField animationField = UIHelper.CreateObjectField<RuntimeAnimatorController>(
                _motionData.RuntimeAnimatorController,
                (runtimeAnimatorController) => _motionData.RuntimeAnimatorController = runtimeAnimatorController,
                "motion-clip-field"
            );
            Add(animationField);

            // Create remove button
            Button removeButton = UIHelper.CreateButton(
                "Remove",
                OnRemoveButtonClick,
                "motion-remove-button"
            );
            Add(removeButton);
        }

        private Image CreateMotionPhotoPreview()
        {
            var previewImage = new Image();
            previewImage.AddToClassList("motion-image-frame");

            if (_motionData.MotionPhoto != null)
            {
                previewImage.image = _motionData.MotionPhoto.texture;
            }
            else
            {
                previewImage.AddToClassList("motion-image-placeholder");
            }

            return previewImage;
        }

        private void OnRemoveButtonClick()
        {
            _onRemoveButtonClick?.Invoke(_motionData);
        }

    }
}

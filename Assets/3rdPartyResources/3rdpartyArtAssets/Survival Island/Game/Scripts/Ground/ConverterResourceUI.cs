using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ConverterResourceUI : ResourceUI
    {
        [SerializeField] TMP_Text recipeText;
        [SerializeField] Image fillBarImage;

        //[Space]
        //[SerializeField] Color defaultFillColor;
        //[SerializeField] Color activeFillColor;


        //public void SetDefaultFillColor()
        //{
        //    fillBarImage.color = defaultFillColor;
        //}

        //public void SetActiveFillColor()
        //{
        //    fillBarImage.color = activeFillColor;
        //}

        public void SetRecipeText(string text)
        {
            recipeText.text = text;
        }

        public void SetFillState(float state)
        {
            fillBarImage.fillAmount = state;
        }
    }
}

﻿// <copyright file="CrimePanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting crime-related options.
    /// </summary>
    internal class CrimePanel : OptionsPanelTab
    {
        /// <summary>
        /// Adds crime options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CrimePanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            panel = PanelUtils.AddTextTab(tabStrip, Translations.Translate("RPR_OPT_CRI"), tabIndex, out UIButton _, autoLayout: true);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType());

                UIHelper helper = new UIHelper(panel);

                // Add slider component.
                UISlider newSlider = UISliders.AddPlainSlider(panel, 0f, 0f, Translations.Translate("RPR_OPT_CML"), 1f, 200f, 1f, ModSettings.crimeMultiplier);
                newSlider.tooltipBox = UIToolTips.WordWrapToolTip;
                newSlider.tooltip = Translations.Translate("RPR_OPT_CML_TIP");

                // Value label.
                UIPanel sliderPanel = (UIPanel)newSlider.parent;
                UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
                valueLabel.name = "ValueLabel";
                valueLabel.relativePosition = UILayout.PositionRightOf(newSlider, 8f, 1f);

                // Set initial text.
                PercentSliderText(newSlider, newSlider.value);

                // Slider change event.
                newSlider.eventValueChanged += (control, value) =>
                {
                    // Update value label.
                    PercentSliderText(control, value);

                    // Update setting.
                    ModSettings.crimeMultiplier = value;
                };
            }
        }

        /// <summary>
        /// Updates the displayed percentage value on a multiplier slider.
        /// </summary>
        /// <param name="control">Calling component.</param>
        /// <param name="value">New value.</param>
        private void PercentSliderText(UIComponent control, float value)
        {
            if (control?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                decimal decimalNumber = new Decimal(Mathf.RoundToInt(value));
                valueLabel.text = "x" + Decimal.Divide(decimalNumber, 100).ToString("0.00");
            }
        }
    }
}
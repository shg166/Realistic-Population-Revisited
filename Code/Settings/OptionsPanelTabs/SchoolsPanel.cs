﻿using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting school options.
    /// </summary>
    internal class EducationPanel
    {
        /// <summary>
        /// Adds school options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal EducationPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_SCH"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Enable realistic schools checkbox.
            UICheckBox enableEdCheck = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_SCH_ENB"));
            enableEdCheck.isChecked = ModSettings.enableSchools;
            enableEdCheck.eventCheckChanged += (control, isChecked) => ModSettings.enableSchools = isChecked;
        }
    }
}
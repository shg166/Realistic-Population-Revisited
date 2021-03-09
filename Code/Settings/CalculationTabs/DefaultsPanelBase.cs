﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class DefaultsPanel
    {
        // Layout constants.
        protected const float Margin = 5f;

        // Dropdown menus.
        protected readonly UIDropDown[] popMenus, floorMenus;

        // Available packs arrays.
        protected readonly PopDataPack[][] availablePopPacks;
        protected DataPack[] availableFloorPacks;

        // Instance reference.
        internal static DefaultsPanel instance;


        // Service/subservice arrays.
        protected abstract string[] SubServiceNames { get; }
        protected abstract ItemClass.Service[] Services { get; }
        protected abstract ItemClass.SubService[] SubServices { get; }
        protected abstract string[] IconNames { get; }
        protected abstract string[] AtlasNames { get; }

        // Legacy settings link.
        protected abstract bool LegacyCategory { get; set; }

        // Translation key for legacy settings label.
        protected abstract string LegacyCheckLabel { get; }

        // Tab width.
        protected virtual float TabWidth => 100f;


        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal DefaultsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Layout constants.
            const float TabIconSize = 23f;


            // Y position indicator.
            float currentY = 5f;

            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, "", tabIndex, out UIButton tabButton, TabWidth);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Add tab sprites.
            float spriteBase = (TabWidth - 2f) / IconNames.Length;
            float spriteOffset = (spriteBase - TabIconSize) / 2f;
            for (int i = 0; i < IconNames.Length; ++i)
            {
                UISprite thumbSprite = tabButton.AddUIComponent<UISprite>();
                thumbSprite.relativePosition = new Vector2(1f + (spriteBase * i) + spriteOffset, 1f);
                thumbSprite.width = TabIconSize;
                thumbSprite.height = TabIconSize;
                thumbSprite.atlas = TextureUtils.GetTextureAtlas(AtlasNames[i]);
                thumbSprite.spriteName = IconNames[i];

                // Put later sprites behind earlier sprites, for clarity.
                thumbSprite.SendToBack();
            }

            // Set tooltip.
            tabButton.tooltip = Translations.Translate("RPR_OPT_DEF");

            // Initialise arrays.
            availablePopPacks = new PopDataPack[SubServiceNames.Length][];
            availableFloorPacks = FloorData.instance.Packs;
            popMenus = new UIDropDown[SubServiceNames.Length];
            floorMenus = new UIDropDown[SubServiceNames.Length];

            // Add 'Use legacy by default' checkboxes.
            UILabel legacyLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate(LegacyCheckLabel), textScale: 0.9f);
            currentY += legacyLabel.height + 5f;

            UICheckBox legacyThisSaveCheck = UIControls.LabelledCheckBox(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LTS"));
            legacyThisSaveCheck.label.wordWrap = true;
            legacyThisSaveCheck.label.autoSize = false;
            legacyThisSaveCheck.label.width = 710f;
            legacyThisSaveCheck.label.autoHeight = true;
            legacyThisSaveCheck.isChecked = LegacyCategory;
            legacyThisSaveCheck.eventCheckChanged += (control, isChecked) =>
            {
                LegacyCategory = isChecked;
                UpdateMenus();
            };
            currentY += 20f;

            UICheckBox legacyNewSaveCheck = UIControls.LabelledCheckBox(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LAS"));
            legacyNewSaveCheck.label.wordWrap = true;
            legacyNewSaveCheck.label.autoSize = false;
            legacyNewSaveCheck.label.width = 710f;
            legacyNewSaveCheck.label.autoHeight = true;
            legacyNewSaveCheck.isChecked = ModSettings.newSaveLegacy;
            legacyNewSaveCheck.eventCheckChanged += (control, isChecked) =>
            {
                ModSettings.newSaveLegacy = isChecked;
                UpdateMenus();
            };

            // Add menus.
            currentY = SetUpMenus(panel);

            // Add buttons- add extra space.
            FooterButtons(panel, currentY + Margin);

            // Populate menus.
            UpdateMenus();

            // Set instance.
            instance = this;
        }


        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal void UpdateMenus()
        {
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Save current index in object user data.
                popMenus[i].objectUserData = i;
                floorMenus[i].objectUserData = i;

                // Get available packs for this service/subservice combination.
                availablePopPacks[i] = PopData.instance.GetPacks(Services[i]);
                availableFloorPacks = FloorData.instance.Packs;

                // Get current and default packs for this item.
                DataPack currentPopPack = PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack currentFloorPack = FloorData.instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Build preset menus.
                popMenus[i].items = new string[availablePopPacks[i].Length];
                floorMenus[i].items = new string[availableFloorPacks.Length];

                // Iterate through each item in pop menu.
                for (int j = 0; j < popMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    popMenus[i].items[j] = availablePopPacks[i][j].displayName;

                    // Check for default name match.
                    if (availablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - add default postscript.
                        popMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (availablePopPacks[i][j].name.Equals(currentPopPack.name))
                    {
                        popMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < floorMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    floorMenus[i].items[j] = availableFloorPacks[j].displayName;

                    // Check for deefault name match.
                    if (availableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - add default postscript.
                        floorMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (availableFloorPacks[j].name.Equals(currentFloorPack.name))
                    {
                        floorMenus[i].selectedIndex = j;
                    }
                }
            }
        }


        /// <summary>
        /// Sets up the defaults dropdown menus.
        /// </summary>
        /// <param name="panel">Parent panel</param>
        /// <param name="currentY">Current Y position</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected virtual float SetUpMenus(UIPanel panel)
        {
            // Layout constants.
            const float LeftColumn = 200f;
            const float RowHeight = 30f;
            const float MenuWidth = 300f;
            const float ButtonX = LeftColumn + MenuWidth + (Margin * 2);

            // Starting y position.
            float currentY = 90f;

            for (int i = 0; i < SubServiceNames.Length; ++i)
            {

                // Row icon and label.
                PanelUtils.RowHeaderIcon(panel, ref currentY, SubServiceNames[i], IconNames[i], AtlasNames[i]);

                // Pop pack dropdown.
                popMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_DEN"), MenuWidth, false);

                // Save current index in object user data.
                popMenus[i].objectUserData = i;

                // Event handler.
                popMenus[i].eventSelectedIndexChanged += (control, index) =>
                {
                    // Retrieve stored index.
                    int serviceIndex = (int)control.objectUserData;

                    // Hide floor menu if we've selected legacy calcs, otherwise show it.
                    if (availablePopPacks[serviceIndex][index].version == (int)DataVersion.legacy)
                    {
                        floorMenus[serviceIndex].Hide();
                    }
                    else
                    {
                        floorMenus[serviceIndex].Show();
                    }
                };

                // Floor pack on next row.
                currentY += RowHeight;

                // Floor pack dropdown.
                floorMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_BFL"), MenuWidth, false);


                // Next row.
                currentY += RowHeight + Margin;
            }

            // Return finishing Y position.
            return currentY;
        }


        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position for buttons</param>
        protected virtual void FooterButtons(UIPanel panel, float yPos)
        {
            // Reset button.
            UIButton resetButton = UIControls.AddButton(panel, Margin, yPos, Translations.Translate("RPR_OPT_RTD"), 150f);
            resetButton.eventClicked += ResetDefaults;

            // Revert button.
            UIButton revertToSaveButton = UIControls.AddButton(panel, (Margin * 2) + 150f, yPos, Translations.Translate("RPR_OPT_RTS"), 150f);
            revertToSaveButton.eventClicked += (component, clickEvent) => UpdateMenus();
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        private void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get current and default packs for this item.
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Iterate through each item in pop menu.
                for (int j = 0; j < popMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (availablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - set selection to this one.
                        popMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < floorMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (availableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - set selection to this one.
                        floorMenus[i].selectedIndex = j;
                    }
                }
            }
        }
    }
}
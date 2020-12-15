﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Panel for editing and creating building settings.
    /// </summary>
    public class UIEditPanel : UIPanel
    {
        // Layout constants.
        private const float MarginPadding = 10f;
        private const float LabelWidth = 150f;
        private const float TitleY = 5f;
        private const float PopCheckY = TitleY + 30f;
        private const float HomeJobY = PopCheckY + 25f;
        private const float FloorCheckY = HomeJobY + 25f;
        private const float FirstFloorY = FloorCheckY + 25f;
        private const float FloorHeightY = FirstFloorY + 25f;
        private const float SaveY = FloorHeightY + 35f;
        private const float DeleteY = SaveY + 35f;
        private const float MessageY = DeleteY + 35f;


        // Panel components
        private UILabelledTextfield homeJobsCount, firstFloorField, floorHeightField;
        //private UICheckBox popCheck, floorCheck;
        private UILabel homeJobLabel;
        private UIButton saveButton;
        private UIButton deleteButton;
        private UILabel messageLabel;

        // Currently selected building.
        private BuildingInfo currentSelection;


        /// <summary>
        /// Create the panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;
            autoLayoutPadding.right = 5;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Panel title.
            UILabel title = this.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(0, TitleY);
            title.textAlignment = UIHorizontalAlignment.Center;
            title.text = Translations.Translate("RPR_CUS_TITLE");
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;
            title.height = 30;

            // Checkboxes.
            //popCheck = UIControls.AddCheckBox(this, Translations.Translate("RPR_EDT_POP"), yPos: PopCheckY, textScale: 1.0f);
            //floorCheck = UIControls.AddCheckBox(this, Translations.Translate("RPR_EDT_FLR"), yPos: FloorCheckY, textScale: 1.0f);

            // Text fields.
            homeJobsCount = AddLabelledTextfield(HomeJobY, "RPR_LBL_HOM");
            //firstFloorField = AddLabelledTextfield(FirstFloorY,"RPR_LBL_OFF");
            //floorHeightField = AddLabelledTextfield(FloorHeightY, "RPR_LBL_OFH");
            homeJobLabel = homeJobsCount.label;

            // Save button.
            saveButton = UIUtils.CreateButton(this, 200);
            saveButton.relativePosition = new Vector3(MarginPadding, SaveY);
            saveButton.text = Translations.Translate("RPR_CUS_ADD");
            saveButton.tooltip = Translations.Translate("RPR_CUS_ADD_TIP");
            saveButton.Disable();

            // Delete button.
            deleteButton = UIUtils.CreateButton(this, 200);
            deleteButton.relativePosition = new Vector3(MarginPadding, DeleteY);
            deleteButton.text = Translations.Translate("RPR_CUS_DEL");
            deleteButton.tooltip = Translations.Translate("RPR_CUS_DEL_TIP");
            deleteButton.Disable();

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector3(MarginPadding, MessageY);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.autoSize = false;
            messageLabel.autoHeight = true;
            messageLabel.wordWrap = true;
            messageLabel.width = this.width - (MarginPadding * 2);
            messageLabel.isVisible = false;
            messageLabel.text = "No message to display";

            /*
            // Checkbox event handlers.
            popCheck.eventCheckChanged += (component, isChecked) =>
            {
                // If this is now selected and floorCheck is also selected, deselect floorCheck.
                if (isChecked && floorCheck.isChecked)
                {
                    floorCheck.isChecked = false;
                }
            };
            floorCheck.eventCheckChanged += (component, isChecked) =>
            {
                // If this is now selected and popCheck is also selected, deselect popCheck.
                if (isChecked && popCheck.isChecked)
                {
                    popCheck.isChecked = false;
                }
                
                // If this is now checked, try to parse the floors.
                if (isChecked)
                {
                    FloorDataPack overridePack = TryParseFloors();
                    BuildingDetailsPanel.Panel.OverridePack = overridePack;
                    BuildingDetailsPanel.Panel.FloorDataPack = overridePack;
                }
                else
                {
                    // If not checked, set override pack to null.
                    BuildingDetailsPanel.Panel.OverridePack = null;
                }
            };*/

            // Save button event handler.
            saveButton.eventClick += (component, clickEvent) =>
            {
                // Hide message.
                messageLabel.isVisible = false;

                // Don't do anything with invalid entries.
                if (currentSelection == null || currentSelection.name == null)
                {
                    return;
                }

                // Are we doing population overrides?
                //if (popCheck.isChecked)
                {
                    // Read total floor count textfield if possible; ignore zero values
                    if (int.TryParse(homeJobsCount.textField.text, out int homesJobs) && homesJobs != 0)
                    {
                        // Minimum value of 1.
                        if (homesJobs < 1)
                        {
                            // Print warning message in red.
                            messageLabel.textColor = new Color32(255, 0, 0, 255);
                            messageLabel.text = Translations.Translate("RPR_ERR_ZERO");
                            messageLabel.isVisible = true;
                        }
                        else
                        {
                            // Homes or jobs?
                            if (currentSelection.GetService() == ItemClass.Service.Residential)
                            {
                                // Residential building.
                                ExternalCalls.SetResidential(currentSelection, homesJobs);

                                // Update household counts for existing instances of this building - only needed for residential buildings.
                                // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                                PopData.instance.UpdateHouseholds(currentSelection.name);
                            }
                            else
                            {
                                // Employment building.
                                ExternalCalls.SetWorker(currentSelection, homesJobs);
                            }

                            // Repopulate field with parsed value.
                            homeJobLabel.text = homesJobs.ToString();

                            // Refresh the display so that all panels reflect the updated settings.
                            BuildingDetailsPanel.Panel.Refresh();
                        }
                    }
                    else
                    {
                        // TryParse couldn't parse any data; print warning message in red.
                        messageLabel.textColor = new Color32(255, 0, 0, 255);
                        messageLabel.text = Translations.Translate("RPR_ERR_INV");
                        messageLabel.isVisible = true;
                    }
                }
                /*else if (floorCheck.isChecked)
                {
                    // Attempt to parse values into override floor pack.
                    FloorDataPack overridePack = TryParseFloors();

                    // Were we successful?.
                    if (overridePack != null)
                    {
                        // Successful parsing - add override.
                        FloorData.instance.AddOverride(currentSelection, overridePack);

                        // Repopulate fields with parsed values.
                        firstFloorField.textField.text = overridePack.firstFloorMin.ToString();
                        floorHeightField.textField.text = overridePack.floorHeight.ToString();

                        // Refresh the display so that all panels reflect the updated settings.
                        BuildingDetailsPanel.Panel.Refresh();
                    }
                    else
                    {
                        // Couldn't parse values; print warning message in red.
                        messageLabel.textColor = new Color32(255, 0, 0, 255);
                        messageLabel.text = Translations.Translate("RPR_ERR_INV");
                        messageLabel.isVisible = true;
                    }
                }*/
            };

            // Delete button event handler.
            deleteButton.eventClick += (component, clickEvent) =>
            {
                // Hide message.
                messageLabel.isVisible = false;

                // Don't do anything with invalid entries.
                if (currentSelection == null || currentSelection.name == null)
                {
                    return;
                }

                Debugging.Message("deleting custom entry for " + currentSelection.name);

                // Homes or jobs?  Remove custom entry as appropriate.
                if (currentSelection.GetService() == ItemClass.Service.Residential)
                {
                    // Residential building.
                    ExternalCalls.RemoveResidential(currentSelection);

                    // Update household counts for existing instances of this building - only needed for residential buildings.
                    // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                    PopData.instance.UpdateHouseholds(currentSelection.name);
                }
                else
                {
                    // Employment building.
                    ExternalCalls.RemoveWorker(currentSelection);
                }

                // Remove any floor override.
                FloorData.instance.DeleteOverride(currentSelection);

                // Refresh the display so that all panels reflect the updated settings.
                BuildingDetailsPanel.Panel.Refresh();
                homeJobsCount.textField.text = string.Empty;
            };
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building"></param>
        public void SelectionChanged(BuildingInfo building)
        {
            // Hide message.
            messageLabel.isVisible = false;

            // Set current selecion.
            currentSelection = building;

            // Set text field to blank and disable buttons if no valid building is selected.
            if (building == null || building.name == null)
            {
                homeJobsCount.textField.text = string.Empty;
                saveButton.Disable();
                deleteButton.Disable();
                return;
            }

            int homesJobs;

            if (building.GetService() == ItemClass.Service.Residential)
            {
                // See if a custom number of households applies to this building.
                homesJobs = ExternalCalls.GetResidential(building);
                homeJobLabel.text = Translations.Translate("RPR_LBL_HOM");
            }
            else
            {
                // Workplace building; see if a custom number of jobs applies to this building.
                homesJobs = ExternalCalls.GetWorker(building);
                homeJobLabel.text = Translations.Translate("RPR_LBL_JOB");
            }

            // If no custom settings have been found (return value was zero) and theres' no floor override, then blank the text field, rename the save button, and disable the delete button.
            if (homesJobs == 0 && FloorData.instance.HasOverride(building) == null)
            {
                homeJobsCount.textField.text = string.Empty;
                saveButton.text = Translations.Translate("RPR_CUS_ADD");
                deleteButton.Disable();
            }
            else
            {
                // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                homeJobsCount.textField.text = homesJobs.ToString();
                saveButton.text = Translations.Translate("RPR_CUS_UPD");
                deleteButton.Enable();
            }

            // We've got a valid building, so enable the save button.
            saveButton.Enable();
        }

        /*
        /// <summary>
        /// Attempts to parse floor data fields into a valid override floor pack.
        /// </summary>
        /// <returns></returns>
        private FloorDataPack TryParseFloors()
        {
            Debugging.Message("try floor parse");

            // Attempt to parse fields.
            if (float.TryParse(firstFloorField.textField.text, out float firstFloor) && float.TryParse(floorHeightField.textField.text, out float floorHeight))
            {
                Debugging.Message("success!");

                // Success - create new override floor pack with parsed data.
                return new FloorDataPack
                {
                    version = (int)DataVersion.overrideOne,
                    firstFloorMin = firstFloor,
                    floorHeight = floorHeight
                };
            }

            // If we got here, we didn't get a valid parse; return null.
            return null;
        }*/


        /// <summary>
        /// Adds a textfield with a label to the left.
        /// </summary>
        /// <param name="yPos">Relative y-position of textfield</param>
        /// <param name="key">Translation key for label</param>
        /// <returns></returns>
        private UILabelledTextfield AddLabelledTextfield(float yPos, string key)
        {
            // Create textfield.
            UILabelledTextfield newField = new UILabelledTextfield();
            newField.textField = UIUtils.CreateTextField(this, this.width - (MarginPadding * 3) - LabelWidth, 20);
            newField.textField.relativePosition = new Vector3(MarginPadding + LabelWidth + MarginPadding, yPos);
            newField.textField.clipChildren = false;

            // Label.
            newField.label = newField.textField.AddUIComponent<UILabel>();
            newField.label.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            newField.label.relativePosition = new Vector2(-MarginPadding * 2f, newField.textField.height / 2);
            newField.label.textAlignment = UIHorizontalAlignment.Right;
            newField.label.textScale = 0.7f;
            newField.label.text = Translations.Translate(key);

            return newField;
        }
    }
}
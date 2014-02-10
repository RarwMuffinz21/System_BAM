// Create parent controls and button for the colorset tab.
function createColorsetGUI()
{
    %btn = new GuiBitmapButtonCtrl(CustomGameGui_ColorSetButton)
    {
        position = "0 159";
        extent = "160 34";
        horizSizing = "relative";
        vertSizing = "relative";
        profile = "ImpactForwardButtonProfile";
        command = "CustomGameGui.clickColorSet();";
        text = "Color Sets > ";
        bitmap = "base/client/ui/btnBlank";
        mColor = "255 255 255 255";
    };

    CustomGameGui.add(%btn);

    %hl = new GuiSwatchCtrl(CustomGameGui_ColorsetHilight)
    {
        position = "0 159";
        extent = "160 34";
        horizSizing = "relative";
        vertSizing = "relative";
        visible = 0;
        color = "255 255 255 128";
    };

    CustomGameGui.add(%hl);

    %window = new GuiSwatchCtrl(CustomGameGui_ColorsetWindow)
    {
        position = "160 40";
        extent = "480 410";
        horizSizing = "relative";
        vertSizing = "relative";
        visible = 0;
        color = "0 0 0 0";

        new GuiScrollCtrl(CustomGameGui_ColorsetScroll)
        {
            position = "0 0";
            extent = "270 410";
            horizSizing = "relative";
            vertSizing = "relative";
            profile = "ImpactScrollProfile";
            willFirstRespond = 0;
            hScrollBar = "alwaysOff";
            vScrollBar = "alwaysOn";
            constantThumbHeight = "0";
            childMargin = "0 0";
            rowHeight = "81";
            columnWidth = "30";

            new GuiSwatchCtrl(CustomGameGui_ColorsetSwatch)
            {
                extent = "270 400";
                horizSizing = "relative";
                vertSizing = "relative";
                color = "0 0 0 0";
            };
        };

        new GuiSwatchCtrl()
        {
            position = "270 0";
            extent = "210 410";
            horizSizing = "relative";
            vertSizing = "relative";
            profile = "GuiDefaultProfile";
            color = "0 0 0 128";

            new GuiSwatchCtrl(CustomGameGui_ColorsetPreview)
            {
                extent = "10 10";
                horizSizing = "right";
                vertSizing = "bottom";
                profile = "GuiDefaultProfile";
                color = "0 0 0 0";
            };
        };
    };

    CustomGameGui.add(%window);

    if (!isFile("Add-Ons/Colorset_Default/colorSet.txt"))
    {
        fileCopyManual(
            "Add-Ons/System_BAM/resources/Colorset_Default/description.txt",
            "Add-Ons/Colorset_Default/description.txt"
        );

        fileCopyManual(
            "Add-Ons/System_BAM/resources/Colorset_Default/colorSet.txt",
            "Add-Ons/Colorset_Default/colorSet.txt"
        );
    }
}

// Build a list of colorsets by searching for files and analyzing them.
function loadColorsets()
{
    $BAM::MCCM::Colorsets = 0;

    %match = "Add-Ons/Colorset_*/colorSet.txt";
    %fo = new FileObject();

    for (%file = findFirstFile(%match); %file !$= ""; %file = findNextFile(%match))
    {
        %desc = filePath(%file) @ "/description.txt";

        if (!isFile(%desc))
        {
            continue;
        }

        %title = "Unnamed Colorset";

        if (%fo.openForRead(%desc))
        {
            while (!%fo.isEOF())
            {
                %line = %fo.readLine();

                if (firstWord(%line) $= "Title:")
                {
                    %title = restWords(%line);
                    break;
                }
            }

            %fo.close();
        }

        $BAM::MCCM::Colorset[$BAM::MCCM::Colorsets] = %file TAB %title;
        $BAM::MCCM::Colorsets++;
    }

    %fo.delete();
}

// Open the colorset tab and warn the user if colorSet.txt is read-only.
function CustomGameGui::clickColorSet(%this)
{
    %this.hideAllTabs();

    CustomGameGui_ColorsetWindow.setVisible(1);
    CustomGameGui_ColorsetHilight.setVisible(1);

    if (!isWriteableFileName("config/server/colorSet.txt"))
    {
        %message = "It appears as though your colorset.txt file is read-only. That means you can't use the color manager feature.";
        %message = %message @ "\n\nYou can try deleting colorSet.txt in the config/server/ folder to fix this problem.";

        messageBoxOK("Error", %message);
        return;
    }
}

// Update the preview pane to display the given colorset.
function CustomGameGui::previewColorSet(%this, %file)
{
    CustomGameGui_ColorsetPreview.clear();
    %fo = new FileObject();

    if (%fo.openForRead(%file))
    {
        while (!%fo.isEOF())
        {
            %line = %fo.readLine();
            %div = strPos(%line, "DIV:") == 0;

            if (%line !$= "" && !%div)
            {
                %numRows++;
            }
            else if (%div)
            {
                %numCols++;

                if(%numRows > %maxRows)
                {
                    %maxRows = %numRows;
                }

                %numRows = 0;
            }
        }

        %fo.close();
    }

    %numCols += 2;
    %maxRows += 2;

    %dimension = mFloor(getWord(CustomGameGui_ColorsetPreview.getGroup().extent,0)/%numCols);

    if ((%maxRows * %dimension) > getWord(CustomGameGui_ColorsetPreview.getGroup().extent,1))
    {
        %dimension = mFloor(getWord(CustomGameGui_ColorsetPreview.getGroup().extent,1)/%maxRows);
    }

    %extent = %dimension SPC %dimension;

    if (%fo.openForRead(%file))
    {
        while (!%fo.isEOF())
        {
            %line = %fo.readLine();
            %div = strPos(%line, "DIV:") == 0;

            if (%line !$= "" && !%div)
            {
                %r = getWord(%line, 0);
                %g = getWord(%line, 1);
                %b = getWord(%line, 2);
                %a = getWord(%line, 3);

                if (%r !$= mFloor(%r))
                {
                    %r = mFloor(%r * 255);
                    %g = mFloor(%g * 255);
                    %b = mFloor(%b * 255);
                    %a = mFloor(%a * 255);
                }

                %color = %r SPC %g SPC %b SPC %a;

                %xPos = (%currCol * %dimension);
                %yPos = (%currRow * %dimension);

                if (%a < 255)
                {
                    %b = new GuiBitmapCtrl()
                    {
                        position = %xPos SPC %yPos;
                        extent = %extent;
                        bitmap = "Add-Ons/System_BAM/resources/images/checkedGrid";
                        wrap = 1;
                    };

                    CustomGameGui_ColorsetPreview.add(%b);
                }

                %c = new GuiSwatchCtrl()
                {
                    position = %xPos SPC %yPos;
                    extent = %extent;
                    color = %color;
                };

                CustomGameGui_ColorsetPreview.add(%c);

                %d = new GuiBitmapCtrl()
                {
                    position = %xPos SPC %yPos;
                    extent = %extent;
                    bitmap = "base/client/ui/btnColor_n";
                };

                CustomGameGui_ColorsetPreview.add(%d);
                %currRow++;

                if (%currRow > %maxRow)
                {
                    %maxRow = %currRow;
                }
            }
            else if (%div)
            {
                %currRow = 0;
                %currCol++;
            }
        }

        %fo.close();
    }

    %fo.delete();
    CustomGameGui_ColorsetPreview.extent = (%currcol*%dimension) SPC (%maxRow*%dimension);

    %xpos = (mFloor(getWord(CustomGameGui_ColorsetPreview.getGroup().extent,0)/2))-(mFloor(getWord(CustomGameGui_ColorsetPreview.extent,0)/2));
    %ypos = (mFloor(getWord(CustomGameGui_ColorsetPreview.getGroup().extent,1)/2))-(mFloor(getWord(CustomGameGui_ColorsetPreview.extent,1)/2));

    CustomGameGui_ColorsetPreview.position = %xpos SPC %ypos;
}

package BAM_ColorsetPackage
{
    // Create controls for selecting and previewing colorsets.
    function CustomGameGui::onRender(%this)
    {
        Parent::onRender(%this);
        %this.selectedColorSet = "";

        CustomGameGui_ColorsetSwatch.clear();
        CustomGameGui_ColorsetPreview.clear();

        for (%i = 0; %i < $BAM::MCCM::Colorsets; %i++)
        {
            %colorSet = getField($BAM::MCCM::Colorset[%i], 0);

            if (fileMatch(%colorSet, "config/server/colorSet.txt"))
            {
                %this.selectedColorSet = %colorSet;
                break;
            }
        }

        %k = 0;

        if (%this.selectedColorSet $= "")
        {
            %ctrl = new GuiRadioCtrl()
            {
                profile = "ImpactCheckProfile";
                group = 1;

                command = "CustomGameGui.selectedColorSet=\"config/server/colorSet.txt\";CustomGameGui.previewColorSet(\"config/server/colorSet.txt\");";
            };

            CustomGameGui_ColorsetSwatch.add(%ctrl);
            CustomGameGui.selectedColorSet = "config/server/colorSet.txt";

            %ctrl.resize(10, 0, getWord(CustomGameGui_ColorsetSwatch.extent, 0), %this.fontSize);
            %ctrl.setText("Current Colorset");
            %ctrl.setValue(1);

            %k++;
        }

        %this.previewColorSet(%this.selectedColorSet);

        for (%i = $BAM::MCCM::Colorsets - 1; %i >= 0; %i--)
        {
            %data = $BAM::MCCM::Colorset[%i];
            %file = getField(%data, 0);

            %ctrl = new GuiRadioCtrl()
            {
                profile = "ImpactCheckProfile";
                group = 1;

                command = "CustomGameGui.selectedColorSet=\"" @ %file @ "\";CustomGameGui.previewColorSet(\"" @ %file @ "\");";
            };

            CustomGameGui_ColorsetSwatch.add(%ctrl);

            %ctrl.resize(
                10, %k * %this.fontSize,
                getWord(CustomGameGui_ColorsetSwatch.extent, 0),
                %this.fontSize
            );

            %ctrl.setText(getField(%data, 1));

            if (%file $= %this.selectedColorSet)
            {
                %ctrl.setValue(1);
            }

            %k++;
        }

        CustomGameGui_ColorsetSwatch.resize(
            0, 0,
            getWord(CustomGameGui_ColorsetSwatch.extent, 0),
            %k * %this.fontSize
        );
    }

    // Hide the colorset selection along with other tabs.
    function CustomGameGui::hideAllTabs(%this)
    {
        Parent::hideAllTabs(%this);

        CustomGameGui_ColorsetHilight.setVisible(0);
        CustomGameGui_ColorsetWindow.setVisible(0);
    }

    // Write the selected colorset file to colorSet.txt.
    function CustomGameGui::clickSelect(%this)
    {
        Parent::clickSelect(%this);

        if (isFile(%this.selectedColorSet))
        {
            if (!fileCopyManual(%this.selectedColorSet, "config/server/colorSet.txt"))
            {
                messageBoxOK("Error", "The selected colorset could not be applied.");
            }
        }
    }
};

activatePackage("BAM_ColorsetPackage");

if (!isObject(CustomGameGui_ColorSetButton))
{
    createColorsetGUI();
}

if (!$BAM::MCCM::Colorsets)
{
    loadColorsets();
}
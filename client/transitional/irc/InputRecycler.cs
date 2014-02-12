function BAM_InputRecycler::onAdd(%this)
{
    for (%i = 0; %i < 50; %i++)
    {
        %this.add(new GuiTextEditCtrl()
        {
            historySize = 32;
            accelerator = "return";
        });
    }
}

function BAM_InputRecycler::acquire(%this)
{
    if (%this.getCount())
    {
        %control = %this.getObject(0);
        %this.remove(%control);

        return %control;
    }

    return 0;
}

function BAM_InputRecycler::release(%this, %control)
{
    %control.command = "";
    %control.altCommand = "";

    %control.setProfile(GuiDefaultProfile);
    %control.setValue("");

    %this.add(%control);
}

if (!isObject(BAM_InputRecycler))
{
    new GuiSwatchCtrl(BAM_InputRecycler);
}
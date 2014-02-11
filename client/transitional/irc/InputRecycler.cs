function RTBIC_createInputRecycler()
{
   if(isObject(RTBIC_InputRecycler))
   {
      echo("\c2ERROR: Cannot destroy input recycler!");
      return RTBIC_InputRecycler;
   }

   %recycler = new GuiSwatchCtrl(RTBIC_InputRecycler)
   {
      visible = 0;
   };

   %recycler.populate(50);
   return %recycler;
}

function RTBIC_InputRecycler::get(%this)
{
   if (%this.getCount())
   {
      %input = %this.getObject(0);
      %input.setValue("");

      return %input;
   }
   else
   {
      echo("\c2ERROR: Unable to allocate free input object!");
      return 0;
   }
}

function RTBIC_InputRecycler::reclaim(%this, %input)
{
   if (!isObject(%input))
   {
      return 0;
   }
   
   %input.setValue("");
   %input.command = "";
   %input.altCommand = "";

   %this.add(%input);
   return true;
}

function RTBIC_InputRecycler::populate(%this, %amount)
{
   if (%this.getCount())
   {
      echo("\c2ERROR: Cannot re-populate input recycler!");
      return 0;
   }

   for (%i = 0; %i < %amount; %i++)
   {
      %input = new GuiTextEditCtrl()
      {
         profile = RTB_TextEditProfile;
         horizSizing = "right";
         vertSizing = "bottom";
         position = "0 0";
         extent = "64 16";
         maxLength = "255";
         command = "";
         altCommand = "";
         accelerator = "return";
         historySize = 32;
      };

      %this.add(%input);
   }

   return %this.getCount() == %amount;
}
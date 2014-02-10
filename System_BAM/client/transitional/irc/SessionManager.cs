function RTBIC_createSessionManager()
{
   if(isObject(RTBIC_SessionManager))
   {
      RTBIC_SessionManager.destroy();
      RTBIC_SessionManager.delete();
   }

   return new ScriptGroup(RTBIC_SessionManager);
}

function RTBIC_SessionManager::getSession(%this, %name, %search)
{
   %count = %this.getCount();

   for (%i = 0; %i < %count; %i++)
   {
      %session = %this.getObject(%i);

      if (%session.name $= %name)
      {
         return %session;
      }
   }

   if (%search)
   {
      return 0;
   }

   %session = new ScriptObject()
   {
      class = "RTBIC_Session";
      name = %name;
   };

   %this.add(%session);
   return %session;
}

function RTBIC_SessionManager::resetCursor(%this)
{
   if (isObject(%this.lastFocus) && %this.isMember(%this.lastFocus))
   {
      %this.lastFocus.focus();
   }
   else if (%this.getCount())
   {
      for (%i = %this.getCount() - 1; %i >= 0; %i--)
      {
         %this.getObject(%i).focus();
         break;
      }
   }
}
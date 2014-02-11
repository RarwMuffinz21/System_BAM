function RTBIC_createRoomSessionManager()
{
   if (isObject(RTBIC_RoomSessionManager))
   {
      RTBIC_RoomSessionManager.destroy();
      RTBIC_RoomSessionManager.delete();
   }

   return new ScriptGroup(RTBIC_RoomSessionManager);
}

function RTBIC_RoomSessionManager::getSession(%this, %name, %search)
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

   %session = new ScriptGroup()
   {
      class = "RTBIC_RoomSession";
      name = %name;
   };

   %this.add(%session);

   %manifest = new ScriptGroup()
   {
      class = "RTBIC_RoomSessionManifest";
      session = %session;
   };

   %session.add(%manifest);
   %session.manifest = %manifest;

   return %session;
}

function RTBIC_RoomSessionManager::resetCursor(%this)
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
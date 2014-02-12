// More to come..

function clientCmdBAM_SetPermission(%permission, %allowed)
{
	if (getWordCount(%permission) != 1)
	{
		return;
	}

	if (!ServerConnection._permissionExists[%permission])
	{
		ServerConnection._permissionExists[%permission] = 1;

		ServerConnection._permissionList = ServerConnection._permissionList @ (
			ServerConnection._permissionList $= "" ? "" : " ") @ %permission;
	}

	ServerConnection.hasPermission[%permission] = %allowed ? 1 : 0;
}

function clientCmdBAM_ClearPermissions()
{
	%count = getWordCount(ServerConnection._permissionList);

	for (%i = 0; %i < %count; %i++)
	{
		%permission = getWord(ServerConnection._permissionList, %i);

		ServerConnection._permissionExists[%permission] = "";
		ServerConnection.hasPermission[%permission] = "";
	}

	ServerConnection._permissionList = "";
}
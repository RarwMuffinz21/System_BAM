package BAMC_ServerControlPackage
{
	function openAdminWindow(%state)
	{
		if (!ServerConnection.bamServer)
		{
			return Parent::openAdminWindow(%state);
		}

		if (AdminGui.isAwake())
		{
			Canvas.popDialog(AdminGui);
		}

		if (!%state)
		{
			return "";
		}

		if (ServerControlGui.isAwake())
		{
			Canvas.popDialog(ServerControlGui);
		}
		else if ($IAmAdmin || ServerConnection.hasPermission["openServerControl"])
		{
			Canvas.pushDialog(ServerControlGui);
		}
	}
};

activatePackage("BAMC_ServerControlPackage");
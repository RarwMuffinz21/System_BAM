if (isObject(ServerControlWindow))
{
	ServerControlWindow.delete();
}

new GuiWindowCtrl(ServerControlWindow)
{
	profile = BAMWindowProfile;
	extent = "800 600";

	canMaximize = 0;
	canMinimize = 0;

	resizeWidth = 0;
	resizeHeight = 0;

	text = "Server Control";

	new GuiSwatchCtrl()
	{
		position = "3 39";
		extent = "794 40";

		color = "238 238 238 255";

		new GuiBitmapButtonCtrl()
		{
			profile = GuiCenterProfile;

			position = "0 0";
			extent = "60 40";

			bitmap = "Add-Ons/System_BAM/resources/images/white";
			text = "Main";

			mColor = "221 221 221 255";
		};

		new GuiBitmapButtonCtrl()
		{
			profile = GuiCenterProfile;

			position = "60 0";
			extent = "81 40";

			bitmap = "Add-Ons/System_BAM/resources/images/white";
			text = "Settings";

			mColor = "0 0 0 0";
		};

		new GuiBitmapButtonCtrl()
		{
			profile = GuiCenterProfile;

			position = "141 0";
			extent = "119 40";

			bitmap = "Add-Ons/System_BAM/resources/images/white";
			text = "Administration";

			mColor = "0 0 0 0";
		};

		new GuiBitmapButtonCtrl()
		{
			profile = GuiCenterProfile;

			position = "260 0";
			extent = "106 40";

			bitmap = "Add-Ons/System_BAM/resources/images/white";
			text = "Permissions";

			mColor = "0 0 0 0";
		};

		new GuiBitmapButtonCtrl()
		{
			profile = GuiCenterProfile;

			position = "366 0";
			extent = "61 40";

			bitmap = "Add-Ons/System_BAM/resources/images/white";
			text = "Logs";

			mColor = "0 0 0 0";
		};
	};
};
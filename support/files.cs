function fileCopyManual(%a, %b)
{
	if (!isFile(%a) || !isWriteableFileName(%b))
	{
		return 0;
	}

	%i = new FileObject();

	if (!%i.openForRead(%a))
	{
		%i.delete();
		return 0;
	}

	%j = new FileObject();

	if (!%j.openForWrite(%b))
	{
		%i.close();
		%i.delete();
		%j.delete();
		return 0;
	}

	while (!%i.isEOF())
	{
		%j.writeLine(%i.readLine());
	}

	%i.delete();
	%j.delete();

	return isFile(%b);
}

function fileMatch(%a, %b)
{
	if (!isFile(%a) || !isFile(%b))
	{
		return 0;
	}

	%i = new FileObject();

	if (!%i.openForRead(%a))
	{
		%i.delete();
		return 0;
	}

	%j = new FileObject();

	if (!%j.openForRead(%b))
	{
		%i.close();
		%i.delete();
		%j.delete();
		return 0;
	}

	while (!%i.isEOF())
	{
		if (strStr(%i.readLine(), %j.readLine()) != 0)
		{
			%i.close();
			%i.delete();

			%j.close();
			%j.delete();

			return 0;
		}
	}

	return 1;
}
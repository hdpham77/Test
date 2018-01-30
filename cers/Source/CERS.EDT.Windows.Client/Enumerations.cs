using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public enum HttpMethod
	{
		Get,
		Post
	}

	public enum BackgroundOperationType
	{
		Primary,
		Secondary
	}

	public enum InputDataMode
	{
		BinaryFile,
		Text
	}

	public enum TargetEnvironment
	{
		Production,
		Staging,
        Testing,
		Development
	}
}
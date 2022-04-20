using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
namespace Gnu.Getopt
{
	public class LongOpt
	{
		private string name;
		private Argument hasArg;
		private StringBuilder flag;
		private int val;
		private ResourceManager resManager = new ResourceManager("Gnu.Getopt.MessagesBundle", Assembly.GetExecutingAssembly());
		private CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
		public string Name
		{
			get
			{
				return this.name;
			}
		}
		public Argument HasArg
		{
			get
			{
				return this.hasArg;
			}
		}
		public StringBuilder Flag
		{
			get
			{
				return this.flag;
			}
		}
		public int Val
		{
			get
			{
				return this.val;
			}
		}
		public LongOpt(string name, Argument hasArg, StringBuilder flag, int val)
		{
			try
			{
				if ((bool)new AppSettingsReader().GetValue("Gnu.PosixlyCorrect", typeof(bool)))
				{
					this.cultureInfo = new CultureInfo("en-US");
				}
			}
			catch (Exception)
			{
			}
			if (hasArg != Argument.No && hasArg != Argument.Required && hasArg != Argument.Optional)
			{
				object[] args = new object[]
				{
					hasArg
				};
				throw new ArgumentException(string.Format(this.resManager.GetString("getopt.invalidValue", this.cultureInfo), args));
			}
			this.name = name;
			this.hasArg = hasArg;
			this.flag = flag;
			this.val = val;
		}
	}
}

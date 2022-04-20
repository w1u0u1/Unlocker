using System;
using System.Configuration;
using System.Globalization;
using System.Resources;
using System.Text;
namespace Gnu.Getopt
{
	public class Getopt
	{
		private enum Order
		{
			RequireOrder = 1,
			Permute,
			ReturnInOrder
		}
		private string optarg;
		private int optind;
		private bool opterr = true;
		private int optopt = 63;
		private string nextchar;
		private string optstring;
		private LongOpt[] longOptions;
		private bool longOnly;
		private int longind;
		private bool posixlyCorrect;
		private bool longoptHandled;
		private int firstNonopt = 1;
		private int lastNonopt = 1;
		private bool endparse;
		private string[] argv;
		private Getopt.Order ordering;
		private string progname;
		private CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
		public string Optstring
		{
			get
			{
				return this.optstring;
			}
			set
			{
				if (value.Length == 0)
				{
					value = " ";
				}
				this.optstring = value;
			}
		}
		public int Optind
		{
			get
			{
				return this.optind;
			}
			set
			{
				this.optind = value;
			}
		}
		public string[] Argv
		{
			get
			{
				return this.argv;
			}
			set
			{
				this.argv = value;
			}
		}
		public string Optarg
		{
			get
			{
				return this.optarg;
			}
		}
		public bool Opterr
		{
			get
			{
				return this.opterr;
			}
			set
			{
				this.opterr = value;
			}
		}
		public int Optopt
		{
			get
			{
				return this.optopt;
			}
		}
		public int Longind
		{
			get
			{
				return this.longind;
			}
		}
		public static string digest(LongOpt[] longOpts)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < longOpts.Length; i++)
			{
				LongOpt longOpt = longOpts[i];
				stringBuilder.Append((char)longOpt.Val);
				if (longOpt.HasArg == Argument.Optional)
				{
					stringBuilder.Append("::");
				}
				if (longOpt.HasArg == Argument.Required)
				{
					stringBuilder.Append(":");
				}
			}
			return stringBuilder.ToString();
		}
		public Getopt(string progname, string[] argv, string optstring) : this(progname, argv, optstring, null, false)
		{
		}
		public Getopt(string progname, string[] argv, string optstring, LongOpt[] longOptions) : this(progname, argv, optstring, longOptions, false)
		{
		}
		public Getopt(string progname, string[] argv, string optstring, LongOpt[] longOptions, bool longOnly)
		{
			if (optstring.Length == 0)
			{
				optstring = " ";
			}
			this.progname = progname;
			this.argv = argv;
			this.optstring = optstring;
			this.longOptions = longOptions;
			this.longOnly = longOnly;
			try
			{
				if ((bool)new AppSettingsReader().GetValue("Gnu.PosixlyCorrect", typeof(bool)))
				{
					this.posixlyCorrect = true;
					this.cultureInfo = new CultureInfo("en-US");
				}
				else
				{
					this.posixlyCorrect = false;
				}
			}
			catch (Exception)
			{
				this.posixlyCorrect = false;
			}
			if (optstring[0] == '-')
			{
				this.ordering = Getopt.Order.ReturnInOrder;
				if (optstring.Length > 1)
				{
					this.optstring = optstring.Substring(1);
					return;
				}
			}
			else
			{
				if (optstring[0] == '+')
				{
					this.ordering = Getopt.Order.RequireOrder;
					if (optstring.Length > 1)
					{
						this.optstring = optstring.Substring(1);
						return;
					}
				}
				else
				{
					if (this.posixlyCorrect)
					{
						this.ordering = Getopt.Order.RequireOrder;
						return;
					}
					this.ordering = Getopt.Order.Permute;
				}
			}
		}
		private void exchange(string[] argv)
		{
			int num = this.firstNonopt;
			int num2 = this.lastNonopt;
			int num3 = this.optind;
			while (num3 > num2 && num2 > num)
			{
				if (num3 - num2 > num2 - num)
				{
					int num4 = num2 - num;
					for (int i = 0; i < num4; i++)
					{
						string text = argv[num + i];
						argv[num + i] = argv[num3 - (num2 - num) + i];
						argv[num3 - (num2 - num) + i] = text;
					}
					num3 -= num4;
				}
				else
				{
					int num5 = num3 - num2;
					for (int j = 0; j < num5; j++)
					{
						string text = argv[num + j];
						argv[num + j] = argv[num2 + j];
						argv[num2 + j] = text;
					}
					num += num5;
				}
			}
			this.firstNonopt += this.optind - this.lastNonopt;
			this.lastNonopt = this.optind;
		}
		private int checkLongOption()
		{
			LongOpt longOpt = null;
			this.longoptHandled = true;
			bool flag = false;
			bool flag2 = false;
			this.longind = -1;
			int num = this.nextchar.IndexOf("=");
			if (num == -1)
			{
				num = this.nextchar.Length;
			}
			for (int i = 0; i < this.longOptions.Length; i++)
			{
				if (this.longOptions[i].Name.StartsWith(this.nextchar.Substring(0, num)))
				{
					if (this.longOptions[i].Name.Equals(this.nextchar.Substring(0, num)))
					{
						longOpt = this.longOptions[i];
						this.longind = i;
						flag2 = true;
						break;
					}
					if (longOpt == null)
					{
						longOpt = this.longOptions[i];
						this.longind = i;
					}
					else
					{
						flag = true;
					}
				}
			}
			if (flag && !flag2)
			{
				if (this.opterr)
				{
					object[] arg = new object[]
					{
						this.progname,
						this.argv[this.optind]
					};
					Console.Error.WriteLine("{0}: option ''{1}'' is ambiguous", arg);
				}
				this.nextchar = "";
				this.optopt = 0;
				this.optind++;
				return 63;
			}
			if (longOpt == null)
			{
				this.longoptHandled = false;
				return 0;
			}
			this.optind++;
			if (num != this.nextchar.Length)
			{
				if (longOpt.HasArg == Argument.No)
				{
					if (this.opterr)
					{
						if (this.argv[this.optind - 1].StartsWith("--"))
						{
							object[] arg2 = new object[]
							{
								this.progname,
								longOpt.Name
							};
							Console.Error.WriteLine("{0}: option ''--{1}'' doesn't allow an argument", arg2);
						}
						else
						{
							object[] arg3 = new object[]
							{
								this.progname,
								this.argv[this.optind - 1][0],
								longOpt.Name
							};
							Console.Error.WriteLine("{0}: option ''{1}{2}'' doesn't allow an argument", arg3);
						}
					}
					this.nextchar = "";
					this.optopt = longOpt.Val;
					return 63;
				}
				if (this.nextchar.Substring(num).Length > 1)
				{
					this.optarg = this.nextchar.Substring(num + 1);
				}
				else
				{
					this.optarg = "";
				}
			}
			else
			{
				if (longOpt.HasArg == Argument.Required)
				{
					if (this.optind < this.argv.Length)
					{
						this.optarg = this.argv[this.optind];
						this.optind++;
					}
					else
					{
						if (this.opterr)
						{
							object[] arg4 = new object[]
							{
								this.progname,
								this.argv[this.optind - 1]
							};
							Console.Error.WriteLine("{0}: option ''{1}'' requires an argument", arg4);
						}
						this.nextchar = "";
						this.optopt = longOpt.Val;
						if (this.optstring[0] == ':')
						{
							return 58;
						}
						return 63;
					}
				}
			}
			this.nextchar = "";
			if (longOpt.Flag != null)
			{
				longOpt.Flag.Length = 0;
				longOpt.Flag.Append(longOpt.Val);
				return 0;
			}
			return longOpt.Val;
		}
		public int getopt()
		{
			this.optarg = null;
			if (this.endparse)
			{
				return -1;
			}
			if (this.nextchar == null || this.nextchar.Length == 0)
			{
				if (this.lastNonopt > this.optind)
				{
					this.lastNonopt = this.optind;
				}
				if (this.firstNonopt > this.optind)
				{
					this.firstNonopt = this.optind;
				}
				if (this.ordering == Getopt.Order.Permute)
				{
					if (this.firstNonopt != this.lastNonopt && this.lastNonopt != this.optind)
					{
						this.exchange(this.argv);
					}
					else
					{
						if (this.lastNonopt != this.optind)
						{
							this.firstNonopt = this.optind;
						}
					}
					while (this.optind < this.argv.Length && (this.argv[this.optind].Length == 0 || this.argv[this.optind][0] != '-' || this.argv[this.optind].Equals("-")))
					{
						this.optind++;
					}
					this.lastNonopt = this.optind;
				}
				if (this.optind != this.argv.Length && this.argv[this.optind].Equals("--"))
				{
					this.optind++;
					if (this.firstNonopt != this.lastNonopt && this.lastNonopt != this.optind)
					{
						this.exchange(this.argv);
					}
					else
					{
						if (this.firstNonopt == this.lastNonopt)
						{
							this.firstNonopt = this.optind;
						}
					}
					this.lastNonopt = this.argv.Length;
					this.optind = this.argv.Length;
				}
				if (this.optind == this.argv.Length)
				{
					if (this.firstNonopt != this.lastNonopt)
					{
						this.optind = this.firstNonopt;
					}
					return -1;
				}
				if (this.argv[this.optind].Length == 0 || this.argv[this.optind][0] != '-' || this.argv[this.optind].Equals("-"))
				{
					if (this.ordering == Getopt.Order.RequireOrder)
					{
						return -1;
					}
					this.optarg = this.argv[this.optind++];
					return 1;
				}
				else
				{
					if (this.argv[this.optind].StartsWith("--"))
					{
						this.nextchar = this.argv[this.optind].Substring(2);
					}
					else
					{
						this.nextchar = this.argv[this.optind].Substring(1);
					}
				}
			}
			if (this.longOptions != null && (this.argv[this.optind].StartsWith("--") || (this.longOnly && (this.argv[this.optind].Length > 2 || this.optstring.IndexOf(this.argv[this.optind][1]) == -1))))
			{
				int result = this.checkLongOption();
				if (this.longoptHandled)
				{
					return result;
				}
				if (!this.longOnly || this.argv[this.optind].StartsWith("--") || this.optstring.IndexOf(this.nextchar[0]) == -1)
				{
					if (this.opterr)
					{
						if (this.argv[this.optind].StartsWith("--"))
						{
							object[] arg = new object[]
							{
								this.progname,
								this.nextchar
							};
							Console.Error.WriteLine("{0}: unrecognized option ''--{1}''", arg);
						}
						else
						{
							object[] arg2 = new object[]
							{
								this.progname,
								this.argv[this.optind][0],
								this.nextchar
							};
							Console.Error.WriteLine("{0}: unrecognized option ''{1}{2}''", arg2);
						}
					}
					this.nextchar = "";
					this.optind++;
					this.optopt = 0;
					return 63;
				}
			}
			int num = (int)this.nextchar[0];
			if (this.nextchar.Length > 1)
			{
				this.nextchar = this.nextchar.Substring(1);
			}
			else
			{
				this.nextchar = "";
			}
			string text = null;
			if (this.optstring.IndexOf((char)num) != -1)
			{
				text = this.optstring.Substring(this.optstring.IndexOf((char)num));
			}
			if (this.nextchar.Length == 0)
			{
				this.optind++;
			}
			if (text == null || num == 58)
			{
				if (this.opterr)
				{
					if (this.posixlyCorrect)
					{
						object[] arg3 = new object[]
						{
							this.progname,
							(char)num
						};
						Console.Error.WriteLine("{0}: illegal option -- {1}", arg3);
					}
					else
					{
						object[] arg4 = new object[]
						{
							this.progname,
							(char)num
						};
						Console.Error.WriteLine("{0}: invalid option -- {1}", arg4);
					}
				}
				this.optopt = num;
				return 63;
			}
			if (text[0] != 'W' || text.Length <= 1 || text[1] != ';')
			{
				if (text.Length > 1 && text[1] == ':')
				{
					if (text.Length > 2 && text[2] == ':')
					{
						if (this.nextchar.Length != 0)
						{
							this.optarg = this.nextchar;
							this.optind++;
						}
						else
						{
							this.optarg = null;
						}
						this.nextchar = null;
					}
					else
					{
						if (this.nextchar.Length != 0)
						{
							this.optarg = this.nextchar;
							this.optind++;
						}
						else
						{
							if (this.optind == this.argv.Length)
							{
								if (this.opterr)
								{
									object[] arg5 = new object[]
									{
										this.progname,
										(char)num
									};
									Console.Error.WriteLine("{0}: option requires an argument -- {1}", arg5);
								}
								this.optopt = num;
								if (this.optstring[0] == ':')
								{
									return 58;
								}
								return 63;
							}
							else
							{
								this.optarg = this.argv[this.optind];
								this.optind++;
								if (this.posixlyCorrect && this.optarg.Equals("--"))
								{
									if (this.optind == this.argv.Length)
									{
										if (this.opterr)
										{
											object[] arg6 = new object[]
											{
												this.progname,
												(char)num
											};
											Console.Error.WriteLine("{0}: option requires an argument -- {1}", arg6);
										}
										this.optopt = num;
										if (this.optstring[0] == ':')
										{
											return 58;
										}
										return 63;
									}
									else
									{
										this.optarg = this.argv[this.optind];
										this.optind++;
										this.firstNonopt = this.optind;
										this.lastNonopt = this.argv.Length;
										this.endparse = true;
									}
								}
							}
						}
						this.nextchar = null;
					}
				}
				return num;
			}
			if (this.nextchar.Length != 0)
			{
				this.optarg = this.nextchar;
			}
			else
			{
				if (this.optind == this.argv.Length)
				{
					if (this.opterr)
					{
						object[] arg7 = new object[]
						{
							this.progname,
							(char)num
						};
						Console.Error.WriteLine("{0}: option requires an argument -- {1}", arg7);
					}
					this.optopt = num;
					if (this.optstring[0] == ':')
					{
						return 58;
					}
					return 63;
				}
				else
				{
					this.nextchar = this.argv[this.optind];
					this.optarg = this.argv[this.optind];
				}
			}
			num = this.checkLongOption();
			if (this.longoptHandled)
			{
				return num;
			}
			this.nextchar = null;
			this.optind++;
			return 87;
		}
	}
}

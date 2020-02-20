using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace FFWSC
{
    /// <summary>
    /// github page https://github.com/zapezhman
    /// </summary>
    public partial class Builder : Window
    {
        public string Hash { get; set; }
		public string Startup { get; set; } = "false";
		public string Autoscan { get; set; } = "false";
		public string Customdirectory { get; set; } = "";
		public string Filelentgh { get; set; }
        public Builder(string hash, string filelentgh , string customdirectory)
        {
            InitializeComponent();
            Hash = hash; Filelentgh = filelentgh;
			if (!string.IsNullOrEmpty(customdirectory))
			{
				Customdirectory = customdirectory;
				LBLdirectory.Content += customdirectory;
			}
			else
			{
				Customdirectory = "all";
				LBLdirectory.Content += "All Driver {c,d,e,f,g...}";
			}

        }

        private void BTNbuild_Click(object sender, RoutedEventArgs e)
        {

			//Assembly assembly;
			//using (MemoryStream assemblyStream = new MemoryStream(File.ReadAllBytes("FFWSC_Core.exe")))
			//{
			//    // 1. Get the reference to third-party method.
			//    AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(assemblyStream);

			//    ReplaceString("{hash}", "12324253414", assemblyDef);



			//}
		
		
			AssemblyDefinition definition = AssemblyDefinition.ReadAssembly("FFWSC_Core.exe");
			bool flag2;
			try
			{
				Collection<ModuleDefinition>.Enumerator enumerator3 = definition.Modules.GetEnumerator();
				while (enumerator3.MoveNext())
				{
					ModuleDefinition definition2 = enumerator3.Current;
					try
					{
						IEnumerator<TypeDefinition> enumerator = (IEnumerator<TypeDefinition>)definition2.Types.GetEnumerator();
						while (enumerator.MoveNext())
						{
							TypeDefinition current = enumerator.Current;
				
							try
							{
								IEnumerator<MethodDefinition> enumerator2 = (IEnumerator<MethodDefinition>)current.Methods.GetEnumerator();
								while (enumerator2.MoveNext())
								{
									MethodDefinition definition3 = enumerator2.Current;
									//bool flag = definition3.IsConstructor && definition3.HasBody;
									bool flag =  definition3.HasBody;
									if (flag)
									{
										try
										{
											Collection<Instruction>.Enumerator enumerator4 = definition3.Body.Instructions.GetEnumerator();
											while (enumerator4.MoveNext())
											{
												Instruction instruction = enumerator4.Current;
												flag2 = (instruction.OpCode.Code == Code.Ldstr & instruction.Operand != null);
												if (flag2)
												{
													string str2 = instruction.Operand.ToString();

													flag2 = str2.Contains("{hash}");
													if (flag2)
													{
														instruction.Operand = Hash;
													}
													flag2 = str2.Contains("{Len}");
													if (flag2)
													{
														instruction.Operand = Filelentgh;
													}
													flag2 = str2.Contains("{startup}");
													if (flag2)
													{
														instruction.Operand = Startup;
													}
													flag2 = str2.Contains("{autoscan}");
													if (flag2)
													{
														instruction.Operand = Autoscan;
													}
													
													flag2 = str2.Contains("{customdirectory}");
													if (flag2)
													{
														instruction.Operand = Customdirectory;
													}
													flag2 = str2.Contains("{name}");
													if (flag2)
													{
														instruction.Operand = TXTantivirus_name.Text;
													}

												}
											}
										}
										finally
										{
											//Collection<Instruction>.Enumerator enumerator4 = definition3.Body.Instructions.GetEnumerator();
											//((IDisposable)enumerator4).Dispose();
										}
									}
								}
							}
							finally
							{
								//IEnumerator<MethodDefinition> enumerator2 = null;
								//enumerator2.Dispose();
							}
						} 
					}
					finally
					{
						//IEnumerator<TypeDefinition> enumerator = null;
						//enumerator.Dispose();
					}
				}
			}
			finally
			{
				//Collection<ModuleDefinition>.Enumerator enumerator3 = definition.Modules.GetEnumerator();
				//((IDisposable)enumerator3).Dispose();
			}
			using (SaveFileDialog dialog2 = new SaveFileDialog())
			{
				dialog2.Filter = "(.exe) |*.exe";
				dialog2.FileName = TXTantivirus_name.Text;
				dialog2.ShowDialog();

				definition.Write(dialog2.FileName);
				this.Close();
			};
			
		

		}


		private void CHstartup_Checked(object sender, RoutedEventArgs e)
		{
			if ((bool)CHstartup.IsChecked)
			{
				Startup = "true";
			}
			else
			{
				Startup = "false";
			}
		}

		private void BTNautoscan_Checked(object sender, RoutedEventArgs e)
		{
			if ((bool)BTNautoscan.IsChecked)
			{
				Autoscan = "true";
			}
			else
			{
				Autoscan = "false";
			}
		}

	
	}
}

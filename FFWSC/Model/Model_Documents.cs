using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FFWSC.Model
{
    class Model_Documents
    {

		public string FileName { get; set; }  
	        public string FilePath { get; set; }  
	        public string FileDescription { get; set; }

		public string Color { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_FilePath"></param>
		/// <param name="_FileName"></param>
		/// <param name="fileDescription"></param>
		/// <param name="type"> file = 1 , folder = 2 , drive = 3</param>
		public Model_Documents(string _FilePath, string _FileName, string fileDescription , int type)
        {  
	              
	            FilePath = _FilePath;  
	            FileName = _FileName;  
	            FileDescription = fileDescription;
			switch (type)
			{
				case 1:
					Color = "#778ca3";
					break;
				case 2:
					Color = "#4b6584";
					break;
				case 3:
					Color = "#4b7bec";
					break;
				default:
					Color = "";
					break;
			}



		}  
    }
}

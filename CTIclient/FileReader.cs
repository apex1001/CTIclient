using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace CTIclient
{
    class FileReader
    {
        /**
         * Read the given file 
         * 
         * @param filename including path
         * @return ArrayList with all text lines;
         * 
         */        
        public static ArrayList read(String fileName)
        {
            try
            {
                ArrayList text = new ArrayList();
                StreamReader reader = File.OpenText(fileName);                
                {
                    
                    String line="";
                    while ((line = reader.ReadLine()) != null)
                    {                        
                        text.Add(line);
                    }
                }
                return text;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }
    }
}


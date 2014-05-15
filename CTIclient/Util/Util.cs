/*
 * Utility class for CTIclient.
 * 
 * Author: V. Vogelesang 
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace CTIclient
{
    static class Util
    {
        // Imports for terminal services check & messagebox        
        [DllImport("user32.dll", EntryPoint = ("GetSystemMetrics"))]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSSendMessage(
                    IntPtr hServer,
                    [MarshalAs(UnmanagedType.I4)] int SessionId,
                    String pTitle,
                    [MarshalAs(UnmanagedType.U4)] int TitleLength,
                    String pMessage,
                    [MarshalAs(UnmanagedType.U4)] int MessageLength,
                    [MarshalAs(UnmanagedType.U4)] int Style,
                    [MarshalAs(UnmanagedType.U4)] int Timeout,
                    [MarshalAs(UnmanagedType.U4)] out int pResponse,
                    bool bWait);

        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        public static int WTS_CURRENT_SESSION = -1;
        
        /**
         * Clean unwanted chars from a phonenumber
         * 
         * @param string to clean
         * @return cleaned string
         * 
         */
        public static string CleanPhoneNumber(String number)
        {
            string output = Regex.Replace(number, "[^0-9+]", "");
            return output.Replace("+", "00");            
        }

        /**
         * Convert object to JSON
         * 
         * @param Object to convert
         * @return JSON string
         * 
         */
        public static string toJSON(Object obj)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(obj);
            return json;
        }

        /**
         * Convert JSON to Object
         * 
         * @param JSON string to convert
         * @return CommandObject
         * 
         */
        public static CommandObject fromJSON(string json)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                CommandObject commandObject = js.Deserialize<CommandObject>(json);
                return commandObject;
            }
            catch // (Exception e)
            {
                // MessageBox.Show(e.Message);
                return null;
            }
        } 

        /**
         * Remove an item from an array
         * 
         * @param index of item to remove
         * @param the array to remove it from
         * @return the new array
         * 
         */
        public static String[][] ArrayRemoveAt(int index, String[][] array)
        {
            int newIndex = 0;
            String[][] newArray = new String[array.Length-1][];
            for (int i = 0; i < array.Length; i++)
            {
                if (i == index) continue;
                newArray[newIndex] = array[i];
                newIndex++;
            }
            return newArray;
        }

        /**
         * Add an array item to the end of another array
         * 
         * @param item to add
         * @param the array to add to
         * @return the new array
         * 
         */
        public static String[][] ArrayAddItem(String[] item, String[][] array, int size)
        {
            int itemSize = size;
            String[][] newArray = new String[array.Length + 1][];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
           newArray[array.Length] = item;
           return newArray;
        }

        /**
         * Parses a settings file into a dictionary with 
         * key / value pairs
         * 
         * @param filename
         * @return dicitionary
         * 
         */
        public static Dictionary<String, String> parseSettingsFile(String fileName)
        {        
            ArrayList inputText = FileReader.read(fileName);           
            if (inputText != null)
            {
                Dictionary<String, String> settingsList = new Dictionary<String, String>();
                foreach (String line in inputText)
                {
                    if (line.Equals("") || line.StartsWith(";") || line.StartsWith(" ")) continue;
                    String[] kvString = line.Split(new Char[] {'='});                    
                    settingsList.Add(kvString.GetValue(0).ToString().Trim(), kvString.GetValue(1).ToString().Trim());
                }               
                return settingsList;
            }            
            return null;
        }

        /**
         * Get an MD5 hash from a string
         * 
         * @param textstring
         * @return hashString
         * 
         */
        public static String getHash(String text)
        {
            String hashString = "";
            if (!text.Equals("") && text != null)
            {
                MD5 md5Hash = MD5.Create();
                byte[] input = Encoding.ASCII.GetBytes(text + "C7117C");
                byte[] output = md5Hash.ComputeHash(input);

                foreach (byte part in output)
                {
                    hashString += part.ToString("X2");
                }
            }
            return hashString;
        }

        /**
         * Show a messagebox via MessageBox or WTSSendMessage
         * 
         * @param message
         * @param title
         * @param MessageBoxButtons
         * @return DialogResult
         * 
         */
        public static DialogResult showMessageBox(String message, String title = "", Object buttons = null)
        {
            if (!checkRemoteSession())
            {
                if (buttons != null)
                    return MessageBox.Show(message, title, (MessageBoxButtons) buttons);
                return MessageBox.Show(message, title);
            }
            else
            {
                return showWTSmsg(message, title, buttons);
            }
        }

        /**
         * Check if the session is remote / rdp
         * 
         * @return boolean if remote session
         * 
         */
        private static Boolean checkRemoteSession()
        {
            int SM_REMOTESESSION = 0x2001;
            //This function will return 0 if the session is local
            return (GetSystemMetrics(SM_REMOTESESSION) > 0);
            
        }

        /**
         * Send WTS message for WTS / RDP purposes
         * 
         * @param message
         * @param title
         * @param MessageBoxButtons
         * @return DialogResult
         * 
         */
        private static DialogResult showWTSmsg(String message, String title, Object buttons)
        {
            DialogResult dialogResult = new DialogResult();
            Boolean result = false;         
            int tlen = title.Length;   
            int mlen = message.Length;
            int response = 0;

            // Check if other buttons should be shown
            // TODO: Check actual required buttons
            int MB_YESNOCANCEL = 0;
            if (buttons != null)
                MB_YESNOCANCEL = 0x00000003;

            // Show the messagebox
            result = WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, title, tlen,
                                    message, mlen, MB_YESNOCANCEL, 0, out response, true);

            // Map response to DialogResult
            switch (response)
            {
                case 2:
                    dialogResult = DialogResult.Cancel;
                    break;

                case 6:
                    dialogResult = DialogResult.Yes;
                    break;

                case 7:
                    dialogResult = DialogResult.No;
                    break;
            }

            return dialogResult;
        }

        /**
         * Get a unique pipe name from the username+hash
         *
         * @return pipeName
         * 
         */
        public static String getPipeName(String userName, String prefix = "")
        {          
            userName = (userName.Split('-').Last());
            return prefix + "Pipe" + getHash(userName).Substring(0, 8);
        }
    }
}

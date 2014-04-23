/* 
 * ADUser for for CTIclient.
 * 
 * @Author: V. Vogelesang
 * 
 * Built from the information @
 * http://stackoverflow.com/questions/1785751/how-to-get-company-and-department-from-active-directory-given-a-userprincipa
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Windows.Forms;

namespace CTIclient
{
    public class ADUser
    {
        private UserPrincipal user;
        private string username;
        private string mobilePhone;
        
        public ADUser()
        {
            try
            {
                user = UserPrincipal.Current;
                mobilePhone = "";
                username = user.SamAccountName;

                // Get the underlying object for user properties
                DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                if (directoryEntry.Properties.Contains("mobile"))
                    mobilePhone = directoryEntry.Properties["mobile"].Value.ToString();               
            }

            catch (Exception e)
            {
                MessageBox.Show("Authentication error:" + e.Message);
            }
        }
    
        /**
         * Return the userprincipal object
         * 
         * @return UserPrincipal
         * 
         */
        public UserPrincipal getUserObject()
        {
            return this.user;
        }

        /**
         * Return the userName
         * 
         * @return username
         * 
         */
        public string getUserName()
        {
            return this.username;
        }

        /**
         * Return the mobile phone number
         * 
         * @return phone number
         * 
         */
        public string getMobilePhone()
        {
            return this.mobilePhone;
        }
    }
}


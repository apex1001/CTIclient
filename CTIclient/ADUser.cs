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
using System.Security.Principal;

namespace CTIclient
{
    public class ADUser
    {
        private UserPrincipal user = null;
        private string username;
        private string mobilePhone;
        
        public ADUser()
        {
            mobilePhone = "";
            username = Environment.UserName;

            // Check for local user. This check is faster than getting UserPrincipal
            // on domain query timeout.
            string machineUsername = WindowsIdentity.GetCurrent().Name;
            if (machineUsername.ToLower().Contains("politienl"))
            {
                try
                {
                    getUserPrincipal();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Authentication error:" + e.Message);
                }
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
            if (this.user == null)
                getUserPrincipal();
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

        /**
         * Get user principal
         * 
         * @return UserPrincipal
         * 
         */
        private UserPrincipal getUserPrincipal()
        {
            user = UserPrincipal.Current;
            username = user.SamAccountName;
                   
            //Get the underlying object for user properties
            DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry.Properties.Contains("mobile"))
            {
                mobilePhone = directoryEntry.Properties["mobile"].Value.ToString();
            }
            return user;
        }   
    }
}


/*
 * POCO class for CTIclient
 * Stores commands for client server communications
 * 
 * @author: V. Vogelesang
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTIclient
{
    /**
     * A simple POCO for storing commands
     * 
     */    
    public class CommandObject
    {
        private string command;
        private string status;
        private string from;
        private string to;
        private string target;
        private string pin;
        private string user;
        private string value;

        public CommandObject(
            string command = "",
            string status = "",
            string from = "", 
            string to = "", 
            string target = "", 
            string pin = "", 
            string user = "", 
            string value = "")
        {
            this.command = command;
            this.status = status;
            this.from = from;
            this.to = to;
            this.target = target;
            this.pin = pin;
            this.user = user;
            this.value = value;
        }
        
        public string Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }

        public string From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.from = value;
            }
        }

        public string To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        public string Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
            }
        }

        public string Pin
        {
            get
            {
                return this.pin;
            }
            set
            {
                this.pin = value;
            }
        }

        public string User
        {
            get
            {
                return this.user;
            }
            set
            {
                this.user = value;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

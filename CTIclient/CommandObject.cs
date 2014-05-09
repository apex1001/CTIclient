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
    [Serializable()]
    public class CommandObject
    {
        private String command;
        private String status;
        private String from;
        private String to;
        private String target;
        private String pin;
        private String user;
        private String role;
        private String[][] value;

        public CommandObject()
        {
        }

        public CommandObject(
            String command = "",
            String status = "",
            String from = "",
            String to = "",
            String target = "",
            String pin = "",
            String user = "",
            String role = "",
            String[][] value = null)
        {
            this.command = command;
            this.status = status;
            this.from = from;
            this.to = to;
            this.target = target;
            this.pin = pin;
            this.User = user;
            this.role = role;
            this.value = value;
        }

        public String Command
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

        public String Status
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

        public String From
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

        public String To
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

        public String Target
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

        public String Pin
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

        public String User
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

        public String Role
        {
            get
            {
                return this.role;
            }
            set
            {
                this.role = value;
            }
        }

        public String[][] Value
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

﻿/*
 * Utility class for CTIclient.
 * 
 * Author: V. Vogelesang 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace CTIclient
{
    static class Util
    {
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
    }
}

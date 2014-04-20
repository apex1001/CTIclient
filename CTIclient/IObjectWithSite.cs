/**
 * IObjectWithSite interface for BHOController
 * 
 * Borrowed from:
 * http://www.codeproject.com/Articles/19971/How-to-attach-to-Browser-Helper-Object-BHO-with-C
 * 
 * author: V. Vogelesang
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CTIclient
{
    [
        ComVisible(true),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")
    ]

    public interface IObjectWithSite
    {
        [PreserveSig]
        int SetSite([MarshalAs(UnmanagedType.IUnknown)]object site);
        [PreserveSig]
        int GetSite(ref Guid guid, out IntPtr ppvSite);
    }
}
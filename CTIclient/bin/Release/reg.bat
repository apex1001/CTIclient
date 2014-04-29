gacutil /uf CTIclient
gacutil /uf WebSocket4Net
gacutil /uf Interop.SHDocVw
gacutil /uf BandObjectLib
%windir%\Microsoft.NET\Framework\v4.0.30319\regasm CTIclient.dll /u

gacutil /if CTIclient.dll
gacutil /if WebSocket4Net.dll
gacutil /if Interop.SHDocVw.dll
gacutil /if BandObjectLib.dll
%windir%\Microsoft.NET\Framework\v4.0.30319\regasm /codebase CTIclient.dll

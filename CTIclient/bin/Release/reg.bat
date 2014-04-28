gacutil /uf CTIclient
gacutil /uf WebSocket4Net
gacutil /uf Interop.SHDocVw
regasm CTIclient.dll /u

gacutil /if CTIclient.dll
gacutil /if WebSocket4Net.dll
gacutil /if Interop.SHDocVw.dll
regasm /codebase CTIclient.dll


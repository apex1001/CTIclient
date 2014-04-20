gacutil /uf CTIclient
gacutil /uf WebSocket4Net
regasm CTIclient.dll /u

gacutil /if CTIclient.dll
gacutil /if WebSocket4Net.dll
regasm /codebase CTIclient.dll




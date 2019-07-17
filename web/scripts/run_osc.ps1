$ip=get-WmiObject Win32_NetworkAdapterConfiguration|Where {$_.Ipaddress.length -gt 1} 
$ip = $ip.ipaddress[0] 

py -3 ($PSScriptRoot + "\..\..\oscserver\server.py") --ip $ip

pause
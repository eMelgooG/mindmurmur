$ip=get-WmiObject Win32_NetworkAdapterConfiguration|Where {$_.Ipaddress.length -gt 1} 
$ip = $ip.ipaddress[0] 

py -3 "C:\Users\Mind Murmur\Documents\mindmurmur\oscserver\server.py" --ip $ip
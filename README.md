# WCFLoadUI

WCFLoadUI is a utility to perform load test on WSDL based services and rest services.

##### Features:
1.	Load test WCF services on HTTP and TCP protocol
2.	Load test rest services
3.	Functional testing
4.	Distributed testing
5.	Client readiness is not required
6.	UI based editing of complex types
7.	Logs all error in application logs

##### For distributed testing:
1.	All nodes should be in same network
2.	User running the WCFLoadUI should be admin on all nodes
3.	WMI should be enabled on all nodes and should be added as exception in firewall
4.	Net.tcp listener service should be running on master node

This utility uses InnoSetup to create the installer. If you are only interested in installing this utility, you can do so by installing application using setup in WCFLoadUISetupOutput/setup.exe folder.

I  created this utility during my spare time and as of now this utility lacks in a lot of features, which I am working on.

If you would like to contribute, please drop in an email at (lokeshlal [at] gmail.com).

Hopefully someone may find this useful.

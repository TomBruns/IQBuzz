https://www.mongodb.com/
	v4.0.4 2008R2Plus SSL 64 Bit Community Edition
	C:\Program Files\MongoDB\Server\4.0\data\

	Compass 1.16.3

https://ngrok.com/
	ngrok is a reverse proxy that creates a secure tunnel from a public endpoint to a locally running web service.
============
Setup
============
	ngrok	$ ./ngrok authtoken 85p8DQmoDYf4ZhVAQShB9_4m37dzHFeEHNsWdkVWmYe

	C:\Users\xtobr\Source\Repos\Worldpay\Twilio>"C:\Users\xtobr\Downloads\General Windows\ngrok\ngrok-stable-windows-amd64\ngrok.exe" authtoken 85p8DQmoDYf4ZhVAQShB9_4m37dzHFeEHNsWdkVWmYe
	Authtoken saved to configuration file: C:\Users\xtobr/.ngrok2/ngrok.yml

============
Start ngrok
============

	C:\Users\xtobr\Source\Repos\Worldpay\Twilio>"C:\Users\xtobr\Downloads\General Windows\ngrok\ngrok-stable-windows-amd64\ngrok.exe" http 5000

	ngrok by @inconshreveable                                                                               (Ctrl+C to quit)

	Session Status                online
	Account                       Tom Bruns (Plan: Free)
	Version                       2.2.8
	Region                        United States (us)
	Web Interface                 http://127.0.0.1:4040
	Forwarding                    http://e9ba9abd.ngrok.io -> localhost:5000
	Forwarding                    https://e9ba9abd.ngrok.io -> localhost:5000

	Connections                   ttl     opn     rt1     rt5     p50     p90
								  5       0       0.01    0.01    0.04    0.31

	HTTP Requests
	-------------

	POST /sms                      200 OK
	POST /sms                      200 OK
	POST /sms                      200 OK
	POST /sms                      200 OK
	POST /sms                      200 OK

============
Update Twilio Portal
============

	https://www.twilio.com/console/phone-numbers/incoming

	Messaging > A messgae Comes in > WebHook > https://e9ba9abd.ngrok.io/sms

============
Run MVC App
============

	C:\Users\xtobr\Source\Repos\Worldpay\Twilio\TwilioReceive>dotnet run


	$userName = 'desktop-650ch8f\xtobr'

Unhandled Exception: System.IO.IOException: Failed to bind to address http://localhost:5000. ---> System.AggregateException: One or more errors occurred. (An attempt was made to access a socket in a way forbidden by its access permissions) (An attempt was made to access a socket in a way forbidden by its access permissions) ---> System.Net.Sockets.SocketException: An attempt was made to access a socket in a way forbidden by its access permissions

netsh http add urlacl url=http://127.0.0.1:5000/ user=$userName
netsh http add urlacl url=http://localhost:5000/ user=$userName
netsh http add urlacl url=http://$env:computername`:5000/ user=$userName



https://github.com/aspnet/Tooling/blob/AspNetVMs/docs/create-asp-net-vm-with-webdeploy.md

============
Azure Get possible outbound addresses
============

TwilioReceive20181217080037

az webapp show --resource-group Default-MachineLearning-SouthCentralUS --name TwilioReceive20181217080037 --query outboundIpAddresses --output tsv

52.170.46.174,
52.170.41.18,
13.72.72.252,
52.170.41.233,
52.186.122.173

============
App COnfig
============

www.twilio.com				WP Hackathon-v1			(812) 594-4088
www.azure.com				App Service				https://twilioreceive20181217080037.azurewebsites.net/sms
https://cloud.mongodb.com	Atlas DB				IQBuzzCluster	mongodb+srv://IQBuzzMongoUser:@iqbuzzcluster-ueauw.azure.mongodb.net/admin


====================================
How to setup Local Debugging
====================================
1. Tools > Start nGrok Tunnels

		ngrok by @inconshreveable                                                                               (Ctrl+C to quit) 
		Session Status                online                                                                                    
		Account                       Tom Bruns (Plan: Free)                                                                    
		Version                       2.3.34                                                                                    
		Region                        United States (us)                                                                        
		Web Interface                 http://127.0.0.1:4040                                                                     
		Forwarding                    http://e26eda19.ngrok.io -> http://localhost:44370                                        
		Forwarding                    https://e26eda19.ngrok.io -> http://localhost:44370 

2.	Start WebSite

		C:\Users\xtobr\Source\Repos\Worldpay\Hackathon\TwilioReceive>dotnet run --urls=http://localhost:44370

3.	Configure Twilio
		Twillio > Manage Numbers > Active Numbers > configure > messaging > a message comes in
		
			Webhook	> https://e26eda19.ngrok.io/sms  > HTTP Post

4. Start Console Tool

		C:\Users\xtobr\Source\Repos\Worldpay\Hackathon\WP.Learning.BackOffice.Console>dotnet run


====================================
Find out Azure outbound addresses
====================================

az webapp show --resource-group cloud-shell-storage-eastus --name TwilioReceive20190915105921 --query outboundIpAddresses --output tsv

168.61.152.85,		==> 168.61.152.1/24
168.61.157.210,		==> 168.61.157.1/24
168.61.155.254,		==> 168.61.155.1/24
168.61.154.160		==> 168.61.154.1/24

1. Config Twillo to call azure webapi
2. Add Azure subnets to MongoDB whitelist

====================================
Publish Notes
====================================
you may need to add a gloabl.json file to set the .net core fx version to use if no content is published
(issue is target .NETCore ver << latest version installed)

C:\Users\xtobr\Source\Repos\Worldpay\Hackathon>dotnet --info
	.NET Core SDK (reflecting any global.json):
	 Version:   2.2.101
	 Commit:    236713b0b7

	Runtime Environment:
	 OS Name:     Windows
	 OS Version:  10.0.18362
	 OS Platform: Windows
	 RID:         win10-x64
	 Base Path:   C:\Program Files\dotnet\sdk\2.2.101\

	Host (useful for support):
	  Version: 3.0.0-preview9-19423-09
	  Commit:  2be172345a

	.NET Core SDKs installed:
	  2.1.2 [C:\Program Files\dotnet\sdk]
	  2.1.4 [C:\Program Files\dotnet\sdk]
	  2.1.104 [C:\Program Files\dotnet\sdk]
	  2.1.200 [C:\Program Files\dotnet\sdk]
	  2.1.201 [C:\Program Files\dotnet\sdk]
	  2.1.202 [C:\Program Files\dotnet\sdk]
	  2.1.302 [C:\Program Files\dotnet\sdk]
	  2.1.403 [C:\Program Files\dotnet\sdk]
	  2.1.500 [C:\Program Files\dotnet\sdk]
	  2.1.600-preview-009426 [C:\Program Files\dotnet\sdk]
	  2.1.600-preview-009472 [C:\Program Files\dotnet\sdk]
	  2.1.600-preview-009497 [C:\Program Files\dotnet\sdk]
	  2.1.600 [C:\Program Files\dotnet\sdk]
	  2.1.601 [C:\Program Files\dotnet\sdk]
	  2.1.602 [C:\Program Files\dotnet\sdk]
	  2.1.700-preview-009597 [C:\Program Files\dotnet\sdk]
	  2.1.700-preview-009601 [C:\Program Files\dotnet\sdk]
	  2.1.700-preview-009618 [C:\Program Files\dotnet\sdk]
	  2.1.800-preview-009677 [C:\Program Files\dotnet\sdk]
	  2.1.800-preview-009696 [C:\Program Files\dotnet\sdk]
	  2.2.101 [C:\Program Files\dotnet\sdk]
	  3.0.100-preview9-014004 [C:\Program Files\dotnet\sdk]

	.NET Core runtimes installed:
	  Microsoft.AspNetCore.All 2.1.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.5 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.6 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.7 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.8 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.9 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.11 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.1.12 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.All 2.2.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
	  Microsoft.AspNetCore.App 2.1.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.5 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.6 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.7 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.8 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.9 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.11 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.1.12 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 2.2.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.AspNetCore.App 3.0.0-preview9.19424.4 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
	  Microsoft.NETCore.App 1.0.5 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.0.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.0.5 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.0.6 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.0.7 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.0.9 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.2 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.5 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.6 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.7 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.8 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.9 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.11 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.1.12 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 2.2.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.NETCore.App 3.0.0-preview9-19423-09 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
	  Microsoft.WindowsDesktop.App 3.0.0-preview9-19423-09 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
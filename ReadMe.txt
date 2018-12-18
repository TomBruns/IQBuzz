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
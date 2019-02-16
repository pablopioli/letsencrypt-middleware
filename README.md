# Let's Encrypt

An ASP.NET Core Middleware to automate the generation and renewal of TLS certificates.

Let's Encrypt is a free, automated, and open Certificate Authority (https://letsencrypt.org/). 

This middleware integrates Let's Encrypt with your app. It will automatically contact the https://letsencrypt.org CA and generate an TLS/SSL certificate. It then automatically configures Kestrel to use this certificate for all HTTPs traffic.
Prior to the certificate expiration, renews it automatically.

## How to use it
Documentation is coming soon ...


## Thanks

A lot of code from https://github.com/natemcmaster/LetsEncrypt is included in this project. It also gave me good ideas from where to start.

The bulk of the work of contacting Let's Encrypt is done through https://github.com/fszlin/certes


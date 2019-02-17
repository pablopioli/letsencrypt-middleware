# Let's Encrypt

An ASP.NET Core Middleware to automate the generation and renewal of TLS certificates.

Let's Encrypt is a free, automated, and open Certificate Authority (https://letsencrypt.org/). 

This middleware integrates Let's Encrypt with your app. It will automatically contact the https://letsencrypt.org CA and generate an TLS/SSL certificate. It then automatically configures Kestrel to use this certificate for all HTTPs traffic.
Prior to the certificate expiration, renews it automatically.

## How to use it

The goal is to make de mimimum changes, all inside startup.cs

In **ConfigureServices** set the middleware options and add it to the services collection
```csharp
var leOptions = new LetsEncryptOptions
             {
                 EmailAddress = "hostmaster@example.com",
                 AcceptTermsOfService = true,
                 Hosts = new[] { "example.com" }
             };

services.AddLetsEncrypt(leOptions);
```
You can find the complete options reference in the next section.

In the **Configure** methods you need to insert the middleware in the pipeline. One important thing: Let's Encrypt servers will call your server back to check you are the owner of the domain you are requesting the certificate for (HTTP Challenge). So the */.well-known/acme-challenge* path must be reserved to LetsEncryptMiddleware. For this path you should not use HTTPS because you still don't have a certificate and Let's Encrypt validation will fail.

So you must replace

```csharp
app.UseHttpsRedirection();
app.UseMvc();
```

with 

```csharp
app.MapWhen(
  httpContext => !httpContext.Request.Path.StartsWithSegments(Constants.ChallengePath),
  appBuilder =>
      {
        appBuilder.UseHttpsRedirection();
        appBuilder.UseMvc();
      });
```

In the source code you can find a working sample.

## Options

* AcceptTermsOfService: You must read and accept Let's Encrypt Terms of Service. After you do this set this property to true. Setting this property to false will trigger an exception.

* UseStagingServer: Set this to true in development to use Let's Encrypt staging servers instead of the production servers. https://letsencrypt.org/docs/staging-environment/

* Hosts: An array of the domains you want to request certificates for.

* DaysBefore: The number of days before expiration when LetsEncryptMiddleware will renew the certificate. Defaults to 15.

* EmailAddress: To use Let's Encrypt services you must create an account bound to a valid email adress. LetsEncryptMiddleware can do this for you, just set this property with the appropiate mail adress.

* AccountKey: If you want to handle the Let's Encrypt account for yourself set this property with the private key of the account. You can build an account using [Certes](https://github.com/fszlin/certes)

```csharp
var acme = new AcmeContext(WellKnownServers.LetsEncryptV2);
var account = await acme.NewAccount("admin@example.com", true);

// Save the account key for later use
var pemKey = acme.AccountKey.ToPem();
```

* CacheFolder: Optional, but highly recommended. LetsEncryptMiddleware will use it to store the obtained certificates (and account key if asked to get one). If you not set a cache folder all the certificates (and account) will be kept only in memory. When the server is restarted they will be requested again, abusing Let's Encrypt services.

* EncryptionPassword: Sets a password used to protect the generated certificates. Can be empty but you should use it if you have set a cache folder.

## Nuget

To install the Nuget Package use

```
    Install-Package LetsEncryptMiddleware
```

## Testing in development

See https://github.com/natemcmaster/LetsEncrypt/blob/master/CONTRIBUTING.md for details on how to test LetsEncryptMiddleware in a non-production environment while docs for this project are created.


## Thanks

A lot of code from https://github.com/natemcmaster/LetsEncrypt is included in this project. It also gave me good ideas from where to start.

The bulk of the work of contacting Let's Encrypt is done through https://github.com/fszlin/certes

Pull requests (of the source code or my ugly english) are welcome.

## Other projects where LetsEncryptMiddleware can help

LetsEncryptMiddleware has been tested with https://github.com/damianh/ProxyKit. You can use it to build a reverse proxy and handle all TLS/SSL configuracion in one place. In the source code you can find a working sample.



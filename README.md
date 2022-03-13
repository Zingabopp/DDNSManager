# DDNSManager
Automatically updates DDNS entries. Currently only supports Google DDNS.

## Installation
Can be run as a console app or registered as a Windows service (recommended).

### Windows Service
* Unzip to the location you want to run it from.
* From admin Command Prompt or Powershell:
```
sc.exe create "DDNS Manager" binpath= "C:\Path\To\DDNSManager.Service.exe -c C:\Path\To\config.json"
```
* The folder for `config.json` must already exist, a `config.json` file will be generated if it doesn't already exist.
  * A valid configuration path is required for the service to run.
* To Remove:
```
sc.exe delete "DDNS Manager"
```
## Configuration
The following properties are available for user configuration:
* Interval - The time between update runs
  * Minutes
  * Hours
  * Days
* ServiceSettings - A list of domains you want to update (no practical limit)
**Example:**
```json
{
  "Interval": {
    "Minutes": 0,
    "Hours": 6,
    "Days": 0
  },
  "ServiceSettings": [
    {
      "ServiceId": "GoogleDns",
      "Name": "Friendly Name",
      "Hostname": "your.dynamic.domain",
      "IP": null,
      "Enabled": false,
      "Username": "asdf1234asdf1234",
      "Password": "asdf1234asdf1234"
    },
    {
      "ServiceId": "GoogleDns",
      "Name": null,
      "Hostname": "another.domain.com",
      "IP": "5.5.5.5",
      "Enabled": true,
      "Username": "lkjh4321lkjh4321",
      "Password": "lkjh4321lkjh4321"
    }
  ]
}
```
### Service Configuration
The following properties are common across all services in `ServiceSettings`:
* ServiceId - Identifies which DDNS Manager service is used for this set of settings
* Name - A name assigned to this set of settings by the user (if null, hostname is used)
* Hostname - The domain name to update
* IP - The IP to assign to the given domain name. Leave null to use the external IP of that device DDNS Manager is running on
* Enabled - Set to true if you want this set of settings to be active, false otherwise

#### Service-Specific Settings
The following service(s) are available to configure in `ServiceSettings`:

**Google DNS**
* ServiceId - `GoogleDns` (**the ServiceId must be `GoogleDns`**)
* Username - The username credential supplied by Google for the domain
* Password - The password credential supplied by Google for the domain

## Contributions
It hasn't been put to the test yet, but I did try to structure things to make it easy to add additional services. Pull requests are welcome.

### Adding a Service
DDNSManager.Lib Project
  * Implement `IDDNSService` in the `Services` folder
  * Implement `IServiceSettings` in the `ServiceConfiguration` folder
  * Add an entries for your service in `DefaultServiceRegistration`:
    * `settingsTypeMap` - This is **required** for the `ServiceSettingsConverter` to deserialize your service's settings
    * `serviceFactories` and `serviceSettingsFactories` - Recommended so your service is available by default.
